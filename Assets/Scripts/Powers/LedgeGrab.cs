using UnityEngine;

public class LedgeGrab : MonoBehaviour
{
    private PlayerStatusManager playerStatusManager;

    [SerializeField] private Transform ledgeGrab;

    private float ledgeHangCooldown = .1f;
    private float ledgeHangCooldownCounter;

    private void Awake()
    {
        playerStatusManager = PlayerStatusManager.instance;
    }

    private void Update()
    {
        if(PowersManager.instance.IsPowerActive("Ledge Grab")) {

            // Reset ledge grab cooldown (prevent grabbing the same ledge over and over)
            if(ledgeHangCooldownCounter > 0) {
                ledgeHangCooldownCounter -= Time.deltaTime;
            }
            // Grab ledge in mid air
            else if(PlayerStatusManager.instance.IsNearLedgeMidAir) {

                // TODO move to GFX
               
                transform.position = new Vector2(transform.position.x - (PlayerStatusManager.instance.FacingDirection * .25f), transform.position.y);

                PlayerStatusManager.instance.IsGrabbingLedge = true;
                NewPlayerController.instance.SetRigidbodyVelocity(Vector2.down * 7f); // Accelerate rigidbody to make grab more responsive
            }
        }
    }
}
