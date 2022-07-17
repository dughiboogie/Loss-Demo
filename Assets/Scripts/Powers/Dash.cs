using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : MonoBehaviour
{
    private PlayerController2D instance;

    [SerializeField] private float dashValue = 20f;
    [SerializeField] private float dashDuration = .15f;
    [SerializeField] private float dashCooldown = 1f;
    private int maxDashInAir = 1;
    private int dashInAirCounter = 0;

    private float currentDashDuration = 0f;
    public float CurrentDashDuration
    {
        get { return currentDashDuration; }
        set { currentDashDuration = value; }
    }

    private float dashCooldownCounter = 0f;
    public float DashCooldownCounter
    {
        get { return dashCooldownCounter; }
        set { dashCooldownCounter = value; }
    }

    private void Awake()
    {
        instance = PlayerController2D.instance;
    }

    private void Update()
    {
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

            SetRigidbodyVelocity(dashValue * facingDirection, 0f);

            currentDashDuration -= Time.deltaTime;
        }

    }

    #region Input actions

    /*
     * Thrust player forward and set a cooldown for the next time he can dash.
     * If player is in mid-air he can dash only one time.
     */
    public void TryDash(InputAction.CallbackContext context)
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

    #endregion
}
