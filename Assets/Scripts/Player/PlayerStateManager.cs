using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager p;

    // animation states
        public const string PLAYER_IDLE = "Player_Idle";
        public const string PLAYER_RUN  = "Player_Move";
        public const string PLAYER_JUMP = "Player_Jump";
        public const string PLAYER_FALL = "Player_Fall";
        public const string PLAYER_LAND = "Player_Land";
        public const string PLAYER_DASH = "Player_Dash";

            private const string DASH_SIDE      = "_Side";
            private const string DASH_UP        =   "_Up";
            private const string DASH_DOWN      = "_Down";
            private const string DASH_DIAG_UP   = "_Diagonal_Up";
            private const string DASH_DIAG_DOWN = "_Diagonal_Down";

    [SerializeField]
    private PlayerSettings _settings;
    public PlayerSettings Settings => _settings;

    [ShowInInspector]
    public string CurrentState { get; private set; } = PLAYER_IDLE;

    [ShowInInspector]
    public bool IsFacingRight { get; private set; } = true;

    [ShowInInspector]
    public bool IsGrounded { get; private set; }

    [ShowInInspector, MaxValue(1), MinValue(-1)]
    public Vector2 MoveInput { get; private set; } = Vector2.zero;

    [ShowInInspector]
    public bool JumpInput { get; private set; } = false;

    [ShowInInspector]
    public bool IsDashing => CurrentState == PLAYER_DASH;

    [ShowInInspector]
    public Vector2 DashDirection { get; private set; } = Vector2.zero;

    private Animator animator;

    private void OnValidate() => UpdateFacingDirection();

    private void Start()
    {
        p = this;

        animator = GetComponent<Animator>();
        UpdateState(PLAYER_IDLE);
        SetupInputListeners();
    }

    private void SetupInputListeners()
    {
        InputController.i.Player_onMove    += _input => {     MoveInput = new Vector2(_input, 0); if(MoveInput.x != 0) UpdateFacingDirection(); };
        InputController.i.Player_onJump    +=     () =>       JumpInput = true;
        InputController.i.Player_onDashAim += _input => { DashDirection = (_input != Vector2.zero) ? _input : DashDirection; };
    }

    private bool CanTransistion(string newState)
    {
        if(newState == PLAYER_DASH && (CurrentState == PLAYER_JUMP || CurrentState == PLAYER_FALL))
            return true;
        if(CurrentState == PLAYER_DASH && newState != PLAYER_FALL)
            return false;
        if(CurrentState == PLAYER_JUMP && newState != PLAYER_FALL)
            return false;
        if(CurrentState == PLAYER_FALL && newState != PLAYER_LAND)
            return false;

        return true;
    }

    private void UpdateFacingDirection()
    {
        // calculate only if input has been provided
        if(MoveInput.x != 0)
            IsFacingRight = MoveInput.x > 0;

        int magnitude = IsFacingRight ? 1 : -1;

        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * magnitude;

        transform.localScale = newScale;
    }

    public void UpdateState(string newState)
    {
        // don't play an animation over itself
        if(CurrentState == newState) return;

        //Debug.Log("new state: " + newState);

        if(CanTransistion(newState))
        {
            CurrentState = newState;

            // play correct dash direction
            if(newState == PLAYER_DASH)
            {
                if(DashDirection.y == 0) // side
                    animator.Play(newState + DASH_SIDE);
                else if(DashDirection.x == 0) // up or down
                {
                    if(DashDirection.y > 0) // up
                        animator.Play(newState + DASH_UP);
                    else // down
                        animator.Play(newState + DASH_DOWN);
                }
                else if(DashDirection.y > 0) // diag up
                    animator.Play(newState + DASH_DIAG_UP);
                else if(DashDirection.y < 0) // diag down
                    animator.Play(newState + DASH_DIAG_DOWN);

                return;
            }

            animator.Play(newState);
        }
    }

    public void UpdateGrounded(bool newState) => IsGrounded = newState;

    public void EndJump()
    {
        JumpInput = false;
        UpdateState(PLAYER_FALL);
    }

    public void EndDash()
    {
        JumpInput = false;
        UpdateState(PLAYER_FALL);
    }
}
