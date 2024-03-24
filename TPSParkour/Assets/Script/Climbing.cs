using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Climbing : MonoBehaviour
{
    [SerializeField] Player player;

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
            if (Input.GetButtonDown("Jump") && !player.isAction)
            {
                if (Check(transform.forward, out RaycastHit hit))
                {
                    Debug.Log("Climb Point found");
                    player.BlockMovement(true);
                    StartCoroutine(ClimbToLedge("IdleToHang", hit.transform, 0.40f, 53f));
                }
            }
        }
        else
        {
            // Debug.Log("Ledge to ledge");
        }
    }


    private IEnumerator ClimbToLedge(string anim, Transform ledgePoint, float stTime, float enTime)
    {
        DataTarget dataTarget = new DataTarget()
        {
            position = ledgePoint.position,
            bodyParts = AvatarTarget.RightHand,
            positionWt = Vector3.one,
            stTime = stTime,
            enTime = enTime
        };

        Quaternion rotation = Quaternion.LookRotation(-ledgePoint.forward);

        yield return player.PerformAction(dataTarget, anim, rotation, 0, true);

        player.isHanging = true;
    }

    private IEnumerator Perform()
    {
        player.BlockMovement(false);

        DataTarget dataTarget = null;

        if (isTargetMatching)
        {
            dataTarget = new DataTarget()
            {
                position = position,
                bodyParts = bodyParts,
                positionWt = position,
                stTime = stTime,
                enTime = enTime
            };
        }

        yield return player.PerformAction(dataTarget, anim, rotation, delay, isLookAt);


        player.BlockMovement(true);
    }


    private bool Check(Vector3 dir, out RaycastHit hit)
    {
        hit = new RaycastHit();

        if (dir == Vector3.zero) return false;

        Vector3 climbOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 offset = new Vector3(0, 0.19f, 0);

        for (int i = 0; i < raysCnt; i++)
        {
            Debug.DrawRay(climbOrigin + offset * i, dir, Color.red);
            if (Physics.Raycast(climbOrigin + offset * i, dir, out RaycastHit raycast, rayLen, layerMask))
            {
                hit = raycast;
                return true;
            }
        }

        return false;
    }
}