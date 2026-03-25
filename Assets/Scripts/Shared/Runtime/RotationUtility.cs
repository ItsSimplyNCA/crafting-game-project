using UnityEngine;

namespace Game.Shared {
    public static class RotationUtility {
        public static int NormalizeQuarterTurns(int quarterTurns) {
            int normalized = quarterTurns % GameConstants.QuarterTurnCount;

            if (normalized < 0) {
                normalized += GameConstants.QuarterTurnCount;
            }

            return normalized;
        }

        public static Direction4 Normalize(Direction4 direction) {
            return (Direction4)NormalizeQuarterTurns((int)direction);
        }

        public static Direction4 Rotate(Direction4 direction, int quarterTurnsClockwise) {
            int value = NormalizeQuarterTurns((int)direction + quarterTurnsClockwise);
            return (Direction4)value;
        }

        public static Quaternion ToQuaternion(Direction4 direction) {
            return Quaternion.Euler(0f, (int)Normalize(direction) * 90f, 0f);
        }

        public static Vector3Int ToGridOffset(Direction4 direction) {
            switch (Normalize(direction)) {
                case Direction4.North:
                    return new Vector3Int(0, 0, 1);
                case Direction4.East:
                    return new Vector3Int(1, 0, 0);
                case Direction4.South:
                    return new Vector3Int(0, 0, -1);
                case Direction4.West:
                    return new Vector3Int(-1, 0, 0);
                default:
                    return new Vector3Int(0, 0, 1);
            }
        }

        public static Vector2Int RotateSize(Vector2Int size, int quarterTurnsClockwise) {
            int turns = NormalizeQuarterTurns(quarterTurnsClockwise);
            return (turns % 2 == 0) ? size : new Vector2Int(size.y, size.x);
        }
    }
}