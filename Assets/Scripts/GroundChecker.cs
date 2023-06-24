using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public bool isGrounded;

    void Start()
    {
        isGrounded = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            isGrounded = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            isGrounded = false;
        }
    }
}
