using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] internal Camera cam;
    [SerializeField] internal Animator animator;
    [SerializeField] internal Rigidbody rigidBody;
    [SerializeField] internal CapsuleCollider capsuleCollider;
    [SerializeField] internal PlayerMovement playerMovement;
}