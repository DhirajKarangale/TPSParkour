using UnityEngine;
using UnityEngine.Video;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] float groundCheckDistance;
    [SerializeField] float forwardSpeed;
    [SerializeField] float otherSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float sensitivity;
    [SerializeField] AnimationCurve slopeCurveModifier;

    private RaycastHit hit;
    private Animator animator;
    private Vector3 groundDir;
    internal Vector2 moveInput;

    private float currSpeed;
    internal bool isJumping, isGrounded, isJump, isPreviouslyGrounded;
    internal bool isBlock;


    private void Start()
    {
        currSpeed = forwardSpeed;
        animator = player.animator;
    }

    private void Update()
    {
        GetInput();

        if (isBlock) return;

        Anim();
        UpdatePos();
        LookRotation();
        GroundCheck();
        AddGroundForce();
    }


    #region Move

    private void UpdatePos()
    {
        if (Mathf.Abs(moveInput.magnitude) > 0.1f && isGrounded)
        {
            UpdateSpeed();
            Rotate();
            Move();
        }
    }
   
    private void Move()
    {
        float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + player.cam.transform.eulerAngles.y;
        Vector3 desiredMove = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

        if (player.rigidBody.velocity.sqrMagnitude < (currSpeed * currSpeed))
        {
            player.rigidBody.AddForce(desiredMove.normalized * currSpeed * Time.deltaTime * slopeCurveModifier.Evaluate(Vector3.Angle(groundDir, Vector3.up)), ForceMode.Impulse);
        }
    }

    private void Rotate()
    {
        float angle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + player.cam.transform.eulerAngles.y;
        Quaternion targetAngle = Quaternion.Euler(0, angle, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetAngle, 5 * Time.deltaTime);
    }

    private void UpdateSpeed()
    {
        currSpeed = Input.GetKey(KeyCode.LeftShift) ? forwardSpeed : otherSpeed;
    }

    private void Anim()
    {
        player.animator.SetBool("isJump", !isGrounded && isJumping);
        if (!isJumping) player.animator.SetFloat("speed", moveInput.normalized.magnitude * currSpeed / forwardSpeed);
    }


    #endregion


    #region Look

    private void LookRotation()
    {
        if (moveInput.y > 0)
        {
            Vector3 camDir = player.cam.transform.forward;
            camDir.y = 0f;

            Quaternion rot = Quaternion.LookRotation(camDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, sensitivity * Time.deltaTime);

            float oldYRotation = transform.eulerAngles.y;
            if (isGrounded)
            {
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                player.rigidBody.velocity = velRotation * player.rigidBody.velocity;
            }
        }
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -45, 15);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    #endregion


    #region Ground

    private void AddGroundForce()
    {
        if (isGrounded)
        {
            player.rigidBody.drag = 5f;

            if (isJump)
            {
                float force = jumpForce * Time.deltaTime;
                player.rigidBody.velocity = new Vector3(player.rigidBody.velocity.x, 0f, player.rigidBody.velocity.z);
                player.rigidBody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
                isJumping = true;
            }

            if (!isJumping && Mathf.Abs(moveInput.x) < float.Epsilon && Mathf.Abs(moveInput.y) < float.Epsilon && player.rigidBody.velocity.magnitude < 1f)
            {
                player.rigidBody.Sleep();
            }
        }
        else
        {
            player.rigidBody.drag = 0f;
        }

        isJump = false;
    }

    private void GroundCheck()
    {
        isPreviouslyGrounded = isGrounded;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
        {
            isGrounded = true;
            groundDir = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundDir = Vector3.up;
        }

        if (!isPreviouslyGrounded && isGrounded && isJumping) isJumping = false;
    }

    #endregion


    #region Input

    private void GetInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && !isJump) isJump = true;
    }

    #endregion
}