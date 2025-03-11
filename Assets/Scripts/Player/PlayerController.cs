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

    public int currentjumps = 0;
    public int maxJumps;
    private bool isJumpBoosted = false;
    public float jumpPower;
    public LayerMask groundLayerMask;

    public float staminaDrain = 10f; 
    public float staminaRecovery = 5f;
    private bool isWallClimbing = false;
    private bool isWallHanging = false;


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
    private Rigidbody _rigidbody;

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        defaultSpeed = moveSpeed;
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
            bool isWall = IsTouchingWall();

            if(isWall)
            {
                if(curMovementInput.y > 0 && CharacterManager.Instance.Player.condition.stamina.curVal > 0)
                {
                    StartWallClimb();
                }
                else if(curMovementInput == Vector2.zero && CharacterManager.Instance.Player.condition.stamina.curVal > 0)
                {
                    StartWallHang();
                }
                else if (curMovementInput.y < 0 || CharacterManager.Instance.Player.condition.stamina.curVal <= 0) 
                {
                    StopWallClimb();
                }
            }
            else
            {
                Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
                dir *= moveSpeed;
                dir.y = _rigidbody.velocity.y;
                _rigidbody.velocity = dir;
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
            if (IsTouchingWall())
            {
                isWallClimbing = false;
                isWallHanging = true;
                _rigidbody.useGravity = false;
                _rigidbody.velocity = Vector3.zero;
            }
            else
            {
                isWallHanging = false;
                _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
            }
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        IsGrounded();
        if (context.phase == InputActionPhase.Started && currentjumps < maxJumps)
        {
            currentjumps++;
            GetComponent<Rigidbody>().AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
        }
        else if (isWallClimbing || isWallHanging)
        {
            StopWallClimb();
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
                currentjumps = 0;
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


    private bool IsTouchingWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            return hit.collider.CompareTag("Wall"); // "Wall" 태그가 붙은 오브젝트만 감지
        }
        return false;
    }


    private void StartWallClimb()
    {
        if (!isWallClimbing)
        {
            isWallClimbing = true;
            isWallHanging = false;
            _rigidbody.useGravity = false;
        }

        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 10f, _rigidbody.velocity.z);
        CharacterManager.Instance.Player.condition.stamina.Subtract(staminaDrain * Time.deltaTime);
    }

    private void StartWallHang()
    {
        if (!isWallHanging)
        {
            isWallHanging = true;
            isWallClimbing = false;
            _rigidbody.useGravity = false;
            _rigidbody.velocity = Vector3.zero;
        }
    }

    private void StopWallClimb()
    {
        isWallClimbing = false;
        isWallHanging = false;
        _rigidbody.useGravity = true;
    }


}