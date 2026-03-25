using Game.Shared;
using UnityEngine;

namespace Game.Gameplay.WorldEntities.Data {
    [CreateAssetMenu(fileName = "FootprintDefinition", menuName = GameConstants.CreateFrootprintMenu)]
    public sealed class FootprintDefinition : ScriptableObject {
        [Header("Grid Size")]
        [SerializeField] private Vector2Int size = Vector2Int.one;

        [SerializeField, Min(GameConstants.MinHeight)]
        private int height = GameConstants.MinHeight;

        public Vector2Int Size => size;
        public int Height => height;

        public Vector2Int GetRotatedSize(int quarterTurnsClockwise) {
            return RotationUtility.RotateSize(size, quarterTurnsClockwise);
        }

        public Vector3Int[] GetOccupiedCells(Vector3Int originCell, int quarterTurnsClockwise) {
            Vector2Int rotatedSize = GetRotatedSize(quarterTurnsClockwise);
            int finalHeight = Mathf.Max(GameConstants.MinHeight, height);

            Vector3Int[] cells = new Vector3Int[rotatedSize.x * rotatedSize.y * finalHeight];
            int index = 0;

            for (int y = 0; y < finalHeight; y++) {
                for (int x = 0; x < rotatedSize.x; x++) {
                    for (int z = 0; z < rotatedSize.y; z++) {
                        cells[index++] = new Vector3Int(
                            originCell.x + x,
                            originCell.y + y,
                            originCell.z + z
                        );
                    }
                }
            }

            return cells;
        }

        private void OnValidate() {
            size.x = Mathf.Max(GameConstants.MinGridSize, size.x);
            size.y = Mathf.Max(GameConstants.MinGridSize, size.y);
            height = Mathf.Max(GameConstants.MinHeight, height);
        }
    }
}