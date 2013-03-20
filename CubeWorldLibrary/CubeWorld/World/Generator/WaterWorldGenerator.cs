using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class WaterWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;

        private byte waterTileType;

        public WaterWorldGenerator(
            WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV,
            byte waterTileType)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
            this.waterTileType = waterTileType;
        }

        public override bool Generate(CubeWorld world)
        {
            int fromY = fromYRV.EvaluateInt(world);
            int toY = toYRV.EvaluateInt(world);

			TileManager tileManager = world.tileManager;
			
            for (int x = 0; x < tileManager.sizeX; x++)
            {
                for (int z = 0; z < tileManager.sizeZ; z++)
                {
                    int y = tileManager.GetTopPosition(x, z);

                    if (tileManager.GetTileType(new TilePosition(x, y, z)) != TileDefinition.EMPTY_TILE_TYPE)
                        y++;

                    if (y >= fromY)
                    {
                        for (int i = y; i < toY; i++)
                            tileManager.SetTileType(new TilePosition(x, i, z), waterTileType);
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Adding water";
        }
    }
}