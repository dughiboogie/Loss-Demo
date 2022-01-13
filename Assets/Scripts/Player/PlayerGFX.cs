using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGFX : MonoBehaviour
{
    private Animator animator;

    [Header("Particle systems")]
    [SerializeField] private ParticleSystem footstepsParticles;
    private ParticleSystem.EmissionModule footstepsEmission;
    [SerializeField] private ParticleSystem groundImpactParticles;
    [SerializeField] private ParticleSystem wallSlideParticles;
    private ParticleSystem.EmissionModule wallSlideEmission;
    [SerializeField] ParticleSystem playerHurtParticles;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        footstepsEmission = footstepsParticles.emission;
        wallSlideEmission = wallSlideParticles.emission;
    }

    #region MovementParameters

    public void SetIsGrounded(bool isGrounded)
    {
        animator.SetBool("Grounded", isGrounded);
    }

    public void SetHangLedge(bool isHanging)
    {
        animator.SetBool("HangLedge", isHanging);
    }

    public void SetLookUp(bool lookUp)
    {
        animator.SetBool("LookUp", lookUp);
    }
    
    public void SetLookDown(bool lookDown)
    {
        animator.SetBool("LookDown", lookDown);
    }

    public void SetHorizontalSpeed(float moveHorizontal)
    {
        animator.SetFloat("Horizontal speed", Mathf.Abs(moveHorizontal));
    }

    public void SetYVelocity(float yVelocity)
    {
        animator.SetFloat("Y velocity", yVelocity);
    }

    public void Dash()
    {
        animator.SetTrigger("Dash");
    }

    public void SetWallSlide(bool isWallSliding)
    {
        animator.SetBool("WallSlide", isWallSliding);
    }

    #endregion

    #region CombatParameters

    public void Hurt()
    {
        animator.SetTrigger("Hurt");
    }

    public void Death()
    {
        animator.SetTrigger("Death");
    }

    public void UpwardAttack()
    {
        animator.SetTrigger("Attack_upwards");
    }

    public void HorizontalAttack()
    {
        animator.SetTrigger("Attack_horizontal");
    }

    public void DownwardAttack()
    {
        animator.SetTrigger("Attack_downwards");
    }

    #endregion

    #region Particles

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
