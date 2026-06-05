using System.Collections;
using UnityEngine;

/// <summary>
/// Squirrel boss — center-pivot sprite, dynamic gravity, Bowser patrol, throw/charge cycle.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SquirrelBossAI : MonoBehaviour
{
    private enum BossAttack { Charge, Throw }

    // Random patrol between attacks: walk 1–3 s, pause briefly, repeat.
    private enum PatrolState { Walking, Pausing }

    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int IsThrowingHash = Animator.StringToHash("isThrowing");
    private static readonly int JumpStateHash = Animator.StringToHash("Jump");

    [Header("References")]
    public Transform playerTransform;
    public Transform throwPoint;
    public Transform feetPosition;
    public BossAcornProjectile acornPrefab;
    public Sprite[] acornSprites;

    [Header("Grounding (center-pivot sprite)")]
    [Tooltip("Resize the solid collider to match the drawn sprite so the visible feet rest on the floor, not the collider center.")]
    public bool autoFitColliderToSprite = true;
    [Tooltip("How wide the body collider is relative to the sprite width.")]
    [Range(0.3f, 1f)] public float colliderWidthScale = 0.7f;
    [Tooltip("Extra vertical nudge after settling on the floor. Negative sinks the boss, positive raises it.")]
    public float groundOffset = 0f;

    [Header("Movement")]
    public float patrolSpeed = 0.55f;
    public float chargeSpeed = 1.6f;
    public float attackRange = 8f;
    public float leftBound = -20f;
    public float rightBound = 20f;
    public float patrolWalkMin = 1f;
    public float patrolWalkMax = 3f;
    public float patrolPauseMin = 0.35f;
    public float patrolPauseMax = 0.75f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Attacks")]
    public float chargeDuration = 2f;
    public float attackCyclePause = 1.5f;
    public float throwWindup = 0.35f;
    public float throwAnimDuration = 0.9f;
    public float acornSpeed = 7f;
    [Tooltip("Random launch spread in degrees — gives a dodge window without making shots miss entirely.")]
    public float acornLaunchSpread = 12f;
    public int throwsBeforeCharge = 3;

    [Header("Debug")]
    public bool debugPhysics;

    private Rigidbody2D rb;
    private CapsuleCollider2D solidCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BossHealth health;
    private MonsterDamage contactDamage;

    private int facingDirection = 1;
    private bool isGrounded;
    private bool isCharging;
    private bool isThrowing;
    private float chargeEndTime;
    private float attackReadyTime;
    private float chargeTargetX;
    private BossAttack nextAttack = BossAttack.Throw;
    private int throwsSinceCharge;

    private PatrolState patrolState = PatrolState.Pausing;
    private float patrolStateEndTime;
    private int patrolDirection = 1;
    private bool patrolActive;

    private ContactFilter2D groundContactFilter;
    private readonly RaycastHit2D[] groundCastBuffer = new RaycastHit2D[4];

    public bool IsCharging() => isCharging;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        solidCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        health = GetComponent<BossHealth>();
        contactDamage = GetComponent<MonsterDamage>();

        groundContactFilter = new ContactFilter2D();
        groundContactFilter.useTriggers = false;
    }

    void Start()
    {
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");

        groundContactFilter.SetLayerMask(groundLayer);

        ResolvePlayer();

        if (throwPoint == null)
            throwPoint = transform.Find("ThrowPoint");

        if (feetPosition == null)
            feetPosition = transform.Find("FeetPosition");

        FitColliderToSprite();
        SyncFeetPositionToSprite();
        InitializePhysics();
        SetContactDamage(false);

        attackReadyTime = Time.time + 1f;
        BeginPatrolPause();

        StartCoroutine(SettleOnGroundRoutine());
    }

    void FixedUpdate()
    {
        if (health != null && health.isDead)
            return;

        EnsurePhysicsActive();

        if (!isCharging)
            SetContactDamage(false);

        isGrounded = CheckGrounded();
        ApplyGroundStick();

        if (isCharging)
            UpdateChargePhysics();
        else if (isThrowing || (health != null && (health.isHurt || health.isInvincible)))
            StopHorizontal();
        else if (patrolActive && patrolState == PatrolState.Walking)
            ApplyPatrolMovement();
        else
            StopHorizontal();

        AlignVisualFeetToGround();
    }

    void Update()
    {
        if (health != null && health.isDead)
            return;

        if (debugPhysics && rb != null)
            Debug.Log($"Velocity: {rb.linearVelocity}, Position: {transform.position}, GravityScale: {rb.gravityScale}, Grounded: {isGrounded}");

        ResolvePlayer();
        FacePlayer();

        if (playerTransform == null || health == null)
        {
            patrolActive = false;
            SyncAnimatorBools();
            return;
        }

        if (isThrowing)
        {
            patrolActive = false;
            SyncAnimatorBools();
            return;
        }

        if (health.isHurt || health.isInvincible)
        {
            patrolActive = false;
            SyncAnimatorBools();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool playerInRange = distanceToPlayer <= attackRange;

        if (isCharging)
        {
            patrolActive = false;
            SyncAnimatorBools();
            return;
        }

        if (!playerInRange)
        {
            patrolActive = false;
            SyncAnimatorBools();
            return;
        }

        if (Time.time >= attackReadyTime)
        {
            patrolActive = false;
            BeginAttack();
        }
        else
        {
            patrolActive = true;
            UpdatePatrolStateMachine();
        }

        SyncAnimatorBools();
    }

    public void OnDamaged()
    {
        isCharging = false;
        isThrowing = false;
        patrolActive = false;
        StopHorizontal();
        SetContactDamage(false);

        if (animator != null)
            animator.SetBool(IsRunningHash, false);

        attackReadyTime = Time.time + attackCyclePause;
        BeginPatrolPause();
    }

    // -------------------------------------------------------------------------
    // Physics — dynamic gravity, drop to floor, never cancel velocity.y
    // -------------------------------------------------------------------------

    void InitializePhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.WakeUp();
    }

    void EnsurePhysicsActive()
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic)
            rb.bodyType = RigidbodyType2D.Dynamic;

        if (rb.gravityScale <= 0f)
            rb.gravityScale = 1f;

        if (!rb.simulated)
            rb.simulated = true;
    }

    // Match the solid collider to the drawn sprite so the visible bottom is what touches the floor.
    void FitColliderToSprite()
    {
        if (!autoFitColliderToSprite || solidCollider == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Bounds b = spriteRenderer.sprite.bounds;
        float width = Mathf.Abs(b.size.x) * Mathf.Clamp01(colliderWidthScale);
        float height = Mathf.Abs(b.size.y);

        if (width <= 0f || height <= 0f)
            return;

        solidCollider.direction = CapsuleDirection2D.Vertical;
        solidCollider.size = new Vector2(width, height);
        solidCollider.offset = new Vector2(b.center.x, b.center.y);
    }

    IEnumerator SettleOnGroundRoutine()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        SettleOnGround();
    }

    void SettleOnGround()
    {
        if (solidCollider == null)
            return;

        RaycastHit2D hit = FindGroundBelow(40f);
        if (hit.collider == null)
            return;

        Bounds bounds = solidCollider.bounds;
        float bottomY = bounds.min.y;
        float deltaY = (hit.point.y - bottomY) + groundOffset;

        if (Mathf.Abs(deltaY) < 0.001f)
            return;

        rb.position = new Vector2(rb.position.x, rb.position.y + deltaY);
        rb.linearVelocity = Vector2.zero;
        rb.WakeUp();
    }

    void ApplyGroundStick()
    {
        if (!isGrounded)
            return;

        if (rb.linearVelocity.y < 0f)
        {
            // Preserve X velocity so horizontal movement continues; only clamp downward sink.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    // Snap only Y to the floor; always restore horizontal velocity so patrol/charge movement is not cancelled.
    void AlignVisualFeetToGround()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        if (!isGrounded && !HasGroundNearby())
            return;

        Bounds spriteBounds = spriteRenderer.bounds;
        Vector2 probe = new Vector2(spriteBounds.center.x, spriteBounds.min.y + 0.02f);
        RaycastHit2D hit = Physics2D.Raycast(probe, Vector2.down, 0.8f, groundLayer);

        if (hit.collider == null)
            return;

        float deltaY = (hit.point.y + groundOffset) - spriteBounds.min.y;
        if (Mathf.Abs(deltaY) < 0.001f)
            return;

        Vector2 velocity = rb.linearVelocity;
        rb.position = new Vector2(rb.position.x, rb.position.y + deltaY);
        rb.linearVelocity = velocity;
    }

    void SyncFeetPositionToSprite()
    {
        if (feetPosition == null)
            return;

        float feetLocalY = solidCollider != null
            ? solidCollider.offset.y - solidCollider.size.y * 0.5f
            : (spriteRenderer != null ? -spriteRenderer.sprite.bounds.size.y * 0.5f : -0.41f);

        feetPosition.localPosition = new Vector3(0f, feetLocalY, 0f);
    }

    Vector2 GetFeetWorldPosition()
    {
        if (solidCollider != null)
            return new Vector2(solidCollider.bounds.center.x, solidCollider.bounds.min.y);

        if (feetPosition != null)
            return feetPosition.position;

        float halfHeight = spriteRenderer != null && spriteRenderer.sprite != null
            ? spriteRenderer.sprite.bounds.size.y * 0.5f
            : 0.41f;
        return (Vector2)transform.position + new Vector2(0f, -halfHeight);
    }

    void MoveHorizontal(float directionX, float speed)
    {
        // Preserve Y velocity so gravity is never cancelled.
        rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);
    }

    void StopHorizontal()
    {
        // Preserve Y velocity so gravity is never cancelled.
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    RaycastHit2D FindGroundBelow(float maxDistance)
    {
        if (solidCollider == null)
            return default;

        Bounds bounds = solidCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.max.y + 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxDistance, groundLayer);

        if (hit.collider != null)
            return hit;

        hit = Physics2D.Raycast(origin, Vector2.down, maxDistance);
        if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject != gameObject)
            return hit;

        return default;
    }

    bool CheckGrounded()
    {
        if (solidCollider != null && solidCollider.IsTouching(groundContactFilter))
            return true;

        if (rb.IsTouchingLayers(groundLayer))
            return true;

        if (solidCollider != null)
        {
            int hitCount = solidCollider.Cast(Vector2.down, groundContactFilter, groundCastBuffer, groundCheckDistance);
            if (hitCount > 0)
                return true;
        }

        Bounds bounds = solidCollider != null ? solidCollider.bounds : new Bounds(transform.position, Vector3.one * 0.5f);
        float probeY = bounds.min.y + 0.01f;
        float distance = groundCheckDistance + 0.15f;

        Vector2[] probes =
        {
            new Vector2(bounds.center.x, probeY),
            new Vector2(bounds.min.x + 0.08f, probeY),
            new Vector2(bounds.max.x - 0.08f, probeY)
        };

        foreach (Vector2 probe in probes)
        {
            if (Physics2D.Raycast(probe, Vector2.down, distance, groundLayer))
                return true;
        }

        return false;
    }

    bool HasGroundNearby()
    {
        return CheckGrounded() || FindGroundBelow(groundCheckDistance + 0.3f).collider != null;
    }

    // -------------------------------------------------------------------------
    // Random patrol state machine
    // -------------------------------------------------------------------------

    void UpdatePatrolStateMachine()
    {
        if (Time.time < patrolStateEndTime)
            return;

        if (patrolState == PatrolState.Walking)
            BeginPatrolPause();
        else
            BeginPatrolWalk();
    }

    void BeginPatrolWalk()
    {
        patrolState = PatrolState.Walking;
        patrolDirection = Random.value > 0.5f ? 1 : -1;
        patrolStateEndTime = Time.time + Random.Range(patrolWalkMin, patrolWalkMax);
    }

    void BeginPatrolPause()
    {
        patrolState = PatrolState.Pausing;
        patrolStateEndTime = Time.time + Random.Range(patrolPauseMin, patrolPauseMax);
    }

    void ApplyPatrolMovement()
    {
        float x = transform.position.x;

        if (x <= leftBound && patrolDirection < 0)
            patrolDirection = 1;

        if (x >= rightBound && patrolDirection > 0)
            patrolDirection = -1;

        // Preserve Y velocity so gravity is never cancelled.
        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);
    }

    // -------------------------------------------------------------------------
    // Attacks — 3 throws, then charge (unchanged cycle)
    // -------------------------------------------------------------------------

    void BeginAttack()
    {
        if (nextAttack == BossAttack.Charge)
            BeginCharge();
        else
            StartCoroutine(ThrowRoutine());
    }

    void BeginCharge()
    {
        isCharging = true;
        patrolActive = false;
        chargeEndTime = Time.time + chargeDuration;
        chargeTargetX = playerTransform.position.x;
        SetContactDamage(true);

        if (animator != null)
        {
            animator.SetBool(IsRunningHash, false);
            animator.SetBool(IsGroundedHash, false);
            animator.Play(JumpStateHash, 0, 0f);
        }
    }

    void UpdateChargePhysics()
    {
        if (Time.time >= chargeEndTime)
        {
            EndCharge();
            return;
        }

        float dir = Mathf.Sign(chargeTargetX - transform.position.x);
        if (Mathf.Abs(chargeTargetX - transform.position.x) < 0.1f)
            dir = facingDirection;

        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            animator.Play(JumpStateHash, 0, 0f);

        // Preserve Y velocity so gravity is never cancelled.
        MoveHorizontal(dir, chargeSpeed);
    }

    void EndCharge()
    {
        isCharging = false;
        StopHorizontal();
        SetContactDamage(false);

        if (animator != null)
            animator.SetBool(IsRunningHash, false);

        attackReadyTime = Time.time + attackCyclePause;
        throwsSinceCharge = 0;
        nextAttack = BossAttack.Throw;
        BeginPatrolPause();
    }

    IEnumerator ThrowRoutine()
    {
        isThrowing = true;
        isCharging = false;
        patrolActive = false;
        SetContactDamage(false);
        StopHorizontal();

        if (animator != null)
            animator.SetBool(IsRunningHash, false);

        if (animator != null)
            animator.SetTrigger(IsThrowingHash);

        yield return new WaitForSeconds(throwWindup);
        SpawnAcorn();
        yield return new WaitForSeconds(Mathf.Max(0f, throwAnimDuration - throwWindup));

        isThrowing = false;
        attackReadyTime = Time.time + attackCyclePause;

        throwsSinceCharge++;
        nextAttack = throwsSinceCharge >= throwsBeforeCharge ? BossAttack.Charge : BossAttack.Throw;
        BeginPatrolPause();
    }

    void SpawnAcorn()
    {
        if (acornPrefab == null)
            return;

        Vector2 origin = throwPoint != null
            ? (Vector2)throwPoint.position
            : (Vector2)transform.position + new Vector2(0.48f * facingDirection, 0.55f);

        Sprite acornSprite = null;
        if (acornSprites != null && acornSprites.Length > 0)
            acornSprite = acornSprites[Random.Range(0, acornSprites.Length)];

        Vector2 launchDirection = new Vector2(facingDirection, 0.05f).normalized;
        if (playerTransform != null)
        {
            Vector2 aimPoint = GetPlayerAimPoint();
            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                aimPoint += playerRb.linearVelocity * 0.12f;

            Vector2 toPlayer = aimPoint - origin;
            if (toPlayer.sqrMagnitude > 0.01f)
                launchDirection = toPlayer.normalized;
        }

        if (acornLaunchSpread > 0f)
            launchDirection = ApplyLaunchSpread(launchDirection, acornLaunchSpread);

        BossAcornProjectile acorn = Instantiate(acornPrefab, origin, Quaternion.identity);
        acorn.Launch(launchDirection * acornSpeed, acornSprite, playerTransform);
    }

    void SetContactDamage(bool enabled)
    {
        if (contactDamage == null)
            return;

        contactDamage.enabled = enabled && isCharging;
    }

    bool ShouldRun()
    {
        if (isThrowing || isCharging)
            return false;

        if (health != null && (health.isHurt || health.isInvincible))
            return false;

        return patrolActive && patrolState == PatrolState.Walking;
    }

    void SyncAnimatorBools()
    {
        if (animator == null)
            return;

        if (isCharging)
        {
            animator.SetBool(IsGroundedHash, false);
            animator.SetBool(IsRunningHash, false);
            return;
        }

        animator.SetBool(IsGroundedHash, isGrounded || HasGroundNearby());
        animator.SetBool(IsRunningHash, ShouldRun());
    }

    Vector2 GetPlayerAimPoint()
    {
        if (playerTransform == null)
            return Vector2.zero;

        Collider2D col = playerTransform.GetComponent<Collider2D>();
        if (col == null)
            col = playerTransform.GetComponentInChildren<Collider2D>();

        if (col != null)
            return col.bounds.center;

        return playerTransform.position;
    }

    static Vector2 ApplyLaunchSpread(Vector2 direction, float maxSpreadDegrees)
    {
        if (direction.sqrMagnitude < 0.01f)
            return Vector2.right;

        direction.Normalize();
        float spread = Random.Range(-maxSpreadDegrees, maxSpreadDegrees) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(spread);
        float sin = Mathf.Sin(spread);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    void ResolvePlayer()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void FacePlayer()
    {
        if (playerTransform == null)
            return;

        if (Mathf.Abs(playerTransform.position.x - transform.position.x) < 0.05f)
            return;

        facingDirection = playerTransform.position.x > transform.position.x ? 1 : -1;
        ApplyFacing();
    }

    void ApplyFacing()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = facingDirection < 0;

        if (throwPoint != null)
        {
            Vector3 local = throwPoint.localPosition;
            local.x = Mathf.Abs(local.x) * facingDirection;
            throwPoint.localPosition = local;
        }
    }
}
