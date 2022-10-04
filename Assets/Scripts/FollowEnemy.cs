using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowEnemy : MonoBehaviour
{
    public float speed;
    public Transform target;
    public float detectionRange;
    public float escapeRange;
    private bool detected = false;
    public bool grounded = false;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(target.position, transform.position) < detectionRange)
            detected = true;
        else if (Vector2.Distance(target.position, transform.position) > escapeRange)
            detected = false;

        if (detected)
        {
            if (grounded)
            {
                float x = Mathf.MoveTowards(
                    transform.position.x,
                    target.position.x,
                    speed * Time.deltaTime
                );
                float y = transform.position.y;
                transform.position = new Vector2(x, y);
            }
            else
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    target.position,
                    speed * Time.deltaTime
                );
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, escapeRange);
    }
}
