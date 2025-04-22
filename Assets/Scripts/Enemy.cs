using System;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
}

public class Enemy : Character
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveDistance = 5f;   // Distanza da percorrere a destra e a sinistra
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 1f; // Tempo di attesa tra gli attacchi

    [Header("DEBUG VARIABLES")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;
    [SerializeField] private Player player;
    [SerializeField] private float attackTimer; // Velocità di movimento

    private Rigidbody2D rb2d;

    // PATROL 
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool movingRight = true;

    // Ottimizzazione controllo collider: esegue il raycast a intervalli regolari
    private float colliderCheckTimer = 0f;
    private float colliderCheckInterval = 0.2f; // Controllo ogni 0.2 secondi

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        currentState = EnemyState.Chase;
        initialPosition = transform.position;
        targetPosition = initialPosition + new Vector3(moveDistance, 0, 0);
    }

    // Il rilevamento del giocatore rimane in Update
    void Update()
    {
        if (currentState == EnemyState.Attack)
        {
            AttackPlayer();
            return;
        }

        CheckingForPlayer();
    }

    // La movimentazione viene gestita in FixedUpdate per interagire con la fisica
    void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
        }
    }

    #region PATROL
    private void CheckingForPlayer()
    {
        LayerMask playerMask = LayerMask.GetMask("Player");
        Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerMask);
        if (playerColliders.Length > 0)
        {
            currentState = EnemyState.Chase;
            player = playerColliders[0].GetComponent<Player>();
        }
        else
        {
            // Passa allo stato Patrol soltanto se non si sta attaccando
            if (currentState != EnemyState.Attack)
            {
                currentState = EnemyState.Patrol;
            }
            player = null;
        }
    }

    protected void Patrol()
    {
        colliderCheckTimer += Time.fixedDeltaTime;
        // Esegue il controllo collisione solo a intervalli definiti
        if (colliderCheckTimer >= colliderCheckInterval)
        {
            if (CheckCollider())
            {
                colliderCheckTimer = 0f;
                return; // Skip il movimento se viene rilevato un ostacolo
            }
            colliderCheckTimer = 0f;
        }

        Vector3 newPos = Vector3.MoveTowards(rb2d.position, targetPosition, characterData.speed * Time.fixedDeltaTime);
        rb2d.MovePosition(newPos);

        Debug.Log("targetPosition: " + targetPosition);
        Debug.Log("IF: " + (Vector3.Distance(rb2d.position, targetPosition) <= 0.01f));

        // Quando il target viene raggiunto, inverte la direzione
        if (Mathf.Abs(rb2d.position.x - targetPosition.x) <= 0.01f)
        {
            if (movingRight)
            {
                targetPosition = initialPosition - new Vector3(moveDistance, 0, 0);
                movingRight = false;
            }
            else
            {
                targetPosition = initialPosition + new Vector3(moveDistance, 0, 0);
                movingRight = true;
            }
        }
    }

    private bool CheckCollider()
    {
        float rayDistance = 1f;
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, direction, rayDistance);

        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            if (movingRight)
            {
                targetPosition = initialPosition - new Vector3(moveDistance, 0, 0);
                movingRight = false;
            }
            else
            {
                targetPosition = initialPosition + new Vector3(moveDistance, 0, 0);
                movingRight = true;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region CHASE
    private void ChasePlayer()
    {
        if (player == null) return;

        Vector3 direction = player.transform.position - transform.position;
        direction.Normalize();

        Vector3 newPos = rb2d.position + (Vector2)(direction * characterData.speed * Time.fixedDeltaTime);
        rb2d.MovePosition(newPos);

        Debug.LogWarning("CHASE: " + (Vector3.Distance(rb2d.position, player.transform.position) < attackRange));
        // Se il giocatore è abbastanza vicino, passa allo stato ATTACK
        if (Vector3.Distance(rb2d.position, player.transform.position) < attackRange)
        {
            currentState = EnemyState.Attack;
        }
    }
    #endregion

    #region ATTACK
    private void AttackPlayer()
    {
        Debug.Log("Player == null? " + player == null);
        if (player == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            PerformAction();
            attackTimer = attackCooldown;
        }

        if (Vector3.Distance(rb2d.position, player.transform.position) >= attackRange)
        {
            currentState = EnemyState.Chase;
        }
    }
    #endregion  

    protected override void PerformAction()
    {
        Debug.Log("Attacco il Player");
        player.GetComponent<Player>().TakeDamage(1f);
    }
}
