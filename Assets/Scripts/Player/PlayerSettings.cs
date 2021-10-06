using UnityEngine;

[CreateAssetMenu(fileName = "Player Settings", menuName = "Platformer/Player/Settings")]
public class PlayerSettings : ScriptableObject
{
    [SerializeField, Tooltip("Movement speed of the player.")]
    private float _movementSpeed = 2.5f;
    public float MovementSpeed => _movementSpeed;

    [SerializeField, Tooltip("Force to be applied when the player is jumping.")]
    private float _jumpForce = 9f;
    public float JumpForce => _jumpForce;

    [SerializeField, Tooltip("Force to be applied when the player is falling.")]
    private float _fallMultiplier = 3f;
    public float FallMultiplier => _fallMultiplier;

    [SerializeField, Tooltip("Minimum y-velocity needed to be falling.")]
    private float _minimumFallVelocity = -4f;
    public float MinimumFallVelocity => _minimumFallVelocity;

    /*[SerializeField, Tooltip("Minimum force applied when the player performs a short jump.")]
    private float _lowJumpMultiplier = 4f;
    public float LowJumpMultiplier => _lowJumpMultiplier;*/

    [SerializeField, Tooltip("The grace period (in seconds) during which a player can jump while technically in the air after leaving the ground, for more user-friendly input. See: Wile E. Coyote")]
    private float _coyoteTimeGraceTimer = .2f;
    public float CoyoteTimeGraceTimer => _coyoteTimeGraceTimer;

    [SerializeField, Tooltip("Speed/force of dash.")]
    private float _dashSpeedX = 20f;
    public float DashSpeedX => _dashSpeedX;

    [SerializeField, Tooltip("Speed/force of dash.")]
    private float _dashSpeedY = 10f;
    public float DashSpeedY => _dashSpeedY;

    [SerializeField, Tooltip("Time of dash (seconds).")]
    private float _dashTime = .2f;
    public float DashTime => _dashTime;

    [SerializeField, Tooltip("Time allowed to dash (seconds).")]
    private float _dashAllowedTime = 1f;
    public float DashAllowedTime => _dashAllowedTime;
}