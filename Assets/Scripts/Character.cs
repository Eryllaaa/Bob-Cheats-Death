using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private BoxCollider2D collider;

    private CharacterInputsManager inputs;
    private Vector2 velocity = Vector2.zero;

    private float gravity = -100;
    private float horizontalAccel = 130f;
    private float maxHorizontalSpeed = 15f;
    private float maxFallSpeed = -25;
    private float jumpStrength = 60f;
    private float groundFriction = 90f;
    private float fastFallSpeed = -500f;
    private float maxApexTime = 2f;
    private float apexYVel = 10f;
    private float apexXVel = 10f;
    private float coyoteTime = 5f;

    private bool releasedJumpEarly = false;
    private bool grounded = false;
    private bool hasGrounded = false;
    private float notGroundedTime = 0;

    private void Start()
    {
        inputs = CharacterInputsManager.GetInstance();
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        CheckCollision();
        HandleVertical();
        ApplyVelocity();
    }

    private void CheckCollision()
    {
        bool groundHit   = Physics2D.BoxCast(collider.bounds.center - (Vector3)new Vector2(0, collider.bounds.size.y * 0.4f), new Vector2(collider.bounds.size.x * 0.5f, collider.bounds.size.y * 0.1f), 0, Vector2.down, 0.05f);
        bool ceillingHit = Physics2D.BoxCast(collider.bounds.center + (Vector3)new Vector2(0, collider.bounds.size.y * 0.4f), new Vector2(collider.bounds.size.x * 0.5f, collider.bounds.size.y * 0.1f), 0, Vector2.up, 0.05f);
        bool leftHit     = Physics2D.BoxCast(collider.bounds.center - (Vector3)new Vector2(collider.bounds.size.x * 0.4f, 0), new Vector2(collider.bounds.size.x * 0.1f, collider.bounds.size.y * 0.5f), 0, Vector2.left, 0.05f);
        bool rightHit    = Physics2D.BoxCast(collider.bounds.center + (Vector3)new Vector2(collider.bounds.size.x * 0.4f, 0), new Vector2(collider.bounds.size.x * 0.1f, collider.bounds.size.y * 0.5f), 0, Vector2.right, 0.05f);
        if (groundHit)
        {
            grounded = true;
            velocity = new Vector2(velocity.x, 0);
            hasGrounded = false;
        }
        else
        {
            if (!hasGrounded)
            {
                hasGrounded = true;
                notGroundedTime = 0;
            }
            else
            {
                notGroundedTime += Time.fixedDeltaTime;
            }
            grounded = false;
        }
        if (leftHit)
        {
            velocity = new Vector2(Mathf.Max(velocity.x, 0), velocity.y);
        }
        if (rightHit)
        {
            velocity = new Vector2(Mathf.Min(velocity.x, 0), velocity.y);
        }
        if (ceillingHit)
        {
            Vector2 dir = Vector2.zero;
            //dir = Physics2D.BoxCast(collider.bounds.center + (Vector3)new Vector2(0, collider.bounds.size.y * 0.4f), new Vector2(collider.bounds.size.x * 0.5f, collider.bounds.size.y * 0.1f), 0, Vector2.up, 0.05f).point - (Vector2)collider.bounds.center;
            velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, 0)) + dir * 10;
        }
    }

    private void ApplyVelocity()
    {
        velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
        transform.position += (Vector3)velocity * Time.fixedDeltaTime;
    }

    private void HandleHorizontalMovement()
    {
        if (inputs.Left)
        {
            velocity -= new Vector2(horizontalAccel, 0) * Time.fixedDeltaTime;
        }
        if (inputs.Right)
        {
            velocity += new Vector2(horizontalAccel, 0) * Time.fixedDeltaTime;
        }
        if (!inputs.Left && !inputs.Right)
        {
            velocity = new Vector2(Mathf.MoveTowards(velocity.x, 0, groundFriction * Time.fixedDeltaTime), velocity.y);
        }
    }

    private float appliedGravity = 0;
    private bool hasJumped = false;
    private void HandleVertical()
    {
        if (inputs.Up && (grounded || (notGroundedTime < coyoteTime && !hasJumped)))
        {
            Jump();
        }
        if (grounded)
        {
            if (hasJumped)
            {
                hasJumped = false;
            }
            appliedGravity = 0;
            releasedJumpEarly = false;
        }
        else
        {
            appliedGravity = gravity;
            HandleVariableJump();
            HandleApexModifier();
        }
        velocity += new Vector2(0, appliedGravity) * Time.fixedDeltaTime;
    }

    private float apexTime = 0;
    private bool inApex = false;
    private bool hasApexed = false;
    private void HandleApexModifier()
    {
        if (releasedJumpEarly)
        {
            return;
        }
        if (velocity.y <= 1f && !hasApexed) //if in apex set mode inApex and apply start values.
        {
            inApex = true;
            hasApexed = true;
            velocity.y = apexYVel;
            velocity.x += apexXVel;
        }
        if (inApex) //stuff to do while in apex.
        {
            if (apexTime > maxApexTime)
            {
                inApex = false;
            }
            apexTime += Time.fixedDeltaTime;
            appliedGravity = 0;
        }
    }

    private void HandleVariableJump()
    {
        if (velocity.y > 0)
        {
            if (!inputs.Up)
            {
                releasedJumpEarly = true;
            }
            if (releasedJumpEarly)
            {
                appliedGravity = fastFallSpeed;
            }
        }
    }

    private void Jump()
    {
        grounded = false;
        hasJumped = true;
        velocity = new Vector2(velocity.x, jumpStrength);
        releasedJumpEarly = false;
        apexTime = 0;
        inApex = false;
        hasApexed = false;
    }
}
