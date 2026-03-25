using System;
using System.Collections.Generic;
using Game.Gameplay.WorldEntities.Data;
using Game.Gameplay.WorldEntities.Runtime;
using Game.Shared;
using UnityEngine;

namespace Game.Gameplay.Grid.Runtime {
    [Serializable]
    public sealed class PlacementFootprint {
        private readonly GridCellCoord[] occupiedCells;

        public GridCellCoord OriginCell { get; }
        public Vector2Int BaseSize { get; }
        public Vector2Int RotatedSize { get; }
        public int Height { get; }
        public int RotationQuarterTurns { get; }

        public int CellCount => occupiedCells.Length;
        public IReadOnlyList<GridCellCoord> OccupiedCells => occupiedCells;

        private PlacementFootprint(
            GridCellCoord originCell,
            Vector2Int baseSize,
            Vector2Int rotatedSize,
            int height,
            int rotationQuarterTurns,
            GridCellCoord[] occupiedCells
        ) {
            OriginCell = originCell;
            BaseSize = baseSize;
            RotatedSize = rotatedSize;
            Height = Mathf.Max(GameConstants.MinHeight, height);
            RotationQuarterTurns = RotationUtility.NormalizeQuarterTurns(rotationQuarterTurns);
            this.occupiedCells = occupiedCells ?? Array.Empty<GridCellCoord>();
        }

        public static PlacementFootprint FromDefinition(
            PlaceableDefinition definition,
            Vector3Int originCell,
            int rotationQuarterTurns
        ) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            Vector3Int[] rawCells = definition.GetOccupiedCells(originCell, rotationQuarterTurns);
            GridCellCoord[] convertedCells = new GridCellCoord[rawCells.Length];

            for (int i = 0; i < rawCells.Length; i++) {
                convertedCells[i] = new GridCellCoord(rawCells[i]);
            }

            return new PlacementFootprint(
                originCell,
                definition.Size,
                definition.GetRotatedSize(rotationQuarterTurns),
                definition.Height,
                rotationQuarterTurns,
                convertedCells
            );
        }

        public static PlacementFootprint FromRuntime(PlaceableRuntime runtime) {
            if (runtime == null) {
                throw new ArgumentNullException(nameof(runtime));
            }

            return FromDefinition(runtime.Definition, runtime.OriginCell, runtime.RotationQuarterTurns);
        }

        public bool Contains(GridCellCoord cell) {
            for (int i = 0; i < occupiedCells.Length; i++) {
                if (occupiedCells[i] == cell) return true;
            }

            return false;
        }

        public GridCellCoord[] CopyCells() {
            GridCellCoord[] copy = new GridCellCoord[occupiedCells.Length];

            for (int i = 0; i < occupiedCells.Length; i++) {
                copy[i] = occupiedCells[i];
            }

            return copy;
        }

        public override string ToString() {
            return $"Origin={OriginCell}, Size={RotatedSize}, Height={Height}, Rot={RotationQuarterTurns}, Cells={CellCount}";
        }
    }
}