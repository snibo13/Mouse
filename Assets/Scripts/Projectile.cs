using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float range;
    public float damage;
    public float knockback;
    public float detectionOffset = 0.5f;
    public float gravity = 5;
    public float groundedMargin = 0.1f;
    private LayerMask enemyLayer;
    private LayerMask groundLayer;
    public Rigidbody2D body;
    public float fallModifier = 2.5f;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        Gravity();
        Explode();
    }

    void Gravity()
    {
        float vx = body.velocity.x;
        float vy = body.velocity.y - gravity * Time.deltaTime;
        body.velocity = new Vector2(vx, vy);
        if (body.velocity.y < 0) // Going down
        {
            body.velocity += Vector2.up * Physics2D.gravity.y * (fallModifier - 1) * Time.deltaTime;
        }
    }

    private bool Hit()
    {
        Vector2 pos = (Vector2)transform.position;
        Vector2 size = new Vector2(
            GetComponent<SpriteRenderer>().bounds.size.x * 1.1f,
            GetComponent<SpriteRenderer>().bounds.size.y * 1.1f
        );
        return Physics2D.OverlapBox(pos, size, 0, groundLayer)
            || Physics2D.OverlapBox(pos, size, 0, enemyLayer);
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
            Vector2 pos = (Vector2)transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, range, enemyLayer);
            BasicEnemy enemy;
            foreach (Collider2D enemyCollider in hits)
            {
                enemyCollider.TryGetComponent<BasicEnemy>(out enemy);
                enemy.takeDamage(damage);
                Vector2 direction = (Vector2)enemy.transform.position - (Vector2)transform.position;
                enemy.push(knockback * direction.normalized);
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
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
