using System;
using CubeWorld.World;
using CubeWorld.World.Objects;
using CubeWorld.Serialization;

namespace CubeWorld.Items
{
	public class ItemDefinition : CWDefinition
	{
		public ItemDefinition()
			: base(DefinitionType.Item)
		{
			
		}

        protected ItemDefinition(DefinitionType type)
            : base(type)
        {

        }

        public CWVisualDefinition visualDefinition;
		
		public int durability;
		public int damage;
		public bool setOnFire;

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref visualDefinition, "visualDefinition");
            serializer.Serialize(ref durability, "durability");
            serializer.Serialize(ref damage, "damage");
            serializer.Serialize(ref setOnFire, "setOnFire");
        }
	}
}

