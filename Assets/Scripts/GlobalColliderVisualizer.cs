using UnityEngine;

public class GlobalColliderVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);

        BoxCollider[] colliders = Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);

        foreach (var col in colliders)
        {
            if (!col.enabled) continue;

            Gizmos.matrix = col.transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}