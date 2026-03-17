using UnityEngine;

public class PlacedObject : MonoBehaviour {
    public Vector2Int size = Vector2Int.one;
    public int height = 1;

    [HideInInspector] public Vector3Int originCell;
    [HideInInspector] public int rotationSteps;

    private void OnValidate() {
        size.x = Mathf.Max(1, size.x);
        size.y = Mathf.Max(1, size.y);
        height = Mathf.Max(1, height);
    }
}
