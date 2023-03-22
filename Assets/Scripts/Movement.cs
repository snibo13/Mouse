using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Unlocking abilities
    private bool doubleJumpUnlocked = false;
    private bool dashUnlocked = false;

    public enum abilities
    {
        doubleJump,
        dash,
        shockwave
    }

    public float maxSpeed = 8;
    public Vector2 jumpForce = new Vector2(0, 16);
    public float wallForce = 16f;
    public float jumpSpeedBoost = 1.2f;
    public float accel = 90f;

    public float deaccel = 50;

    public float aerialDeaccel = 10.0f;
    public float detectionOffsetY = 0.75f;
    public float detectionOffsetX = 0.6f;
    public float fallModifier = 5f;
    public float gravity = 16;
    public float groundedMargin = 0.05f;
    public float maxFallSpeed = -16;
    public float lowJumpModifier = 20f;

    private bool dashed;
    private bool canMove;
    private bool wallJumping;

    // private bool grabbingWall;

    private Rigidbody2D body;
    private LayerMask groundLayer;
    private List<Vector3> positions;
    private bool canWallJump;
    public int face { get; set; }
    private LayerMask enemyLayer;

    // Coyote
    private float lastGrounded;
    public float coyoteTime = 0.1f;
    private bool hasCoyoted = false;

    // Jump margin
    private bool queuedJump = false;
    private bool canQueueJump = false;
    public float queueOffset = 1.5f;
    public float queableJumpMargin = 2f;

    private bool jumped;
    private bool checkTime = true;
    private bool canDoubleJump;
    public float dashTime = 0.3f;

    public GameObject projectilePrefab;
    public GameObject doubleJumpPrefab;

    private GameObject platform;

    public Character character;

    private Animator Ub;
    private SpriteRenderer sprite;

    // Start is called before the first frame update
    void OnEnable()
    {
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        // Ub = character.Ub;
        // sprite = character.sprite;
    }

    void Start()
    {
        jumped = false;
        body = GetComponent<Rigidbody2D>();
        Ub = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void moveUpdate(float horizontal)
    {
        Dash();
        Groundtime();
        jumpQueable();
        // Jump();
        Gravity();
        Ub.SetBool("Grounded", grounded());
        Ub.SetFloat("Vy", body.velocity.y);
        if (dashing || character.attacking)
            return;
        Walk(horizontal);
        // Attack();
        Ub.SetBool("Grounded", grounded());
        Ub.SetFloat("Vy", body.velocity.y);
    }

    public void push(Vector2 knockback)
    {
        body.velocity = new Vector2(0, 0);
        body.AddForce(knockback, ForceMode2D.Impulse);
        FollowEnemy fe = gameObject.GetComponent<FollowEnemy>();
        PatrolEnemy pe = gameObject.GetComponent<PatrolEnemy>();
        if (fe != null)
            fe.enabled = false;
        if (pe != null)
            pe.enabled = false;
        StartCoroutine("stunBlock");
    }

    IEnumerator stunBlock()
    {
        yield return new WaitForSeconds(1f);
        // TODO: Disable motion
    }

    private void Walk(float horizontal)
    {
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

    public void Jump()
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

    private bool canDash = true;
    private bool dashing = false;
    public Vector2 dashForce = new Vector2(20, 0);

    public void Dash()
    {
        if (canDash && dashUnlocked)
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

    // private void wallGrab()
    // {
    //     if (onWall() && !grounded())
    //     {
    //         grabbingWall = true;
    //     }
    // }

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
}
