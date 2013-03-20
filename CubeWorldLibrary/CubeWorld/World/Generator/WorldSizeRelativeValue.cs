using System;
using System.Collections.Generic;

namespace CubeWorld.World.Generator
{
    public class WorldSizeRelativeValue
    {
        private enum RelativeTo
        {
            Nothing,
            SizeX,
            SizeY,
            SizeZ,
            SizeXY,
            SizeXZ,
            SizeYZ,
            SizeVolumen
        }

        private RelativeTo relativeTo;
        private float value;

        private string expression;

        public string Expression
        {
            get { return this.expression; }
        }

        public WorldSizeRelativeValue(string expression)
        {
            expression = expression.ToLower().Trim();
            this.expression = expression;

            if (expression.IndexOf('*') <= 0)
            {
                if (Char.IsDigit(expression[0]) == true)
                {
                    relativeTo = RelativeTo.Nothing;
                    value = float.Parse(expression, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    relativeTo = (RelativeTo)System.Enum.Parse(typeof(RelativeTo), expression, true);
                    value = 1.0f;
                }
            }
            else
            {
                string[] strs = expression.Split('*');
                if (strs.Length != 2)
                    throw new Exception("Invalid relative value string: " + expression);

                relativeTo = (RelativeTo)System.Enum.Parse(typeof(RelativeTo), strs[0], true);

                value = float.Parse(strs[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public int EvaluateInt(CubeWorld world)
        {
            return (int) Evaluate(world);
        }

        public float Evaluate(CubeWorld world)
        {
            switch (relativeTo)
            {
                case RelativeTo.Nothing:
                    return value;

                case RelativeTo.SizeX:
                    return world.sizeX * value;
                case RelativeTo.SizeY:
                    return world.sizeY * value;
                case RelativeTo.SizeZ:
                    return world.sizeZ * value;

                case RelativeTo.SizeXY:
                    return (world.sizeX * world.sizeY) * value;
                case RelativeTo.SizeXZ:
                    return (world.sizeX * world.sizeZ) * value;
                case RelativeTo.SizeYZ:
                    return (world.sizeY * world.sizeZ) * value;

                case RelativeTo.SizeVolumen:
                    return (world.sizeX * world.sizeY * world.sizeZ) * value;
            }

            return 0;
        }
    }
}
