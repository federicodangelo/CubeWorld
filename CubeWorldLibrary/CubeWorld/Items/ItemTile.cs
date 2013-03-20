using System;
using CubeWorld.Utils;
using CubeWorld.World;
using CubeWorld.World.Objects;
using CubeWorld.Tiles;

namespace CubeWorld.Items
{
    public class ItemTile : Item
    {
        public ItemTileDefinition itemTileDefinition;

        public ItemTile(CubeWorld.World.CubeWorld world, ItemTileDefinition itemTileDefinition, int objectId)
            : base(world, itemTileDefinition, objectId)
        {
            this.itemTileDefinition = itemTileDefinition;
        }
    }
}
