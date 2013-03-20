using System.Collections.Generic;
using System;
using System.IO;

namespace CubeWorld.Tiles
{
    public struct TilePosition : IEquatable<TilePosition>
    {
        public int x, y, z;

        public int this[int index]
        {
            set { if (index == 0) x = value; else if (index == 1) y = value; else if (index == 2) z = value; }
            get { if (index == 0) return x; else if (index == 1) return y; else if (index == 2) return z; return 0; }
        }

        public TilePosition(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int GetDistance()
        {
            return (int) Math.Sqrt(x * x + y * y + z * z);
        }

        public int GetDistanceSquared()
        {
            return x * x + y * y + z * z;
        }

        public int GetSumComponents()
        {
            return x + y + z;
        }

        public override bool Equals(object obj)
        {
            return obj is TilePosition && this == (TilePosition)obj;
        }

        public override int GetHashCode()
        {
            return x | (y << 8) | (z << 16);
        }

        public static TilePosition operator +(TilePosition left, TilePosition right)
        {
            return new TilePosition(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public static TilePosition operator -(TilePosition left, TilePosition right)
        {
            return new TilePosition(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        public static TilePosition operator *(TilePosition left, int v)
        {
            return new TilePosition(left.x * v, left.y * v, left.z * v);
        }

        public static TilePosition operator *(TilePosition left, TilePosition right)
        {
            return new TilePosition(left.x * right.x, left.y * right.y, left.z * right.z);
        }

        public static bool operator !=(TilePosition left, TilePosition right)
        {
            return left.x != right.x || left.y != right.y || left.z != right.z;
        }

        public static bool operator ==(TilePosition left, TilePosition right)
        {
            return left.x == right.x && left.y == right.y && left.z == right.z;
        }

        public bool Equals(TilePosition other)
        {
            return other.x == x && other.y == y && other.z == z;
        }

        public override string ToString()
        {
            return x + "," + y + "," + z;
        }
    }
}