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
    public float forceDistance = 2.0f;
    public float forceConstant = -2.0f;
    private LayerMask groundLayer;
    private Rigidbody2D body;
    private Vector2 wallForce;
    public float maxForce;
    public float targetMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        body = GetComponent<Rigidbody2D>();
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
                // transform.position = Vector2.MoveTowards(
                //     transform.position,
                //     target.position,
                //     speed * Time.deltaTime
                // );

                wallForce = PushPullPathLines();
                body.AddForce(wallForce, ForceMode2D.Force);

                body.velocity = Vector2.ClampMagnitude(body.velocity, speed);
                Debug.Log(Vector2.SqrMagnitude(body.velocity));
            }
        }
    }

    // Push Pull Path Planning
    // Cast Rays in each direction
    // Exert forces away from contact points
    // Exert force towards target
    // Cap velocity
    Vector2 PushPullPath()
    {
        Collider2D wallDetector = Physics2D.OverlapCircle(
            transform.position,
            forceDistance,
            groundLayer
        );
        ContactPoint2D[] contacts = { };
        wallDetector.GetContacts(contacts);

        Vector2 netForce = new Vector2(0, 0);

        foreach (ContactPoint2D contact in contacts)
        {
            Vector2 direction = Vector2.ClampMagnitude(
                -(Vector2)transform.position - contact.point,
                1
            );
            netForce += direction * forceConstant / contact.separation;
            // if (distance < forceDistance)
            //     netForce += Vector2.right * forceConstant / distance;
        }

        Vector2 targetForce =
            forceConstant
            * targetMultiplier
            * Vector2.ClampMagnitude((Vector2)target.position - (Vector2)transform.position, 1);

        netForce += targetForce;

        netForce = Vector2.ClampMagnitude(netForce, maxForce);

        return netForce;
    }

    Vector2 PushPullPathLines()
    {
        Ray leftRay = new Ray(transform.position, Vector3.left);
        Ray rightRay = new Ray(transform.position, Vector3.right);
        Ray upRay = new Ray(transform.position, Vector3.up);
        Ray downRay = new Ray(transform.position, Vector3.down);

        RaycastHit2D leftHit = Physics2D.Raycast(
            transform.position,
            Vector2.left,
            forceDistance,
            groundLayer
        );
        RaycastHit2D rightHit = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            forceDistance,
            groundLayer
        );
        RaycastHit2D upHit = Physics2D.Raycast(
            transform.position,
            Vector2.up,
            forceDistance,
            groundLayer
        );
        RaycastHit2D downHit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            forceDistance,
            groundLayer
        );

        List<RaycastHit2D> hits = new List<RaycastHit2D>() { leftHit, rightHit, upHit, downHit };

        Vector2 netForce = new Vector2(0, 0);
        float distance;
        RaycastHit2D hit;
        hit = leftHit;
        if (hit.collider != null)
        {
            distance = Vector2.SqrMagnitude((Vector2)transform.position - hit.point);
            if (distance < forceDistance)
                netForce += Vector2.right * forceConstant / distance;
        }
        hit = rightHit;
        if (hit.collider != null)
        {
            distance = Vector2.SqrMagnitude((Vector2)transform.position - hit.point);
            if (distance < forceDistance)
                netForce += Vector2.left * forceConstant / distance;
        }
        hit = upHit;
        if (hit.collider != null)
        {
            distance = Vector2.SqrMagnitude((Vector2)transform.position - hit.point);
            if (distance < forceDistance)
                netForce += Vector2.down * forceConstant / distance;
        }
        hit = downHit;
        if (hit.collider != null)
        {
            distance = Vector2.SqrMagnitude((Vector2)transform.position - hit.point);
            if (distance < forceDistance)
                netForce += Vector2.up * forceConstant / distance;
        }

        Vector2 targetForce =
            forceConstant
            * targetMultiplier
            * Vector2.ClampMagnitude((Vector2)target.position - (Vector2)transform.position, 1);

        netForce += targetForce;

        netForce = Vector2.ClampMagnitude(netForce, maxForce);

        return netForce;
    }

    public void OnDrawGizmos()
    {
        // Gizmos.DrawWireSphere(transform.position, detectionRange);
        // Gizmos.DrawWireSphere(transform.position, escapeRange);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * forceDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * forceDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * forceDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * forceDistance);

        Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, trans)
        Gizmos.DrawRay(transform.position, wallForce);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, forceDistance);
    }
}
