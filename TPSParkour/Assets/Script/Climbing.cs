using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Climbing : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] float duration;

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
                if (Check(out RaycastHit hit))
                {
                    Debug.Log("Climb Point found");
                    player.BlockMovement(true);
                    // StartCoroutine(ClimbToLedge("IdleToHang", hit.transform, 0.40f, 53f));
                    ClimbToLedge(hit.transform);
                }
            }
        }
        else
        {
            // Debug.Log("Ledge to ledge");
        }
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
        // DataTarget dataTarget = new DataTarget()
        // {
        //     position = ledgePoint.position,
        //     bodyParts = AvatarTarget.RightHand,
        //     positionWt = Vector3.one,
        //     stTime = stTime,
        //     enTime = enTime
        // };

        // Quaternion rotation = Quaternion.LookRotation(-ledgePoint.forward);

        // yield return player.PerformAction(dataTarget, anim, rotation, 0, true);

        // float angle = Mathf.Atan2(ledgePoint.position.x, ledgePoint.position.z) * Mathf.Rad2Deg + player.cam.transform.eulerAngles.y;
        // transform.position = ledgePoint.position - new Vector3(0, 1.5f, ledgePoint.localScale.z / 2);

        float angle = Mathf.Atan2(ledgePoint.position.x, ledgePoint.position.z) * Mathf.Rad2Deg;
        StartCoroutine(IEMove(ledgePoint.position - new Vector3(0, 1.5f, ledgePoint.localScale.z / 2)));
        transform.rotation = Quaternion.Euler(0, angle, 0);
        player.animator.Play("IdleToHang");

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


    private bool Check(out RaycastHit hit)
    {
        // hit = new RaycastHit();

        // Vector3 climbOrigin = transform.position + Vector3.up * 2f;
        // Vector3 offset = new Vector3(0, 0f, 0);
        // // Vector3 offset = new Vector3(0, 0.19f, 0);

        // for (int i = 0; i < raysCnt; i++)
        // {
        //     Debug.DrawRay(climbOrigin + offset * i, transform.forward, Color.red);
        //     if (Physics.Raycast(climbOrigin + offset * i, transform.forward, out RaycastHit raycast, rayLen, layerMask))
        //     {
        //         hit = raycast;
        //         return true;
        //     }
        // }

        // return false;

        hit = new RaycastHit();
        return Physics.SphereCast(transform.position, rayLen, Vector3.up, out hit, layerMask);
    }
}