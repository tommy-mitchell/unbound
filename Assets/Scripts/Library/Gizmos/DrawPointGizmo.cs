using UnityEngine;

public class DrawPointGizmo : MonoBehaviour
{
    public float radius = .25f;

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, radius, 0)); // line down
        Gizmos.DrawLine(transform.position - new Vector3(radius / 2, 0, 0), transform.position + new Vector3(radius / 2, 0, 0)); // line perpendicular

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, radius / 2, 0)); // line up
    }
}
