using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.Serialization;

public class InputController : MonoBehaviour
{
    public static InputController i;

    public const string MAP_PLAYER   = "Player";
    public const string MAP_UI       = "UI";

    [OdinSerialize]
    public PlayerInput PlayerInput { get; private set; }

    private void Awake()
    {
        i = this;
        PlayerInput = GetComponent<PlayerInput>();
    }

    public void SwitchActionMap(string newMap) => PlayerInput.SwitchCurrentActionMap(newMap);
    
    public event Action<float>   Player_onMove;
    //public event Action<bool>  Player_onJump;
    public event Action          Player_onJump;
    public event Action          Player_onDash;
    public event Action<Vector2> Player_onDashAim;

    public event Action Collapse_onToggle;

    public event Action Controls_onChanged;

    public void OnMove(InputAction.CallbackContext context) => Player_onMove?.Invoke(context.ReadValue<float>());

    //public void OnJump(InputAction.CallbackContext context) => Player_onJump?.Invoke(context.ReadValue<float>() != 0);
    public void OnJump(InputAction.CallbackContext context) => Player_onJump?.Invoke();

    public void OnDash(InputAction.CallbackContext context) => Player_onDash?.Invoke();

    public void OnDashAim(InputAction.CallbackContext context) => Player_onDashAim?.Invoke(context.ReadValue<Vector2>());

    public void OnToggleCollapse(InputAction.CallbackContext context) => Collapse_onToggle?.Invoke();    

    public void OnControlsChanged() => Controls_onChanged?.Invoke();
}