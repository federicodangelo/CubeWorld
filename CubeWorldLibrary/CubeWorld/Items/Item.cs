using System;
using CubeWorld.Utils;
using CubeWorld.World;
using CubeWorld.World.Objects;

namespace CubeWorld.Items
{
	public class Item : CWObject
	{
		public ItemDefinition itemDefinition;
		
        public Item(CubeWorld.World.CubeWorld world, ItemDefinition itemDefinition, int objectId) 
            : base(objectId)
		{
			this.world = world;
			this.definition = itemDefinition;
			this.itemDefinition = itemDefinition;
		}
    }
}

