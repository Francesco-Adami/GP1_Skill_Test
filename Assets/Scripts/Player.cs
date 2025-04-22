using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [Header("Player Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float extraHeight = 0.1f;
    [SerializeField] private Collider2D col;

    [Header("Debug Variables")]
    [SerializeField] private bool isGrounded;

    private void Update()
    {
        _Input.x = Input.GetAxisRaw("Horizontal");
        _Input.y = Input.GetButtonDown("Jump") ? jumpForce : 0;

        Move();
        CheckGroundAndJump();
        Interact();
    }

    private void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformAction();
        }
    }

    public void CheckGroundAndJump()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + extraHeight, groundLayer);
        isGrounded = rayHit.collider != null;

        if (!isGrounded) return;
        if (_Input.y > 0)
        {
            rigidBody2D.AddForce(new Vector2(0, _Input.y), ForceMode2D.Impulse);
            isGrounded = false;
        }
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

}
