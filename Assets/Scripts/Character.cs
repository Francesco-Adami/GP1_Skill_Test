using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class Character : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] protected CharacterData characterData;
    [SerializeField] protected Health health;

    protected Rigidbody2D rigidBody2D;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected AnimatorStates animationState;

    protected Vector2 _Input;

    [System.Serializable]
    public struct Health
    {
        public float maxHealth;
        public float currentHealth;
        public bool isInvincible;
    }

    private void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        animationState = AnimatorStates.Idle;
        spriteRenderer.sprite = characterData.sprite;
        health.maxHealth = characterData.maxHealth;
        health.currentHealth = health.maxHealth;
        health.isInvincible = false;
    }

    protected void Move()
    {
        if (_Input.x == 0)
        {
            SetAnimatorState(AnimatorStates.Idle);
            return;
        }

        rigidBody2D.velocity = new Vector2(_Input.x * characterData.speed, rigidBody2D.velocity.y);
        SetAnimatorState(AnimatorStates.Movement);
    }

    public void TakeDamage(float damage)
    {
        if (health.isInvincible) return;

        health.currentHealth -= damage;
        if (health.currentHealth <= 0)
        {
            print("GameOver");
        }
    }

    public void GainHealth(float hpGained)
    {
        health.currentHealth += hpGained;
        if (health.currentHealth > health.maxHealth)
        {
            health.currentHealth = health.maxHealth;
        }
    }

    protected void SetAnimatorState(AnimatorStates state)
    {
        animationState = state;
        switch (state)
        {
            case AnimatorStates.Idle:
                animator.SetBool("Idle", true);
                animator.SetBool("Movement", false);
                animator.SetBool("Jump", false);
                animator.SetBool("Attack", false);
                break;
            case AnimatorStates.Movement:
                animator.SetBool("Idle", false);
                animator.SetBool("Movement", true);
                animator.SetBool("Jump", false);
                animator.SetBool("Attack", false);
                break;
            case AnimatorStates.Jump:
                animator.SetBool("Idle", false);
                animator.SetBool("Movement", false);
                animator.SetBool("Jump", true);
                animator.SetBool("Attack", false);
                break;
            case AnimatorStates.Attack:
                animator.SetBool("Idle", false);
                animator.SetBool("Movement", false);
                animator.SetBool("Jump", false);
                animator.SetBool("Attack", true);
                break;
        }
    }

    protected abstract void PerformAction();
}


public enum AnimatorStates
{
    Idle,
    Movement,
    Jump,
    Attack,
}