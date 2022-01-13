using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour 
{
    private EnemyGFX gfx;
    private new Rigidbody2D rigidbody;

    [SerializeField] private int maxHealth = 2;
    private int currentHealth;

    private LayerMask playerLayer;

    [SerializeField] private int attackDamage = 1;

    private int deadEnemyLayer = 10; // Layer for dead enemies
    private string deadEnemySortingLayer = "DeadEnemies";

    // Knockback variables
    private Vector2 knockbackVector;
    private float currentKnockbackDuration = 0;
    [SerializeField] private float knockbackDuration = .1f;
    [SerializeField] private float knockbackValueX = 100f;
    [SerializeField] private float knockbackValueY = 100f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        gfx = GetComponent<EnemyGFX>();

        playerLayer = LayerMask.GetMask("Player");
    }

    /*
     * Make enemy hurt player when in contact.
     */
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerCombat>().TakeDamage(attackDamage, transform.position);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerCombat>().TakeDamage(attackDamage, transform.position);
        }
    }

    private void Update()
    {
        if(currentKnockbackDuration > 0) {
            currentKnockbackDuration -= Time.deltaTime;

            rigidbody.AddForce(knockbackVector);
        }
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        currentHealth -= damage;
        // rigidbody.velocity = Vector2.zero;

        gfx.Hurt();
        ApplyKnockback(damageSourcePosition);

        if(currentHealth <= 0) {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 playerPosition)
    {
        currentKnockbackDuration = knockbackDuration;

        Vector2 knockbackDirection = new Vector2(transform.position.x - playerPosition.x, transform.position.y - playerPosition.y).normalized;
        knockbackVector = new Vector2(knockbackDirection.x * knockbackValueX, knockbackDirection.y * knockbackValueY);
    }

    private void Die()
    {
        gfx.Death();

        // Change layer of dead enemy to ignore collisions with player
        gameObject.layer = deadEnemyLayer;
        foreach(Transform child in transform) {
            child.gameObject.layer = deadEnemyLayer;
        }
        GetComponent<SpriteRenderer>().sortingLayerName = deadEnemySortingLayer;

        // GetComponent<Enemy>().DisableEnemy();
    }

}
