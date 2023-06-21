using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//�Q�l:https://mochisakucoco.hatenablog.com/entry/2021/03/17/075601
public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform stepRay;

    [Space(10)]
    [Header("�p�����[�^")]
    [SerializeField] bool isGrounded;
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpPower;
    [SerializeField] float stepDistance;
    [SerializeField] float stepOffset;
    [SerializeField] float slopeLimit;
    [SerializeField] float slopeDistance;
    [SerializeField] float colliderRadius;

    private float horizontal;
    private float vertical;
    private Animator anim;
    private Vector3 velocity;
    private Vector3 input;
    private Rigidbody rb;
    private int layerMask;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        layerMask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (isGrounded)
        {
            velocity = Vector3.zero;
            input = new Vector3(horizontal, 0f, vertical);

            //�����L�[��������Ă��鎞
            if (input.magnitude > 0)
            {
                Vector3 lineStartPos = transform.position + new Vector3(0, stepOffset, 0);
                Vector3 lineEndPos = lineStartPos + transform.forward * slopeDistance;
                Debug.DrawLine(lineStartPos, lineEndPos, Color.green);

                //�X�e�b�v�p��Ray���n�ʂɐڐG
                Vector3 start = stepRay.position;
                Vector3 end = start + stepRay.forward * stepDistance;
                if (Physics.Linecast(start, end, out RaycastHit stepHit, LayerMask.GetMask("Ground")))
                {
                    Debug.DrawLine(start, end, Color.green);
                    Debug.Log(stepHit.normal);
                    //�i�s�����̒n�ʂ̊p�x���w��ȉ��A�܂��͓o���i����艺�������ꍇ�̈ړ�����
                    if (Vector3.Angle(transform.up, stepHit.normal) <= slopeLimit
                        || (Vector3.Angle(transform.up, stepHit.normal) > slopeLimit
                        && !Physics.Linecast(lineStartPos, lineEndPos, LayerMask.GetMask("Ground"))))
                    {
                        velocity = new Vector3(0, (Quaternion.FromToRotation(Vector3.up, stepHit.normal) * transform.forward * walkSpeed).y, 0) + transform.forward * walkSpeed;
                        Debug.Log(Vector3.Angle(transform.up, stepHit.normal));
                    }
                    else
                    {
                        velocity += transform.forward * walkSpeed;
                    }

                    Debug.Log(Vector3.Angle(Vector3.up, stepHit.normal));
                }
                //�n�ʂɐڐG���Ă��Ȃ��ꍇ
                else
                {
                    velocity += transform.forward * walkSpeed;
                }
            }

            //�W�����v����
            if (Input.GetButtonDown("Jump"))
            {
                isGrounded = false;
                velocity.y += jumpPower;
            }
        }
    }

    private void FixedUpdate()
    {
        //�L�����N�^�[�̈ړ��������鏈��
        rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
        Vector3 moveForward = transform.forward * vertical + transform.right * horizontal;
        velocity = moveForward * walkSpeed + new Vector3(0, velocity.y, 0);

        if (moveForward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveForward);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Physics.CheckSphere(transform.position, colliderRadius, LayerMask.GetMask("Ground")))
        {
            isGrounded = true;
            velocity.y = 0;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.GetMask("Ground"))
        {
            isGrounded = false;
        }
    }
}
