using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�Q�l:https://nekojara.city/unity-input-system-character-controller
public class PlayerController : MonoBehaviour
{
    [Header("�p�����[�^")]
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float gravity;
    [SerializeField] float fallSpeed; //�������x�̐���
    [SerializeField] float initFallSpeed; //�����̏���

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
