using System;
using CubeWorld.World.Generator;

namespace CubeWorld.Configuration
{
    public class ConfigWorldGenerator
    {
        public string name;

        public ConfigSurroundings surroundings = new ConfigSurroundings();

        public CubeWorldGenerator generator;
    }
}
