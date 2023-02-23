using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Unlocking abilities
    private bool doubleJumpUnlocked = false;
    private bool dashUnlocked = false;
    private bool shockwaveUnlocked = false;

    // Unlcoking locked doors
    public bool lock1 = true;

    public enum abilities
    {
        doubleJump,
        dash,
        shockwave
    }

    public float maxSpeed = 10;
    public Vector2 jumpForce = new Vector2(0, 10);
    public float wallForce = 10;
    public float jumpSpeedBoost = 5;
    public float accel = 90;

    public float deaccel = 50;

    public float aerialDeaccel = 10.0f;
    public float detectionOffsetY = 0.5f;
    public float detectionOffsetX = 0.3f;
    public float fallModifier = 2.5f;
    public float gravity = 5;
    public float groundedMargin = 0.1f;
    public float maxFallSpeed = -10;
    public float lowJumpModifier = 2f;

    private bool dashed;
    private bool canMove;
    private bool wallJumping;
    private bool grabbingWall;

    private Rigidbody2D body;
    private LayerMask groundLayer;
    private List<Vector3> positions;
    private bool canWallJump;
    private int face { get; set; }
    private LayerMask enemyLayer;

    // Coyote
    private float lastGrounded;
    public float coyoteTime;
    private bool hasCoyoted = false;

    // Jump margin
    private bool queuedJump = false;
    private bool canQueueJump = false;
    public float queueOffset = 1.5f;
    public float queableJumpMargin = 1.5f;

    private bool jumped;
    private bool checkTime = true;
    private bool canDoubleJump;
    public float dashTime = 0.5f;

    public GameObject projectilePrefab;
    public GameObject doubleJumpPrefab;
    public GameObject shockwavePrefab;
    public GameObject spellPrefab;
    private GameObject platform;

    private Animator Ub;
    private SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        positions = new List<Vector3>();
        Ub = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        jumped = false;
        cameraTransform = GameObject.Find("Canvas").transform;
        showHearts();
    }

    // Update is called once per frame
    void Update()
    {
        // clearHearts();
        // showHearts();
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal < 0)
            face = -1;

        Dash();
        Groundtime();
        jumpQueable();
        Jump();
        Gravity();
        Ub.SetBool("Grounded", grounded());
        Ub.SetFloat("Vy", body.velocity.y);
        if (dashing || attacking)
            return;
        Walk();
        Attack();
        Ub.SetBool("Grounded", grounded());
        Ub.SetFloat("Vy", body.velocity.y);
    }

    public void Unlocking(abilities ability)
    {
        switch (ability)
        {
            case abilities.doubleJump:
                doubleJumpUnlocked = true;
                break;
            case abilities.dash:
                dashUnlocked = true;
                break;
            case abilities.shockwave:
                shockwaveUnlocked = true;
                break;
        }
    }

#region UI
    public GameObject heart;
    private float x_offset = -2;
    private float y_offset = -4;
    private float heart_width = 1;
    private GameObject[] hearts = new GameObject[5];
    public Transform cameraTransform;
    public GameObject heart_percent;

    public void showHearts()
    {
        float offset = -40 * (hp - 100) / 100;
        heart_percent.GetComponent<RectTransform>().localPosition = new Vector2(0, offset);
    }

    public void clearHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                Destroy(hearts[i]);
            }
        }
    }
#endregion

#region Movement
    private void Walk()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal < 0)
        {
            if (!Ub.GetBool("Moving"))
            {
                Ub.SetBool("Moving", true);
            }
        }
        else if (horizontal > 0)
        {
            face = 1;
            if (!Ub.GetBool("Moving"))
            {
                Ub.SetBool("Moving", true);
            }
        }
        else
            Ub.SetBool("Moving", false);
        if (grounded())
        {
            GroundMovement(horizontal);
        }
        else
        {
            AerialMovement(horizontal);
        }
        sprite.flipX = (face == -1);
    }

    private void Groundtime()
    {
        if (!checkTime)
        {
            return;
        }
        if (jumped)
        {
            if (grounded())
            {
                jumped = false;
            }
        }
        if (grounded())
        {
            // Debug.Log("Grounded");
            lastGrounded = Time.time;
        }
    }

    private IEnumerator PauseGroundcheck()
    {
        yield return new WaitForSeconds(0.1f);
        checkTime = true;
    }

    private void GroundMovement(float horizontal)
    {
        float newXVelocity = body.velocity.x;
        if (horizontal != 0)
        {
            // If trying to go left or right, accelerate in that direction
            newXVelocity = body.velocity.x + horizontal * accel * Time.deltaTime;
            // Clamping max velocity
            newXVelocity = Mathf.Clamp(newXVelocity, -maxSpeed, maxSpeed);
        }
        else
        {
            //Slowing down
            if (Mathf.Abs(body.velocity.x) > 0)
            {
                newXVelocity = Mathf.MoveTowards(body.velocity.x, 0, deaccel * Time.deltaTime);
            }
            else
            {
                return;
            }
        }
        body.velocity = new Vector2(newXVelocity, body.velocity.y);
    }

    private void AerialMovement(float horizontal)
    {
        float newXVelocity;
        if (horizontal != 0)
        {
            // If trying to go left or right, accelerate in that direction
            newXVelocity = body.velocity.x + horizontal * accel * Time.deltaTime;
            newXVelocity = Mathf.Clamp(newXVelocity, -maxSpeed, maxSpeed);
            newXVelocity += speedBoost(horizontal);
        }
        else
        {
            //Slowing down
            newXVelocity = Mathf.MoveTowards(body.velocity.x, 0, aerialDeaccel * Time.deltaTime);
        }

        body.velocity = new Vector2(newXVelocity, body.velocity.y);
    }

    private float jumpPosition()
    {
        // Position in the jump is mappping of current velocity to scale between 0 and jumppForce;
        return Mathf.InverseLerp(jumpForce.y, 0, Mathf.Abs(body.velocity.y));
    }

    private float speedBoost(float horizontal)
    {
        return jumpPosition() * horizontal * jumpSpeedBoost;
    }

    private bool canCoyoteJump()
    {
        return (Time.time - lastGrounded) < coyoteTime && !hasCoyoted;
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (grounded())
            {
                body.AddForce(jumpForce, ForceMode2D.Impulse);
                jumped = true;
                checkTime = false;
                StartCoroutine("PauseGroundcheck");
                return;
            }
            else if (canCoyoteJump())
            {
                body.AddForce(jumpForce, ForceMode2D.Impulse);
                jumped = true;
                checkTime = false;
                StartCoroutine("PauseGroundcheck");
                hasCoyoted = true;
                return;
            }
            else if (canQueueJump)
            {
                // Debug.Log("Jump queued");
                queuedJump = true;
                return;
            }
            else if (onWall())
            {
                WallJump();
                jumped = true;
                checkTime = false;
                StartCoroutine("PauseGroundcheck");
                return;
            }
            else if (canDoubleJump && doubleJumpUnlocked)
            {
                if (!jumped)
                    return;
                canDoubleJump = false;
                body.AddForce(jumpForce, ForceMode2D.Impulse);
                Vector2 pos = (Vector2)transform.position + Vector2.down * detectionOffsetY;
                platform = (GameObject)Instantiate(doubleJumpPrefab, pos, transform.rotation);
                return;
            }
        }
    }

    private bool canDash = true;
    private bool dashing = false;
    public Vector2 dashForce;

    private void Dash()
    {
        if (Input.GetButtonDown("Dash") && canDash && dashUnlocked)
        {
            StartCoroutine("DashMove");
        }
    }

    private IEnumerator DashMove()
    {
        canDash = false;
        dashing = true;
        Vector2 dashVelocity = body.velocity;
        dashVelocity.x = dashForce.x * face;
        body.velocity = dashVelocity;
        yield return new WaitForSeconds(dashTime);
        dashing = false;
        yield return new WaitForSeconds(1.3f);
        canDash = true;
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            Vector2 wallJumpForce = new Vector2(wallTouch() * -wallForce, jumpForce.y * 0.6f);
            body.AddForce(wallJumpForce, ForceMode2D.Impulse);
            // Debug.Log("Wall Jump");
            canWallJump = false;
        }
    }

    private void wallGrab()
    {
        if (onWall() && !grounded())
        {
            grabbingWall = true;
        }
    }

    private void Gravity()
    {
        float vx = body.velocity.x;
        float vy = body.velocity.y;
        if (grounded())
        {
            if (vy < 0)
                body.velocity = new Vector2(vx, 0);
            canWallJump = true;
            canDoubleJump = true;
            hasCoyoted = false;
            if (queuedJump)
            {
                // Debug.Log("Jump from queue");
                body.AddForce(jumpForce, ForceMode2D.Impulse);
                jumped = true;
                checkTime = false;
                StartCoroutine("PauseGroundcheck");
                queuedJump = false;
                return;
            }
        }
        else
        {
            vy -= gravity * Time.deltaTime;
        }

        if (vy < maxFallSpeed)
            vy = maxFallSpeed;

        body.velocity = new Vector2(vx, vy);

        // Better Jumping trajectory
        if (body.velocity.y < 0) // Going down
        {
            body.velocity += Vector2.up * Physics2D.gravity.y * (fallModifier - 1) * Time.deltaTime;
        }
        else if (body.velocity.y > 0 && !Input.GetButton("Jump")) // Going up and button released
        {
            body.velocity +=
                Vector2.up * Physics2D.gravity.y * (lowJumpModifier - 1) * Time.deltaTime;
        }
    }

    #endregion

#region Attacks

    public float hp = 100;
    public float attackOneDamage = 5;
    public float attackTwoDamage = 1;
    public float attackThreeDamage = 20;

    public float attackOneRange = 3;
    public float attackTwoRange = 5;
    public float attackThreeRange = 2;
    private bool attacking = false;

    public float bpRange = 3f;
    public float bpDamage = 1f;
    public float bpKnockback = 30f;
    public float bpCooldown = 2f;

    private bool bpOnCooldown = false;

    public float attackFreeze = 0.4f;

    public float spawnOffset = 0.7f;

    private void Attack()
    {
        if (Input.GetButtonDown("Action1"))
        {
            rangedAttack();
            attacking = true;
            body.velocity = new Vector2(0, 0);
            StartCoroutine("AttackFreeze");
        }
        else if (Input.GetButtonDown("Action2"))
        {
            attackAction(2);
            Vector2 spawnPoint = (Vector2)transform.position + Vector2.right * face * spawnOffset;
            Instantiate(spellPrefab, spawnPoint, transform.rotation);
            attacking = true;
            body.velocity = new Vector2(0, 0);
            StartCoroutine("AttackFreeze");
        }
        else if (Input.GetButtonDown("Action3") && shockwaveUnlocked)
        {
            blackPanther();
            attacking = true;
            body.velocity = new Vector2(0, 0);
            StartCoroutine("AttackFreeze");
        }
    }

    private IEnumerator AttackFreeze()
    {
        yield return new WaitForSeconds(attackFreeze);
        attacking = false;
    }

    private void rangedAttack()
    {
        Vector2 spawnPoint = (Vector2)transform.position + Vector2.right * face * spawnOffset;
        GameObject newProjectile = (GameObject)Instantiate(
            projectilePrefab,
            spawnPoint,
            transform.rotation
        );
        Projectile projectile = newProjectile.GetComponent<Projectile>();
        projectile.addForce(Vector2.right * face * projectile.speed);
    }

    private void blackPanther()
    {
        if (bpOnCooldown)
        {
            return;
        }
        Vector2 pos = (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, bpRange, enemyLayer);
        BasicEnemy enemy;
        Debug.Log(hits.Length);
        foreach (Collider2D enemyCollider in hits)
        {
            enemyCollider.TryGetComponent<BasicEnemy>(out enemy);
            enemy.takeDamage(bpDamage);
            Vector2 direction = face * Vector2.right;
            enemy.push(bpKnockback * direction.normalized);
            //TODO: Add damage falloff
        }
        Instantiate(shockwavePrefab, transform.position, transform.rotation);
        bpOnCooldown = true;
        StartCoroutine("BpCooldown");
    }

    private IEnumerator BpCooldown()
    {
        yield return new WaitForSeconds(bpCooldown);
        bpOnCooldown = false;
    }

    private void attackAction(int attack)
    {
        if (attacking)
            return;
        attacking = true;
        Vector2 attackVector;
        float attackDamage;
        if (attack == 1)
        {
            attackVector = attackOneRange * Vector2.right * face;
            attackDamage = attackOneDamage;
        }
        else if (attack == 2)
        {
            attackVector = attackTwoRange * Vector2.right * face;
            attackDamage = attackTwoDamage;
        }
        else if (attack == 3)
        {
            attackVector = attackThreeRange * Vector2.right * face;
            attackDamage = attackThreeDamage;
        }
        else
        {
            return;
        }
        Collider2D[] attackHit = attackHits(attackVector);
        BasicEnemy enemy;
        foreach (Collider2D hit in attackHit)
        {
            hit.TryGetComponent<BasicEnemy>(out enemy);
            enemy.takeDamage(attackOneDamage);
        }
        StartCoroutine("DelayReAttack");
    }

    private Collider2D[] attackHits(Vector2 attackVector)
    {
        Vector2 pos = (Vector2)transform.position + attackVector;
        float range = Vector2.SqrMagnitude(attackVector);
        return Physics2D.OverlapCircleAll(pos, range, enemyLayer);
        // return Physics2D.Linecast(transform.position, attackVector, enemyLayer);
    }

    IEnumerator DelayReAttack()
    {
        // Debug.Log("Reset");
        // attacking = false;
        yield return new WaitForSeconds(0.25f);
        attacking = false;
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
            die();
        Debug.Log(hp);
        showHearts();
    }

    private void die()
    {
        Destroy(gameObject);
    }

    public void push(Vector2 knockback)
    {
        body.velocity = new Vector2(0, 0);
        body.AddForce(knockback, ForceMode2D.Impulse);
        StartCoroutine("stunBlock");
    }

    IEnumerator stunBlock()
    {
        Debug.Log("Free");
        yield return new WaitForSeconds(1f);
    }
#endregion Attacks

#region Collisions

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    };

    private Vector2 getDetectionVector(Direction dir)
    {
        int v = 0,
            h = 0;
        switch (dir)
        {
            case Direction.Down:
                v = -1;
                break;
            case Direction.Up:
                v = 1;
                break;
            case Direction.Left:
                h = -1;
                break;
            case Direction.Right:
                h = 1;
                break;
        }
        Vector2 pos = transform.position;
        return pos + new Vector2(h * detectionOffsetX, v * detectionOffsetY);
    }

    private bool grounded()
    {
        Vector2 pos = (Vector2)transform.position + Vector2.down * detectionOffsetY;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x / 8f,
            groundedMargin
        );
        return Physics2D.OverlapBox(pos, size, 0, groundLayer);
    }

    private void jumpQueable()
    {
        Vector2 pos = (Vector2)transform.position + Vector2.down * queueOffset;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x / 8f,
            groundedMargin
        );
        canQueueJump = Physics2D.OverlapBox(pos, size, 0, groundLayer);
    }

    private bool lineCollidesWithWorld(Vector2 vect)
    {
        return Physics2D.Linecast(transform.position, vect, 1 << LayerMask.NameToLayer("Ground"));
    }

    private int wallTouch()
    {
        if (lineCollidesWithWorld(getDetectionVector(Direction.Right)))
            return 1;
        if (lineCollidesWithWorld(getDetectionVector(Direction.Left)))
            return -1;
        return 0;
    }

    private bool onWall()
    {
        return wallTouch() != 0;
    }

#endregion
    public void OnDrawGizmos()
    {
        if (!Application.IsPlaying(gameObject))
            return;

        Gizmos.color = Color.blue;
        // Vector3 pos = transform.position + Vector3.left * detectionOffset;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * detectionOffsetX);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * detectionOffsetX);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionOffsetY);

        // Grounded

        Vector3 pos = (Vector2)transform.position + Vector2.down * detectionOffsetY;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x / 8f,
            groundedMargin
        );
        Gizmos.DrawWireCube(pos, size);

        Gizmos.color = Color.green;
        size = new Vector2(GetComponent<SpriteRenderer>().bounds.size.x / 8f, queableJumpMargin);
        pos = (Vector2)transform.position + Vector2.down * queueOffset;
        Gizmos.DrawWireCube(pos, size);
        // pos = (Vector2)transform.position + Vector2.down * detectionOffset;
        // size = new Vector2(GetComponent<SpriteRenderer>().bounds.size.x / 1.2f, queableJumpMargin);
        // Gizmos.DrawWireCube(pos, size);

        Gizmos.color = Color.red;
        for (int i = 0; i < positions.Count; i++)
        {
            pos = positions[i];
            Gizmos.DrawWireSphere(pos, 0.1f);
        }

        Vector2 attackVector = attackOneRange * Vector2.right * face;
        pos = (Vector2)transform.position + attackVector;
        float range = Vector2.SqrMagnitude(attackVector);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, range);
        Gizmos.DrawWireSphere(transform.position, bpRange);
    }
}
