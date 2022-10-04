using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    public float hp = 100;
    public float minAttackDamage;
    public float maxAttackDamage;
    public float attackRange;
    public float attackTime;
    private int face;
    private LayerMask playerLayer;
    private bool attacking = false;

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = 1 << LayerMask.NameToLayer("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // If player within attack range
        Vector2 pos = (Vector2)transform.position + Vector2.right * face * attackRange;
        Collider2D attackCollider = Physics2D.OverlapCircle(pos, attackRange, playerLayer);
        // Calculate and apply damage
        if (attackCollider && !attacking)
        {
            // Give character time to exit attack range
            StartCoroutine("Attack");
            Debug.Log("Attacking");
            attacking = true;
            StartCoroutine("DelayReAttack");
        }
    }

    private float computeDamage()
    {
        return (int)Random.Range(minAttackDamage, maxAttackDamage);
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(attackTime);
        Vector2 pos = (Vector2)transform.position + Vector2.right * face * attackRange;
        Collider2D attackCollider = Physics2D.OverlapCircle(pos, attackRange, playerLayer);
        Movement character;

        if (attackCollider)
        {
            attackCollider.TryGetComponent<Movement>(out character);
            character.takeDamage(computeDamage());
        }
        else
            Debug.Log("Dodged");
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
            die();
        Debug.Log(hp);
    }

    IEnumerator DelayReAttack()
    {
        // Debug.Log("Reset");
        // attacking = false;
        yield return new WaitForSeconds(1.3f);
        Debug.Log("Reset");
        attacking = false;
    }

    IEnumerator attackDuration()
    {
        yield return new WaitForSeconds(attackTime);
    }

    private void die()
    {
        Destroy(gameObject);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
