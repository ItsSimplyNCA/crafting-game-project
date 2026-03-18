using System.Collections.Generic;
using UnityEngine;

public class WorldGrid : MonoBehaviour {
    public static WorldGrid Instance { get; private set; }

    [Header("Grid Size")]
    [Min(1)] public int width = 100;
    [Min(1)] public int length = 100;
    [Min(1)] public int maxHeight = 8;

    [Min(0.1f)] public float cellSize = 1f;
    [Min(0.1f)] public float cellHeight = 1f;

    public Vector3 origin = Vector3.zero;

    [Header("Debug")]
    public bool drawGrid = true;
    public bool drawOccupiedCells = true;

    private readonly Dictionary<Vector3Int, PlacedObject> occupiedCells = new();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Vector3Int WorldToCell(Vector3 worldPosition) {
        Vector3 local = worldPosition - origin;

        return new Vector3Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellHeight),
            Mathf.FloorToInt(local.z / cellSize)
        );
    }

    public Vector2Int GetRotatedSize(Vector2Int size, int rotationSteps) {
        rotationSteps = Mathf.Abs(rotationSteps) % 4;
        return (rotationSteps % 2 == 0) ? size : new Vector2Int(size.y, size.x);
    }

    public Vector3 GetPlacementWorldPosition(Vector3Int originCell, Vector2Int size, int height, int rotationSteps) {
        Vector2Int rotatedSize = GetRotatedSize(size, rotationSteps);
        
        return origin + new Vector3(
            (originCell.x + rotatedSize.x * 0.5f) * cellSize,
            (originCell.y * cellHeight * 0.5f) * cellHeight,
            (originCell.z + rotatedSize.y * 0.5f) * cellSize
        );
    }

    public bool IsInsideGrid(Vector3Int cell) {
        return cell.x >= 0 && cell.x < width &&
            cell.z >= 0 && cell.z < length &&
            cell.y >= 0 && cell.y < maxHeight;
    }

    public List<Vector3Int> GetFootprintCells(Vector3Int originCell, Vector2Int size, int height, int rotationSteps) {
        List<Vector3Int> cells = new();
        Vector2Int rotatedSize = GetRotatedSize(size, rotationSteps);
        int finalHeight = Mathf.Max(1, height);

        for (int y = 0; y < finalHeight; y++) {
            for (int x = 0; x < rotatedSize.x; x++) {
                for (int z = 0; z < rotatedSize.y; z++) {
                    cells.Add(new Vector3Int(originCell.x + x, originCell.y + y, originCell.z + z));
                }
            }
        }

        return cells;
    }

    public bool CanPlace(Vector3Int originCell, Vector2Int size, int height, int rotationSteps) {
        List<Vector3Int> cells = GetFootprintCells(originCell, size, height, rotationSteps);

        foreach (Vector3Int cell in cells) {
            if (!IsInsideGrid(cell)) return false;
            if (occupiedCells.ContainsKey(cell)) return false;
        } return true;
    }

    public void RegisterObject(PlacedObject placedObject) {
        List<Vector3Int> cells = GetFootprintCells(
            placedObject.originCell,
            placedObject.size,
            placedObject.height,
            placedObject.rotationSteps
        );

        foreach (Vector3Int cell in cells) {
            occupiedCells[cell] = placedObject;
        }
    }

    public void UnregisterObject(PlacedObject placedObject) {
        List<Vector3Int> cells = GetFootprintCells(
            placedObject.originCell,
            placedObject.size,
            placedObject.height,
            placedObject.rotationSteps
        );

        foreach (Vector3Int cell in cells) {
            if (occupiedCells.TryGetValue(cell, out PlacedObject existing) && existing == placedObject) {
                occupiedCells.Remove(cell);
            }
        }
    }

    public bool TryGetObjectAtCell(Vector3Int cell, out PlacedObject placedObject) {
        return occupiedCells.TryGetValue(cell, out placedObject);
    }

    private void OnDrawGizmos() {
        if (!drawGrid) return;

        //Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.color = Color.white;

        for (int x = 0; x <= width; x++) {
            Vector3 start = origin + new Vector3(x * cellSize, 0f, 0f);
            Vector3 end = origin + new Vector3(x * cellSize, 0f, length * cellSize);
            Gizmos.DrawLine(start, end);
        }

        if (!drawOccupiedCells) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);

        foreach (Vector3Int cell in occupiedCells.Keys) {
            Vector3 center = origin + new Vector3(
                (cell.x + 0.5f) * cellSize,
                (cell.y + 0.5f) * cellHeight,
                (cell.z + 0.5f) * cellSize
            );

            Vector3 size = new Vector3(cellSize, cellHeight, cellSize);
            Gizmos.DrawCube(center, size);
        }
    }
}
