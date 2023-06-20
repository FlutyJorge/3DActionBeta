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
                Debug.DrawLine(transform.position + new Vector3(0, stepOffset, 0), transform.position + new Vector3(0, stepOffset, 0) + transform.forward * slopeDistance, Color.green);

                
            }
        }
    }
}
