using System;
using CubeWorld.Tiles;
using System.Collections.Generic;

namespace CubeWorld.World.Generator
{
    public class ParticleDepositionWorldGenerator : CubeWorldGenerator
    {
        private Random generator = new Random();

        private byte tileType;
        private WorldSizeRelativeValue minParticlesRV;
        private WorldSizeRelativeValue maxParticlesRV;
        private WorldSizeRelativeValue minDropsRV;
        private WorldSizeRelativeValue maxDropsRV;

        private int sidesToCheck = 9;
        private int[][] nearPositions;

        public ParticleDepositionWorldGenerator(
            WorldSizeRelativeValue minParticlesRV,
            WorldSizeRelativeValue maxParticlesRV,
            WorldSizeRelativeValue minDropsRV,
            WorldSizeRelativeValue maxDropsRV, 
            byte tileType)
        {
            this.minParticlesRV = minParticlesRV;
            this.maxParticlesRV = maxParticlesRV;
            this.minDropsRV = minDropsRV;
            this.maxDropsRV = maxDropsRV;
            this.tileType = tileType;

            nearPositions = new int[3][];
            nearPositions[0] = new int[3];
            nearPositions[1] = new int[3];
            nearPositions[2] = new int[3];
        }

        public override bool Generate(CubeWorld world)
        {
            int drops = generator.Next(minDropsRV.EvaluateInt(world), maxDropsRV.EvaluateInt(world));
			
			TileManager tileManager = world.tileManager;

            for (int i = 0; i < drops; i++)
            {
                int x = generator.Next(2, tileManager.sizeX - 2);
                int z = generator.Next(2, tileManager.sizeZ - 2);
                sidesToCheck = generator.Next(3, 10);

                int particles = generator.Next(minParticlesRV.EvaluateInt(world), maxParticlesRV.EvaluateInt(world));

                for (int j = 0; j < particles; j++)
                    DropParticle(x, z, world);
            }

            return true;
        }

        private void DropParticle(int x, int z, CubeWorld world)
        {
			TileManager tileManager = world.tileManager;
			
            bool particleDropped = false;

            while (x > 0 && z > 0 && x < tileManager.sizeX - 1 && z < tileManager.sizeZ - 1 && particleDropped == false)
            {
                int middleY = 0;

                for (int ax = -1; ax <= 1; ax++)
                {
                    for (int az = -1; az <= 1; az++)
                    {
                        int topY = tileManager.GetTopPosition(x + ax, z + az);
                        if (tileManager.GetTileType(new TilePosition(x + ax, topY, z + az)) != TileDefinition.EMPTY_TILE_TYPE)
                            topY++;

                        nearPositions[ax + 1][az + 1] = topY;

                        if (ax == 0 && az == 0)
                            middleY = topY;
                    }
                }

                int finalY = middleY;
                int finalX = x;
                int finalZ = z;

                int from = generator.Next(9);
                bool finalPosition = true;

                for (int j = 0; j < sidesToCheck; j++)
                {
                    int ax = (((j + from) % 9) % 3) - 1;
                    int az = (((j + from) % 9) / 3) - 1;

                    int y = nearPositions[ax + 1][az + 1];
                    if (y < finalY)
                    {
                        finalX = x + ax;
                        finalZ = z + az;
                        finalY = y;
                        finalPosition = false;
                        break;
                    }
                }

                if (finalY >= tileManager.sizeY)
                    break;

                if (finalPosition)
                {
                    tileManager.SetTileType(new TilePosition(finalX, finalY, finalZ), tileType);
                    particleDropped = true;
                }
                else
                {
                    x = finalX;
                    z = finalZ;
                }
            }

            if (particleDropped == false && tileManager.IsValidTile(new TilePosition(x, 0, z)))
            {
                int y = tileManager.GetTopPosition(x, z);

                TilePosition pos = new TilePosition(x, y, z);

                if (tileManager.GetTileType(pos) != TileDefinition.EMPTY_TILE_TYPE)
                    pos.y++;
                if (y < tileManager.sizeY)
                    tileManager.SetTileType(pos, tileType);
            }
        }

        public override string ToString()
        {
            return "Creating mountains";
        }
    }
}