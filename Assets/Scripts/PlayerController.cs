using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//参考:https://mochisakucoco.hatenablog.com/entry/2021/03/17/075601
public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform stepRay;

    [Space(10)]
    [Header("パラメータ")]
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

            //方向キーが推されている時
            if (input.magnitude > 0)
            {
                Vector3 lineStartPos = transform.position + new Vector3(0, stepOffset, 0);
                Vector3 lineEndPos = lineStartPos + transform.forward * slopeDistance;
                Debug.DrawLine(lineStartPos, lineEndPos, Color.green);

                //ステップ用のRayが地面に接触
                Vector3 start = stepRay.position;
                Vector3 end = start + stepRay.forward * stepDistance;
                if (Physics.Linecast(start, end, out RaycastHit stepHit, LayerMask.GetMask("Ground")))
                {
                    Debug.DrawLine(start, end, Color.green);
                    Debug.Log(stepHit.normal);
                    //進行方向の地面の角度が指定以下、または登れる段差より下だった場合の移動処理
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
                //地面に接触していない場合
                else
                {
                    velocity += transform.forward * walkSpeed;
                }
            }

            //ジャンプ処理
            if (Input.GetButtonDown("Jump"))
            {
                isGrounded = false;
                velocity.y += jumpPower;
            }
        }
    }

    private void FixedUpdate()
    {
        //キャラクターの移動をさせる処理
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
