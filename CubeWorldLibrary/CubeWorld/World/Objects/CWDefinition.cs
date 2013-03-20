using System;
using CubeWorld.Serialization;

namespace CubeWorld.World.Objects
{
	public class CWDefinition : ISerializable
	{
		public enum DefinitionType
		{
			Tile,
			Item,
            ItemTile,
			Avatar
		}
		
		public DefinitionType type;
		public string id;
		public string description;
		
		public int energy;

        public CWDefinition()
        {
        }

        public CWDefinition(DefinitionType definitionType)
		{
			this.type = definitionType;
		}

        public virtual void Serialize(Serializer serializer)
        {
            serializer.SerializeEnum(ref type, "type");
            serializer.Serialize(ref id, "id");
            serializer.Serialize(ref description, "description");
            serializer.Serialize(ref energy, "energy");
        }
    }
}

