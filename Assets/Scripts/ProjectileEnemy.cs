using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    public float range;
    public float damage;
    public float knockback;
    public float detectionOffset = 0.5f;
    public float gravity = 5;
    public float groundedMargin = 0.1f;
    private LayerMask playerLayer;
    private LayerMask groundLayer;
    public Rigidbody2D body;
    public float fallModifier = 2.5f;
    public Vector2 offset;

    public float speed;
    private bool grounded;
    public bool groundDetonate = false;

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = 1 << LayerMask.NameToLayer("Player");
        body.velocity = Vector2.left * speed;
    }

    // Update is called once per frame
    void Update()
    {
        // Gravity();
        Explode();
    }

    void Gravity()
    {
        float vx = body.velocity.x;
        float vy = body.velocity.y - gravity * Time.deltaTime;
        body.velocity = new Vector2(vx, 0);
        if (body.velocity.y < 0) // Going down
        {
            body.velocity += Vector2.up * Physics2D.gravity.y * (fallModifier - 1) * Time.deltaTime;
        }
    }

    private bool Hit()
    {
        Vector2 pos = (Vector2)transform.position - offset;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x * 1.1f,
            GetComponent<SpriteRenderer>().bounds.size.y * 1.1f
        );
        if (groundDetonate) {
            grounded = Physics2D.OverlapBox(pos, size, 0, groundLayer);
        } else {
            grounded = false;
        }
        if (GetComponent<BoxCollider2D>() != null) {
            size = new Vector2(
            GetComponent<BoxCollider2D>().bounds.size.x * 1f,
            GetComponent<BoxCollider2D>().bounds.size.y * 1f
        );
        return Physics2D.OverlapBox(pos, size, 0, playerLayer) || grounded;
        } else {
            return Physics2D.OverlapCircle(pos, GetComponent<CircleCollider2D>().radius, playerLayer) || grounded;
        }
        
    }

    public void addForce(Vector2 vel)
    {
        body.AddForce(vel, ForceMode2D.Impulse);
    }

    public Vector2 getVelocity()
    {
        return body.velocity;
    }

    private void Explode()
    {
        if (Hit())
        {
            Vector2 pos = (Vector2)transform.position - offset;
            Vector2 size = new Vector2(
                GetComponent<BoxCollider2D>().bounds.size.x * 1f,
                GetComponent<BoxCollider2D>().bounds.size.y * 1f
            );
            Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, 0, playerLayer);
            Movement player;
            foreach (Collider2D playerCollider in hits)
            {
                playerCollider.TryGetComponent<Movement>(out player);
                player.takeDamage(damage);
                Vector2 direction = (Vector2)player.transform.position - (Vector2)transform.position;
                player.push(knockback * direction.normalized);
                //TODO: Add damage falloff
            }
            Debug.Log("Exploded");
            Destroy(gameObject);
        }
        return;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 posDraw = (Vector2)transform.position - offset;
        if (GetComponent<BoxCollider2D>() != null) {
        Vector2 boxSize = new Vector2(
                GetComponent<BoxCollider2D>().bounds.size.x * 1f,
                GetComponent<BoxCollider2D>().bounds.size.y * 1f
            );
        Gizmos.DrawWireCube(posDraw, boxSize);
        }
        
        if (GetComponent<CircleCollider2D>() != null)
            Gizmos.DrawWireSphere(posDraw, GetComponent<CircleCollider2D>().radius);
    }
}
