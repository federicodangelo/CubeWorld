using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class DepositWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;
        private WorldSizeRelativeValue minRadiusRV;
        private WorldSizeRelativeValue maxRadiusRV;
        private WorldSizeRelativeValue iterationsRV;
        private byte overTile;
        private byte tileType;
        private bool allowEmptyAbove;

        public DepositWorldGenerator(
            WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV,
            WorldSizeRelativeValue iterationsRV,
            WorldSizeRelativeValue minRadiusRV,
            WorldSizeRelativeValue maxRadiusRV,
            byte overTile,
            byte tileType,
            bool allowEmptyAbove)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
            this.iterationsRV = iterationsRV;
            this.minRadiusRV = minRadiusRV;
            this.maxRadiusRV = maxRadiusRV;
            this.overTile = overTile;
            this.tileType = tileType;
            this.allowEmptyAbove = allowEmptyAbove;
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
                int cx = random.Next(maxRadius + 1, tileManager.sizeX - maxRadius - 1);
                int cy = random.Next(maxRadius + 1, tileManager.sizeY - maxRadius - 1);
                int cz = random.Next(maxRadius + 1, tileManager.sizeZ - maxRadius - 1);

                int radiusX = random.Next(minRadius, maxRadius);
                int radiusY = random.Next(minRadius, maxRadius);
                int radiusZ = random.Next(minRadius, maxRadius);

                for (int x = cx - radiusX; x < cx + radiusX; x++)
                    for (int z = cz - radiusZ; z < cz + radiusZ; z++)
                        for (int y = cy - radiusY; y < cy + radiusY; y++)
                        {
                            if (y >= fromY && y < toY)
                            {
                                if (random.NextDouble() > 0.25)
                                {
                                    if (allowEmptyAbove || y == tileManager.sizeY || tileManager.GetTileType(new TilePosition(x, y + 1, z)) != TileDefinition.EMPTY_TILE_TYPE)
                                        if (tileManager.GetTileType(new TilePosition(x, y, z)) == overTile)
                                            tileManager.SetTileType(new TilePosition(x, y, z), tileType);
                                }
                            }
                        }
            }

            return true;
        }

        public override string ToString()
        {
            return "Adding deposits";
        }
    }
}