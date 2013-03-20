using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class PlainWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;
        private byte tileType;

        public PlainWorldGenerator(
            WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV, 
            byte tileType)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
            this.tileType = tileType;
        }

        public override bool Generate(CubeWorld world)
        {
            int fromY = fromYRV.EvaluateInt(world);
            int toY = toYRV.EvaluateInt(world);
			
			TileManager tileManager = world.tileManager;
			
            for (int x = 0; x < tileManager.sizeX; x++)
                for (int z = 0; z < tileManager.sizeZ; z++)
                    for (int y = fromY; y < toY; y++)
                    {
                        TilePosition pos = new TilePosition(x, y, z);
                        if (tileManager.GetTileType(pos) == TileDefinition.EMPTY_TILE_TYPE)
                            tileManager.SetTileType(pos, tileType);
                    }

            return true;
        }

        public override string ToString()
        {
            return "Filling terrain";
        }
    }
}