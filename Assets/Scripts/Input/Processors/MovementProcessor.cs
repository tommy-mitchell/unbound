using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif

public class MovementProcessor : InputProcessor<float>
{
    #if UNITY_EDITOR
    static MovementProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<MovementProcessor>();
    }

    private bool pressed = false;

    // turns movement input into a tribool (-1, 0, 1)
    public override float Process(float value, InputControl control)
    {
        pressed = !pressed;

        if(pressed)
            return value > 0 ? 1 : value < 0 ? -1 : 0;
        else // defaults value to 0 on release
            return 0;
    }
}