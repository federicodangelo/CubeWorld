using System;
using CubeWorld.Configuration;
using CubeWorld.Tiles;
using CubeWorld.Items;
using CubeWorld.Avatars;

namespace CubeWorld.Configuration
{
    public class AvailableConfigurations
    {
        public ConfigWorldSize[] worldSizes;
        public ConfigDayInfo[] dayInfos;

        public ConfigExtraMaterials extraMaterials;
        public ConfigWorldGenerator[] worldGenerators;

        public TileDefinition[] tileDefinitions;
        public ItemDefinition[] itemDefinitions;
        public AvatarDefinition[] avatarDefinitions;
    }
}
