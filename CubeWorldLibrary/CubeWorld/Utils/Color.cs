namespace CubeWorld.Utils
{
    public struct Color
    {
        public float r, g, b, a;

        public Color(float r, float g, float b) 
            : this(r, g, b, 0.0f)
        {
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public override bool Equals(object obj)
        {
            return this == (Color)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Color operator *(Color left, float v)
        {
            return new Color(left.r * v, left.g * v, left.b * v, left.a);
        }

        public static bool operator !=(Color left, Color right)
        {
            return left.r != right.r || left.g != right.g || left.b != right.b || left.a != right.a;
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.r == right.r && left.g == right.g && left.b == right.b && left.a == right.a;
        }
    }
}
