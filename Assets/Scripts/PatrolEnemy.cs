using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public float distance = 5f;
    private Vector2 leftEdge;
    private Vector2 rightEdge;
    public float maxSpeed = 2f;
    public float pursuingBoost = 1f;
    public Transform target;
    public float detectionRange;
    public float escapeRange;
    private bool detected = false;

    private float epsilon = 2f;
    private int face = -1;
    private Vector2 destination;
    private Rigidbody2D body;
    private bool wasChasing = false;
    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        leftEdge = (Vector2) transform.position - new Vector2(distance,0);
        rightEdge = (Vector2) transform.position + new Vector2(distance,0);
        destination = leftEdge;
        face = -1;
        body = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Vector2.Distance(target.position, transform.position) < detectionRange)
            detected = true;
        else if (Vector2.Distance(target.position, transform.position) > escapeRange)
            detected = false;

        if (detected)
        {
    
            
                if (transform.position.x > target.position.x) face = -1;
                else face = 1;
                sr.flipX = (face == 1);
                body.velocity = Vector2.right * face * maxSpeed * pursuingBoost;
                if (!wasChasing) wasChasing = true;
        } else {
            if (wasChasing) {
                if (Vector2.Distance(transform.position, leftEdge) < Vector2.Distance(transform.position, rightEdge))
            {
                destination = rightEdge;
                face = 1;
            }
            else {destination = leftEdge; face = -1;}
            wasChasing = false;
            }
            
            
    
            if (Vector2.Distance(transform.position, destination) < epsilon) {
                if (destination == leftEdge) destination = rightEdge;
                else if (destination == rightEdge) destination = leftEdge;
                face = face * -1;
                sr.flipX = (face == 1);
            }
            body.velocity = Vector2.right * face * maxSpeed;
        }
            
        
            
        
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2) transform.position - new Vector2(distance,0), (Vector2) transform.position + new Vector2(distance,0));
        
    }

}
