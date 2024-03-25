using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Climbing : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float speedClimb;
    [SerializeField] float speedMove;
    [SerializeField] float radius;
    public float force;

    internal bool isHanged;
    internal bool isClimbAllow;


    private void Start()
    {
        isClimbAllow = true;
        isHanged = false;
    }

    private void Update()
    {
        if (!isHanged) Check();
        else ClimbMovement();
    }


    private IEnumerator IEHang(Vector3 climbPoint)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = climbPoint;
        float journeyLength = Vector3.Distance(startPosition, endPosition);

        player.animator.Play("IdleToHang");

        float startTime = Time.time;
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float fracJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, endPosition, fracJourney);
            distanceCovered = (Time.time - startTime) * speedClimb;
            yield return null;
        }

        player.animator.Play("HangIdle");
    }


    private void AllowClimb()
    {
        isClimbAllow = true;
    }

    private void Fall(bool isJump)
    {
        isHanged = false;
        player.BlockMovement(false);
        player.animator.Play("Movement");

        if (isJump) player.rigidBody.AddForce(Vector3.up * force, ForceMode.Impulse);

        CancelInvoke();
        Invoke(nameof(AllowClimb), 0.5f);
    }

    private void Check()
    {
        if (isClimbAllow && player.playerMovement.isJumping)
        {
            if (Physics.SphereCast(transform.position + new Vector3(0, 1.5f, 0), radius, transform.forward, out RaycastHit hit, 3, layerMask))
            {
                player.BlockMovement(true);
                Climb(hit.transform);
            }
        }
    }

    private void Climb(Transform ledgePoint)
    {
        Vector3 hangPos = new(transform.position.x, ledgePoint.position.y - 1.5f, ledgePoint.position.z - ledgePoint.localScale.z / 2);
        StartCoroutine(IEHang(hangPos));
        float angle = Mathf.Atan2(ledgePoint.position.x, ledgePoint.position.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);

        isHanged = true;
        isClimbAllow = false;
    }

    private void ClimbMovement()
    {
        if (player.playerMovement.moveInput.x != 0)
        {
            player.rigidBody.MovePosition(transform.position + new Vector3(player.playerMovement.moveInput.normalized.x * speedMove, 0, 0));
        }
        else if (player.playerMovement.moveInput.y < 0)
        {
            Fall(false);
        }
        else if (player.playerMovement.moveInput.y > 0 || Input.GetButtonDown("Jump"))
        {
            Fall(true);
        }
    }

    private void CheckPointOld()
    {
        // if (Physics.SphereCast(transform.position + new Vector3(0, 1.5f, 0), radius, transform.forward, out hit, 1, layerMask))
        // {
        //     // Visualize the SphereCast
        //     Debug.DrawLine(transform.position + new Vector3(0, 1.5f, 0), hit.point, Color.green); // Draw a line from player to hit point
        //     DrawCircle(transform.position + new Vector3(0, 1.5f, 0) + transform.forward * hit.distance, radius, Color.green); // Draw a circle at hit point

        //     return true;
        // }
        // else
        // {
        //     // Visualize the SphereCast
        //     Debug.DrawRay(transform.position + new Vector3(0, 1.5f, 0), transform.forward * 1, Color.red); // Draw a ray indicating max distance
        //     DrawCircle(transform.position + new Vector3(0, 1.5f, 0) + transform.forward * 1, radius, Color.red); // Draw a circle at max distance

        //     return false;
        // }
    }


    void DrawCircle(Vector3 center, float radius, Color color)
    {
        int segments = 36;
        float angleIncrement = 360f / segments;

        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleIncrement;
            Vector3 nextPoint = center + Quaternion.Euler(0, angle, 0) * new Vector3(radius, 0, 0);
            Debug.DrawLine(prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }
    }
}