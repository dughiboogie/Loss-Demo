using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerController : MonoBehaviour
{
    public static NewPlayerController instance;

    private new Rigidbody2D rigidbody;
    private new BoxCollider2D collider;


    [SerializeField] private float movementSpeed = 7f;
    private float moveHorizontal; // Horizontal axis (left stick) value
    public float MoveHorizontal
    {
        get { return moveHorizontal; }
    }


    [SerializeField] private float lookAtDistance = 3.5f; // Distance that the target of the camera moves on input
    private float lookVertical; // Vertical axis (left stick) value

    private void Awake()
    {
        #region Singleton

        if(instance != null) {
            Debug.LogWarning("Multiple instances of PlayerController2D found!");
            return;
        }
        instance = this;

        #endregion

        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Prevent movement if player is grabbing ledge
        if(!PlayerStatusManager.instance.IsGrabbingLedge) {
            
            if(moveHorizontal != 0 && InputManager.instance.InputIsFree) {
                    rigidbody.velocity = new Vector2(moveHorizontal * movementSpeed, rigidbody.velocity.y);
            }


        }


    }

    public void SetRigidbodyVelocity(Vector2 velocityVector)
    {
        rigidbody.velocity = velocityVector;
    }
}