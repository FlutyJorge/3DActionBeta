using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//参考:https://nekojara.city/unity-input-system-character-controller
public class PlayerController : MonoBehaviour
{
    [Header("パラメータ")]
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float gravity;
    [SerializeField] float fallSpeed; //落下速度の制限
    [SerializeField] float initFallSpeed; //落下の初速

    private Transform transform;
    private CharacterController charaCon;
    private Vector2 input;
    private float verticalVelocity;
    private float turnVelocity;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
