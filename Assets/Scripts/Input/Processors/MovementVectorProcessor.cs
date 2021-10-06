using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif

public class MovementVectorProcessor : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static MovementVectorProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<MovementVectorProcessor>();
    }

    private bool pressed = false;

    // turns movement input into a tribool on each axis (-1, 0, 1)
    public override Vector2 Process(Vector2 value, InputControl control)
    {
        // avoid double press (change to context based?)
        pressed = !pressed;

        if(pressed)
        {
            float x = value.x > 0 ? 1 : value.x < 0 ? -1 : 0;
            float y = value.y > 0 ? 1 : value.y < 0 ? -1 : 0;

            return new Vector2(x, y);
        }
        else // defaults value to 0 on release
            return Vector2.zero;
    }
}
