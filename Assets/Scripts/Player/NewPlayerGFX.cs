using UnityEngine;

public class NewPlayerGFX : MonoBehaviour
{
    static public NewPlayerGFX instance;

    [SerializeField] private Animator animator;

    [Header("Particle systems")]
    [SerializeField] private ParticleSystem footstepsParticles;
    private ParticleSystem.EmissionModule footstepsEmission;
    [SerializeField] private ParticleSystem groundImpactParticles;
    [SerializeField] private ParticleSystem wallSlideParticles;
    private ParticleSystem.EmissionModule wallSlideEmission;
    [SerializeField] ParticleSystem playerHurtParticles;

    private bool wasGrabbingLedge = false;

    private void Awake()
    {
        #region Singleton

        if(instance != null) {
            Debug.LogWarning("Multiple instances of PlayerGFX found!");
            return;
        }
        instance = this;

        #endregion

        footstepsEmission = footstepsParticles.emission;
        wallSlideEmission = wallSlideParticles.emission;
    }


    private void Update()
    {
        #region ListenForAnimatorParameters

        animator.SetBool("Grounded", PlayerStatusManager.instance.IsGrounded);

        animator.SetFloat("Horizontal speed", Mathf.Abs(NewPlayerController.instance.MoveHorizontal));

        // Compensate grab ledge spritesheet dimensions
        if(PlayerStatusManager.instance.IsGrabbingLedge && !wasGrabbingLedge) {
            transform.position = new Vector2(transform.position.x - (PlayerStatusManager.instance.FacingDirection * .25f), transform.position.y);
            wasGrabbingLedge = true;
        }

        animator.SetBool("HangLedge", PlayerStatusManager.instance.IsGrabbingLedge);

        // TODO all the others

        #endregion


    }

    #region Utilities

    

    #endregion

    #region ParticleSystems

    public void SetFootstepsParticles(float moveHorizontal, bool isGrounded)
    {
        if(moveHorizontal != 0 && isGrounded) {
            footstepsEmission.rateOverTime = 20f;
        }
        else {
            footstepsEmission.rateOverTime = 0f;
        }
    }

    public void SetGroundImpactParticles(bool wasOnGround, bool isGrounded)
    {
        if(!wasOnGround && isGrounded) {
            if(groundImpactParticles.gameObject.activeInHierarchy) {
                groundImpactParticles.Stop();
            }
            groundImpactParticles.gameObject.SetActive(true);
            groundImpactParticles.Play();
        }
    }

    public void StopGroundImpactParticles()
    {
        groundImpactParticles.Stop();
    }

    public void StartWallSlideParticles()
    {
        wallSlideEmission.rateOverTime = 20f;
    }

    public void StopWallSlideParticles()
    {
        wallSlideEmission.rateOverTime = 0f;
    }

    public void StartHurtParticles()
    {
        playerHurtParticles.Play();
    }

    #endregion

}
