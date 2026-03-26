using System;
using Game.Gameplay.Grid.Runtime;
using Game.Gameplay.WorldEntities.Data;
using UnityEngine;

namespace Game.Gameplay.Building.Runtime {
    public enum PlacementValidationStatus {
        Valid = 0,
        MissingGridService = 1,
        MissingDefinition = 2,
        MissingFootprint = 3,
        OutsideGrid = 4,
        Occupied = 5
    }

    [Serializable]
    public readonly struct PlacementValidationResult {
        public PlacementValidationStatus Status { get; }
        public string Message { get; }
        public PlacementFootprint Footprint { get; }

        public bool IsValid => Status == PlacementValidationStatus.Valid;

        public PlacementValidationResult(
            PlacementValidationStatus status,
            string message,
            PlacementFootprint footprint
        ) {
            Status = status;
            Message = message ?? string.Empty;
            Footprint = footprint;
        }
        
        public static PlacementValidationResult Valid (PlacementFootprint footprint) {
            return new PlacementValidationResult(
                PlacementValidationStatus.Valid,
                string.Empty,
                footprint
            );
        }
        public static PlacementValidationResult Invalid(
            PlacementValidationStatus status,
            string message,
            PlacementFootprint footprint = null
        ) {
            return new PlacementValidationResult(status, message, footprint);
        }

        public override string ToString() {
            return IsValid ? "Valid placement." : $"{Status}: {Message}";
        }
    }

    public sealed class PlacementValidator {
        private readonly GridService gridService;

        public PlacementValidator(GridService gridService) {
            this.gridService = gridService;
        }

        public bool CanPlace(PlaceableDefinition definition, Vector3Int originCell, int rotationQuarterTurns) {
            return Validate(definition, originCell, rotationQuarterTurns).IsValid;
        }

        public PlacementValidationResult Validate(
            PlaceableDefinition definition,
            Vector3Int originCell,
            int rotationQuarterTurns
        ) {
            if (gridService == null) {
                return PlacementValidationResult.Invalid(
                    PlacementValidationStatus.MissingGridService,
                    "Nincs GridService példány."
                );
            }

            if (definition == null) {
                return PlacementValidationResult.Invalid(
                    PlacementValidationStatus.MissingDefinition,
                    "A placeable definition null."
                );
            }

            if (definition.Footprint == null) {
                return PlacementValidationResult.Invalid(
                    PlacementValidationStatus.MissingFootprint,
                    $"A(z) '{definition.DisplayName}' definitionhöz nincs footprint rendelve."
                );
            }

            PlacementFootprint footprint = gridService.BuildFootprint(
                definition,
                originCell,
                rotationQuarterTurns
            );

            for (int i = 0; i < footprint.OccupiedCells.Count; i++) {
                GridCellCoord cell = footprint.OccupiedCells[i];

                if (!gridService.IsInsideGrid(cell)) {
                    return PlacementValidationResult.Invalid(
                        PlacementValidationStatus.OutsideGrid,
                        $"A footprint kilóg a gridből. Problémás cella: {cell}",
                        footprint
                    );
                }

                if (gridService.TryGetPlaceableAtCell(cell, out _)) {
                    return PlacementValidationResult.Invalid(
                        PlacementValidationStatus.Occupied,
                        $"A cella már foglalt: {cell}",
                        footprint
                    );
                }
            }

            return PlacementValidationResult.Valid(footprint);
        }
    }
}