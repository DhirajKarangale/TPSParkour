using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Climbing : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float force;
    [SerializeField] float radius;
    [SerializeField] float speedMove;
    [SerializeField] float speedClimb;

    private bool isHanged;
    private bool isClimbAllow, isChangeAllow;
    private Transform climbPoint;

    private void Start()
    {
        isChangeAllow = true;
        isClimbAllow = true;
        isHanged = false;
    }

    private void Update()
    {
        if (!isHanged) Check();
        else ClimbMovement();

        Anim();
    }


    private IEnumerator IEHang(Vector3 climbPoint)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = climbPoint;
        float journeyLength = Vector3.Distance(startPosition, endPosition);

        // player.animator.Play("IdleToHang");

        float startTime = Time.time;
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float fracJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, endPosition, fracJourney);
            distanceCovered = (Time.time - startTime) * speedClimb;
            yield return null;
        }

        // player.animator.Play("HangIdle");
    }


    private void AllowClimb()
    {
        isClimbAllow = true;
    }

    private void AllowChange()
    {
        isChangeAllow = true;
    }

    private void Anim()
    {
        if (!isHanged) return;

        player.animator.SetFloat("climbVertical", player.playerMovement.moveInput.y);
        player.animator.SetFloat("climbHorrizontal", player.playerMovement.moveInput.x);
    }

    private void PositionPlayer(Transform ledge)
    {
        float xPos = transform.position.x;
        float zPos = ledge.position.z - ledge.localScale.z / 2;

        if (ledge.rotation.y != 0)
        {
            xPos = ledge.position.x + ledge.localScale.z / 2;
            zPos = transform.position.z + ledge.localScale.x * 0.2f;
        }
        Vector3 hangPos = new(xPos, ledge.position.y - 1.5f, zPos);
        StartCoroutine(IEHang(hangPos));
    }

    private void RotatePlayer(Transform ledge)
    {
        // Vector3 directionToLedge = (ledge.position - transform.position).normalized;
        // float angle = Mathf.Atan2(directionToLedge.x, directionToLedge.z) * Mathf.Rad2Deg;
        // Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

        Quaternion targetRotation = ledge.rotation;
        if (ledge.rotation.y != 0) targetRotation = Quaternion.Euler(0, -90, 0);

        transform.rotation = targetRotation;
    }

    private void Fall(bool isJump)
    {
        isHanged = false;
        player.BlockMovement(false);
        player.animator.Play("Movement");
        climbPoint = null;

        if (isJump) player.rigidBody.AddForce(Vector3.up * force, ForceMode.Impulse);

        CancelInvoke();
        Invoke(nameof(AllowClimb), 0.5f);
    }

    private void Check()
    {
        if (isClimbAllow && player.playerMovement.isJumping)
        {
            if (Physics.SphereCast(transform.position + new Vector3(0, 1.5f, 0), radius, transform.forward, out RaycastHit hit, 2f, layerMask)) Climb(hit.transform);
            // if (Physics.Raycast(transform.position + new Vector3(0, 1.5f, 0), transform.forward, out RaycastHit hit, 2f, layerMask)) Climb(hit.transform);
        }
    }

    private void Climb(Transform ledge)
    {
        climbPoint = ledge;
        player.BlockMovement(true);
        PositionPlayer(ledge);
        RotatePlayer(ledge);
        isHanged = true;
        isClimbAllow = false;
        isChangeAllow = false;
        player.animator.Play("Climb");
        Invoke(nameof(AllowChange), 1f);
    }

    private void ClimbMovement()
    {
        if (player.playerMovement.moveInput.x != 0)
        {
            Vector3 origin = transform.position;
            Vector3 movePos = transform.position;

            if (climbPoint.rotation.y != 0)
            {
                movePos += new Vector3(0, 0, player.playerMovement.moveInput.normalized.x * speedMove);
                origin += new Vector3(0, 1.5f, player.playerMovement.moveInput.normalized.x * 0.5f);
            }
            else
            {
                movePos += new Vector3(player.playerMovement.moveInput.normalized.x * speedMove, 0, 0);
                origin += new Vector3(player.playerMovement.moveInput.normalized.x * 0.5f, 1.5f, 0);
            }

            Collider[] hitColliders1 = Physics.OverlapSphere(origin, 0.2f, layerMask);
            foreach (var item in hitColliders1)
            {
                if (item.transform == climbPoint)
                {
                    player.rigidBody.MovePosition(movePos);
                    break;
                }
            }

            if (!Physics.Raycast(origin, climbPoint.forward, 2f, layerMask))
            {
                if (isChangeAllow)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(origin, 0.2f, layerMask);

                    foreach (var otherPoint in hitColliders)
                    {
                        if (otherPoint.transform != climbPoint)
                        {
                            // isChangeAllow = false;
                            // climbPoint = otherPoint.transform;
                            // PositionPlayer(climbPoint);
                            // RotatePlayer(climbPoint);
                            // isClimbAllow = false;
                            // Invoke(nameof(AllowChange), 4f);

                            Climb(otherPoint.transform);

                            return;
                        }
                    }
                }
            }
        }
        else if (player.playerMovement.moveInput.y < 0) Fall(false);
        else if (player.playerMovement.moveInput.y > 0 || Input.GetButtonDown("Jump")) Fall(true);
    }
}