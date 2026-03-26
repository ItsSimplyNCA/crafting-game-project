using System;
using System.Collections.Generic;
using Game.Gameplay.WorldEntities.Runtime;

namespace Game.Gameplay.Grid.Runtime {
    [Serializable]
    public sealed class GridOccupancyMap {
        private readonly Dictionary<GridCellCoord, PlaceableRuntime> occupantByCell = new();
        private readonly Dictionary<PlaceableRuntime, GridCellCoord[]> cellsByRuntime = new();

        public int OccupiedCellCount => occupantByCell.Count;
        public int RegisteredObjectCount => cellsByRuntime.Count;

        public bool IsOccupied(GridCellCoord cell) {
            return occupantByCell.ContainsKey(cell);
        }

        public bool TryGetOccupant(GridCellCoord cell, out PlaceableRuntime runtime) {
            return occupantByCell.TryGetValue(cell, out runtime);
        }

        public bool CanOccupy(PlacementFootprint footprint) {
            if (footprint == null || footprint.CellCount == 0) return false;

            IReadOnlyList<GridCellCoord> cells = footprint.OccupiedCells;

            for (int i = 0; i < cells.Count; i++) {
                if (occupantByCell.ContainsKey(cells[i])) return false;
            }

            return true;
        }

        public bool TryOccupy(PlaceableRuntime runtime, PlacementFootprint footprint) {
            if (runtime == null || footprint == null) return false;
            if (cellsByRuntime.ContainsKey(runtime)) return false;
            if (!CanOccupy(footprint)) return false;

            GridCellCoord[] cells = footprint.CopyCells();
            cellsByRuntime.Add(runtime, cells);

            for (int i = 0; i < cells.Length; i++) {
                occupantByCell[cells[i]] = runtime;
            }

            return true;
        }

        public bool Release(PlaceableRuntime runtime) {
            if (runtime == null) return false;
            if (!cellsByRuntime.TryGetValue(runtime, out GridCellCoord[] cells)) return false;

            for (int i = 0; i < cells.Length; i++) {
                if (occupantByCell.TryGetValue(cells[i], out PlaceableRuntime existing) && existing == runtime) {
                    occupantByCell.Remove(cells[i]);
                }
            }

            cellsByRuntime.Remove(runtime);
            return true;
        }

        public bool TryGetCells(PlaceableRuntime runtime, out IReadOnlyList<GridCellCoord> cells) {
            if (runtime != null && cellsByRuntime.TryGetValue(runtime, out GridCellCoord[] rawCells)) {
                cells = rawCells;
                return true;
            }

            cells = null;
            return false;
        }

        public IEnumerable<KeyValuePair<GridCellCoord, PlaceableRuntime>> EnumerateOccupiedCells() {
            foreach (KeyValuePair<GridCellCoord, PlaceableRuntime> pair in occupantByCell) {
                yield return pair;
            }
        }

        public void Clear() {
            occupantByCell.Clear();
            cellsByRuntime.Clear();
        }
    }
}