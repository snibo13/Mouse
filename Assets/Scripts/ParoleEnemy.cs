using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParoleEnemy : MonoBehaviour
{
    public Vector2 leftEdge;
    public Vector2 rightEdge;
    public float maxSpeed;

    private float epsilon = 2f;
    private int face = -1;
    private Vector2 target;
    private Rigidbody2D body;
    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        target = leftEdge;
        face = -1;
        body = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        body.velocity = Vector2.right * face * maxSpeed;
        
        if (Vector2.Distance(transform.position, target) < epsilon) {
            if (target == leftEdge) target = rightEdge;
            else if (target == rightEdge) target = leftEdge;
            face = face * -1;
            sr.flipX = (face == 1);
        }
            
        
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftEdge, 1f);
        Gizmos.DrawWireSphere(rightEdge, 1f);
    }

}
