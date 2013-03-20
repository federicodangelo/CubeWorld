using System;
using CubeWorld.World;
using CubeWorld.World.Objects;
using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Avatars
{
	public class AvatarDefinition : CWDefinition
	{
        public TilePosition sizeInTiles;
        public AvatarPartDefinition[] parts;

		public AvatarDefinition()
			: base(DefinitionType.Avatar)
		{
			
		}

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref sizeInTiles, "sizeInTiles");
            serializer.Serialize(ref parts, "parts");
        }
	}
}

