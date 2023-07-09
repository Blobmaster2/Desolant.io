using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Movement
{
    //THIS IS A WORK IN PROGRESS

    public class PredictiveMovement : NetworkBehaviour
    {
        private int tick = 0;
        private float tickRate = 1f / 60f;
        private float tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private readonly InputState[] inputStates = new InputState[BUFFER_SIZE];
        private readonly TransformState[] transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> serverTransformState = new();
        public TransformState previousTransformState;

        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Transform playerObject;

        public NetworkVariable<float> moveSpeed;
        public NetworkVariable<Vector2> serverMoveDir;

        private int _lastProcessedTick = -0;

        private void OnEnable()
        {
            serverTransformState.OnValueChanged += OnServerStateChanged;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState serverState)
        {
            if (!IsLocalPlayer) return;

            if (previousTransformState == null)
            {
                previousTransformState = serverState;
            }

            TransformState calculatedState = transformStates.First(localState => localState.Tick == serverState.Tick);

            if (calculatedState.Position != serverState.Position)
            {
                Debug.Log("Corrected player");

                CorrectPlayer(serverState);

                IEnumerable <InputState> inputs = inputStates.Where(input => input.Tick > serverState.Tick);
                inputs = from input in inputs orderby input.Tick select input;

                foreach (var inputState in inputs)
                {
                    MovePlayer(inputState.MovementInput, moveSpeed.Value);

                    TransformState newTransformState = new()
                    {
                        Tick = tick,
                        Position = transform.position,
                        Look = playerObject.rotation.eulerAngles.z,
                        HasStartedMoving = true
                    };

                    for (int i = 0; i < transformStates.Length; i++)
                    {
                        if (transformStates[i].Tick == inputState.Tick)
                        {
                            transformStates[i] = newTransformState;
                            break;
                        }
                    }
                }
            }
        }

        private void CorrectPlayer(TransformState state)
        {
            transform.position = state.Position;

            for (int i = 0; i < transformStates.Length; i++)
            {
                if (transformStates[i].Tick == state.Tick)
                {
                    transformStates[i] = state;
                    break;
                }
            }
        }

        public void ProcessLocalPlayerMovement(Vector2 _movementInput, float _lookInput, float speed)
        {
            tickDeltaTime = 0;
            tickDeltaTime += Time.deltaTime;

            if (tickDeltaTime > tickRate)
            {
                int bufferIndex = tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRpc(tick, _movementInput, _lookInput, speed);
                    MovePlayer(_movementInput, speed);
                    Look(_lookInput);
                    SaveState(_movementInput, _lookInput, bufferIndex);
                }
                else
                {
                    //MovePlayer(_movementInput, moveSpeed.Value);
                    Look(_lookInput);

                    TransformState state = new()
                    {
                        Tick = tick,
                        Position = transform.position,
                        Look = playerObject.rotation.eulerAngles.z,
                        HasStartedMoving = true
                    };

                    SaveState(_movementInput, _lookInput, bufferIndex);

                    previousTransformState = serverTransformState.Value;
                    serverTransformState.Value = state;
                }

                tickDeltaTime -= tickRate;

                tick++;
            }
        }

        private void SaveState(Vector2 movementInput, float lookInput, int bufferIndex)
        {
            InputState inputState = new InputState()
            {
                Tick = tick,
                MovementInput = movementInput,
                LookInput = lookInput
            };

            TransformState transformState = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Look = playerObject.rotation.eulerAngles.z,
                HasStartedMoving = true
            };

            inputStates[bufferIndex] = inputState;
            transformStates[bufferIndex] = transformState;
        }

        [ServerRpc]
        void MovePlayerServerRpc(int tick, Vector2 _movementInput, float _lookInput, float speed)
        {
            if (_lastProcessedTick + 1 != tick)
            {
                Debug.Log("packets were lost!");
            }

            _lastProcessedTick = tick;

            MovePlayer(_movementInput, speed);
            Look(_lookInput);

            TransformState state = new()
            {
                Tick = tick,
                Position = transform.position,
                Look = playerObject.rotation.eulerAngles.z,
                HasStartedMoving = true
            };

            previousTransformState = serverTransformState.Value;
            serverTransformState.Value = state;
        }

        void MovePlayer(Vector2 movementInput, float speed)
        {
            var newPos = tickRate * new Vector2(movementInput.x, movementInput.y);

            if (IsServer)
            {
                body.MovePosition(new Vector2(transform.position.x, transform.position.y) + (newPos * moveSpeed.Value));

                //Debug.Log(new Vector2(transform.position.x, transform.position.y) + (newPos * moveSpeed.Value));
            }
            else if (IsLocalPlayer)
            {
                body.MovePosition(new Vector2(transform.position.x, transform.position.y) + (newPos * speed));

                //Debug.Log(new Vector2(transform.position.x, transform.position.y) + (newPos * speed));
            }
        }

        void Look(float _lookInput)
        {
            playerObject.rotation = Quaternion.Euler(new Vector3(0, 0, _lookInput));
        }

        public void SimulateOtherPlayers()
        {
            tickDeltaTime = 0;
            tickDeltaTime += Time.deltaTime;

            if (tickDeltaTime > tickRate)
            {
                if (serverTransformState.Value == null)
                {
                    return;
                }

                if (serverTransformState.Value.HasStartedMoving)
                {
                    transform.position = serverTransformState.Value.Position;
                    playerObject.rotation =
                        Quaternion.Euler(new Vector3(0, 0, serverTransformState.Value.Look));
                }

                tickDeltaTime -= tickRate;

                tick++;
            }
        }
    }

    public class InputState
    {
        public int Tick;
        public Vector2 MovementInput;
        public float LookInput;
    }

    public class TransformState : INetworkSerializable
    {
        public int Tick;
        public Vector2 Position;
        public float Look;
        public bool HasStartedMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Position);
                reader.ReadValueSafe(out Look);
                reader.ReadValueSafe(out HasStartedMoving);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Position);
                writer.WriteValueSafe(Look);
                writer.WriteValueSafe(HasStartedMoving);
            }
        }
    }
}

