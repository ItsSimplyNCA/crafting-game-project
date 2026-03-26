using System;
using System.Collections.Generic;
using Game.Gameplay.WorldEntities.Data;
using Game.Gameplay.WorldEntities.Runtime;
using Game.Shared;
using UnityEngine;

namespace Game.Gameplay.Grid.Runtime {
    [DisallowMultipleComponent]
    public sealed class GridService : MonoBehaviour {
        [Header("Grid Size")]
        [SerializeField, Min(1)] private int width = 100;
        [SerializeField, Min(1)] private int length = 100;
        [SerializeField, Min(1)] private int maxHeight = 8;

        [Header("Cell Metrics")]
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField, Min(0.1f)] private float cellHeight = 1f;

        [Header("Origin")]
        [SerializeField] private Vector3 origin = Vector3.zero;

        private readonly GridOccupancyMap occupancyMap = new();
        private readonly HashSet<PlaceableRuntime> registeredPlaceables = new();

        public event Action<PlaceableRuntime> PlaceableRegistered;
        public event Action<PlaceableRuntime> PlaceableRemoved;
        public event Action GridCleared;

        public int Width => width;
        public int Length => length;
        public int MaxHeight => maxHeight;
        public float CellSize => cellSize;
        public float CellHeight => cellHeight;
        public Vector3 Origin => origin;

        public int OccupiedCellCount => occupancyMap.OccupiedCellCount;
        public IReadOnlyCollection<PlaceableRuntime> RegisteredPlaceables => registeredPlaceables;

        private void OnValidate() {
            width = Mathf.Max(GameConstants.MinGridSize, width);
            length = Mathf.Max(GameConstants.MinGridSize, length);
            maxHeight = Mathf.Max(GameConstants.MinHeight, maxHeight);
            cellSize = Mathf.Max(0.1f, cellSize);
            cellHeight = Mathf.Max(0.1f, cellHeight);
        }

        public Vector3Int WorldToCell(Vector3 worldPosition) {
            Vector3 local = worldPosition - origin;

            return new Vector3Int(
                Mathf.FloorToInt(local.x / cellSize),
                Mathf.FloorToInt(local.y / cellHeight),
                Mathf.FloorToInt(local.z / cellSize)
            );
        }

        public Vector3 GetPlacementWorldPosition(PlaceableDefinition definition, Vector3Int originCell, int rotationQuarterTurns) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            Vector2Int rotatedSize = definition.GetRotatedSize(rotationQuarterTurns);
            int finalHeight = Mathf.Max(GameConstants.MinHeight, definition.Height);

            return origin + new Vector3(
                (originCell.x + rotatedSize.x * 0.5f) * cellSize,
                (originCell.y + finalHeight * 0.5f) * cellHeight,
                (originCell.z + rotatedSize.y * 0.5f) * cellSize
            );
        }

        public bool IsInsideGrid(GridCellCoord cell) {
            return cell.X >= 0 && cell.X < width &&
                cell.Z >= 0 && cell.Z < length &&
                cell.Y >= 0 && cell.Z < maxHeight;
        }

        public PlacementFootprint BuildFootprint(PlaceableDefinition definition, Vector3Int originCell, int rotationQuarterTurns) {
            return PlacementFootprint.FromDefinition(definition, originCell, rotationQuarterTurns);
        }

        public bool CanPlace(PlaceableDefinition definition, Vector3Int originCell, int rotationQuarterTurns) {
            if (!TryBuildValidatedFootprint(definition, originCell, rotationQuarterTurns, out PlacementFootprint footprint)) {
                return false;
            }

            return occupancyMap.CanOccupy(footprint);
        }

        public bool CanRegister(PlaceableRuntime runtime) {
            if (!TryBuildValidatedFootprint(runtime, out PlacementFootprint footprint)) {
                return false;
            }

            return occupancyMap.CanOccupy(footprint);
        }

        public bool TryRegister(PlaceableRuntime runtime) {
            if (runtime == null) return false;
            if (registeredPlaceables.Contains(runtime)) return false;
            if (!TryBuildValidatedFootprint(runtime, out PlacementFootprint footprint)) return false;
            if (!occupancyMap.TryOccupy(runtime, footprint)) return false;

            registeredPlaceables.Add(runtime);
            PlaceableRegistered?.Invoke(runtime);
            return true;
        }

        public bool Remove(PlaceableRuntime runtime) {
            if (runtime == null) return false;

            bool released = occupancyMap.Release(runtime);
            bool removed = registeredPlaceables.Remove(runtime);

            if (released || removed) {
                PlaceableRemoved?.Invoke(runtime);
                return true;
            }

            return false;
        }

        public bool TryGetPlaceableAtCell(Vector3Int cell, out PlaceableRuntime runtime) {
            return occupancyMap.TryGetOccupant(new GridCellCoord(cell), out runtime);
        }

        public bool TryGetPlaceableAtCell(GridCellCoord cell, out PlaceableRuntime runtime) {
            return occupancyMap.TryGetOccupant(cell, out runtime);
        }

        public bool IsCellOccupied(Vector3Int cell) {
            return occupancyMap.IsOccupied(new GridCellCoord(cell));
        }

        public IEnumerable<KeyValuePair<GridCellCoord, PlaceableRuntime>> EnumerateOccupiedCells() {
            return occupancyMap.EnumerateOccupiedCells();
        }

        public void ClearGrid() {
            occupancyMap.Clear();
            registeredPlaceables.Clear();
            GridCleared?.Invoke();
        }

        private bool TryBuildValidatedFootprint(
            PlaceableDefinition definition,
            Vector3Int originCell,
            int rotationQuarterTurns,
            out PlacementFootprint footprint
        ) {
            footprint = null;

            if (definition == null) return false;

            footprint = BuildFootprint(definition, originCell, rotationQuarterTurns);
            return IsFootprintInsideGrid(footprint);
        }

        private bool TryBuildValidatedFootprint(PlaceableRuntime runtime, out PlacementFootprint footprint) {
            footprint = null;

            if (runtime == null || runtime.Definition == null) return false;

            footprint = PlacementFootprint.FromRuntime(runtime);
            return IsFootprintInsideGrid(footprint);
        }

        private bool IsFootprintInsideGrid(PlacementFootprint footprint) {
            if (footprint == null || footprint.CellCount == 0) return false;

            IReadOnlyList<GridCellCoord> cells = footprint.OccupiedCells;

            for (int i = 0; i < cells.Count; i++) {
                if (!IsInsideGrid(cells[i])) return false;
            }

            return true;
        }
    }
}