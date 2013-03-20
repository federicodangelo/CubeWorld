using System;
using CubeWorld.Tiles;
using System.Collections.Generic;

namespace CubeWorld.World.Generator
{
    public class PlainRandomWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;
        private TileTypeProbability[] probabilities;
        private int probabilityRange;

        public struct TileTypeProbability
        {
            public int probability;
            public byte tileType;

            public TileTypeProbability(int probability, byte tileType)
            {
                this.probability = probability;
                this.tileType = tileType;
            }
        }

        public PlainRandomWorldGenerator(
            WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV,
            TileTypeProbability[] originalProbabilities)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;

            List<TileTypeProbability> probs = new List<TileTypeProbability>();

            probs.AddRange(originalProbabilities);

            probs.Sort(SortByProbability);

            probabilityRange = 0;
            foreach (TileTypeProbability p in probs)
                probabilityRange += p.probability;

            for (int i = 1; i < probs.Count; i++)
            {
                TileTypeProbability p = probs[i];
                p.probability += probs[i - 1].probability;
                probs[i] = p;
            }

            this.probabilities = probs.ToArray();
        }

        static private int SortByProbability(TileTypeProbability a, TileTypeProbability b)
        {
            return b.probability.CompareTo(a.probability);
        }

        public override bool Generate(CubeWorld world)
        {
            int fromY = fromYRV.EvaluateInt(world);
            int toY = toYRV.EvaluateInt(world);

			TileManager tileManager = world.tileManager;
			
            Random rnd = new Random();

            for (int x = 0; x < tileManager.sizeX; x++)
                for (int z = 0; z < tileManager.sizeZ; z++)
                    for (int y = fromY; y < toY; y++)
                    {
                        TilePosition pos = new TilePosition(x, y, z);
                        if (tileManager.GetTileType(pos) == TileDefinition.EMPTY_TILE_TYPE)
                        {
                            int n = rnd.Next(0, probabilityRange);
                            foreach (TileTypeProbability s in probabilities)
                            {
                                if (n < s.probability)
                                {
                                    tileManager.SetTileType(pos, s.tileType);
                                    break;
                                }
                            }
                        }
                    }

            return true;
        }

        public override string ToString()
        {
            return "Filling terrain with random materials";
        }
    }
}