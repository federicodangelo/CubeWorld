using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class HoleWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue iterationsRV;
        private WorldSizeRelativeValue minRadiusRV;
        private WorldSizeRelativeValue maxRadiusRV;
        private WorldSizeRelativeValue minDepthRV;
        private WorldSizeRelativeValue maxDepthRV;

        public HoleWorldGenerator(
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
                int cx = random.Next(maxRadius + 1, tileManager.sizeX - maxRadius - 1);
                int cz = random.Next(maxRadius + 1, tileManager.sizeZ - maxRadius - 1);

                int radiusX = random.Next(minRadius, maxRadius);
                int radiusZ = random.Next(minRadius, maxRadius);

                int depth = random.Next(minDepth, maxDepth);

                int sum = 0;

                for (int x = cx - radiusX; x < cx + radiusX; x++)
                    for (int z = cz - radiusZ; z < cz + radiusZ; z++)
                        sum += tileManager.GetTopPosition(x, z);

                int avg = sum / (radiusX * 2 * radiusZ * 2);

                if (avg - depth > 1)
                {
                    for (int x = cx - radiusX; x < cx + radiusX; x++)
                    {
                        for (int z = cz - radiusZ; z < cz + radiusZ; z++)
                        {
                            int y = tileManager.GetTopPosition(x, z);

                            if (y > avg - depth)
                            {
                                //Remove tiles
                                for (int dy = (avg - depth) + 1; dy <= y; dy++)
                                    tileManager.SetTileType(new TilePosition(x, dy, z), 0);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Digging holes";
        }
    }
}