using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class Character : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] protected float speed;
    protected Health health;

    protected Rigidbody2D rigidBody2D;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected Vector2 _Input;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        health.currentHealth = health.maxHealth;
        health.isInvincible = false;
    }

    protected void Move()
    {
        rigidBody2D.velocity = new Vector2(_Input.x * speed, rigidBody2D.velocity.y);
    }

    protected void TakeDamage(float damage)
    {
        if (health.isInvincible) return;

        health.currentHealth -= damage;
        if (health.currentHealth <= 0)
        {
            print("GameOver");
        }
    }

    protected void GainHealth(float hpGained)
    {
        health.currentHealth += hpGained;
        if (health.currentHealth > health.maxHealth)
        {
            health.currentHealth = health.maxHealth;
        }
    }

    protected abstract void PerformAction();
}
