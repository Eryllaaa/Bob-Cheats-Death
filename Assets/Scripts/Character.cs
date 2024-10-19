using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private BoxCollider2D collider;

    private CharacterInputsManager inputs;
    private Vector2 velocity = Vector2.zero;

    [SerializeField] private CharacterStats stats;

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
        HandleVertical();
        CheckCollision();
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
            if (hasGrounded)
            {
                velocity = new Vector2(velocity.x, 0);
                hasGrounded = false;
            }
            grounded = true;
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
            dir = Physics2D.BoxCast(collider.bounds.center + (Vector3)new Vector2(0, collider.bounds.size.y * 0.4f), new Vector2(collider.bounds.size.x * 0.5f, collider.bounds.size.y * 0.1f), 0, Vector2.up, 0.05f).point - (Vector2)collider.bounds.center;
            Debug.DrawLine(transform.position, dir);
            Debug.Log(dir);
            velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, 0));// + dir * 10;
        }
    }

    private void ApplyVelocity()
    {
        velocity.x = Mathf.Clamp(velocity.x, -stats.maxHorizontalSpeed, stats.maxHorizontalSpeed);
        velocity.y = Mathf.Max(velocity.y, stats.maxFallSpeed);
        transform.position += (Vector3)velocity * Time.fixedDeltaTime;
    }

    private void HandleHorizontalMovement()
    {
        if (inputs.Left && inputs.Right)
        {
            return;
        }
        if (inputs.Left)
        {
            if (velocity.x > 0) //if we were going right
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, stats.groundFriction * Time.fixedDeltaTime);
            }
            velocity -= new Vector2(stats.horizontalAccel, 0) * Time.fixedDeltaTime;
        }
        if (inputs.Right)
        {
            if (velocity.x < 0) //if we were going left
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, stats.groundFriction * Time.fixedDeltaTime);
            }
            velocity += new Vector2(stats.horizontalAccel, 0) * Time.fixedDeltaTime;
        }
        if (!inputs.Left && !inputs.Right)
        {
            velocity = new Vector2(Mathf.MoveTowards(velocity.x, 0, stats.groundFriction * Time.fixedDeltaTime), velocity.y);
        }
    }

    private float appliedGravity = 0;
    private bool hasJumped = false;
    private void HandleVertical()
    {
        if (inputs.Up && (grounded || (notGroundedTime < stats.coyoteTime && !hasJumped)))
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
            appliedGravity = stats.gravity;
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
            velocity.y = stats.apexYVel;
            if (velocity.x != 0)
            {
                velocity.x += Mathf.Sign(velocity.x) * stats.apexXVel;
            }
        }
        if (inApex) //stuff to do while in apex.
        {
            if (apexTime > stats.maxApexTime)
            {
                inApex = false;
            }
            apexTime += Time.fixedDeltaTime;
            appliedGravity = stats.apexGravity;
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
                appliedGravity = stats.fastFallSpeed;
            }
        }
    }

    private void Jump()
    {
        grounded = false;
        hasJumped = true;
        velocity = new Vector2(velocity.x, stats.jumpStrength);
        releasedJumpEarly = false;
        apexTime = 0;
        inApex = false;
        hasApexed = false;
    }
}
