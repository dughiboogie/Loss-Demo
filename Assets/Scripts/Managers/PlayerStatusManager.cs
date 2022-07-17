using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    static public PlayerStatusManager instance;


    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform cameraTarget;


    [SerializeField] private Transform groundCheck;
    private bool isGrounded;
    private float groundCheckCircleRadius; // Radius of CircleCollider2D that checks if player is grounded
    public bool IsGrounded
    {
        get { return isGrounded; }
    }


    private int facingDirection = 1;
    public int FacingDirection
    {
        get { return facingDirection; }
        set { facingDirection = value; }
    }


    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private Transform wallCheckHigh;
    [SerializeField] private Transform wallCheckLow;
    private RaycastHit2D ledgeCheckHit;
    private RaycastHit2D wallCheckHighHit;
    private RaycastHit2D wallCheckLowHit;

    private bool isNearWallMidAir = false;
    public bool IsNearWallMidAir
    {
        get { return isNearWallMidAir; }
    }

    private bool isNearLedgeMidAir = false;
    public bool IsNearLedgeMidAir
    {
        get { return isNearLedgeMidAir; }
    }


    private bool isDashing = false;
    public bool IsDashing
    {
        get { return isDashing; }
        set { isDashing = value; }
    }

    private bool isGrabbingLedge = false;
    public bool IsGrabbingLedge
    {
        get { return isGrabbingLedge; }
        set { isGrabbingLedge = value; }
    }

    private bool isGrabbingWall = false;
    public bool IsGrabbingWall
    {
        get { return isGrabbingWall; }
        set { isGrabbingWall = value; }
    }

    /*
     * is jumping
     * is running
     * is falling
     * is wall sliding
     * is wall jumping
     * is dashing
     * is hanging
     * is grabbing edge
     * is near ledge
     * is near wall in mid air
     * 
     * 
     */





    private void Awake()
    {
        #region Singleton

        if(instance != null) {
            Debug.LogWarning("Multiple instances of PlayerStatusManager found!");
            return;
        }
        instance = this;

        #endregion

        groundCheckCircleRadius = groundCheck.GetComponent<CircleCollider2D>().radius;
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckCircleRadius, whatIsGround);

        // If player is in mid-air check for ledge or wall nearby
        if(!isGrounded) {
            ledgeCheckHit = Physics2D.Raycast(ledgeCheck.position, new Vector2(facingDirection, 0), .1f, whatIsGround);
            wallCheckHighHit = Physics2D.Raycast(wallCheckHigh.position, new Vector2(facingDirection, 0), .1f, whatIsGround);
            wallCheckLowHit = Physics2D.Raycast(wallCheckLow.position, new Vector2(facingDirection, 0), .1f, whatIsGround);

            // If the two wallCheckHit hit the ground and ledgeCheckHit hasn't, then the player is near a ledge in mid air
            if(wallCheckHighHit.collider != null && wallCheckLowHit.collider != null) {
                if(ledgeCheckHit.collider == null) {
                    isNearLedgeMidAir = true;
                    isNearWallMidAir = false;
                }
                else {
                    isNearLedgeMidAir = false;
                    isNearWallMidAir = true;
                }
            }
        }
        

    }
}
