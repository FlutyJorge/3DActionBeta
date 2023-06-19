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
    [Header("�p�����[�^�[")]
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

    //�v���C���[�̍s����񋓌^�ŊǗ�
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
        //�x�N�g���̐��K��
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
                    //���_�b�V��
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        anim.SetBool("isIdling", false);
                        anim.SetBool("isRunning", true);
                        state = State.isRunning;
                    }
                    //������
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

                //���ҋ@
                if (input.magnitude == 0)
                {
                    anim.SetBool("isIdling", true);
                    anim.SetBool("isWalking", false);
                    state = State.isIdling;
                }
                //���_�b�V��
                //���V�t�g�̓C���v�b�g�}�l�[�W���[�Ń_�b�V���ȊO�̖��O�ŊǗ�����Ă��邽�߁AGetKey���g�p����
                else if (Input.GetKey(KeyCode.LeftShift) && groundChecker.isGrounded)
                {
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isRunning", true);
                    state = State.isRunning;
                }
                //���W�����v
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

                //���ҋ@
                if (input.magnitude == 0)
                {
                    anim.SetBool("isIdling", true);
                    anim.SetBool("isRunning", false);
                    state = State.isIdling;
                }
                //������
                else if (!Input.GetKey(KeyCode.LeftShift) && groundChecker.isGrounded)
                {
                    anim.SetBool("isWalking", true);
                    anim.SetBool("isRunning", false);
                    state = State.isWalking;
                }
                //���W�����v
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
                //�ō����B�_�𑬓x�x�N�g���ɂ���Č��m
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
                    //���_�b�V��
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        anim.SetBool("isJumping", false);
                        anim.SetBool("isRunning", true);
                        state = State.isRunning;
                    }
                    //������
                    else
                    {
                        anim.SetBool("isJumping", false);
                        anim.SetBool("isWalking", true);
                        state = State.isWalking;
                    }
                }
                //���ҋ@
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
        //�Q�l:https://www.youtube.com/watch?v=DrFk5Q_IwG0

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
