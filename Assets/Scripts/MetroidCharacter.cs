using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class MetroidCharacter : MonoBehaviour
{

    [SerializeField] private float detectionOffset;

    private Vector2 movementDirection;
    private Vector2 lastPosition, currentPosition;
    private Vector2 velocity;
    private float _horizontalSpeed, _verticalSpeed;
    private enum Direction {Left, Right, Up, Down};
    [SerializeField] private Bounds _bounds;
    [SerializeField] private LayerMask _groundLayer;


    // Update is called once per frame
    void Update()
    {
        currentPosition = transform.position;
        velocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;

        Walk();
        computeFallSpeed();
        Gravity();
        Jump();
        // FastFall();
        Move();
    }


    // Movement parameters
    [Header("Movement")]
    [SerializeField] private float _maxSpeed = 10;
    [SerializeField] private float _accel = 90;
    [SerializeField] private float _deaccel = 50;
    [SerializeField] private float _apexBoost = 2;
    [SerializeField] private float _apexThreshold = 10f;

    private void Walk() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal != 0) {
            _horizontalSpeed += horizontal * _accel * Time.deltaTime;
            // Limit speed to max value
            _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, -_maxSpeed, _maxSpeed);

            //Speed boost at jump apex
            _horizontalSpeed += _apexBoost * Mathf.Sign(horizontal) * getJumpPosition();
        } else {
            _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, 0, _deaccel * Time.deltaTime);
        }


        var wall = onWall();
        
        
        if (_horizontalSpeed > 0 && wall == 1 || _horizontalSpeed < 0 && wall == -1) {
            Debug.Log("On Wall");
            _horizontalSpeed = 0;
        }
    }


        [Header("Gravity")]
            [SerializeField] private float _shortJumpGravity = 1f;
    [SerializeField] private float _maxFallSpeed = 120f;
    [SerializeField] private float _minFallSpeed = 80f;
    [SerializeField] private float _fallClamp = -40f;
    private float _fallSpeed;

     private void Gravity() {
        float fallSpeed = 0;
        if (isGrounded()) {
            if (_verticalSpeed < 0) _verticalSpeed = 0;
        } else {
            // Compute speed of fall based on position in jump
            fallSpeed = _shortJump && _verticalSpeed > 0 ? _fallSpeed * _shortJumpGravity : _fallSpeed;
        
            _verticalSpeed -= fallSpeed * Time.deltaTime;

            if (_verticalSpeed < _fallClamp) _verticalSpeed = _fallClamp;
        }

        

    }

    
    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 2;
    [SerializeField] private float _fastFallForce = 2;
    [SerializeField] private float _wallJumpSpeed = 5;
    [SerializeField] private float _coyoteTimeThresh = 0.1f;
    private bool _shortJump = false;
    private bool _canWallJump = true;
    private float _jumpPos;



    private void Jump() {
        // When Jump is pressed
        if  (Input.GetAxisRaw("Jump") == 1 && isGrounded()) {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0.0f, _jumpForce), ForceMode2D.Impulse);
        } 
    }

    private void FastFall() {
        if (Input.GetAxisRaw("Jump") == 0 && !isGrounded() ) {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0.0f, -_fastFallForce), ForceMode2D.Impulse);
        }
    }

    private float getJumpPosition() {
        if (!isGrounded()) {
            _jumpPos = Mathf.InverseLerp(_apexThreshold, 0, Mathf.Abs(velocity.y));
            return _jumpPos;
        }
        return 0;
    }

    private void computeFallSpeed() {
        float pos = getJumpPosition();
        if (pos == 0) _fallSpeed = 0;
        _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, pos);
    }

    
    private void Move() {
        
        Vector3 pos = transform.position + _bounds.center;
        Vector3 rawMovement = new Vector3(_horizontalSpeed, _verticalSpeed, 0);
        Vector3 move = rawMovement * Time.deltaTime;
        Vector3 endPoint = pos + move;

        var collides = Physics2D.OverlapBox(endPoint, _bounds.size, 0, _groundLayer);
        if (!collides) {
            transform.position += move;
            return;
        }

        // Debug.Log("Collision");
        var positionToMoveTo = transform.position;
        for (float i = 0; i < 10; i++) {
            var t = i / 10f;
            var posToTry = Vector2.Lerp(pos, endPoint, t);
            if (Physics2D.OverlapBox(posToTry, _bounds.size, 0, _groundLayer)) {
                transform.position = positionToMoveTo;

                if (i == 1) {
                    if (_verticalSpeed < 0) _verticalSpeed = 0;
                    var dir = transform.position - collides.transform.position;
                    transform.position += dir.normalized * move.magnitude;
                }

                return;
            }

            positionToMoveTo = posToTry;
        }
    }



    // Utilities

   void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        var pos = transform.position;
        Gizmos.DrawLine(pos, new Vector3(-1 * detectionOffset, 0,0) + pos);
        Gizmos.DrawLine(pos, new Vector3(1 * detectionOffset, 0,0) + pos);
        Gizmos.DrawLine(pos, new Vector3(0, -1 * detectionOffset, 0) + pos);
        Gizmos.DrawLine(pos, new Vector3(0, 1 * detectionOffset, 0) + pos);

        Gizmos.DrawCube(pos + _bounds.center, _bounds.size);
    }

    private Vector2 getDetectionVector(Direction dir) {
        int v =0, h = 0;
        switch (dir) {
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

    private Vector2 getVectorBelow()
    {
        var v = getDetectionVector(Direction.Down);
        return v;
    }

    

    private bool isGrounded()
    {
        return lineCollidesWithWorld(getDetectionVector(Direction.Down));
    }

    

    private bool lineCollidesWithWorld(Vector2 vect) {
        return Physics2D.Linecast(transform.position, vect, 1 << LayerMask.NameToLayer("Ground")); //00000001 << 7 => 01000000
        //Checks if the linecast from the center of the object to the groundChecker object passes through the Ground layer
        //Implies that the ground is between the groundChecker and the center of the character
    }

    private int onWall() {
        if (lineCollidesWithWorld(getDetectionVector(Direction.Right))) return 1;
        if (lineCollidesWithWorld(getDetectionVector(Direction.Left))) return -1;
        return 0;
    }

    private Collider2D movementCollides(Vector2 pos) {
        return Physics2D.OverlapBox(pos, _bounds.size, 0, 1 << LayerMask.NameToLayer("Ground"));
        // var l = getDetectionVector(Direction.Left);
        // var r = getDetectionVector(Direction.Right);
        // var d = getDetectionVector(Direction.Down);
        // var u = getDetectionVector(Direction.Up);

        // return Physics2D.Linecast(pos, l, 1 << LayerMask.NameToLayer("Ground")) ||
        //     Physics2D.Linecast(pos, r, 1 << LayerMask.NameToLayer("Ground")) ||
        //     Physics2D.Linecast(pos, u, 1 << LayerMask.NameToLayer("Ground")) ||
        //     Physics2D.Linecast(pos, d, 1 << LayerMask.NameToLayer("Ground"));


    }

    
}
