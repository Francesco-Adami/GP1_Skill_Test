using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
}

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Character
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveDistance = 5f;   // ampiezza patrol
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Debug")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;

    private Rigidbody2D rb2d;
    private Vector2 initialPos;
    private Vector2 targetPos;
    private bool movingRight = true;
    private Transform player;
    private float attackTimer = 0f;

    protected override void Start()
    {
        base.Start();
        rb2d = GetComponent<Rigidbody2D>();
        initialPos = rb2d.position;
        SetNextPatrolTarget();
        attackTimer = attackCooldown;
    }

    private void Update()
    {
        if (UIManager.instance.GetCurrentActiveUI() != UIManager.GameUI.HUD) return;


        if (currentState == EnemyState.Attack)
        {
            AttackPlayer();
        }


    }

    private void FixedUpdate()
    {
        if (UIManager.instance.GetCurrentActiveUI() != UIManager.GameUI.HUD) return;

        // Rilevamento player  
        DetectPlayer();

        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase: ChasePlayer(); break;
                // Attack non si muove
        }
    }

    private void DetectPlayer()
    {
        var hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            LayerMask.GetMask("Player")
        );
        if (hits.Length > 0)
        {
            player = hits[0].transform;
            currentState = EnemyState.Chase;
        }
        else if (currentState != EnemyState.Attack)
        {
            player = null;
            currentState = EnemyState.Patrol;
        }
    }

    #region Patrol
    private void Patrol()
    {
        // se arrivato al target o ostacolato, inverti direzione
        if (IsBlocked() || Vector2.Distance(rb2d.position, targetPos) < 0.1f)
        {
            movingRight = !movingRight;
            SetNextPatrolTarget();
        }

        Vector2 dir = (targetPos - rb2d.position).normalized;
        TryMove(dir);
    }

    private void SetNextPatrolTarget()
    {
        float dir = movingRight ? +1f : -1f;
        targetPos = initialPos + Vector2.right * moveDistance * dir;
    }
    #endregion

    #region Chase
    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 chaseDir = (player.position - transform.position).normalized;

        // se entro in attackRange, passo ad Attack
        if (Vector2.Distance(rb2d.position, player.position) <= attackRange * 2)
        {
            currentState = EnemyState.Attack;
            return;
        }

        TryMove(chaseDir);
    }
    #endregion

    #region Attack
    private void AttackPlayer()
    {
        print("entro in attack");
        if (player == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // Countdown attacco
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            print("Attacco nemico!");
            PerformAction();
            attackTimer = attackCooldown;
        }

        // se il player scappa, torno a inseguire
        if (Vector2.Distance(rb2d.position, player.position) > attackRange)
        {
            currentState = EnemyState.Chase;
        }

        // per animazioni ferma input orizzontale
        _Input.x = 0f;
        EvaluateAnimationState();
    }
    #endregion

    /// <summary>
    /// Prova a muovere il rigidbody in dir, verificando ostacoli orizzontali.
    /// Imposta anche _Input.x e aggiorna le animazioni.
    /// </summary>
    private void TryMove(Vector2 dir)
    {
        // controllo ostacolo orizzontale
        RaycastHit2D hit = Physics2D.Raycast(
            rb2d.position,
            new Vector2(dir.x, 0f),
            0.6f
        );
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            dir.x = 0f;
        }

        Vector2 newPos = rb2d.position + dir * characterData.speed * Time.fixedDeltaTime;
        rb2d.MovePosition(newPos);

        // setto input per animazioni
        _Input.x = dir.x;
        EvaluateAnimationState();
    }

    /// <summary>
    /// Spara l'azione base (danno al player).
    /// </summary>
    protected override void PerformAction()
    {
        if (player != null)
        {
            player.GetComponent<Player>().TakeDamage(characterData.damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private bool IsBlocked()
    {
        // raycast in direzione patrol per muro
        Vector2 dir = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, dir, 0.5f);
        return hit.collider != null && hit.collider.gameObject != gameObject;
    }
}
