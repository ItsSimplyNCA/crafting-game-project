using System;
using Game.Gameplay.WorldEntities.Data;
using Game.Shared;
using UnityEngine;

namespace Game.Gameplay.WorldEntities.Runtime {
    [Serializable]
    public sealed class PlaceableRuntime {
        private readonly string runtimeId;
        private readonly PlaceableDefinition definition;

        private Vector3Int originCell;
        private int rotationQuarterTurns;

        public string RuntimeId => runtimeId;
        public PlaceableDefinition Definition => definition;
        public Vector3Int OriginCell => originCell;
        public int RotationQuarterTurns => rotationQuarterTurns;

        public Vector2Int RotatedSize => definition.GetRotatedSize(rotationQuarterTurns);
        public int Height => definition.Height;
        public Quaternion WorldRotation => definition.GetWorldRotation(rotationQuarterTurns);

        public PlaceableRuntime(PlaceableDefinition definition, Vector3Int originCell, int rotationQuarterTurns) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            runtimeId = Guid.NewGuid().ToString("N");
            this.definition = definition;

            SetPlacement(originCell, rotationQuarterTurns);
        }

        public void SetPlacement(Vector3Int newOriginCell, int newRotationQuarterTurns) {
            originCell = newOriginCell;
            rotationQuarterTurns = RotationUtility.NormalizeQuarterTurns(newRotationQuarterTurns);
        }

        public Vector3Int[] GetOccupiedCells() {
            return definition.GetOccupiedCells(originCell, rotationQuarterTurns);
        }

        public override string ToString()
        {
            return $"{definition.DisplayName} [{runtimeId}] @ {originCell} rot={rotationQuarterTurns}";
        }
    }
}