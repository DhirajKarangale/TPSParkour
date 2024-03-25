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
    private bool isClimbAllow;
    private BoxCollider colliderClimb;

    private void Start()
    {
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


    private void Anim()
    {
        if (!isHanged) return;

        player.animator.SetFloat("climbVertical", player.playerMovement.moveInput.y);
        player.animator.SetFloat("climbHorrizontal", player.playerMovement.moveInput.x);
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
        colliderClimb = null;

        if (isJump) player.rigidBody.AddForce(Vector3.up * force, ForceMode.Impulse);

        CancelInvoke();
        Invoke(nameof(AllowClimb), 0.5f);
    }

    private void Check()
    {
        if (isClimbAllow && player.playerMovement.isJumping)
        {
            if (Physics.SphereCast(transform.position + new Vector3(0, 1.5f, 0), radius, transform.forward, out RaycastHit hit, 5, layerMask)) Climb(hit.transform);
        }
    }

    private void Climb(Transform ledgePoint)
    {
        colliderClimb = ledgePoint.GetComponent<BoxCollider>();

        player.BlockMovement(true);

        Vector3 hangPos = new(transform.position.x, ledgePoint.position.y - 1.5f, ledgePoint.position.z - ledgePoint.localScale.z / 2);
        StartCoroutine(IEHang(hangPos));

        float angle = Mathf.Atan2(ledgePoint.position.x, ledgePoint.position.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);

        isHanged = true;
        isClimbAllow = false;

        player.animator.Play("Climb");
    }

    private void ClimbMovement()
    {
        if (player.playerMovement.moveInput.x != 0)
        {
            Vector3 origin = transform.position + new Vector3(player.playerMovement.moveInput.normalized.x * 0.5f, 1.4f, 0);
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, 3, layerMask))
            {
                Vector3 movePos = transform.position + new Vector3(player.playerMovement.moveInput.normalized.x * speedMove, 0, 0);
                player.rigidBody.MovePosition(movePos);
            }
        }
        else if (player.playerMovement.moveInput.y < 0) Fall(false);
        else if (player.playerMovement.moveInput.y > 0 || Input.GetButtonDown("Jump")) Fall(true);
    }
}