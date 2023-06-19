using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GroundChecker groundChecker;
    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    private Animator anim;
    private Rigidbody rigid;
    private Vector3 clampedInput;
    private bool pushJumpButton;

    [Space(10)]
    [Header("パラメーター")]
    [SerializeField] float stepHight;
    [SerializeField] float stepSmooth;
    [SerializeField] float rotateSpeed;
    [SerializeField] float walkspeed;
    [SerializeField] float walkMaxSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float runMaxSpeed;
    [SerializeField] float airSpeed;
    [SerializeField] float airMaxSpeed;
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpDist;

    //プレイヤーの行動を列挙型で管理
    private enum State
    {
        isIdling, isWalking, isRunning, isJumping
    }
    private State state = State.isIdling;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();

        stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHight, stepRayUpper.transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //ベクトルの正規化
        clampedInput = Vector3.ClampMagnitude(input, 1f);
        if (groundChecker.isGrounded)
        {
            transform.LookAt(rigid.position + input);
        }

        ClimbStep();

        switch (state)
        {
            case State.isIdling:

                if (input.magnitude != 0 && groundChecker.isGrounded)
                {
                    //→ダッシュ
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        anim.SetBool("isIdling", false);
                        anim.SetBool("isRunning", true);
                        state = State.isRunning;
                    }
                    //→歩き
                    else
                    {
                        anim.SetBool("isIdling", false);
                        anim.SetBool("isWalking", true);
                        state = State.isWalking;
                    }
                }
                else if (pushJumpButton)
                {
                    Jump();
                    anim.SetBool("isIdling", false);
                    anim.SetBool("isJumping", true);
                    state = State.isJumping;
                }
                break;

            case State.isWalking:
                Move(walkspeed, walkMaxSpeed);

                //→待機
                if (input.magnitude == 0)
                {
                    anim.SetBool("isIdling", true);
                    anim.SetBool("isWalking", false);
                    state = State.isIdling;
                }
                //→ダッシュ
                //左シフトはインプットマネージャーでダッシュ以外の名前で管理されているため、GetKeyを使用する
                else if (Input.GetKey(KeyCode.LeftShift) && groundChecker.isGrounded)
                {
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isRunning", true);
                    state = State.isRunning;
                }
                //→ジャンプ
                else if (pushJumpButton)
                {
                    Jump();
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isJumping", true);
                    state = State.isJumping;
                }
                break;

            case State.isRunning:
                Move(runSpeed, runMaxSpeed);

                //→待機
                if (input.magnitude == 0)
                {
                    anim.SetBool("isIdling", true);
                    anim.SetBool("isRunning", false);
                    state = State.isIdling;
                }
                //→歩き
                else if (!Input.GetKey(KeyCode.LeftShift) && groundChecker.isGrounded)
                {
                    anim.SetBool("isWalking", true);
                    anim.SetBool("isRunning", false);
                    state = State.isWalking;
                }
                //→ジャンプ
                else if (pushJumpButton)
                {
                    Jump();
                    anim.SetBool("isRunning", false);
                    anim.SetBool("isJumping", true);
                    state = State.isJumping;
                }
                break;

            case State.isJumping:

                Move(airSpeed, airMaxSpeed);
                bool isMaxJumpHight = false;
                //最高到達点を速度ベクトルによって検知
                if (rigid.velocity.y < 0.1f)
                {
                    isMaxJumpHight = true;
                }

                if (!isMaxJumpHight || !groundChecker.isGrounded)
                {
                    break;
                }

                if (input.magnitude != 0)
                {
                    //→ダッシュ
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        anim.SetBool("isJumping", false);
                        anim.SetBool("isRunning", true);
                        state = State.isRunning;
                    }
                    //→歩き
                    else
                    {
                        anim.SetBool("isJumping", false);
                        anim.SetBool("isWalking", true);
                        state = State.isWalking;
                    }
                }
                //→待機
                else
                {
                    anim.SetBool("isJumping", false);
                    anim.SetBool("isIdling", true);
                    state = State.isIdling;
                }
                break;
        }
        
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && groundChecker.isGrounded)
        {
            pushJumpButton = true;
        }
    }

    private void Move(float speed, float maxSpeed)
    {
        Vector3 velocity = clampedInput * speed;

        if (rigid.velocity.magnitude < maxSpeed)
        {
            rigid.AddForce(rigid.mass * velocity / Time.fixedDeltaTime, ForceMode.Force);
        }
    }

    private void Jump()
    {
        pushJumpButton = false;
        Vector3 jumpDirectin = clampedInput * jumpDist + transform.up * jumpHeight;
        rigid.AddForce(jumpDirectin, ForceMode.Impulse);
    }

    private void ClimbStep()
    {
        //参考:https://www.youtube.com/watch?v=DrFk5Q_IwG0

        if (!groundChecker.isGrounded)
        {
            return;
        }

        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.1f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.2f))
            {
                rigid.position -= new Vector3(0, -stepSmooth, 0);
                return;
            }
        }

        RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitLower45, 0.1f))
        {
            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitUpper45, 0.2f))
            {
                rigid.position -= new Vector3(0, -stepSmooth, 0);
                return;
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitLowerMinus45, 0.1f))
        {
            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitUpperMinus45, 0.2f))
            {
                rigid.position -= new Vector3(0, -stepSmooth, 0);
                return;
            }
        }
    }
}
