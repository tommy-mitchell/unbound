using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ClampAxis : MonoBehaviour
{
    [SerializeField]
    private bool ClampX;

    [SerializeField]
    private bool ClampY;

    [SerializeField, Tooltip("If the axis should be clamped to a specific position.")]
    private bool ClampToPosition = true;

    [SerializeField, ShowIf("ClampToPosition")]
    private Vector2 ToPosition;

    private Vector2 startingPosition = Vector2.zero;

    private void Start() { if(!ClampToPosition) startingPosition = transform.position; }

    private void OnValidate() { if(ClampToPosition) transform.position = GetNewPosition(ToPosition); }

    // sets local space position to clamp value
    private void Update()
    {
        // if clamped: gives world space values
        Vector2 newPosition = GetNewPosition(startingPosition);

        //newPosition.y = transform.InverseTransformPoint(newPosition).y;
        //transform.localPosition = newPosition;

        // convert to world position if unclamped
        if(!ClampX) newPosition.x = transform.TransformPoint(newPosition).x;
        if(!ClampY) newPosition.y = transform.TransformPoint(newPosition).y;

        transform.position = newPosition;
    }

    private Vector2 GetNewPosition(Vector2 referencePosition)
    {
        Vector2 newPosition = Vector2.zero;

        if(ClampX) newPosition.x = referencePosition.x;
        if(ClampY) newPosition.y = referencePosition.y;

        return newPosition;
    }

    /*private bool init = true;

    private Vector2 startingPosition {
        get {
            Vector2 temp = new Vector2();

            if(ClampX || init) temp.x = transform.position.x;
            if(ClampY || init) temp.y = transform.position.y;

            init = false; // cache starting position so it's not 0

            return temp;
        }
    }

    private Vector2 currentPosition;

    // calculate on start or when clamp has changed
        private void Start()      => currentPosition = startingPosition;
        private void OnValidate() => currentPosition = startingPosition;
    
    private void Update() => transform.position = (Vector3) currentPosition;*/
}
