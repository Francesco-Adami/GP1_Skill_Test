﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [Header("Player Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private Collider2D col;
    [SerializeField] private float jumpHeight = 2f;      // Altezza massima del salto
    [SerializeField] private float jumpDuration = 0.5f;  // Durata totale del salto

    [Header("Debug Variables")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool jumpPressed;

    private Vector2 startingPos;

    protected override void Start()
    {
        base.Start();
        startingPos = transform.position;
    }

    private void Update()
    {
        if (UIManager.instance.GetCurrentActiveUI() != UIManager.GameUI.HUD) return;

        _Input.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }

        CheckIfIsOnMap();

        //CheckDirection();
        EvaluateAnimationState();
        Interact();

        Shoot();
    }

    private void FixedUpdate()
    {
        Move();
        if (jumpPressed)
        {
            CheckGroundAndJump();
            jumpPressed = false;
        }
    }

    private void CheckIfIsOnMap()
    {
        if (transform.position.y < -7)
        {
            transform.position = startingPos;
            health.currentHealth = health.maxHealth;
        }
    }

    private void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformAction();
        }
    }

    private void CheckDirection()
    {
        if (_Input.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (_Input.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }


    private bool isJumping = false;

    public void CheckGroundAndJump()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(
            col.bounds.center,
            Vector2.down,
            col.bounds.extents.y + groundCheckDist,
            groundLayer
        );
        isGrounded = rayHit.collider != null;

        /* PUNTO 5 - JUMP con ADD FORCE
        if (jumpPressed)
        {
            jumpPressed = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            rigidBody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            isGrounded = false;
        }
        */


        // Se siamo già in aria, niente controllo di salto
        if (!isGrounded && !isJumping)
        {
            jumpPressed = true;
            return;
        }

        // Reset del flag quando torniamo a terra
        if (isGrounded && jumpPressed)
        {
            jumpPressed = false;
        }

        // Se premo “Jump” e sono a terra, avvio la coroutine di salto
        if (isGrounded && !isJumping)
        {
            StartCoroutine(JumpArc());
        }
    }

    private IEnumerator JumpArc()
    {
        isJumping = true;

        // 1) Disabilita la gravità in modo che sia solo la nostra curva a controllare l'altezza
        float originalGravity = rigidBody2D.gravityScale;
        rigidBody2D.gravityScale = 0f;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            // t da 0 a π
            float t = Mathf.PI * (elapsed / jumpDuration);

            // Altezza = sin(t) * jumpHeight
            float yOffset = Mathf.Sin(t) * jumpHeight;

            // Velocità verticale = derivata di sin = cos(t) * (π/jumpDuration) * jumpHeight
            float yVelocity = Mathf.Cos(t) * (Mathf.PI / jumpDuration) * jumpHeight;

            // Mantieni la velocità orizzontale (Move() in Update imposta rigidBody2D.velocity.x)
            float xVelocity = rigidBody2D.velocity.x;

            rigidBody2D.velocity = new Vector2(xVelocity, yVelocity);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2) Riattiva la gravità e lascia che la fisica faccia il “resto” (atterraggio, collisioni)
        rigidBody2D.gravityScale = originalGravity;
        isJumping = false;
    }



    protected override void PerformAction()
    {
        float actionRadius = 1f;

        LayerMask enemyMask = LayerMask.GetMask("Enemy");
        LayerMask pickupMask = LayerMask.GetMask("Pickup");

        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(transform.position, actionRadius, enemyMask);
        if (enemyColliders.Length > 0)
        {
            //Enemy enemy = enemyColliders[0].GetComponent<Enemy>();
            //if (enemy != null)
            //{
            //    int attackDamage = 10; // Puoi definire il danno come preferisci
            //    enemy.TakeDamage(attackDamage);
            //    Debug.Log("Attacco eseguito: nemico colpito!");
            //}
            return;
        }

        // Se non ci sono nemici, cerca un oggetto da raccogliere
        Collider2D pickupCollider = Physics2D.OverlapCircle(transform.position, actionRadius, pickupMask);
        if (pickupCollider != null)
        {
            //Pickup pickup = pickupCollider.GetComponent<Pickup>();
            //if (pickup != null)
            //{
            //    pickup.OnPickup();
            //    Debug.Log("Oggetto raccolto!");
            //}
            return;
        }

        Debug.Log("Nessuna azione eseguita: non c'è nemico o oggetto raccoglibile nelle vicinanze.");
    }

    internal void StartInvincibleRoutine(float value)
    {
        health.isInvincible = true;
        spriteRenderer.color = Color.yellow;
        StartCoroutine(InvincibleRoutine(value));
    }

    private IEnumerator InvincibleRoutine(float value)
    {
        yield return new WaitForSeconds(value);
        health.isInvincible = false;
        spriteRenderer.color = Color.white;
    }

    // PUNTO 25 - Shooting with dot product
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    // Raggio di ricerca nemici per mirare
    [SerializeField] private float detectionRadius = 5f;
    // Soglia del dot product per definire il cono di mira (cos(45°) ≈ 0.707)
    [SerializeField, Range(0f, 1f)] private float aimDotThreshold = 0.7f;

    private void Shoot()
    {
        if (!Input.GetButtonDown("Fire1") && !Input.GetKeyDown(KeyCode.E))
            return;

        Vector2 facingDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        LayerMask enemyMask = LayerMask.GetMask("Enemy");
        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, detectionRadius, enemyMask);

        Vector2 shootDir = facingDir;
        float bestDot = -1f;
        Vector2 bestTargetDir = Vector2.zero;

        // Trova il nemico con il dot product più alto (più allineato con facingDir)
        foreach (var c in hits)
        {
            Vector2 toTarget = ((Vector2)c.transform.position - (Vector2)firePoint.position).normalized;
            float dp = Vector2.Dot(facingDir, toTarget);
            if (dp > bestDot)
            {
                bestDot = dp;
                bestTargetDir = toTarget;
            }
        }

        // Se il miglior dot product supera la soglia, miramo quel nemico
        if (bestDot >= aimDotThreshold)
        {
            shootDir = bestTargetDir;
        }

        // Istanzia e lancia il proiettile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        if (proj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = shootDir * projectileSpeed;
        }
    }

}
