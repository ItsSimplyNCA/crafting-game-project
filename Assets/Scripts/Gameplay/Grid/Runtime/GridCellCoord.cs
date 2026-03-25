using System;
using UnityEngine;

namespace Game.Gameplay.Grid.Runtime {
    [Serializable]
    public readonly struct GridCellCoord : IEquatable<GridCellCoord> {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public GridCellCoord(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }

        public GridCellCoord(Vector3Int value) {
            X = value.x;
            Y = value.y;
            Z = value.z;
        }

        public Vector3Int ToVector3Int() {
            return new Vector3Int(X, Y, Z);
        }

        public bool Equals(GridCellCoord other) {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj) {
            return obj is GridCellCoord other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Z;
                return hash;
            }
        }

        public static bool operator ==(GridCellCoord left, GridCellCoord right) {
            return left.Equals(right);
        }

        public static bool operator !=(GridCellCoord left, GridCellCoord right) {
            return !left.Equals(right);
        }

        public static implicit operator GridCellCoord(Vector3Int value) {
            return new GridCellCoord(value);
        }

        public static implicit operator Vector3Int(GridCellCoord value) {
            return value.ToVector3Int();
        }

        public override string ToString() {
            return $"({X}, {Y}, {Z})";
        }
    }
}