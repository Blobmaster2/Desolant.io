using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : NetworkBehaviour
{
    PlayerInput playerInput;
    public Rigidbody2D rb;

    public float walkSpeed;
    public float runSpeed;
    float moveSpeed;

    public List<PlayerState> playerStates = new List<PlayerState>();

    public class PlayerState
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 input;
        public float timestamp;
    }

    private void Start()
    {
        moveSpeed = walkSpeed;
        playerInput = GetComponent<PlayerInput>();
        InitPlayerActions();
    }

    void InitPlayerActions()
    {
        playerInput.actions["HitPlace"].started += ctx => Hit();
        playerInput.actions["Rotate"].started += ctx => Rotate();
        playerInput.actions["Craft"].started += ctx => OpenCraft();
        playerInput.actions["Inventory"].started += ctx => OpenInventory();
        playerInput.actions["Map"].started += ctx => OpenMap();
        playerInput.actions["Interact"].started += ctx => Interact();
        playerInput.actions["Reload"].started += ctx => Reload();
        playerInput.actions["ChangeAnchor"].started += ChangeAnchor;
    }

    private void FixedUpdate()
    {
        Vector2 moveDir = playerInput.actions["Move"].ReadValue<Vector2>();
        float timestamp = Time.time;

        if (IsServer && IsLocalPlayer)
        {
            Move(moveDir);
        }
        else if (IsClient && IsLocalPlayer)
        {
            // Predict client movement
            Vector2 predictedPosition = rb.position + rb.velocity * Time.fixedDeltaTime;
            Vector2 predictedVelocity = (predictedPosition - rb.position) / Time.fixedDeltaTime;

            // Add predicted state to list
            playerStates.Add(new PlayerState
            {
                position = predictedPosition,
                velocity = predictedVelocity,
                input = moveDir,
                timestamp = timestamp
            });

            // Remove old player states
            while (playerStates.Count > 10)
            {
                playerStates.RemoveAt(0);
            }

            // Send input to server
            MoveServerRpc(moveDir, timestamp);
        }
    }


    void Move(Vector2 moveDir)
    {
        Vector2 velocity = moveDir * moveSpeed;
        Vector2 position = rb.position + velocity * Time.fixedDeltaTime;

        rb.MovePosition(position);
        rb.velocity = velocity;

        // Add new player state to list
        playerStates.Add(new PlayerState
        {
            position = position,
            velocity = velocity,
            input = moveDir,
            timestamp = Time.time
        });

        // Remove old player states
        while (playerStates.Count > 10)
        {
            playerStates.RemoveAt(0);
        }
    }


    [ServerRpc]
    void MoveServerRpc(Vector2 moveDir, float timestamp)
    {
        PlayerState state = playerStates.FindLast(s => s.timestamp <= timestamp);

        if (state != null)
        {
            rb.MovePosition(state.position);
            rb.velocity = state.velocity;
        }

        Move(moveDir);
    }

    void Hit()
    {

    }

    void Build()
    {

    }

    void Rotate()
    {


    }

    void OpenCraft()
    {

    }

    void OpenInventory()
    {

    }

    void OpenMap()
    {

    }

    void Interact()
    {

    }

    void Reload()
    {

    }

    private void ChangeAnchor(InputAction.CallbackContext ctx)
    {
        
    }
}
