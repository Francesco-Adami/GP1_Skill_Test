using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class Character : MonoBehaviour
{
    // … (tutto il resto invariato)
    [Header("Character Stats")]
    [SerializeField] protected CharacterData characterData;
    public Health health;

    protected Rigidbody2D rigidBody2D;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected Vector2 _Input;

    // tieni traccia dello stato corrente
    protected AnimatorStates currentAnimState = AnimatorStates.Idle;

    // per il ground check; implementa come preferisci
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundCheckDist = 0.1f;
    private Collider2D myCollider;

    private void Awake()
    {
        // sposta qui l’inizializzazione
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        spriteRenderer.sprite = characterData.sprite;
        health.maxHealth = characterData.maxHealth;
        health.currentHealth = health.maxHealth;
        GameManager.Instance.SetPlayerSlider(health.currentHealth);
        health.isInvincible = false;
    }

    protected void Move()
    {
        rigidBody2D.velocity = new Vector2(_Input.x * characterData.speed, rigidBody2D.velocity.y);
    }

    private bool IsGrounded()
    {
        Vector2 origin = myCollider.bounds.center;
        float dist = myCollider.bounds.extents.y + groundCheckDist;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, dist, groundLayer);
        Debug.DrawRay(origin, Vector2.down * dist, Color.red);
        return hit.collider != null;
    }

    protected void EvaluateAnimationState()
    {
        AnimatorStates newState;

        // 1) se siamo in TakeDamage (magari via trigger)
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("TakeDamage"))
        {
            newState = AnimatorStates.TakeDamage;
        }
        // 2) salto o caduta
        else if (!IsGrounded())
        {
            newState = rigidBody2D.velocity.y > 0
                ? AnimatorStates.Jump
                : AnimatorStates.Fall;
        }
        // 3) movimento orizzontale
        else if (Mathf.Abs(_Input.x) > 0.01f)
        {
            newState = AnimatorStates.Movement;
            // riflessione sprite
            spriteRenderer.flipX = (_Input.x < 0);
        }
        // 4) idle
        else
        {
            newState = AnimatorStates.Idle;
        }

        // solo se cambia stato
        if (newState != currentAnimState)
        {
            currentAnimState = newState;
            animator.SetInteger("State", (int)newState);
        }
    }

    public void TakeDamage(float damage)
    {
        if (health.isInvincible) return;
        health.currentHealth -= damage;
        GameManager.Instance.SetPlayerSlider(health.currentHealth);
        animator.SetTrigger("TakeDamage");  // lancia la transizione
        if (health.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        UIManager.instance.ShowUI(UIManager.GameUI.Lose);
    }

    public void GainHealth(float hpGained)
    {
        health.currentHealth += hpGained;
        if (health.currentHealth > health.maxHealth)
        {
            health.currentHealth = health.maxHealth;
        }
    }

    protected abstract void PerformAction();
}


public enum AnimatorStates
{
    Idle,
    Movement,
    Jump,
    Fall,
    TakeDamage,
}

[System.Serializable]
public struct Health
{
    public float maxHealth;
    public float currentHealth;
    public bool isInvincible;
}
