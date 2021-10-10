using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CommonLibrary.CommonDefinitions;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private DashController dashController;
    private List<Transform> groundChecks;

    private float graceTimer;

    private void Start()
    {
        rb             = GetComponent<Rigidbody2D>();
        dashController = GetComponent<DashController>();
        groundChecks   = new List<Transform>() {
            { transform.Find("Ground Check [L]") },
            { transform.Find("Ground Check [M]") },
            { transform.Find("Ground Check [R]") }
        };
    }

    private void FixedUpdate()
    {
        PlayerStateManager.p.UpdateGrounded(IsOnGround());

        if(!PlayerStateManager.p.IsDashing)
        {
            FallUpdate();
            MoveUpdate();
            JumpUpdate();
        }

        dashController.DashUpdate();

        // stop sliding down slopes
        if(PlayerStateManager.p.IsGrounded)
        {
            float x = PlayerStateManager.p.MoveInput.x != 0 ? rb.velocity.x : 0; // no slide in x
            float y = rb.velocity.y < 0 ? 0 : rb.velocity.y; // no slide down in y

            rb.velocity = new Vector2(x, y);

            rb.isKinematic = rb.velocity == Vector2.zero; // set to kinematic when not moving
        }
        else if(rb.isKinematic)
            rb.isKinematic = false;

        if(PlayerStateManager.p.IsGrounded)
            graceTimer  = PlayerStateManager.p.Settings.CoyoteTimeGraceTimer;
        else // is in air
            graceTimer -= Time.fixedDeltaTime;
    }

    // raycast down to check if player is touching the ground
    private bool IsOnGround()
    {
        System.Func<Transform, bool> checkTransform = (groundCheck) => {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground"));

            // Raycast() returns a list of 'hit' outputs, so if list > 0 then there's a hit
            return Physics2D.Raycast(groundCheck.position, Vector2.down, filter, new List<RaycastHit2D>(), OFFSET_PER_PIXEL) > 0  &&
                 !((Physics2D.Raycast(groundCheck.position + new Vector3(0, OFFSET_PER_PIXEL, 0), Vector2.down, filter, new List<RaycastHit2D>(), OFFSET_PER_PIXEL) > 0) &&
                   (Physics2D.Raycast(groundCheck.position - new Vector3(0, OFFSET_PER_PIXEL, 0), Vector2.down, filter, new List<RaycastHit2D>(), OFFSET_PER_PIXEL) > 0));
        };

        return checkTransform(groundChecks[0]) || checkTransform(groundChecks[1]) || checkTransform(groundChecks[2]);
    }

    private void MoveUpdate()
    {
        if(PlayerStateManager.p.MoveInput.x != 0)
        {
            rb.velocity = new Vector2(PlayerStateManager.p.MoveInput.x * PlayerStateManager.p.Settings.MovementSpeed, rb.velocity.y);

            PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_RUN);
        }
        else if(rb.velocity.x == 0)
        {
            PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_IDLE); // need to automatically transition
            return;
        }
        else // MoveInput = 0 and hasn't stopped moving
        {
            if(PlayerStateManager.p.IsGrounded)
                rb.velocity = Vector2.zero;
            else
                rb.velocity = new Vector2(0, rb.velocity.y);
            PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_IDLE);
        }
    }

    private bool ShouldLand => PlayerStateManager.p.CurrentState == PlayerStateManager.PLAYER_FALL ||
                               PlayerStateManager.p.CurrentState == PlayerStateManager.PLAYER_JUMP;

    private void FallUpdate()
    {
        if(!PlayerStateManager.p.IsGrounded && rb.velocity.y < PlayerStateManager.p.Settings.MinimumFallVelocity) // falling
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * PlayerStateManager.p.Settings.FallMultiplier * Time.fixedDeltaTime;
            if(dashController.DashOver)
                PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_FALL);
        }
        else if(PlayerStateManager.p.IsGrounded && ShouldLand)
        {
            if(PlayerStateManager.p.CurrentState == PlayerStateManager.PLAYER_JUMP)
                PlayerStateManager.p.EndJump();
            else
                PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_LAND);
        }
    }

    private void JumpUpdate()
    {
        if(PlayerStateManager.p.JumpInput)
        {
            if(PlayerStateManager.p.IsGrounded && graceTimer != PlayerStateManager.p.Settings.CoyoteTimeGraceTimer) // has landed
                PlayerStateManager.p.EndJump();
            else if(PlayerStateManager.p.IsGrounded || graceTimer > 0) // allowed to jump
            {
                PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_JUMP);
                rb.velocity = Vector2.up * PlayerStateManager.p.Settings.JumpForce;
                graceTimer = 0; // avoid multiple jumps during grace period
            }
        }
    }
}
