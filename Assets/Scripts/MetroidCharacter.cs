using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class MetroidCharacter : MonoBehaviour
{

    public Animator Ub;
    public float movementSpeed = 1f;
    public Vector2 jumpForce = new Vector2(0, 1f);
    public Vector2 wallForce = new Vector2(1f, 0);
    public float detectionOffset;

    private bool canDoubleJump;
    private Rigidbody2D body;
    private float hp = 100;
    private Vector2 movementDirection;

    public Vector2 velocity {get; private set;}

    private Vector2 lastPosition;
    private float _horizontalSpeed, _verticalSpeed;

    // Movement parameters
    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float _accel = 90;
    [SerializeField] private float _deaccel = 50;
    [SerializeField] private float _apexBoost = 2;
    [SerializeField] private float _apexThreshold = 10f;

    [SerializeField] private float _maxFallSpeed = 120f;
    [SerializeField] private float _minFallSpeed = 80f;
    [SerializeField] private float _maxFall
    private float _fallSpeed;

    private enum Direction {Left, Right, Up, Down};

    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentPosition = transform.position;
        velocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;
        Walk(); //Horizontal movement
    }


    private void Walk() {
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0) {
            _horizontalSpeed += horizontal * _accel * Time.deltaTime;
            // Limit speed to max value
            _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, -_maxSpeed, _maxSpeed);

            //Speed boost at jump apex
            _horizontalSpeed += _apexBoost * Mathf.sign(horizontal) * getJumpPosition();
        } else {
            _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, 0, _deaccel * Time.deltaTime);
        }

        if (_horizontalSpeed > 0 && onWall()) {
            _currentHorizontalSpeed = 0;
        }
    }

    private float getJumpPosition() {
        if (!isGrounded()) {
            return _Math.InverseLerp(_apexThreshold, 0, Mathf.Abs(Velocity.y));
        }
        return 0;
        

    }

    private void Gravity() {
        if (isGrounded()) {
            if (_verticalSpeed < 0) verticalSpeed = 0;

        } else {
            var fallSpeed = _shortJump && _verticalSpeed > 0 ? _fallSpeed * _shortJumpGravity : _fallSpeed;
        }

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
        Debug.Log(v);
        return v;
    }

    

    private bool isGrounded()
    {
        return lineCollidesWithWorld(getDetectionVector(Direction.Down))        ;
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
}
