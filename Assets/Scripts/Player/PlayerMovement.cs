using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    // public float gravity = -9.81f;
    // public float groundDistance = 0.4f;
    // public LayerMask groungMask;
    // public Transform groundCheck;
    // bool isGrounded;
    // Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groungMask);
        // if (isGrounded && velocity.y < 0)
        // {
        //     velocity.y = -2f;
        // }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x) + (transform.forward * z);

        controller.Move(move * speed * Time.deltaTime);
        // velocity.y += gravity * Time.deltaTime;
        // controller.Move(velocity * Time.deltaTime);

    }
}
