using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : NetworkBehaviour
{
    public PlayerInput playerInput;
    public Rigidbody2D rb;

    public float walkSpeed;
    public float runSpeed;
    float moveSpeed;

    public Camera renderCam;

    private void Start()
    {
        moveSpeed = walkSpeed;
        InitPlayerActions();

        if (IsLocalPlayer)
        {
            renderCam.enabled = true;
        }
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
        Vector2 mousePos = playerInput.actions["Mouse"].ReadValue<Vector2>();

        if (IsServer && IsLocalPlayer)
        {
            Move(moveDir);
            Mouse(mousePos);
            MouseClientRpc(mousePos);
        }
        else if (IsClient && IsLocalPlayer)
        {
            Move(moveDir);
            MoveServerRpc(moveDir, mousePos);
        }
    }

    void Move(Vector2 moveDir)
    {
        Vector2 velocity = moveDir * moveSpeed;

        rb.velocity = velocity;
    }

    [ClientRpc]
    void MouseClientRpc(Vector2 mousePos)
    {
        Mouse(mousePos);
    }

    void Mouse(Vector2 mousePos)
    {
        var pos = renderCam.ScreenToViewportPoint(mousePos);

        var angle = Mathf.Atan2(pos.y - 0.5f, pos.x - 0.5f) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    [ServerRpc]
    void MoveServerRpc(Vector2 moveDir, Vector2 mousePos)
    {
        Move(moveDir);

        Mouse(mousePos);
        MouseClientRpc(mousePos);
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
