using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class RoguelikeWorldGenerator : CubeWorldGenerator
    {
        public const int LEVEL_HEIGHT = 8;

        public TilePosition playerStartPosition;


        public RoguelikeWorldGenerator(CubeWorld world)
        {
            playerStartPosition =
                new TilePosition(
                    LEVEL_HEIGHT / 2,
                    world.sizeY - LEVEL_HEIGHT + 3,
                    LEVEL_HEIGHT / 2);
        }

        private byte tileTypeHardRock;
        //private byte tileTypeHardGlass;
        private byte tileTypeCeilingGlass;

        private void InitTileTypes(CubeWorld world)
        {
            tileTypeHardRock = world.tileManager.GetTileDefinitionById("hard_rock").tileType;
            //tileTypeHardGlass = world.tileManager.GetTileDefinitionById("hard_glass").tileType;
            tileTypeCeilingGlass = world.tileManager.GetTileDefinitionById("ceiling_glass").tileType;
        }

        public override bool Generate(CubeWorld world)
        {
            InitTileTypes(world);

            CreateBasicLevels(world);

            return true;
        }

        private void CreateBasicLevels(CubeWorld world)
        {
            for (int level = 0; level < world.sizeY / LEVEL_HEIGHT; level++)
                CreateLevel(world, level);

            //Create ceiling
            for (int x = 0; x < world.sizeX; x++)
                for (int z = 0; z < world.sizeZ; z++)
                    if (x % 3 == 0 && z % 3 == 0)
                        world.tileManager.SetTileType(new TilePosition(x, world.sizeY - 1, z), tileTypeCeilingGlass);
                    else
                        world.tileManager.SetTileType(new TilePosition(x, world.sizeY - 1, z), tileTypeHardRock);
        }

        private void CreateLevel(CubeWorld world, int level)
        {
            int floor_y = level * LEVEL_HEIGHT;
            int ceiling_y = level * (LEVEL_HEIGHT + 1);

            //Create floor
            for (int x = 0; x < world.sizeX; x++)
                for (int z = 0; z < world.sizeZ; z++)
                    world.tileManager.SetTileType(new TilePosition(x, floor_y, z), tileTypeHardRock);

            //Create walls
            for (int y = floor_y; y < ceiling_y; y++)
                for (int z = 0; z < world.sizeZ; z++)
                {
                    world.tileManager.SetTileType(new TilePosition(0, y, z), tileTypeHardRock);
                    world.tileManager.SetTileType(new TilePosition(world.sizeX - 1, y, z), tileTypeHardRock);
                }
            for (int y = floor_y; y < ceiling_y; y++)
                for (int x = 0; x < world.sizeX; x++)
                {
                    world.tileManager.SetTileType(new TilePosition(x, y, 0), tileTypeHardRock);
                    world.tileManager.SetTileType(new TilePosition(x, y, world.sizeZ - 1), tileTypeHardRock);
                }
        }

        public override string ToString()
        {
            return "Maze";
        }
    }
}
