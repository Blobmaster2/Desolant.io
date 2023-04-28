using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Movement
{
    public class NetworkMovement : NetworkBehaviour
    {
        int tick = 0;
        float tickRate = 1f / 60f;
        float tickDeltaTime = 0f;

        const int BUFFER_SIZE = 1024;

        InputState[] inputStates = new InputState[BUFFER_SIZE];
        TransformState[] transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> serverTransformState = new NetworkVariable<TransformState>();
        public TransformState previousTransformState;

        Vector3 targetPosition;
        Vector3 prevPos;

        void OnEnable()
        {
            serverTransformState.OnValueChanged += OnServerStateChanged;
        }

        void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            previousTransformState = previousvalue;

        }

        private void Update()
        {
            if (transform.position != prevPos && IsLocalPlayer && IsClient)
            {
                MovePlayerServerSideServerRpc(serverTransformState.Value.Position);
                prevPos = transform.position;
            }
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
            tickDeltaTime += Time.deltaTime;
            if (tickDeltaTime > tickRate)
            {
                // If the server hasn't started moving yet, update the previous state
                if (!serverTransformState.Value.HasStartedMoving)
                {
                    previousTransformState = serverTransformState.Value;
                }
                // If the server has started moving, interpolate between previous and current states
                else
                {
                    float interpolationFactor = tickDeltaTime / tickRate;
                    Vector3 interpolatedState = Vector3.Lerp(previousTransformState.Position, serverTransformState.Value.Position, interpolationFactor);
                    transform.position = interpolatedState;
                }

                tickDeltaTime -= tickRate;
                tick++;
            }
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

            // Call a server method to validate the movement and update the server state
            ValidateMovementServerRpc(state);

            // Call a client method to update the client state
            MovePlayerServerSideServerRpc(serverTransformState.Value.Position);
        }

        [ServerRpc]
        void ValidateMovementServerRpc(TransformState state)
        {
            // Validate the movement and update the server state
            if (state.Position != serverTransformState.Value.Position)
            {
                serverTransformState.Value = state;
            }
        }

        [ServerRpc]
        void MovePlayerServerSideServerRpc(Vector3 serverPosition)
        {
            MovePlayerClientRpc(serverPosition);
        }

        void MovePlayer(Vector2 movementInput)
        {
            transform.position += new Vector3(movementInput.x, movementInput.y, 0) * Time.fixedDeltaTime;
        }

        [ClientRpc]
        void MovePlayerClientRpc(Vector3 serverPosition)
        {
            targetPosition = serverPosition;

            if (IsLocalPlayer)
            {
                if (serverTransformState.Value.HasStartedMoving) return;

                if (Vector3.Distance(serverPosition, transform.position) < 1f) return;
            }


            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
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

