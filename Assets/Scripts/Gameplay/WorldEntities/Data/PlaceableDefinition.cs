using Game.Shared;
using UnityEngine;

namespace Game.Gameplay.WorldEntities.Data {
    [CreateAssetMenu(fileName = "PlaceableDefinition", menuName = GameConstants.CreatePlaceableMenu)]
    public sealed class PlaceableDefinition : ScriptableObject {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;

        [Header("Visual")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;

        [Header("Placement")]
        [SerializeField] private FootprintDefinition footprint;
        [SerializeField] private bool rotatable = true;
        [SerializeField] private bool canBeRemoved = true;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public FootprintDefinition Footprint => footprint;
        public bool Rotatable => rotatable;
        public bool CanBeRemoved => canBeRemoved;

        public Vector2Int Size => footprint != null ? footprint.Size : Vector2Int.one;
        public int Height => footprint != null ? footprint.Height : GameConstants.MinHeight;

        public Vector2Int GetRotatedSize(int quarterTurnsClockwise) {
            if (footprint == null) return Vector2Int.one;
            if (!rotatable) return footprint.GetRotatedSize(0);
            return footprint.GetRotatedSize(quarterTurnsClockwise);
        }

        public Vector3Int[] GetOccupiedCells(Vector3Int originCell, int quarterTurnsClockwise) {
            if (footprint == null) return new[] { originCell };
            int turns = rotatable ? quarterTurnsClockwise : 0;
            return footprint.GetOccupiedCells(originCell, turns);
        }

        public Quaternion GetWorldRotation(int quarterTurnsClockwise) {
            int turns = rotatable ? quarterTurnsClockwise : 0;
            return RotationUtility.ToQuaternion((Direction4)RotationUtility.NormalizeQuarterTurns(turns));
        }

        private void OnValidate() {
            if (string.IsNullOrWhiteSpace(displayName)) {
                displayName = name;
            }
        }
    }
}