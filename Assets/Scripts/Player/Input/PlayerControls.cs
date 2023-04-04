using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : NetworkBehaviour
{
    PlayerInput playerInput;
    Rigidbody2D rb;

    public bool isBuilding;

    public float walkSpeed;
    public float runSpeed;
    float moveSpeed;

    Vector2 scrollValue;

    void Awake()
    {
        moveSpeed = walkSpeed;
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
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

    private void Update()
    {
        if (IsOwner) Move();
    }

    void Move()
    {
        Vector2 moveDir = playerInput.actions["Move"].ReadValue<Vector2>();

        rb.velocity = moveDir * moveSpeed;
    }

    void Hit()
    {
        if (isBuilding)
        {
            Build();
            return;
        }


    }

    void Build()
    {

    }

    void Rotate()
    {
        if (!isBuilding) return;


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
        scrollValue = ctx.ReadValue<Vector2>();


    }
}
