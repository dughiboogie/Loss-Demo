using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("GameObject assignments")]
    private new Rigidbody2D rigidbody;
    private new BoxCollider2D collider;
    private PlayerGFX GFX;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform ledgeGrab;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform wallCheckHigh;
    [SerializeField] private Transform wallCheckLow;
    [SerializeField] private Transform cameraTarget;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 7f;

    [Header("Jumps")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float hangTime = .05f; // Hang time (coyote effect)
    private float hangCounter;
    [SerializeField] private float jumpBufferTime = .1f; // Expanded time slot to let the player jump before landing
    private float jumpBufferCounter;
    [SerializeField] private float wallJumpMovementCooldown = .15f;
    public bool isGrounded;
    private float groundCheckCircleRadius; // Radius of CircleCollider2D that checks if player is grounded
    private bool wasOnGround = true; // Last frame's isGrounded value

    [Header("Dash")]
    [SerializeField] private float dashValue = 20f;
    private float currentDashDuration = 0;
    [SerializeField] private float dashDuration = .15f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashCooldownCounter = 0f;
    private int maxDashInAir = 1;
    private int dashInAirCounter = 0;

    [Header("Camera movement")]
    [SerializeField] private float lookAtDistance = 3.5f; // Distance that the target of the camera moves on input
    private float lookVertical; // Vertical axis (left stick) value
    private float moveHorizontal; // Horizontal axis (left stick) value
    public int facingDirection = 1; // Facing direction of the player (1: right, -1: left)

    [Header("Wall slide & Ledge hanging")]
    private RaycastHit2D ledgeCheckHit;
    private RaycastHit2D wallCheckHit;
    private RaycastHit2D wallCheckHighHit;
    private RaycastHit2D wallCheckLowHit;
    private bool isHanging = false;
    private Vector2 ledgeHangDirection;
    private bool isWallSliding = false;
    [SerializeField] private float wallStickingDuration = .2f;
    private float wallStickingDurationCounter;
    [SerializeField] private float ledgeHangCooldown = .1f;
    private float ledgeHangCooldownCounter;

    private void Awake()
    {
        GFX = GetComponent<PlayerGFX>();
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        groundCheckCircleRadius = groundCheck.GetComponent<CircleCollider2D>().radius;
    }

    private void Update()
    {
        // Check every frame if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckCircleRadius, whatIsGround);
        GFX.SetIsGrounded(isGrounded);

        // Check if player game object has to be flipped based on moveHorizontal value
        if(moveHorizontal != 0) {
            EvaluatePlayerFacingDirection();
        }

        // If player is in mid-air check every frame for ledge or wall nearby
        if(!isGrounded) {
            ledgeCheckHit = Physics2D.Raycast(ledgeCheck.position, new Vector2(facingDirection, 0), .1f, whatIsGround);
            wallCheckHit = Physics2D.Raycast(wallCheck.position, new Vector2(facingDirection, 0), .1f, whatIsGround);
            wallCheckHighHit = Physics2D.Raycast(wallCheckHigh.position, new Vector2(facingDirection, 0), .1f, whatIsGround);
            wallCheckLowHit = Physics2D.Raycast(wallCheckLow.position, new Vector2(facingDirection, 0), .1f, whatIsGround);
        }

        #region Ledge grab

        if(PowersManager.instance.IsPowerActive("Ledge Grab")) {
            // Reset ledge grab cooldown (prevent grabbing the same ledge over and over)
            if(ledgeHangCooldownCounter > 0) {
                ledgeHangCooldownCounter -= Time.deltaTime;
            }

            // If player is in mid-air check for nearby ledges
            if(!isGrounded && !isHanging && !isWallSliding) {
                CheckLedge();
            }
            // If player is already hanging from a ledge prevent movement
            else if(isHanging) {
                moveHorizontal = 0;
            }
        }

        #endregion

        #region Wall slide

        if(PowersManager.instance.IsPowerActive("Wall Slide")) {
            if(isGrounded && isWallSliding) {
                StopWallSliding();
            }

            if(!isGrounded) {
                CheckWall();
            }
            else if(isWallSliding) {
                WallSlide();
            }
        }
        #endregion

        #region Dashes

        // When player hits the ground or sticks to a wall reset the air dash counter
        if(isGrounded || isWallSliding || isHanging) {
            dashInAirCounter = 0;
            if(!wasOnGround) {
                dashCooldownCounter = 0;
            }
        }

        // Subtract from dash cooldown every frame if cooldown is > 0
        if(dashCooldownCounter > 0) {
            dashCooldownCounter -= Time.deltaTime;
        }

        // If dash duration is > 0 it means that the player is dashing
        if(currentDashDuration > 0) {
            // If player is onto a wall dash in the opposite direction
            if(isWallSliding) {
                // Count the dash from the wall as a dash in mid-air
                dashInAirCounter++;
                FlipPlayerObject();
            }

            // Prevent new inputs if player is dashing
            InputManager.instance.BlockInput(currentDashDuration);
            rigidbody.velocity = new Vector2(dashValue * facingDirection, 0f);

            currentDashDuration -= Time.deltaTime;
        }

        #endregion

        // Reset camera target position if player is moving or not grounded
        if(moveHorizontal != 0 || !isGrounded) {
            cameraTarget.localPosition = Vector2.zero;
            GFX.SetLookUp(false);
            GFX.SetLookDown(false);
        }

        #region Jumps

        // Manage hang time (Coyote Effect)
        if(isGrounded || isWallSliding || isHanging) {
            hangCounter = hangTime;
        }
        else {
            hangCounter -= Time.deltaTime;
        }

        // Manage jump buffer counter
        if(jumpBufferCounter > 0) {
            jumpBufferCounter -= Time.deltaTime;
        }

        if(jumpBufferCounter > 0 && hangCounter > 0) {
            Jump();
        }

        #endregion

        if(InputManager.instance.inputIsFree) {
            rigidbody.velocity = new Vector2(moveHorizontal * movementSpeed, rigidbody.velocity.y);
        }

        #region Graphics

        GFX.SetHorizontalSpeed(moveHorizontal);
        GFX.SetYVelocity(rigidbody.velocity.y);
        GFX.SetFootstepsParticles(moveHorizontal, isGrounded);
        GFX.SetGroundImpactParticles(wasOnGround, isGrounded);

        #endregion

        // wasOnGround is the last frame's grounded status
        wasOnGround = isGrounded;
    }

    #region InputActions

    // Get horizontal movement input value
    public void Move(InputAction.CallbackContext context)
    {
        if(InputManager.instance.inputIsFree) {
            // Read input on the horizontal axis
            moveHorizontal = context.ReadValue<Vector2>().x;

            // If movement input value is too little set it to 0
            if(moveHorizontal > -0.4f && moveHorizontal < 0.4f)
                moveHorizontal = 0;
        }
    }

    /*
     * The jump button interaction is the default interaction. 
     * When the button is pressed, the action is performed, when it's released the action is canceled.
     * If the player's velocity is positive and the action is canceled, the vertical velocity is decreased to make smaller jumps.
     * 
     * OLD REMINDER: If the TapInteraction is started but not performed, it is automatically canceled.
     */
    public void Jump(InputAction.CallbackContext context)
    {
        // Reset jump buffer counter
        if(context.performed) {
            jumpBufferCounter = jumpBufferTime;
        }

        // Small jumps based on jump button release
        if(context.canceled && rigidbody.velocity.y > 0) {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y * .3f);
        }
    }

    /*
     * Thrust player forward and set a cooldown for the next time he can dash.
     * If player is in mid-air he can dash only one time.
     */
    public void Dash(InputAction.CallbackContext context)
    {
        if(PowersManager.instance.IsPowerActive("Dash") && context.performed) {
            if(!isHanging && dashCooldownCounter <= 0) {
                if(isGrounded) {
                    dashCooldownCounter = dashCooldown;
                    currentDashDuration = dashDuration;
                    GFX.Dash();
                }

                if(!isGrounded && dashInAirCounter < maxDashInAir) {
                    dashCooldownCounter = dashCooldown;
                    currentDashDuration = dashDuration;
                    dashInAirCounter++;
                    GFX.Dash();
                }
            }
        }
    }

    /*
     * On left stick held down move the camera target down, on release move it back up
     */
    public void MoveCameraVertical(InputAction.CallbackContext context)
    {
        if(moveHorizontal == 0 && isGrounded && context.performed) {
            lookVertical = context.ReadValue<Vector2>().y;

            if(lookVertical > 0.7) {
                cameraTarget.localPosition = new Vector2(0, lookAtDistance);
                GFX.SetLookUp(true);
            }
            else if(lookVertical < -0.7) {
                cameraTarget.localPosition = new Vector2(0, -lookAtDistance);
                GFX.SetLookDown(true);
            }
        }
        else {
            cameraTarget.localPosition = Vector2.zero;
            GFX.SetLookUp(false);
            GFX.SetLookDown(false);
        }
    }

    // Get the direction of the left stick when player is hanging from a ledge to get the direction in which to jump
    public void LedgeHangDirection(InputAction.CallbackContext context)
    {
        if(isHanging && context.performed) {
            ledgeHangDirection = context.ReadValue<Vector2>();
        }
    }

    #endregion

    #region MovementLogic

    private void Jump()
    {
        // If player is onto a wall jump diagonally
        if(isWallSliding) {
            // Block input for the start of the jump
            InputManager.instance.BlockInput(wallJumpMovementCooldown);
            rigidbody.velocity = new Vector2(jumpForce * (-facingDirection), jumpForce);
            FlipPlayerObject();
            StopWallSliding();
        }
        // If player is hanging from a ledge jump in the direction of the left analog stick
        else if(isHanging) {
            // Reset all ledge hanging variables
            UnfreezeMovement();
            isHanging = false;
            GFX.SetHangLedge(isHanging);
            ledgeGrab.gameObject.SetActive(false);
            ledgeHangCooldownCounter = ledgeHangCooldown;

            // Compensate grab ledge spritesheet dimensions
            transform.position = new Vector2(transform.position.x + (facingDirection * .25f), transform.position.y);

            // if LStick down - jump down
            if(ledgeHangDirection.y <= -0.4f) {
                rigidbody.velocity = new Vector2(0f, -3f);  // Add a little boost downward for the feeling
            }
            // if LStick left and facing direction is right - jump left
            // if LStick right and facing direction is left - jump right
            else if((ledgeHangDirection.x <= -0.4f && facingDirection == 1) || (ledgeHangDirection.x >= 0.4f && facingDirection == -1)) {
                rigidbody.velocity = new Vector2(jumpForce * (-facingDirection), jumpForce);
                moveHorizontal = ledgeHangDirection.x;
                FlipPlayerObject();
            }
            // else jump up
            else {
                rigidbody.velocity = new Vector2(0f, jumpForce);
                moveHorizontal = ledgeHangDirection.x;
            }
        }
        // Normal jump from the ground
        else {
            rigidbody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        jumpBufferCounter = 0;
        GFX.StopGroundImpactParticles();

        // When player jumps set dash cooldown counter to 0, to make him able to dash right away
        dashCooldownCounter = 0;

        // AudioManager.instance.Play("Player_Jump");
    }

    // Check for a ledge near the player
    private void CheckLedge()
    {
        // If ledgeCheckHit hasn't hit a wall and wallCheckHit has, a ledge is in range to be grabbed
        if(ledgeCheckHit.collider == null && wallCheckHighHit.collider != null && wallCheckLowHit.collider != null) {
            LedgeGrab();
        }
        // If there isn't a ledge, return to initial state
        else {
            isHanging = false;
            GFX.SetHangLedge(isHanging);
        }

        /*
        // If ledgeCheckHit hasn't hit a wall and wallCheckHit has, a ledge is in range to be grabbed
        if(ledgeCheckHit.collider == null && wallCheckHit.collider != null) {
            LedgeGrab();
        }
        // If there isn't a ledge, return to initial state
        else {
            isHanging = false;
            GFX.SetHangLedge(isHanging);
        }
        */
    }

    // Make the player grab a nearby ledge
    private void LedgeGrab()
    {
        // If the player was just on a ledge, wait for the cooldown to avoid grabbing the same ledge over and over
        if(ledgeHangCooldownCounter <= 0) {
            // Compensate grab ledge spritesheet dimensions
            transform.position = new Vector2(transform.position.x - (facingDirection * .25f), transform.position.y);

            isHanging = true;
            rigidbody.velocity = Vector2.down * 7f; // Accelerate rigidbody to make grab more responsive
            moveHorizontal = 0;
            GFX.SetHangLedge(isHanging);
        }
    }

    // Check for a wall near the player
    private void CheckWall()
    {
        // If player is not grabbing a ledge and wallCheckHit has hit a wall, a wall is in range
        if(!isHanging && wallCheckHighHit.collider != null && wallCheckLowHit.collider != null && rigidbody.velocity.y < 0) {
            WallSlide();
        }
        else {
            isWallSliding = false;
            GFX.SetWallSlide(isWallSliding);
            collider.sharedMaterial.friction = 0f;
            GFX.StopWallSlideParticles();
        }

        /*
        // If player is not grabbing a ledge and wallCheckHit has hit a wall, a wall is in range
        if(!isHanging && wallCheckHit.collider != null) {
            WallSlide();
        }
        else {
            isWallSliding = false;
            GFX.SetWallSlide(isWallSliding);
            collider.sharedMaterial.friction = 0f;
            GFX.StopWallSlideParticles();
        }
        */
    }

    /*
    private void CheckLedgeGrab()
    {
        // Player is already hanging from a ledge, prevent movement
        if(isHanging) {
            moveHorizontal = 0;
        }
        // Player is near a wall in mid-air
        else if(ledgeHangCooldownCounter <= 0 && !isGrounded && isNearWallHigh && isNearWallLow) {

            // Check if the wall has a ledge (NOT operator because OverlapCircle returns true if the collider hits something, we need the inverse)
            isOnLedge = !Physics2D.OverlapCircle(ledgeCheck.position, ledgeCheckCircleRadius, whatIsGround);

            // Grab edge and prevent movement
            if(isOnLedge) {
                isHanging = true;
                moveHorizontal = 0;
                GFX.SetHangLedge(isHanging);
            }
        }
        else if(ledgeHangCooldownCounter > 0) {
            ledgeHangCooldownCounter -= Time.deltaTime;
        }
        // Player is on the ground or not near a wall
        else if(isGrounded || !isNearWallHigh || !isNearWallLow) {
            isHanging = false;
            GFX.SetHangLedge(isHanging);
        }
    }
    */

    private void WallSlide()
    {
        /*
        * Player starts sliding immediately
        * 
        if(!isWallSliding) {
            if(currentDashDuration > 0) {
                currentDashDuration = 0f;
            }
            isWallSliding = true;
            GFX.SetWallSlide(isWallSliding);
            collider.sharedMaterial.friction = 0.002f;
            GFX.StartWallSlideParticles();
        }
        */

        // Player sticks to wall for a bit and then starts sliding
        // Player is already on wall
        if(isWallSliding) {
            // Player starts sliding down the wall
            if(wallStickingDurationCounter <= 0f) {
                collider.sharedMaterial.friction = 0.0015f;
                collider.enabled = false;
                collider.enabled = true;
                GFX.StartWallSlideParticles();
            }
            // Player still sticks to wall
            else {
                wallStickingDurationCounter -= Time.deltaTime;
            }
        }
        // Player just hit a wall in mid-air
        else {
            // If player is dashing stop it to prevent glitches
            if(currentDashDuration > 0) {
                currentDashDuration = 0f;
            }
            isWallSliding = true;
            GFX.SetWallSlide(isWallSliding);
            wallStickingDurationCounter = wallStickingDuration;
            collider.sharedMaterial.friction = 0.4f;
        }

    }


    /*
    * IF player is in the air && the two colliders for the wall check collide with the ground && the player is moving (towards the wall)
    * THEN check if there is a ledge to hang from, otherwise stick to wall for wall slide
    * ELSE set wall slide and ledge hanging variable to false
    */
    /*
    private void CheckWallSlide()
    {
        // Player is moving towards a wall in mid-air
        if(!isHanging && (!isGrounded && isNearWallHigh && isNearWallLow && moveHorizontal != 0)) {

            /*
             * Player starts sliding immediately
             * 
            if(!isWallSliding) {
                if(currentDashDuration > 0) {
                    currentDashDuration = 0f;
                }
                isWallSliding = true;
                GFX.SetWallSlide(isWallSliding);
                collider.sharedMaterial.friction = 0.002f;
                GFX.StartWallSlideParticles();
            }
            */

    /*

            // Player sticks to wall for a bit and then starts sliding
            // Player is already on wall
            if(isWallSliding) {
                // Player starts sliding down the wall
                if(wallStickingDurationCounter <= 0f) {
                    collider.sharedMaterial.friction = 0.0015f;
                    collider.enabled = false;
                    collider.enabled = true;
                    GFX.StartWallSlideParticles();
                }
                // Player still sticks to wall
                else {
                    wallStickingDurationCounter -= Time.deltaTime;
                }
            }
            // Player just hit a wall in mid-air
            else {
                // If player is dashing stop it to prevent glitches
                if(currentDashDuration > 0) {
                    currentDashDuration = 0f;
                }
                isWallSliding = true;
                GFX.SetWallSlide(isWallSliding);
                wallStickingDurationCounter = wallStickingDuration;
                collider.sharedMaterial.friction = 0.4f;
            }
        }
        else {
            isWallSliding = false;
            GFX.SetWallSlide(isWallSliding);
            collider.sharedMaterial.friction = 0f;
            GFX.StopWallSlideParticles();
        }
    }
    */

    #endregion

    #region Utility

    // Utility methods to freeze and unfreeze rigidbody when player grabs a ledge
    public void FreezeYMovement()
    {
        rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void UnfreezeMovement()
    {
        rigidbody.constraints = RigidbodyConstraints2D.None;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void ActivateLedgeGrab()
    {
        ledgeGrab.gameObject.SetActive(true);
    }

    // Check if player's game object has to be flipped around when moveHorizontal != 0
    private void EvaluatePlayerFacingDirection()
    {
        /* 
         * Prevent changing facing direction if:
         * - player has just jumped from wall
         * - player is dashing
         * - player is hanging from a ledge
         */
        if(!InputManager.instance.inputIsFree || isHanging) {
            return;
        }
        else {
            if((moveHorizontal > 0.1f && facingDirection == -1) || (moveHorizontal < -0.1f && facingDirection == 1)) {
                FlipPlayerObject();
            }
        }
    }
    
    private void FlipPlayerObject()
    {
        if(facingDirection == -1) {
            transform.Rotate(0, 180, 0);
            facingDirection = 1;
        }
        else if(facingDirection == 1) {
            transform.Rotate(0, 180, 0);
            facingDirection = -1;
        }
    }

    private void StopWallSliding()
    {
        isWallSliding = false;
        GFX.SetWallSlide(isWallSliding);
        collider.sharedMaterial.friction = 0f;
        GFX.StopWallSlideParticles();
    }

    #endregion
}