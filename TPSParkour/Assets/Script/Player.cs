using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] internal Camera cam;
    [SerializeField] internal Animator animator;
    [SerializeField] internal Rigidbody rigidBody;
    [SerializeField] internal CapsuleCollider capsuleCollider;
    [SerializeField] internal PlayerMovement playerMovement;

    internal bool isAction;
    internal bool isHanging;


    internal void BlockMovement(bool isActive)
    {
        rigidBody.isKinematic = isActive;
        playerMovement.isBlock = isActive;
    }


    public IEnumerator PerformAction(DataTarget data, string anim, Quaternion rotation, float delay, bool isLookAt)
    {
        isAction = true;
        BlockMovement(true);
        animator.CrossFade(anim, 0.2f);

        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(anim)) Debug.Log(anim + " anim not found");

        float timer = 0;
        while (timer < animState.length)
        {
            timer += Time.deltaTime;

            if (isLookAt)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, playerMovement.sensitivity * Time.deltaTime);
            }

            if (data != null) CompareTarget(data);
            if (animator.IsInTransition(0) && timer > 0.5f) break;

            yield return null;
        }

        yield return new WaitForSeconds(delay);
        BlockMovement(false);
        isAction = false;
    }

    private void CompareTarget(DataTarget data)
    {
        animator.MatchTarget(data.position, transform.rotation, data.bodyParts, new MatchTargetWeightMask(data.positionWt, 0), data.stTime, data.enTime);
    }
}


public class DataTarget
{
    public Vector3 position;
    public AvatarTarget bodyParts;
    public Vector3 positionWt;
    public float stTime;
    public float enTime;
}