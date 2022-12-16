using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Witch : MonoBehaviour
{
    public float maxSpeed = 10;
    public Vector2 jumpForce = new Vector2(0, 10);
    public float wallForce = 10;
    public float jumpSpeedBoost = 5;
    public float accel = 90;

    public float deaccel = 50;

    public float aerialDeaccel = 10.0f;
    public float detectionOffset = 0.5f;
    public float fallModifier = 2.5f;
    public float gravity = 5;
    public float groundedMargin = 0.1f;
    public float maxFallSpeed = -10;
    public float lowJumpModifier = 2f;

    public float floatingScale = 0.2f;

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
    // private bool queuedJump = false;
    // private bool canQueueJump = false;
    // public float queableJumpMargin = 3f;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        positions = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
        // jumpQueable();
        Jump();
        Gravity();
        Attack();
        Groundtime();
        // For debugging jump
        // positions.Add(transform.position);
    }

    private void Walk()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal < 0)
            face = -1;
        else if (horizontal > 0)
            face = 1;

        if (grounded())
        {
            GroundMovement(horizontal);
        }
        else
        {
            AerialMovement(horizontal);
        }
    }

    private void Groundtime()
    {
        if (grounded())
            lastGrounded = Time.time;
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
                Debug.Log("Regular Jump");
                body.AddForce(jumpForce, ForceMode2D.Impulse);
                return;
            }
            else if (canCoyoteJump())
            {
                Debug.Log("Coyote Jump");
                body.AddForce(jumpForce, ForceMode2D.Impulse);
                hasCoyoted = true;
                return;
            }

            // if (canQueueJump)
            // {
            //     Debug.Log("Jump queued");
            //     queuedJump = true;
            //     return;
            // }

            if (onWall())
            {
                WallJump();
                return;
            }
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            Vector2 wallJumpForce = new Vector2(wallTouch() * -wallForce, jumpForce.y * 0.6f);
            body.AddForce(wallJumpForce, ForceMode2D.Impulse);
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
            hasCoyoted = false;
        }
        else
        {
            vy -= gravity * Time.deltaTime;
        }


        float mFallSpeed = Input.GetButton("Float") ? maxFallSpeed * floatingScale : maxFallSpeed;
        Debug.Log(Input.GetButton("Float"));

        if (vy < mFallSpeed)
            vy = mFallSpeed;

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

#region Attacks

    public float hp = 100;
    public float attackOneDamage = 5;
    public float attackTwoDamage = 10;
    public float attackThreeDamage = 20;

    public float attackOneRange = 10;
    public float attackTwoRange = 5;
    public float attackThreeRange = 2;
    private bool attacking = false;

    private void Attack()
    {
        if (Input.GetButtonDown("Action1"))
        {
            attackAction(1);
        }
        else if (Input.GetButtonDown("Action2"))
        {
            attackAction(2);
        }
        else if (Input.GetButtonDown("Action3"))
        {
            attackAction(3);
        }
    }

    private void attackAction(int attack)
    {
        if (attacking)
            return;
        Debug.Log("Attacking");
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
        yield return new WaitForSeconds(1.3f);
        Debug.Log("Reset");
        attacking = false;
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
            die();
        Debug.Log(hp);
    }

    private void die()
    {
        Destroy(gameObject);
    }
#endregion Attacks

    // Collision state checks

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
        return pos + new Vector2(h * detectionOffset, v * detectionOffset);
    }

    private bool grounded()
    {
        Vector2 pos = (Vector2)transform.position + Vector2.down * detectionOffset;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x / 1.2f,
            groundedMargin
        );
        return Physics2D.OverlapBox(pos, size, 0, groundLayer);
    }

    // private void jumpQueable()
    // {
    //     Vector2 pos = (Vector2)transform.position + Vector2.down * detectionOffset;
    //     Vector2 size = new Vector2(
    //         GetComponent<SpriteRenderer>().bounds.size.x / 1.2f,
    //         queableJumpMargin
    //     );
    //     canQueueJump = Physics2D.OverlapBox(pos, size, 0, groundLayer);
    // }

    private bool lineCollidesWithWorld(Vector2 vect)
    {
        return Physics2D.Linecast(transform.position, vect, 1 << LayerMask.NameToLayer("Ground"));
    }

    private bool circleCollidesWithWorld(Vector2 dir)
    {
        Vector2 pos = (Vector2)transform.position + dir * detectionOffset;
        return Physics2D.OverlapCircle(pos, detectionOffset, groundLayer);
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

    public void OnDrawGizmos()
    {
        if (!Application.IsPlaying(gameObject))
            return;

        Gizmos.color = Color.blue;
        // Vector3 pos = transform.position + Vector3.left * detectionOffset;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * detectionOffset);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * detectionOffset);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionOffset);

        // Grounded

        Vector3 pos = (Vector2)transform.position + Vector2.down * detectionOffset;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x / 1.2f,
            groundedMargin
        );
        Gizmos.DrawWireCube(pos, size);

        // Gizmos.color = Color.green;
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
    }
}
