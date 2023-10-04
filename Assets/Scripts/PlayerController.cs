using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//参考:https://mochisakucoco.hatenablog.com/entry/2021/03/17/075601
public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform stepRay;
    [SerializeField] GroundChecker groundChecker;

    [Space(10)]
    [Header("パラメータ")]
    //[SerializeField] bool isGrounded;
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpPower;
    [SerializeField] float stepDistance;
    [SerializeField] float stepOffset;
    [SerializeField] float slopeLimit;
    [SerializeField] float slopeDistance;
    [SerializeField] float colliderRadius;
    [SerializeField] float maxSpeed;
    [SerializeField] float stepSmooth;
    [SerializeField] float rotationSpeed;

    private Animator anim;
    private Vector3 velocity;
    private Vector3 input;
    private Quaternion targetRotation;
    private Rigidbody rb;
    private RaycastHit stepHit;
    private Vector3 lineStartPos;
    private Vector3 lineEndPos;
    private bool pushJump;
    private bool isRightAngle;
    private bool isRunning = false;

    private enum State
    {
        Idle, Walk, Run, Jump
    }
    private State state;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        state = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //ダッシュ判定
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            Debug.Log("Run!!");
        }
        else
        {
            isRunning = false;
        }

        if (input.magnitude > 0 && groundChecker.isGrounded)
        {
            lineStartPos = transform.position + new Vector3(0, stepOffset, 0f);
            lineEndPos = lineStartPos + transform.forward * slopeDistance;
            Debug.DrawLine(lineStartPos, lineEndPos, Color.green);

            //stepRayが坂や階段に接触しているか
            Vector3 stepRayEndPos = stepRay.position + stepRay.forward * stepDistance;
            if (Physics.Linecast(stepRay.position, stepRayEndPos, out stepHit, LayerMask.GetMask("Ground")))
            {
                Debug.DrawLine(stepRay.position, stepRayEndPos, Color.green);

                if (canGoStepOrSlope())
                {
                    if (Mathf.Abs(stepHit.normal.z) > 0 && Mathf.Abs(stepHit.normal.z) < 1)
                    {
                        input = Vector3.ProjectOnPlane(input, stepHit.normal);
                    }
                    else if (stepHit.normal.z == 1)
                    {
                        isRightAngle = true;
                    }
                }
            }
            else
            {
                isRightAngle = false;
            }
        }
        else
        {
            isRightAngle = false;
        }

        //回転方向を向く
        if (input.magnitude > 0)
        {
            targetRotation = Quaternion.LookRotation(input, Vector3.up);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

        //ジャンプ処理
        if (Input.GetButtonDown("Jump") && groundChecker.isGrounded)
        {
            pushJump = true;
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Idle:

                Move(walkSpeed);

                if (input.magnitude >= 0.1f)
                {
                    anim.SetBool("isIdling", false);

                    //Run移行
                    if (isRunning)
                    {
                        anim.SetBool("isRunning", true);
                        state = State.Run;
                        break;
                    }

                    //Walk移行
                    anim.SetBool("isWalking", true);
                    state = State.Walk;
                }
                break;

            case State.Walk:

                Move(walkSpeed);

                //Run移行
                if (isRunning)
                {
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isRunning", true);
                    state = State.Run;
                    break;
                }

                //Idle移行
                if (input.magnitude < 0.1f)
                {
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isIdling", true);
                    state = State.Idle;
                }
                break;

            case State.Run:

                Move(runSpeed);

                //Idle移行
                if (input.magnitude < 0.1f)
                {
                    Debug.Log("アイドル移行");
                    anim.SetBool("isRunning", false);
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isIdling", true);
                    state = State.Idle;
                    break;
                }

                //Walk移行
                if (!isRunning)
                {
                    Debug.Log("ウォーク移行");
                    anim.SetBool("isRunning", false);
                    anim.SetBool("isWalking", true);
                    state = State.Walk;
                }
                break;
        }

        //ジャンプ実行
        if (pushJump)
        {
            pushJump = false;
            groundChecker.isGrounded = false;
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    private bool canGoStepOrSlope()
    {
        if (Vector3.Angle(transform.up, stepHit.normal) <= slopeLimit)
        {
            return true;
        }
        else if (!Physics.Linecast(lineStartPos, lineEndPos, LayerMask.GetMask("Ground")))
        {
            return true;
        }
        return false;
    }

    private void Move(float speed)
    {
        velocity = input.normalized * speed;
        velocity -= rb.velocity;
        velocity = new Vector3(Mathf.Clamp(velocity.x, -speed, speed), 0, Mathf.Clamp(velocity.z, -speed, speed));
        rb.AddForce(rb.mass * velocity / Time.fixedDeltaTime, ForceMode.Force);

        //階段上る
        if (isRightAngle)
        {
            Debug.Log("直角");
            rb.AddForce(rb.mass * Vector3.up * stepSmooth / Time.fixedDeltaTime, ForceMode.Force);
        }
    }
}
