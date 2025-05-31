using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    
    private Vector2 startTouchPosition;
    
    private float swipeThreshold=50f;
    
    private Vector3 targetPosition;
    
    [SerializeField]
    private float forwardSpeed;
    
    [SerializeField]
    private Vector3 spawnPosition;
    
    [SerializeField]
    private float turnSpeed;
    
    [SerializeField]
    private float slideRotation;
    
    [SerializeField]
    private float jumpforce;
    
    [SerializeField]
    private float turnPosition;
    
    private bool jumpRequested = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.position = spawnPosition;
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Debug.Log("Touch Registered");
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 delta=touch.position - startTouchPosition;

                if (Mathf.Abs(delta.x) > swipeThreshold || Mathf.Abs(delta.y) > swipeThreshold)
                {
                    if (Math.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        // Horizontal Swipe
                        if (delta.x > 0)
                        {
                            // Swipe Right
                            if (Mathf.Approximately(transform.position.x, 0))
                                targetPosition = new Vector3(turnPosition, transform.position.y, transform.position.z);
                            else if (Mathf.Approximately(transform.position.x, -turnPosition))
                                targetPosition = new Vector3(0, transform.position.y, transform.position.z);
                        }
                        else
                        {
                            // Swipe Left
                            if (Mathf.Approximately(transform.position.x, 0))
                                targetPosition = new Vector3(-turnPosition, transform.position.y, transform.position.z);
                            else if (Mathf.Approximately(transform.position.x, turnPosition))
                                targetPosition = new Vector3(0, transform.position.y, transform.position.z);
                        }
                    }


                    else
                    {
                        if (delta.y > 0)
                        {
                            jumpRequested = true;
                        }
                        else
                        {
                            Debug.Log("Swipe Down");
                        }
                    }
                }
            }
            
        }
        
        Vector3 newPos = new Vector3(
            Mathf.MoveTowards(transform.position.x, targetPosition.x, turnSpeed * Time.deltaTime),
            transform.position.y,
            transform.position.z // Z is controlled by forward movement
        );
        transform.position = newPos;


    }
    
    

    private void FixedUpdate()
    {
        //rb.AddForce(Vector3.forward * (forwardSpeed * Time.deltaTime),ForceMode.Force);
        transform.Translate(Vector3.forward * (forwardSpeed * Time.deltaTime));

        
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }
    
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.1f, LayerMask.GetMask("Ground"));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (0.2f + 0.1f));
    }


}













