using Networking.Movement;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : NetworkBehaviour
{
    public PlayerInput playerInput;
    public Collider2D hitCollider;

    Rigidbody2D rigidBody;

    [SerializeField] private Transform playerTransform;

    public float hitCooldown;

    private const float walkSpeed = 4;
    private const float runSpeed = 7;
    private float movementPenalty = 1;
    NetworkVariable<float> moveSpeed = new();
    NetworkVariable<Vector2> serverInput = new();
    NetworkVariable<float> serverLookAngle = new();

    public AnimationCurve cameraMovement;

    public Camera renderCam;

    private bool isHitting;

    private bool isSprinting;

    private Vector2 moveDir;
    private Vector2 mouseInput;
    private Vector3 cameraLerpPos;
    private Vector3 previousCameraPos;

    float timeSinceStartCameraMove;
    public float timeToLerpCamera;

    public override void OnNetworkDespawn()
    {

    }

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        if (IsServer)
            moveSpeed.Value = walkSpeed;

        InitPlayerActions();

        if (IsLocalPlayer)
        {
            renderCam.enabled = true;
            SetSpeedServerRPC(walkSpeed);
        }
    }

    void InitPlayerActions()
    {
        playerInput.actions["HitPlace"].performed += ctx => Hit();
        playerInput.actions["Rotate"].performed += ctx => Rotate();
        playerInput.actions["Craft"].performed += ctx => OpenCraft();
        playerInput.actions["Inventory"].performed += ctx => OpenInventory();
        playerInput.actions["Map"].performed += ctx => OpenMap();
        playerInput.actions["Interact"].performed += ctx => Interact();
        playerInput.actions["Reload"].performed += ctx => Reload();
        playerInput.actions["ChangeAnchor"].performed += ChangeAnchor;
        playerInput.actions["Sprint"].started += ctx => SetSpeed(runSpeed, true);
        playerInput.actions["Sprint"].canceled += ctx => SetSpeed(walkSpeed, false);
    }

    private void OnDisable()
    {
        DeInitPlayerActions();
    }

    void DeInitPlayerActions()
    {
        playerInput.actions["HitPlace"].started -= ctx => Hit();
        playerInput.actions["Rotate"].started -= ctx => Rotate();
        playerInput.actions["Craft"].started -= ctx => OpenCraft();
        playerInput.actions["Inventory"].started -= ctx => OpenInventory();
        playerInput.actions["Map"].started -= ctx => OpenMap();
        playerInput.actions["Interact"].started -= ctx => Interact();
        playerInput.actions["Reload"].started -= ctx => Reload();
        playerInput.actions["ChangeAnchor"].started -= ChangeAnchor;
        playerInput.actions["Sprint"].started -= ctx => SetSpeed(runSpeed, true);
        playerInput.actions["Sprint"].canceled -= ctx => SetSpeed(walkSpeed, false);
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            MovePlayer(serverInput.Value);
            Look(serverLookAngle.Value);
            return;
        }

        if (IsClient && IsLocalPlayer)
        {
            moveDir = playerInput.actions["Move"].ReadValue<Vector2>();
            var mousePos = playerInput.actions["Mouse"].ReadValue<Vector2>();

            float lookAngle = GetAngle(mousePos);

            CameraMovement(mousePos);

            if (lookAngle != serverLookAngle.Value)
            {
                UpdateServerLookDirServerRPC(lookAngle);
            }

            if (moveDir != serverInput.Value)
            {
                UpdateServerMoveDirServerRPC(moveDir);
            }

            Look(lookAngle);
        }

        else
        {
            Look(serverLookAngle.Value);
        }
    }

    //Movement

    [ServerRpc]
    private void UpdateServerLookDirServerRPC(float input)
    {
        serverLookAngle.Value = input;
    }

    private void Look(float input)
    {
        playerTransform.rotation = Quaternion.Euler(0, 0, input);
    }

    [ServerRpc]
    private void UpdateServerMoveDirServerRPC(Vector2 moveDir)
    {
        serverInput.Value = moveDir;
    }

    private void MovePlayer(Vector2 input)
    {
        rigidBody.MovePosition(movementPenalty * moveSpeed.Value * Time.fixedDeltaTime * new Vector3(input.x, input.y) + transform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer)
            return;

        if (collision.gameObject.CompareTag("Player"))
            movementPenalty = 0.35f;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsServer)
            return;

        if (collision.gameObject.CompareTag("Player"))
            movementPenalty = 1;
    }

    private void SetSpeed(float speed, bool sprinting)
    {
        isSprinting = sprinting;

        if (!IsLocalPlayer)
            return;

        SetSpeedServerRPC(speed);
    }

    [ServerRpc]
    private void SetSpeedServerRPC(float speed)
    {
        moveSpeed.Value = speed;
    }

    float GetAngle(Vector2 mousePos)
    {
        var pos = renderCam.ScreenToViewportPoint(mousePos);

        var angle = Mathf.Atan2(pos.y - 0.5f, pos.x - 0.5f) * Mathf.Rad2Deg;
        return angle + 90;
    }

    //Vision

    void CameraMovement(Vector2 mousePos)
    {
        if (mouseInput != mousePos)
        {
            previousCameraPos = renderCam.gameObject.transform.localPosition;
            cameraLerpPos = renderCam.ScreenToViewportPoint(mousePos);
            cameraLerpPos = new Vector3(cameraLerpPos.x, cameraLerpPos.y, -10);
            timeSinceStartCameraMove = 0;
        }

        timeSinceStartCameraMove += Time.deltaTime;

        renderCam.gameObject.transform.localPosition = Vector3.Lerp(previousCameraPos, cameraLerpPos, cameraMovement.Evaluate(timeSinceStartCameraMove / timeToLerpCamera));
    }

    //Core Mechanics

    void Hit()
    {
        if (IsClient && IsLocalPlayer)
        {
            if (!isHitting)
            {
                WaitForHitServerRpc();
                StartCoroutine(WaitForHit());
            }
        }
    }

    [ServerRpc]
    void WaitForHitServerRpc()
    {
        StartCoroutine(WaitForHit());
    }

    IEnumerator WaitForHit()
    {
        isHitting = true;

        yield return new WaitForSeconds(hitCooldown / 2);

        hitCollider.enabled = true;

        yield return new WaitForSeconds(0.2f);

        hitCollider.enabled = false;

        yield return new WaitForSeconds(hitCooldown / 2);

        isHitting = false;
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
