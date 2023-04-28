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
    Rigidbody2D rb;
    public Collider2D hitCollider;
    public NetworkMovement networkMovement;

    public float hitCooldown;

    public float walkSpeed;
    public float runSpeed;
    float moveSpeed;

    public AnimationCurve cameraMovement;

    bool isHitting;

    public Camera renderCam;

    Vector2 mouseInput;
    Vector3 cameraLerpPos;
    Vector3 previousCameraPos;

    Vector2 moveDir;
    Vector2 mousePos;

    float timeSinceStartCameraMove;
    public float timeToLerpCamera;

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
        playerInput.actions["HitPlace"].performed += ctx => Hit();
        playerInput.actions["Rotate"].performed += ctx => Rotate();
        playerInput.actions["Craft"].performed += ctx => OpenCraft();
        playerInput.actions["Inventory"].performed += ctx => OpenInventory();
        playerInput.actions["Map"].performed += ctx => OpenMap();
        playerInput.actions["Interact"].performed += ctx => Interact();
        playerInput.actions["Reload"].performed += ctx => Reload();
        playerInput.actions["ChangeAnchor"].performed += ChangeAnchor;
        playerInput.actions["Sprint"].started += ctx => moveSpeed = runSpeed;
        playerInput.actions["Sprint"].canceled += ctx => moveSpeed = walkSpeed;
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
        playerInput.actions["Sprint"].started -= ctx => moveSpeed = runSpeed;
        playerInput.actions["Sprint"].canceled -= ctx => moveSpeed = walkSpeed;
    }

    private void Update()
    {
        if (IsClient && IsLocalPlayer)
        {
            CameraMovement(mousePos);
        }
    }

    private void FixedUpdate()
    {
        moveDir = playerInput.actions["Move"].ReadValue<Vector2>();
        mousePos = playerInput.actions["Mouse"].ReadValue<Vector2>();

        moveDir *= moveSpeed;

        if (IsServer && IsLocalPlayer)
        {
            networkMovement.ProcessSimulatedPlayerMovement();
        }
        else if (IsClient && IsLocalPlayer)
        {
            networkMovement.ProcessLocalPlayerMovement(moveDir);
        }
        

        if (IsServer && IsLocalPlayer)
        {
            Mouse(mousePos);
        }
        else if (IsClient && IsLocalPlayer)
        {
            CameraMovement(mousePos);

            if (mouseInput != mousePos)
            {
                MouseServerRpc(mousePos);
                Mouse(mousePos);

                mouseInput = mousePos;
            }
        }
    }

    void Mouse(Vector2 mousePos)
    {
        var pos = renderCam.ScreenToViewportPoint(mousePos);

        var angle = Mathf.Atan2(pos.y - 0.5f, pos.x - 0.5f) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    [ServerRpc]
    void MouseServerRpc(Vector2 mousePos)
    {
        Mouse(mousePos);
        MouseClientRpc(mousePos);
    }

    [ClientRpc]
    void MouseClientRpc(Vector2 mousePos)
    {
        if (IsLocalPlayer) return;
        Mouse(mousePos);
    }

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
