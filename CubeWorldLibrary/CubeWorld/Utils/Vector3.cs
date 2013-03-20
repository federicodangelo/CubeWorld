using System;
using System.IO;
namespace CubeWorld.Utils
{
    public struct Vector3
    {
        public float x, y, z;

        public float this[int index]
        {
            set { if (index == 0) x = value; else if (index == 1) y = value; else if (index == 2) z = value; }
            get { if (index == 0) return x; else if (index == 1) return y; else if (index == 2) return z; return 0; }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float magnitude
        {
            get
            {
                return (float) Math.Sqrt(x * x + y * y + z * z);
            }
        }

        public Vector3 normalized
        {
            get
            {
                return this * (1.0f / this.magnitude);
            }
        }

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        public static Vector3 operator *(Vector3 left, float v)
        {
            return new Vector3(left.x * v, left.y * v, left.z * v);
        }
        
    }
}