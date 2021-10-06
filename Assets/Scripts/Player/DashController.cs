using UnityEngine;
using Sirenix.OdinInspector;

public class DashController : MonoBehaviour
{
    private Rigidbody2D rb;

    // time since left ground, counts down
    private float airTimer;
    private float dashTimer;
    [ShowInInspector]
    private Vector2 _dashDirection;

    public bool DashOver => dashTimer <= 0 || !PlayerStateManager.p.IsDashing;
public AudioSource sound;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InputController.i.Player_onDash += () => OnDash();
    }

    public void DashUpdate()
    {
        if(PlayerStateManager.p.IsDashing)
        {
            if(dashTimer <= 0)
            {
                rb.velocity = Vector2.zero;
                PlayerStateManager.p.EndDash();
            }
            else
            {
                float velocityX = _dashDirection.x * PlayerStateManager.p.Settings.DashSpeedX;
                float velocityY = _dashDirection.y * PlayerStateManager.p.Settings.DashSpeedY;

                rb.velocity = new Vector2(velocityX, velocityY);
            }
        }

        if(PlayerStateManager.p.IsGrounded)
            airTimer  = PlayerStateManager.p.Settings.DashAllowedTime;
        else // is in air
            airTimer -= Time.fixedDeltaTime;

        dashTimer -= Time.fixedDeltaTime;
    }

    private void OnDash()
    {
        if(airTimer > 0 && airTimer < PlayerStateManager.p.Settings.DashAllowedTime - .1f && !PlayerStateManager.p.IsGrounded) // can dash in air
        {
            PlayerStateManager.p.UpdateState(PlayerStateManager.PLAYER_DASH);
            airTimer = 0; // avoid multiple dashes -> reset to allowedtime to allow chaining
            dashTimer = PlayerStateManager.p.Settings.DashTime;
            _dashDirection = PlayerStateManager.p.DashDirection;
            sound.PlayOneShot(sound.clip);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(PlayerStateManager.p.IsDashing && other.transform.tag == "Ground")
            PlayerStateManager.p.EndDash();
    }
}