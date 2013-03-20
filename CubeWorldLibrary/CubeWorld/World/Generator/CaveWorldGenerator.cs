using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class CaveWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue iterationsRV;
        private WorldSizeRelativeValue minRadiusRV;
        private WorldSizeRelativeValue maxRadiusRV;
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;

        public CaveWorldGenerator(
            WorldSizeRelativeValue iterationsRV, 
            WorldSizeRelativeValue minRadiusRV, 
            WorldSizeRelativeValue maxRadiusRV, 
            WorldSizeRelativeValue fromYRV, 
            WorldSizeRelativeValue toYRV)
        {
            this.iterationsRV = iterationsRV;
            this.minRadiusRV = minRadiusRV;
            this.maxRadiusRV = maxRadiusRV;
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
        }

        public override bool Generate(CubeWorld world)
        {
            int iterations = iterationsRV.EvaluateInt(world);
            int minRadius = minRadiusRV.EvaluateInt(world);
            int maxRadius = maxRadiusRV.EvaluateInt(world);
            int fromY = fromYRV.EvaluateInt(world); 
            int toY = toYRV.EvaluateInt(world);
			
			TileManager tileManager = world.tileManager;

            Random random = new Random();

            for (int i = 0; i < iterations; i++)
            {
                TilePosition center = new TilePosition(
                    random.Next(maxRadius + 1, world.sizeX - maxRadius - 1),
                    random.Next(fromY, toY),
                    random.Next(maxRadius + 1, world.sizeZ - maxRadius - 1));

                TilePosition radius = new TilePosition(
                    random.Next(minRadius, maxRadius),
                    random.Next(minRadius, maxRadius),
                    random.Next(minRadius, maxRadius));

                TilePosition position = new TilePosition();

                int axis = random.Next(3);

                for (int a1 = center[axis] - radius[axis]; a1 <= center[axis] + radius[axis]; a1++)
                {
                    position[axis] = a1;

                    int axis2 = (axis + random.Next(2) + 1) % 3;
                    int axis3;
                    if (axis == 0 && axis2 == 2 || axis == 2 && axis2 == 0)
                        axis3 = 1;
                    else if (axis == 0 && axis2 == 1 || axis == 1 && axis2 == 0)
                        axis3 = 2;
                    else
                        axis3 = 0;

                    radius[axis2] += random.Next(5) - 2;

                    if (radius[axis2] < minRadius)
                        radius[axis2] = minRadius;
                    else if (radius[axis2] > maxRadius)
                        radius[axis2] = maxRadius;

                    for (int a2 = center[axis2] - radius[axis2]; a2 <= center[axis2] + radius[axis2]; a2++)
                    {
                        position[axis2] = a2;

                        radius[axis3] += random.Next(5) - 2;

                        if (radius[axis3] < minRadius)
                            radius[axis3] = minRadius;
                        else if (radius[axis3] > maxRadius)
                            radius[axis3] = maxRadius;

                        for (int a3 = center[axis3] - radius[axis3]; a3 <= center[axis3] + radius[axis3]; a3++)
                        {
                            position[axis3] = a3;

                            if (position[1] < fromY)
                                position[1] = fromY;
                            else if (position[1] > toY)
                                position[1] = toY;

                            tileManager.SetTileType(position, 0);
                        }
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Making caves";
        }
    }
}