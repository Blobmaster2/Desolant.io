using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Movement
{
    public class NetworkMovement : NetworkBehaviour
    {
        [SerializeField] Rigidbody2D rb;
        [SerializeField] Camera camPosition;

        int tick = 0;
        float tickRate = 1f / 60f;
        float tickDeltaTime = 0f;

        const int BUFFER_SIZE = 1024;

        InputState[] inputStates = new InputState[BUFFER_SIZE];
        TransformState[] transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> serverTransformState = new NetworkVariable<TransformState>();
        public TransformState previousTransformState;

        void OnEnable()
        {
            serverTransformState.OnValueChanged += OnServerStateChanged;
        }

        void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            previousTransformState = previousvalue;

        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput)
        {
            tickDeltaTime += Time.deltaTime;

            if (tickDeltaTime > tickRate)
            {
                int bufferIndex = tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRpc(tick, movementInput);
                    MovePlayer(movementInput);
                }
                else
                {
                    MovePlayer(movementInput);
                }

                TransformState state = new TransformState()
                {
                    Tick = tick,
                    Position = transform.position,
                    HasStartedMoving = true
                };

                previousTransformState = serverTransformState.Value;
                UpdateServerVariableServerRpc(state);


                InputState inputState = new InputState()
                {
                    Tick = tick,
                    movementInput = movementInput,
                };

                inputStates[bufferIndex] = inputState;
                transformStates[bufferIndex] = state;

                tickDeltaTime -= tickRate;
                tick++;
            }
        }

        [ServerRpc]
        void UpdateServerVariableServerRpc(TransformState state)
        {
            serverTransformState.Value = state;
        }

        public void ProcessSimulatedPlayerMovement()
        {
            Vector3 serverPosition = serverTransformState.Value.Position;
            Vector3 currentPosition = transform.position;

            float distance = Vector3.Distance(currentPosition, serverPosition);
            float interpolationFactor = (tick - serverTransformState.Value.Tick) / tickRate;

            tickDeltaTime += Time.deltaTime;
            if (tickDeltaTime > tickRate)
            {
                if (serverTransformState.Value.HasStartedMoving)
                {
                    transform.position = serverTransformState.Value.Position;
                }

                tickDeltaTime -= tickRate;
                tick++;
            }

            if (distance > 1)
            {
                transform.position = Vector3.Lerp(currentPosition, serverPosition, interpolationFactor);
            }

            MovePlayerClientRpc();
        }

        [ServerRpc]
        void MovePlayerServerRpc(int tick, Vector2 movementInput)
        {
            MovePlayer(movementInput);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                HasStartedMoving = true
            };

            previousTransformState = serverTransformState.Value;
            serverTransformState.Value = state;
        }

        void MovePlayer(Vector2 movementInput)
        {
            rb.velocity = movementInput;
        }

        [ClientRpc]
        void MovePlayerClientRpc()
        {
            if (IsLocalPlayer) return;

            transform.position = serverTransformState.Value.Position;
        }
    }

    public class InputState
    {
        public int Tick;
        public Vector2 movementInput;
    }

    public class TransformState : INetworkSerializable
    {
        public int Tick;
        public Vector3 Position;
        public bool HasStartedMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Position);
                reader.ReadValueSafe(out HasStartedMoving);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Position);
                writer.WriteValueSafe(HasStartedMoving);
            }
        }
    }
}

