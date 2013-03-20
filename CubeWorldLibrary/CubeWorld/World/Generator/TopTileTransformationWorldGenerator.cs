using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class TopTileTransformationWorldGenerator : CubeWorldGenerator
    {
        private byte fromType;
        private byte toType;
        private float probability;

        public TopTileTransformationWorldGenerator(byte fromType, byte toType, float probability)
        {
            this.fromType = fromType;
            this.toType = toType;
            this.probability = probability;
        }

        public override bool Generate(CubeWorld world)
        {
            Random rnd = new Random();

			TileManager tileManager = world.tileManager;
			
            for (int x = 0; x < tileManager.sizeX; x++)
            {
                for (int z = 0; z < tileManager.sizeZ; z++)
                {
                    int y = tileManager.GetTopPosition(x, z);
                    TilePosition pos = new TilePosition(x, y, z);

                    if (tileManager.GetTileType(pos) == fromType && rnd.NextDouble() <= probability)
                        tileManager.SetTileType(pos, toType);
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Transforming terrain";
        }
    }
}