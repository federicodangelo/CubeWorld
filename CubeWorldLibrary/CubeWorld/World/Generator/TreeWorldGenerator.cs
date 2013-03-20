using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class TreeWorldGenerator : CubeWorldGenerator
    {
        private Random generator = new Random();
        private byte tileTypeTrunk;
        private byte tileTypeLeaves;
        private byte overTileType;
        private WorldSizeRelativeValue minRV;
        private WorldSizeRelativeValue maxRV;

        private int minTrunkHeight = 2;
        private int maxTrunkHeight = 2;

        private int minLeavesHeight = 3;
        private int maxLeavesHeight = 4;

        private int minLeavesRadius = 1;
        private int maxLeavesRadius = 2;

        public TreeWorldGenerator(
            WorldSizeRelativeValue minRV,
            WorldSizeRelativeValue maxRV, 
            int minTrunkHeight,
            int maxTrunkHeight,
            int minLeavesHeight,
            int maxLeavesHeight,
            int minLeavesRadius,
            int maxLeavesRadius,
            byte overTileType, 
            byte tileTypeTrunk, 
            byte tileTypeLeaves)
        {
            this.minRV = minRV;
            this.maxRV = maxRV;

            this.minTrunkHeight = minTrunkHeight;
            this.maxTrunkHeight = maxTrunkHeight;
            this.minLeavesHeight = minLeavesHeight;
            this.maxLeavesHeight = maxLeavesHeight;
            this.minLeavesRadius = minLeavesRadius;
            this.maxLeavesRadius = maxLeavesRadius;

            this.overTileType = overTileType;
            this.tileTypeLeaves = tileTypeLeaves;
            this.tileTypeTrunk = tileTypeTrunk;
        }

        public override bool Generate(CubeWorld world)
        {
            int trees = generator.Next(minRV.EvaluateInt(world), maxRV.EvaluateInt(world));

			TileManager tileManager = world.tileManager;
			
            for (int i = 0; i < trees; i++)
            {
                int x = generator.Next(maxLeavesRadius * 2, tileManager.sizeX - maxLeavesRadius * 2);
                int z = generator.Next(maxLeavesRadius * 2, tileManager.sizeZ - maxLeavesRadius * 2);

                PlantTree(x, z, world);
            }

            return true;
        }

        private void PlantTree(int x, int z, CubeWorld world)
        {
			TileManager tileManager = world.tileManager;
			
			int topY = tileManager.GetTopPosition(x, z);

            if (topY < tileManager.sizeY - (maxTrunkHeight + maxLeavesHeight) * 2)
            {
                if (tileManager.GetTileType(new TilePosition(x, topY, z)) == overTileType)
                {
                    topY++;

                    int trunkHeight = generator.Next(minTrunkHeight, maxTrunkHeight + 1);
                    int leavesHeight = generator.Next(minLeavesHeight, maxLeavesHeight + 1);
                    int leavesRadius = generator.Next(minLeavesRadius, maxLeavesRadius + 1);

                    int treeHeight = trunkHeight + leavesHeight;

                    if (FreeSpaceAvailable(x, topY, z, leavesRadius, treeHeight, world))
                        CreateTree(x, topY, z, trunkHeight, leavesHeight, leavesRadius, world);
                }
            }
        }

        private bool FreeSpaceAvailable(int x, int y, int z, int radius, int height, CubeWorld world)
        {
			TileManager tileManager = world.tileManager;
			
            for (int dx = x - radius; dx <= x + radius; dx++)
                for (int dz = z - radius; dz <= z + radius; dz++)
                    for (int dy = y; dy < y + height; dy++)
                        if (tileManager.GetTileType(new TilePosition(dx, dy, dz)) != TileDefinition.EMPTY_TILE_TYPE)
                            return false;

            return true;
        }

        private void CreateTree(int treeX, int treeY, int treeZ, int trunkHeight, int leavesHeight, int leavesRadius, CubeWorld world)
        {
			TileManager tileManager = world.tileManager;
			
            int minX = treeX - leavesRadius;
            int maxX = treeX + leavesRadius;

            int minZ = treeZ - leavesRadius;
            int maxZ = treeZ + leavesRadius;

            int minY = treeY + trunkHeight;
            int maxY = treeY + trunkHeight + leavesHeight;

            //Leaves
            for (int y = minY; y < maxY; y++)
                for (int x = minX; x <= maxX; x++)
                    for (int z = minZ; z <= maxZ; z++)
                        tileManager.SetTileType(new TilePosition(x, y, z), tileTypeLeaves);

            //Remove extreme leaves
            tileManager.SetTileType(new TilePosition(minX, minY, minZ), 0);
            tileManager.SetTileType(new TilePosition(minX, minY, maxZ), 0);
            tileManager.SetTileType(new TilePosition(maxX, minY, maxZ), 0);
            tileManager.SetTileType(new TilePosition(maxX, minY, minZ), 0);
            tileManager.SetTileType(new TilePosition(minX, maxY - 1, minZ), 0);
            tileManager.SetTileType(new TilePosition(minX, maxY - 1, maxZ), 0);
            tileManager.SetTileType(new TilePosition(maxX, maxY - 1, maxZ), 0);
            tileManager.SetTileType(new TilePosition(maxX, maxY - 1, minZ), 0);

            //Trunk
            for (int y = treeY; y < treeY + trunkHeight + leavesHeight / 2; y++)
                tileManager.SetTileType(new TilePosition(treeX, y, treeZ), tileTypeTrunk);
        }

        public override string ToString()
        {
            return "Generating trees";
        }
    }
}