using System;
using Game.Gameplay.WorldEntities.Data;
using Game.Shared;
using UnityEngine;

namespace Game.Gameplay.Building.Runtime {
    [Serializable]
    public sealed class BuildSelectionState {
        [SerializeField] private PlaceableDefinition selectedDefinition;
        [SerializeField] private int rotationQuarterTurns;

        public event Action<BuildSelectionState> Changed;

        public PlaceableDefinition SelectedDefinition => selectedDefinition;
        public int RotationQuarterTurns => RotationUtility.NormalizeQuarterTurns(rotationQuarterTurns);
        public bool HasSelection => selectedDefinition != null;

        public void Select(PlaceableDefinition definition, bool resetRotation = true) {
            bool changed = selectedDefinition != definition;

            selectedDefinition = definition;

            if (resetRotation) {
                rotationQuarterTurns = 0;
                changed = true;
            } else {
                rotationQuarterTurns = RotationUtility.NormalizeQuarterTurns(rotationQuarterTurns);
            }

            if (changed) NotifyChanged();
        }

        public void Clear() {
            if (selectedDefinition == null && RotationQuarterTurns == 0) return;

            selectedDefinition = null;
            rotationQuarterTurns = 0;
            NotifyChanged();
        }

        public void SetRotation(int quarterTurns) {
            int normalized = RotationUtility.NormalizeQuarterTurns(quarterTurns);

            if (rotationQuarterTurns == normalized) return;

            rotationQuarterTurns = normalized;
            NotifyChanged();
        }

        public void RotateClockwise() {
            if (selectedDefinition != null && !selectedDefinition.Rotatable) return;

            rotationQuarterTurns = RotationUtility.NormalizeQuarterTurns(rotationQuarterTurns + 1);
            NotifyChanged();
        }

        public void RotateCounterClockwise() {
            if (selectedDefinition != null && !selectedDefinition.Rotatable) return;

            rotationQuarterTurns = RotationUtility.NormalizeQuarterTurns(rotationQuarterTurns - 1);
            NotifyChanged();
        }

        private void NotifyChanged() {
            Changed?.Invoke(this);
        }
    }
}