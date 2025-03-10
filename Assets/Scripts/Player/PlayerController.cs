using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float defaultSpeed;
    public float speedBoostMultiplier = 2f;
    private bool isSpeedBoosted = false;
    private Vector2 curMovementInput;

    private int defaultMaxJumps;
    public int maxJumps;
    private bool isJumpBoosted = false;
    public float jumpPower;
    public LayerMask groundLayerMask;


    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;

    private Vector2 mouseDelta;

    [HideInInspector]
    public bool canLook = true;

    public Action inventory;

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        defaultSpeed = moveSpeed;
        defaultMaxJumps = maxJumps;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            GetComponent<Rigidbody>().AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
        }
    }

    private void Move()
    {
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= moveSpeed;
        dir.y = GetComponent<Rigidbody>().velocity.y;

        GetComponent<Rigidbody>().velocity = dir;
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) +(transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.1f, groundLayerMask))
            {
                return true;
            }
        }

        return false;
    }

    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }

    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }
    public void BoostSpeed(float duration, float multiplier)
    {
        if (isSpeedBoosted) return;

        isSpeedBoosted = true;
        moveSpeed *= multiplier; 
        StartCoroutine(ResetSpeed(duration));
    }

    public void BoostJump(float duration, int extraJumps)
    {
        if (isJumpBoosted) return;

        isJumpBoosted = true;
        maxJumps += extraJumps;
        StartCoroutine(ResetJump(duration, extraJumps));
    }

    IEnumerator ResetSpeed(float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed = defaultSpeed; // 원래 속도로 복구
        isSpeedBoosted = false;
    }

    IEnumerator ResetJump(float duration, int extraJumps)
    {
        yield return new WaitForSeconds(duration);
        maxJumps -= extraJumps; // 추가된 점프 횟수 제거
        isJumpBoosted = false;
    }

}