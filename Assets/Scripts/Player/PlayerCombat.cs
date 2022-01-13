using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerCombat : MonoBehaviour 
{
    [Header("GameObject assignments")]
    private PlayerController2D playerController;
    private new Rigidbody2D rigidbody;
    private PlayerGFX GFX;
    private CinemachineImpulseSource impulseSource;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Player combat stats")]
    [SerializeField] private int attackDamage = 1;
    public int maxHealth = 3;
    public int currentHealth;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Attack parameters")]
    private bool readyForAttack = true;
    private float attackDirectionInput = 0f;
    private int attackDirection = 0;
    [SerializeField] private float attackCooldown = .5f;
    private float attackCooldownCounter;

    [Header("Attack hitbox")]
    [SerializeField] private float attackRange = .5f;
    private List<Collider2D> enemiesHit;

    [Header("Gravity change on mid-air attacks")]
    [SerializeField] private float gravityChangeCooldown = .3f;
    private float gravityChangeCounter;

    [Header("Player hits enemies")]
    [SerializeField] private float hitKnockbackValueX = 7f;
    [SerializeField] private float hitKnockbackValueY = 6f;

    [Header("Player gets hit")]
    private int deadPlayerLayer = 13; // Layer for dead player
    private Vector2 knockbackVector;
    [SerializeField] private float knockbackValueX = 10f;
    [SerializeField] private float knockbackValueY = 5f;
    [SerializeField] private float invincibilityTimeAfterHit = 1f;
    private float invincibilityTimeCount;

    [Header("Enemy aggro")]
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float aggroCheckWaitTime = .5f;
    private List<Collider2D> lastEnemiesAggroed;
    private List<Collider2D> currentEnemiesAggroed;

    /*
     * Currently using only one attack animation
     * 
    [Header("Attack animations")]
    private int attackAnimation = 0;    // Current attack animation to play
    [SerializeField] private float attackAnimationResetTime = .8f;  // Time for the attack combo to reset
    private float attackAnimationCount; // Counter that keeps track of the time from the last attack
    */

    private void Awake()
    {
        GFX = GetComponent<PlayerGFX>();
        playerController = GetComponent<PlayerController2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        enemiesHit = new List<Collider2D>();
        currentEnemiesAggroed = new List<Collider2D>();

        currentHealth = maxHealth;
    }

    #region EnemyAggro

    private void Start()
    {
        StartCoroutine(CheckEnemyAggro(aggroCheckWaitTime));
    }

    // Observer pattern to check if an enemy enters the player's aggro range
    private IEnumerator CheckEnemyAggro(float aggroCheckWaitTime)
    {
        while(GameManager.instance.enemiesAreInteractable) {   // TODO change true with GameManager.instance.enemiesAreInteractable

            yield return new WaitForSeconds(aggroCheckWaitTime);

            lastEnemiesAggroed = new List<Collider2D>(currentEnemiesAggroed);
            // Check for new enemies aggros
            currentEnemiesAggroed = new List<Collider2D>(Physics2D.OverlapCircleAll(transform.position, aggroRange, enemyLayers));

            // Remove from current enemies aggroed all objects layered as enemies that don't have an Enemy script.
            // If the current object has an enemy script and was not checked in the previous frame, update its path to follow player.
            for(int i = currentEnemiesAggroed.Count - 1; i >= 0; i--) {
                EnemyPathfinding currentEnemyPathfindingScript = currentEnemiesAggroed[i].GetComponent<EnemyPathfinding>();

                if(currentEnemyPathfindingScript == null) {
                    currentEnemiesAggroed.RemoveAt(i);
                }
                else if(!lastEnemiesAggroed.Contains(currentEnemiesAggroed[i])) {
                    // Debug.Log("New enemy aggroed!");

                    currentEnemiesAggroed[i].GetComponent<EnemyPathfinding>().FollowPlayer();
                }
            }

            // Check for enemies in lastEnemiesAggroed that are not in the currentEnemiesAggroed list (player lost their aggro)
            foreach(Collider2D enemy in lastEnemiesAggroed) {

                // If player lost enemy aggro call enemy method to stop searching for player
                if(!currentEnemiesAggroed.Contains(enemy)) {
                    enemy.GetComponent<EnemyPathfinding>().StopFollowingPlayer();

                    // TODO function to make enemy search near player's last knwown position
                }
            }

            /*
                * TODO set timeout to keep enemy aggro for a few seconds after loss of line of sight
                * 
                * TODO cast a ray or a line to check if enemy has line of sight with player
                * in particular, check for ground colliders and such (a LayerMask may be necessary)
                */

            /*
            // Raycast to every enemy to check if there is line of sight with the player
            for(int i = lastEnemiesAggroed.Count - 1; i >= 0 ; i--) {

                Collider2D currentEnemy = lastEnemiesAggroed[i];
                RaycastHit2D rayHit = Physics2D.Raycast(transform.position, currentEnemy.transform.position);

                if(rayHit.collider != null) {
                    Debug.Log("Linecast object found: " + rayHit.collider.name);
                    Enemy currentEnemyScript = currentEnemy.GetComponent<Enemy>();

                    if(currentEnemyScript == null) {
                        lastEnemiesAggroed.RemoveAt(i);
                    }
                    else {
                        currentEnemy.GetComponent<EnemyAI>().UpdatePath();
                    }

                }

            }
            */

        }
    }

    #endregion

    void Update()
    {
        if(attackCooldownCounter > 0) {
            attackCooldownCounter -= Time.deltaTime;
        }

        // Invincibility frames
        if(invincibilityTimeCount > 0) {
            invincibilityTimeCount -= Time.deltaTime;
        }

        // Reset gravity scale
        if(gravityChangeCounter <= 0) {
            rigidbody.gravityScale = 1f;
        }
        else {
            gravityChangeCounter -= Time.deltaTime;
        }

        /*
         * If player is in a combo reduce attack combo reset counter time
         * 
        if(attackAnimationCount > 0) {
            attackAnimationCount -= Time.deltaTime;
        }
        else {
            attackAnimation = 0;
        }
        */
    }

    #region Player gets hit

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        // Prevent damage when player just got hit
        if(invincibilityTimeCount > 0) {
            return;
        }

        currentHealth -= damage;
        playerHealth.RemoveHealth();

        invincibilityTimeCount = invincibilityTimeAfterHit;

        // Shake camera when player is hit
        impulseSource.GenerateImpulse();
        GFX.StartHurtParticles();
        GFX.Hurt();
        ApplyKnockback(enemyPosition);

        if(currentHealth <= 0) {
            Die();
        }
    }

    private void ApplyKnockback(Vector2 enemyPosition)
    {
        Vector2 knockbackDirection = new Vector2(transform.position.x - enemyPosition.x, transform.position.y - enemyPosition.y).normalized;
        knockbackVector = new Vector2(knockbackDirection.x * knockbackValueX, knockbackDirection.y * knockbackValueY);

        // Block inputs to apply knockback force correctly
        InputManager.instance.BlockInput();
        rigidbody.AddForce(knockbackVector, ForceMode2D.Impulse);
    }

    private void Die()
    {
        GFX.Death();

        // Change layer of dead player to ignore collisions with enemies
        gameObject.layer = deadPlayerLayer;
        foreach(Transform child in transform) {
            child.gameObject.layer = deadPlayerLayer;
        }

        rigidbody.velocity = Vector2.zero;
        GameManager.instance.playerIsDead = true;
        playerController.enabled = false;
        this.enabled = false;
    }

    #endregion

    #region Player attacks

    public void AttackDirection(InputAction.CallbackContext context)
    {
        attackDirectionInput = context.ReadValue<float>();

        if(attackDirectionInput <= -.6) {
            attackDirection = -1;
        }
        else if(attackDirectionInput >= .6) {
            attackDirection = 1;
        }
        else {
            attackDirection = 0;
        }
    }

    /*
     * Get attack input and start the correct attack animation based on the fact that the player is doing a combo or not
     */
    public void Attack(InputAction.CallbackContext context)
    {
        /* player grounded:
         * - horizontal attack
         * - upward attack
         * 
         * player not grounded:
         * - horizontal attack
         * - upward attack
         * - downward attack
         */

        if(attackCooldownCounter <= 0 && context.performed) {

            if(playerController.isGrounded) {
                if(attackDirection == 1) {
                    GFX.UpwardAttack();
                }
                else if(attackDirection == 0 || attackDirection == -1) {

                    GFX.HorizontalAttack();
                }
            }
            else {
                // Slow player and change gravity when player attacks in mid-air
                rigidbody.velocity = new Vector2(rigidbody.velocity.x * 0.3f, rigidbody.velocity.y * 0.3f);
                InputManager.instance.BlockInput(gravityChangeCooldown);
                rigidbody.gravityScale = 0;
                gravityChangeCounter = gravityChangeCooldown;

                if(attackDirection == 1) {
                    GFX.UpwardAttack();
                }
                else if(attackDirection == 0) {

                    GFX.HorizontalAttack();
                }
                else if(attackDirection == -1) {
                    GFX.DownwardAttack();
                }
            }

            attackCooldownCounter = attackCooldown;
        }

        /*
         * Animator for combo attacks
         * 
        if(context.performed && playerController.isGrounded) {

            if(attackAnimationCount > 0) {
                attackAnimation = attackAnimation == 3 ? 1 : ++attackAnimation; // Cycle through attack animation 1, 2, 3
            }
            else {
                attackAnimation = 1;
            }

            attackAnimationCount = attackAnimationResetTime;
            animator.SetTrigger("Attack" + attackAnimation);
        }
        */
    }

    /*
     * This method is called as an AnimatorEvent.
     * Get all the enemies in the player's attack hitbox and add them to the enemiesHit list.
     * If an enemy is already in the list, then the duplicate is removed.
     */
    public void HitEnemy()
    {
        List<Collider2D> currentEnemies = new List<Collider2D>(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers));

        if(currentEnemies.Count > 0) {

            // Remove gameObjects without an Enemy script (sensors, etc. that are layered as enemies)
            for(int i = currentEnemies.Count - 1; i >= 0; i--) {
                Enemy currentEnemyScript = currentEnemies[i].GetComponent<Enemy>();

                if(currentEnemyScript == null) {
                    currentEnemies.RemoveAt(i);
                }
            }

            // Remove duplicates
            foreach(Collider2D enemy in enemiesHit) {
                bool isDuplicate = false;

                for(int j = 0; j < currentEnemies.Count && !isDuplicate; j++) {

                    if(currentEnemies[j] == enemy) {
                        isDuplicate = true;
                        currentEnemies.RemoveAt(j);
                    }
                }
            }

            // Knockback player on enemy hit
            if(currentEnemies.Count > 0) {

                Vector2 hitKnockbackVector = new Vector2();

                if(attackDirection == 0) {
                    hitKnockbackVector = new Vector2(playerController.facingDirection * -1 * hitKnockbackValueX, 0f);
                }
                else if(attackDirection == -1) {
                    hitKnockbackVector = new Vector2(0f, hitKnockbackValueY);
                }
                else if(attackDirection == 1) {
                    hitKnockbackVector = new Vector2(0f, hitKnockbackValueY);
                }

                InputManager.instance.BlockInput();
                rigidbody.AddForce(hitKnockbackVector, ForceMode2D.Impulse);
            }

            enemiesHit.AddRange(currentEnemies);
        }
    }

    /*
     * This method is called as an AnimatorEvent.
     * At the end of the current attack animation, cycle through the enemiesHit list and apply damage to all of them.
     */
    public void DoDamage()
    {
        if(enemiesHit.Count > 0) {
            foreach(Collider2D enemy in enemiesHit) {
                if(enemy != null) {
                    enemy.GetComponent<EnemyCombat>().TakeDamage(attackDamage, attackPoint.position);
                    impulseSource.GenerateImpulse();
                }
            }

            enemiesHit.Clear();
        }
    }

    #endregion

    #region Combat SFX

    /*
    public void PlayLightAttackSound()
    {
        AudioManager.instance.Play("Player_Attack1");
    }

    public void PlayHeavyAttackSound()
    {
        AudioManager.instance.Play("Player_Attack2");
    }
    */

    #endregion

    private void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
