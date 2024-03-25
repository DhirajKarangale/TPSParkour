using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Climbing : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] float duration;
    bool isTransitioning;

    [Header("Climbing")]
    [SerializeField] bool isTargetMatching;
    [SerializeField] Vector3 position;
    [SerializeField] AvatarTarget bodyParts;
    [SerializeField] Vector3 positionWt;
    [SerializeField] float stTime;
    [SerializeField] float enTime;

    [SerializeField] string anim;
    [SerializeField] Quaternion rotation;
    [SerializeField] float delay = 0;
    [SerializeField] bool isLookAt = false;

    [Header("Checking")]
    [SerializeField] float rayLen = 1.6f;
    [SerializeField] LayerMask layerMask;
    [SerializeField] int raysCnt = 12;


    private void Update()
    {
        if (!player.isHanging)
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (Check(out RaycastHit hit))
                {
                    Debug.Log("Climb Point found");
                    player.BlockMovement(true);
                    ClimbToLedge(hit.transform);
                }
            }
        }
        else
        {
            // Debug.Log("Ledge to ledge");
        }
    }

    IEnumerator TransitionToHang(Vector3 climbPoint)
    {
        isTransitioning = true;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = climbPoint;
        float journeyLength = Vector3.Distance(startPosition, endPosition);

        // Play the Jump to Climb animation
        player.animator.Play("IdleToHang");

        float startTime = Time.time;
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float fracJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, endPosition, fracJourney);
            distanceCovered = (Time.time - startTime) * duration;
            yield return null;
        }

        // Play the ClimbIdle animation
        player.animator.Play("HangIdle");
        player.isHanging = true;
        isTransitioning = false;
    }

    private IEnumerator IEMove(Vector3 pos)
    {
        float time = 0;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = pos;
    }


    private void ClimbToLedge(Transform ledgePoint)
    {
        // Vector3 hangPos = ledgePoint.position - new Vector3(0, 1.5f, ledgePoint.localScale.z / 2);
        Vector3 hangPos = new(transform.position.x, ledgePoint.position.y - 1.5f, transform.position.z - ledgePoint.localScale.z / 2);
        StartCoroutine(TransitionToHang(hangPos));
        float angle = Mathf.Atan2(ledgePoint.position.x, ledgePoint.position.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
        // player.animator.Play("IdleToHang");

        player.isHanging = true;
    }

    private bool Check(out RaycastHit hit)
    {
        hit = new RaycastHit();
        return Physics.SphereCast(transform.position, rayLen, Vector3.up, out hit, layerMask);
    }
}