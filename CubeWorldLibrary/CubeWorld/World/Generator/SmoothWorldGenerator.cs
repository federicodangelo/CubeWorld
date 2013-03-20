using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class SmoothWorldGenerator : CubeWorldGenerator
    {
        private byte tileType;
        private WorldSizeRelativeValue iterationsRV;
        private WorldSizeRelativeValue minRadiusRV;
        private WorldSizeRelativeValue maxRadiusRV;

        public SmoothWorldGenerator(
            byte tileType,
            WorldSizeRelativeValue iterationsRV,
            WorldSizeRelativeValue minRadiusRV,
            WorldSizeRelativeValue maxRadiusRV)
        {
            this.tileType = tileType;
            this.iterationsRV = iterationsRV;
            this.minRadiusRV = minRadiusRV;
            this.maxRadiusRV = maxRadiusRV;
        }

        public override bool Generate(CubeWorld world)
        {
            Random random = new Random();

            int iterations = iterationsRV.EvaluateInt(world);
            int minRadius = minRadiusRV.EvaluateInt(world);
            int maxRadius = maxRadiusRV.EvaluateInt(world);

			TileManager tileManager = world.tileManager;
			
            for (int i = 0; i < iterations; i++)
            {
                int cx = random.Next(maxRadius + 1, tileManager.sizeX - maxRadius - 1);
                int cz = random.Next(maxRadius + 1, tileManager.sizeZ - maxRadius - 1);

                int radius = random.Next(minRadius, maxRadius);

                int sum = 0;

                for (int x = cx - radius; x < cx + radius; x++)
                    for (int z = cz - radius; z < cz + radius; z++)
                        sum += tileManager.GetTopPosition(x, z);

                int avg = sum / (radius * 2 * radius * 2);

                for (int x = cx - radius; x < cx + radius; x++)
                {
                    for (int z = cz - radius; z < cz + radius; z++)
                    {
                        int y = tileManager.GetTopPosition(x, z);

                        if (avg - y < -1 || avg - y > 1)
                        {
                            if (y > avg)
                            {
                                //Remove tiles
                                for (int dy = avg + 1; dy <= y; dy++)
                                    tileManager.SetTileType(new TilePosition(x, dy, z), 0);
                            }
                            else if (y < avg)
                            {
                                //Add tiles
                                for (int dy = y; dy <= avg; dy++)
                                    tileManager.SetTileType(new TilePosition(x, dy, z), tileType);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Smoothing terrain";
        }
    }
}