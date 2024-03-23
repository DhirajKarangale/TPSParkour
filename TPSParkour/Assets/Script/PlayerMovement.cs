using UnityEngine;
using UnityEngine.Video;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] float groundCheckDistance;
    [SerializeField] internal float forwardSpeed;
    [SerializeField] internal float otherSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] AnimationCurve slopeCurveModifier;

    private RaycastHit hit;
    private Animator animator;
    private Vector3 groundDir;
    private Vector2 moveInput;

    private float currSpeed;
    private bool isJump, isPreviouslyGrounded;
    private bool isJumping, isGrounded;

    public float rotateTime = 0.1f;
    private float rotateVel;


    private void Start()
    {
        currSpeed = forwardSpeed;
        animator = player.animator;
    }

    private void Update()
    {
        Move();
        Anim();
        GetInput();
        GroundCheck();
        AddGroundForce();
    }


    #region Move

    private void Move()
    {
        if (Mathf.Abs(moveInput.magnitude) > 0.1f && isGrounded)
        {
            UpdateSpeed();

            // float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + player.cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotateVel, rotateTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 desiredMove = transform.forward * moveInput.y + player.cam.transform.right * moveInput.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, groundDir).normalized;

            desiredMove.x = desiredMove.x * currSpeed;
            desiredMove.z = desiredMove.z * currSpeed;
            desiredMove.y = desiredMove.y * currSpeed;

            if (moveInput.y < 0) desiredMove.z = -desiredMove.z;

            // Vector3 moveDir = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;

            if (player.rigidBody.velocity.sqrMagnitude < (currSpeed * currSpeed))
            {
                player.rigidBody.AddForce(desiredMove * Time.deltaTime * slopeCurveModifier.Evaluate(Vector3.Angle(groundDir, Vector3.up)), ForceMode.Impulse);
            }
        }
    }

    private void UpdateSpeed()
    {
        // if (moveInput.x != 0 || moveInput.y < 0) currSpeed = otherSpeed;
        // else if (moveInput.y > 0) currSpeed = forwardSpeed;

        currSpeed = Input.GetKey(KeyCode.LeftShift) ? forwardSpeed : otherSpeed;
    }

    private void Anim()
    {
        player.animator.SetBool("isJump", !isGrounded && isJumping);

        if (!isJumping) player.animator.SetFloat("speed", moveInput.normalized.magnitude * currSpeed / forwardSpeed);
        // {
        //     player.animator.SetFloat("horizontal", moveInput.y);
        //     player.animator.SetFloat("vertical", moveInput.x);
        // }
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