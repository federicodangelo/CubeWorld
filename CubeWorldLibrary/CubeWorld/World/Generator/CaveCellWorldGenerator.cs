using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class CaveCellWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue iterationsRV;
        private WorldSizeRelativeValue minRadiusRV;
        private WorldSizeRelativeValue maxRadiusRV;
        private WorldSizeRelativeValue minDepthRV;
        private WorldSizeRelativeValue maxDepthRV;

        public CaveCellWorldGenerator(
            WorldSizeRelativeValue iterationsRV,
            WorldSizeRelativeValue minRadiusRV,
            WorldSizeRelativeValue maxRadiusRV,
            WorldSizeRelativeValue minDepthRV,
            WorldSizeRelativeValue maxDepthRV)
        {
            this.iterationsRV = iterationsRV;
            this.minRadiusRV = minRadiusRV;
            this.maxRadiusRV = maxRadiusRV;
            this.minDepthRV = minDepthRV;
            this.maxDepthRV = maxDepthRV;
        }

        public override bool Generate(CubeWorld world)
        {
            int iterations = iterationsRV.EvaluateInt(world);
            int minRadius = minRadiusRV.EvaluateInt(world);
            int maxRadius = maxRadiusRV.EvaluateInt(world);
            int minDepth = minDepthRV.EvaluateInt(world);
            int maxDepth = maxDepthRV.EvaluateInt(world);
			
			TileManager tileManager = world.tileManager;

            Random random = new Random();

            for (int i = 0; i < iterations; i++)
            {
                int startX = random.Next(maxRadius + 1, tileManager.sizeX - maxRadius - 1);
                int startZ = random.Next(maxRadius + 1, tileManager.sizeZ - maxRadius - 1);
                int startY = tileManager.GetTopPosition(startX, startZ);

                int radius = minRadius;

                int dx = random.Next(32) - 16;
                int dy = -random.Next(8);
                int dz = random.Next(32) - 16;

                int xf = startX << 4;
                int yf = startY << 4;
                int zf = startZ << 4;

                int depth = random.Next(minDepth, maxDepth);

                for (int d = 0; d < depth; d++)
                {
                    radius += random.Next(3) - 1;

                    if (radius > maxRadius)
                        radius = maxRadius;
                    else if (radius < minRadius)
                        radius = minRadius;

                    TilePosition center = new TilePosition(xf >> 4, yf >> 4, zf >> 4);

                    if (tileManager.IsValidTile(center - new TilePosition(radius + 1, radius + 1, radius + 1)) &&
                        tileManager.IsValidTile(center + new TilePosition(radius + 1, radius + 1, radius + 1)))
                    {
                        for (int x = center.x - radius; x <= center.x + radius; x++)
                            for (int y = center.y - radius; y <= center.y + radius; y++)
                                for (int z = center.z - radius; z <= center.z + radius; z++)
                                    tileManager.SetTileType(new TilePosition(x, y, z), 0);
                    }
                    else
                        break;

                    if (random.Next(0, 100) > 85)
                    {
                        dx = random.Next(32) - 16;
                        dy = -random.Next(8);
                        dz = random.Next(32) - 16;
                    }

                    xf += dx;
                    yf += dy;
                    zf += dz;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Creating tunnels";
        }
    }
}