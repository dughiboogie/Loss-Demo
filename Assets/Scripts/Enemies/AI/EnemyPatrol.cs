using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private new Rigidbody2D rigidbody;
    public float movementSpeed = 2f;

    private Vector3 direction;

    public Collider2D ledgeDetection, wallDetection;
    private LayerMask groundLayer;
    // public Transform normalOrigin;
    
    bool isOnLedge = false;
    bool isAgainstWall = false;

    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");
        rigidbody = GetComponent<Rigidbody2D>();

        // Directions changes based on local rotation
        direction = transform.InverseTransformVector(Vector2.left);
    }

    private void Update()
    {
        if(movementSpeed > 0f) {

            // Enemy turns around
            if(!ledgeDetection.IsTouchingLayers(groundLayer) || wallDetection.IsTouchingLayers(groundLayer)) {
                transform.Rotate(0, 180, 0);
                direction = transform.InverseTransformVector(Vector2.left);
            }

            // rigidbody.AddForce(new Vector2(movementSpeed * direction.x, movementSpeed * (-direction.y)));

            rigidbody.velocity = new Vector2(movementSpeed * direction.x, movementSpeed * (-direction.y));
        }

        /*

        // Is rotating
        if(isOnLedge) {

            // Rotation completed
            if(ledgeDetection.IsTouchingLayers(groundLayer)) {
                isOnLedge = false;
            }
            // Continue rotating
            else {
                transform.Rotate(Vector3.Lerp(transform.localEulerAngles, new Vector3(0, 0, 90), Time.deltaTime));
            }
        }

        // Just encountered ledge
        if(!isOnLedge && !ledgeDetection.IsTouchingLayers(groundLayer)) {

            
            Vector3 newDirection = new Vector3(direction.x, direction.y, direction.z + 90);
            
            Vector3 upDirection = new Vector3(direction.x, direction.y, direction.z + 180);

            transform.localRotation.SetLookRotation(newDirection, upDirection);
            

            transform.Rotate(Vector3.Lerp(transform.localEulerAngles, new Vector3(0, 0, 90), Time.deltaTime));
            

            isOnLedge = true;
        }


        
        if(isOnLedge) {
            if(ledgeDetection.IsTouchingLayers(groundLayer)) {
                if(direction.x == -1) {
                    direction = new Vector2(0, -1);
                }
                else if(direction.x == 1) {
                    direction = new Vector2(0, 1);
                }
                else if(direction.y == -1) {
                    direction = new Vector2(1, 0);
                }
                else if(direction.y == 1) {
                    direction = new Vector2(-1, 0);
                }

                isOnLedge = false;
            }
        }
        */



    }

    public void StopMoving()
    {
        movementSpeed = 0f;
    }

}