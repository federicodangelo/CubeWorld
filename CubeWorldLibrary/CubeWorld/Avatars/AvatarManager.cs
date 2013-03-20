using System;
using CubeWorld.World;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.Utils;
using CubeWorld.Avatars.Components;
using SourceCode.CubeWorld.Utils;

namespace CubeWorld.Avatars
{
	public class AvatarManager
	{
		private CubeWorld.World.CubeWorld world;
		
		public AvatarDefinition[] avatarDefinitions;
        public Player player;
		
		private List<Avatar> avatars = new List<Avatar>();

        public Avatar[] Avatars
        {
            get { return avatars.ToArray(); }
        }
		
		public AvatarManager (CubeWorld.World.CubeWorld world)
		{
			this.world = world;
		}
		
		public void Create(AvatarDefinition[] avatarDefinitions)
		{
			this.avatarDefinitions = avatarDefinitions;
		}

        public Avatar CreateAvatar(AvatarDefinition avatarDefinition, int objectId, Vector3 pos, bool dispatchWorldListener)
        {
            Avatar avatar;

            if (avatarDefinition.id == "player")
            {
                avatar = new Player(world, avatarDefinition, objectId);
                this.player = avatar as Player;
            }
            else if (avatarDefinition.id == "player_remote")
            {
                avatar = new Player(world, avatarDefinition, objectId);
            }
            else
            {
                avatar = new Avatar(world, avatarDefinition, objectId);
                avatar.AddComponent(new AvatarComponentIA());
            }

            avatar.position = pos;

            avatars.Add(avatar);

            if (dispatchWorldListener)
                world.cwListener.CreateObject(avatar);

            return avatar;
        }

        public AvatarDefinition GetAvatarDefinitionById(string id)
        {
            foreach (AvatarDefinition avatarDefinition in avatarDefinitions)
                if (avatarDefinition.id == id)
                    return avatarDefinition;

            return null;
        }
		
		public void Update(float deltaTime)
		{
			foreach(Avatar avatar in avatars)
				avatar.Update(deltaTime);
		}
		
		public void Clear()
		{
			foreach(Avatar avatar in avatars)
				avatar.Clear();
			avatars.Clear();
			
			player = null;
		}


        public bool IsTileBlockedByAnyAvatar(TilePosition tile)
        {
            foreach (Avatar avatar in avatars)
            {
                TilePosition tileMin = Graphics.Vector3ToTilePosition(avatar.position - new Vector3(Graphics.QUART_TILE_SIZE, Graphics.QUART_TILE_SIZE, Graphics.QUART_TILE_SIZE));
                TilePosition tileMax = Graphics.Vector3ToTilePosition(avatar.position + new Vector3(Graphics.QUART_TILE_SIZE, Graphics.TILE_SIZE + Graphics.QUART_TILE_SIZE, Graphics.QUART_TILE_SIZE));

                if (tile.x >= tileMin.x && tile.x <= tileMax.x &&
                    tile.y >= tileMin.y && tile.y <= tileMax.y &&
                    tile.z >= tileMin.z && tile.z <= tileMax.z)
                {
                    return true;
                }
            }

            return false;
        }


        public void Save(System.IO.BinaryWriter bw)
        {
            bw.Write(avatars.Count);

            foreach (Avatar avatar in avatars)
            {
                bw.Write(avatar.definition.id);
                bw.Write(avatar.objectId);
                SerializationUtils.Write(bw, avatar.position);
                avatar.Save(bw);
            }
        }


        public void Load(System.IO.BinaryReader br)
        {
            int n = br.ReadInt32();

            for (int i = 0; i < n; i++)
            {
                string avatarDefinitionId = br.ReadString();
                int objectId = br.ReadInt32();
                Vector3 position = SerializationUtils.ReadVector3(br);

                AvatarDefinition avatarDefinition = GetAvatarDefinitionById(avatarDefinitionId);

                Avatar avatar = CreateAvatar(avatarDefinition, objectId, position, false);

                avatar.Load(br);

                world.cwListener.CreateObject(avatar);
            }
        }

        public void DestroyAvatar(Avatar avatar)
        {
            avatar.Clear();
            avatars.Remove(avatar);
            world.cwListener.DestroyObject(avatar);
        }

        public Avatar GetAvatarByObjectId(int objectId)
        {
            foreach (Avatar avatar in avatars)
                if (avatar.objectId == objectId)
                    return avatar;

            return null;
        }
    }
}

