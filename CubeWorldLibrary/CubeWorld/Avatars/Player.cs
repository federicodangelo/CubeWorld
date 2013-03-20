using System;
using CubeWorld.Utils;
using System.Collections.Generic;
using CubeWorld.Items;
using CubeWorld.Tiles;
using SourceCode.CubeWorld.Utils;

namespace CubeWorld.Avatars
{
	public class Player : Avatar
	{
        public const float HEAD_POSITION = 1.25f;

        public Inventory inventory;
        public TilePosition resetPosition;
		
        public Player(CubeWorld.World.CubeWorld world, AvatarDefinition avatarDefinition, int avatarId)
			: base(world, avatarDefinition, avatarId)
        {
            inventory = new Inventory(this);
        }

        public void ResetPosition()
        {
            int top = resetPosition.y;

            while (world.tileManager.GetTileType(new TilePosition(resetPosition.x, top, resetPosition.z)) != TileDefinition.EMPTY_TILE_TYPE)
                top++;

            position = Graphics.TilePositionToVector3(resetPosition.x, top, resetPosition.z);
        }

        public override void Save(System.IO.BinaryWriter bw)
        {
            base.Save(bw);

            SerializationUtils.Write(bw, resetPosition);
            inventory.Save(bw);
        }

        public override void Load(System.IO.BinaryReader br)
        {
            base.Load(br);

            resetPosition = SerializationUtils.ReadTilePosition(br);
            inventory.Load(br);
        }
	}
}

