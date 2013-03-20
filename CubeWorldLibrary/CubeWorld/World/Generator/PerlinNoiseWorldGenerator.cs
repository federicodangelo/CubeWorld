using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class PerlinNoiseWorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;
        private byte tileType;
		private int octaves;
		private float freq;

        public PerlinNoiseWorldGenerator(
			WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV,
			int octaves,
			float freq,
			byte tileType)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
            this.tileType = tileType;
			this.octaves = octaves;
			this.freq = freq;
        }

        public override bool Generate(CubeWorld world)
        {
			TileManager tileManager = world.tileManager;
			
            int fromY = fromYRV.EvaluateInt(world);
            int toY = toYRV.EvaluateInt(world);
			
			//C3DPerlinNoise.Init();
			int seed = new Random().Next();
			
			int cutY = (int) (fromY + (toY - fromY) * 0.7f);
			
            for (int x = 0; x < tileManager.sizeX; x++)
			{
                for (int z = 0; z < tileManager.sizeZ; z++)
				{
					float d = GetRandomHeight(x, z, 1.0f, freq, 1.0f, 0.5f, octaves, seed);
					
					int y = (int) (fromY + d * (toY - fromY)) - 1;
					
					if (y > cutY)
						y = cutY;
					else if (y < fromY - 1)
						y = fromY - 1;

                    while (y >= fromY)
                        tileManager.SetTileType(new TilePosition(x, y--, z), tileType);
				}
			}
			
			NoiseInitialized = false;

            return true;
        }
		
		//Gets the value for a specific X and Y coordinate
        private float GetRandomHeight(float X, float Y, float MaxHeight,
            float Frequency, float Amplitude, float Persistance,
            int Octaves, int Seed)
        {
            GenerateNoise(Seed);
            float FinalValue = 0.0f;
            for (int i = 0; i < Octaves; ++i)
            {
                FinalValue += GetSmoothNoise(X * Frequency, Y * Frequency) * Amplitude;
                Frequency *= 2.0f;
                Amplitude *= Persistance;
            }
            if (FinalValue < -1.0f)
            {
                FinalValue = -1.0f;
            }
            else if (FinalValue > 1.0f)
            {
                FinalValue = 1.0f;
            }
            return FinalValue * MaxHeight;
        }
		
        //This function is a simple bilinear filtering function which is good enough.
		//You can do cosine or bicubic if you really want though.
        private float GetSmoothNoise(float X, float Y)
        {
            float FractionX = X - (int)X;
            float FractionY = Y - (int)Y;
            int X1 = ((int)X+MAX_WIDTH)%MAX_WIDTH;
            int Y1 = ((int)Y+MAX_HEIGHT)%MAX_HEIGHT;
            //for cool art deco looking images, do +1 for X2 and Y2 instead of -1...
            int X2 = ((int)X + MAX_WIDTH - 1) % MAX_WIDTH;
            int Y2 = ((int)Y + MAX_HEIGHT - 1) % MAX_HEIGHT;
            float FinalValue = 0.0f;
            FinalValue += FractionX * FractionY * Noise[X1, Y1];
            FinalValue += FractionX * (1-FractionY) * Noise[X1, Y2];
            FinalValue += (1-FractionX) * FractionY * Noise[X2, Y1];
            FinalValue += (1-FractionX) * (1-FractionY) * Noise[X2, Y2];
            return FinalValue;
        }



        private const int MAX_WIDTH = 256;
        private const int MAX_HEIGHT = 256;
        private float[,] Noise;
		private bool NoiseInitialized;
		
        private void GenerateNoise(int Seed)
        {
            if (NoiseInitialized)                //A boolean variable in the class to make sure we only do this once
                return;
            Noise = new float[MAX_WIDTH, MAX_HEIGHT];    //Create the noise table where MAX_WIDTH and MAX_HEIGHT are set to some value>0
            Random RandomGenerator = new Random(Seed); //Create the random generator (just using C#'s at the moment)
            for (int x = 0; x < MAX_WIDTH; ++x)
            {
                for (int y = 0; y < MAX_HEIGHT; ++y)
                {
                    Noise[x, y] = ((float)(RandomGenerator.NextDouble()) - 0.5f) * 2.0f;  //Generate noise between -1 and 1
                }
            }
            NoiseInitialized = true;
        }		

        public override string ToString()
        {
            return "Filling terrain with perlin noise";
        }
    }
}