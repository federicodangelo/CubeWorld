using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeWorld.Tiles;
using CubeWorld.Utils;
using CubeWorld.Items;
using CubeWorld.Avatars;
using CubeWorld.Gameplay.Multiplayer;
using SourceCode.CubeWorld.Utils;
using System.IO;
using Ionic.Zlib;

namespace CubeWorld.Gameplay
{
    public class MultiplayerServerGameplay : BaseGameplay, IMultiplayerClientListener, IMultiplayerServerListener
    {
        public const int SERVER_PORT = 9999;

        private MultiplayerStats stats;
        private BaseGameplay baseGameplay;
        private bool createPlayer;
        private MultiplayerServer server;

        public MultiplayerServerGameplay(BaseGameplay baseGameplay, bool createPlayer) :
            base("multiplayerServer")
        {
            this.baseGameplay = baseGameplay;
            this.createPlayer = createPlayer;
        }

        public override void Init(CubeWorld.World.CubeWorld world)
        {
            base.Init(world);

            stats = MultiplayerStats.Singleton;
            stats.Reset();
            stats.serverMode = true;
            stats.connected = true;

            baseGameplay.Init(world);

            server = new MultiplayerServer(9999, this, this);
        }

        public override CubeWorld.World.Generator.GeneratorProcess Generate(CubeWorld.Configuration.Config config)
        {
            return baseGameplay.Generate(config);
        }

        public override void WorldCreated()
        {
            baseGameplay.CalculateLastObjectId();

            if (createPlayer)
            {
                Player player = (Player) CreateAvatar("player", new Vector3());
                player.resetPosition = baseGameplay.GetPlayerResetPosition();
                player.ResetPosition();
                FillPlayerInventory(player.inventory);
            }
        }

        public override void Clear()
        {
            server.Clear();
            server = null;
            baseGameplay.Clear();
            base.Clear();
        }

        public override void FillPlayerInventory(CubeWorld.Items.Inventory inventory)
        {
            baseGameplay.FillPlayerInventory(inventory);
        }

        public override bool ProcessTileDamage(TilePosition pos, Tile tile, TileDefinition tileDefinition, int damage)
        {
            return baseGameplay.ProcessTileDamage(pos, tile, tileDefinition, damage);
        }

        public override Avatar CreateAvatar(string id, Vector3 pos)
        {
            Avatar avatar = baseGameplay.CreateAvatar(id, pos);

            server.SendToEveryone(
                new MultiplayerAction(
                    MultiplayerAction.Action.AVATAR_CREATE,
                    new String[] {
                        avatar.objectId.ToString(),
                        "player_remote",
                        avatar.position.x.ToString(),
                        avatar.position.y.ToString(),
                        avatar.position.z.ToString(),
                        avatar.rotation.x.ToString(),
                        avatar.rotation.y.ToString(),
                        avatar.rotation.z.ToString()
                    }),
                avatar.objectId
            );

            return avatar;
        }

        public override void DestroyAvatar(CubeWorld.Avatars.Avatar avatar)
        {
            baseGameplay.DestroyAvatar(avatar);

            server.SendToEveryone(
                new MultiplayerAction(
                    MultiplayerAction.Action.AVATAR_DESTROY,
                    new String[] {
                        avatar.objectId.ToString()
                    }), 
                avatar.objectId
            );
        }

        public override void TileClicked(TilePosition tilePosition)
        {
            baseGameplay.TileClicked(tilePosition);
        }

        public override void DamageTile(TilePosition tilePosition, int damage)
        {
            baseGameplay.DamageTile(tilePosition, damage);
        }

        public override void TileHit(TilePosition tilePosition, ItemDefinition itemDefinition)
        {
            baseGameplay.TileHit(tilePosition, itemDefinition);
        }

        public override void CreateTile(TilePosition tileCreatePosition, byte tileType)
        {
            baseGameplay.CreateTile(tileCreatePosition, tileType);
        }

        public override Item CreateItem(ItemDefinition itemDefinition, Vector3 position)
        {
            //TODO: Serialize item creation!
            return baseGameplay.CreateItem(itemDefinition, position);
        }

        private HashSet<TilePosition> invalidatedTiles = new HashSet<TilePosition>();

        public override void TileInvalidated(TilePosition pos, bool lightRelated)
        {
            if (lightRelated == false)
                invalidatedTiles.Add(pos);
        }

        private float timerSendPosition;

        public override void Update(float deltaTime)
        {
            baseGameplay.Update(deltaTime);

            if (invalidatedTiles.Count > 0)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                bw.Write(invalidatedTiles.Count);

                foreach (TilePosition pos in invalidatedTiles)
                {
                    Tile tile = world.tileManager.GetTile(pos);

                    SerializationUtils.Write(bw, pos);
                    bw.Write(tile.Serialize());
                }

                stats.multiplayerSentTiles += invalidatedTiles.Count;

                invalidatedTiles.Clear();

                server.SendToEveryone(
                    new MultiplayerAction(
                        MultiplayerAction.Action.TILE_INVALIDATED,
                        ms.ToArray()), 
                    -1);
            }


            if (world.avatarManager.player != null)
            {
                timerSendPosition += deltaTime;

                if (timerSendPosition > 0.1f)
                {
                    server.SendToEveryone(new MultiplayerAction(
                        MultiplayerAction.Action.AVATAR_MOVE,
                        new String[] {
                                world.avatarManager.player.objectId.ToString(),
                                world.avatarManager.player.position.x.ToString(),
                                world.avatarManager.player.position.y.ToString(),
                                world.avatarManager.player.position.z.ToString(),
                                world.avatarManager.player.rotation.x.ToString(),
                                world.avatarManager.player.rotation.y.ToString(),
                                world.avatarManager.player.rotation.z.ToString()
                            }),
                            world.avatarManager.player.objectId);
							
					timerSendPosition = 0.0f;
                }
            }

            server.Update();
        }

        public void ClientConnected(MultiplayerClient client)
        {
            Player player = (Player) CreateAvatar("player_remote", Graphics.TilePositionToVector3(GetPlayerResetPosition()));
            player.resetPosition = GetPlayerResetPosition();
            FillPlayerInventory(player.inventory);

            client.id = player.objectId;

            MultiplayerAction initialDataAction = new MultiplayerAction(MultiplayerAction.Action.INITIAL_DATA, GetInitialData(client, player));
            client.AddAction(initialDataAction);

            //Send others avatars
            foreach (Avatar avatar in world.avatarManager.Avatars)
            {
                if (avatar.objectId != player.objectId)
                {
                    client.AddAction(new MultiplayerAction(
                        MultiplayerAction.Action.AVATAR_CREATE, 
                        new String[] {
                            avatar.objectId.ToString(),
                            "player_remote",
                            avatar.position.x.ToString(),
                            avatar.position.y.ToString(),
                            avatar.position.z.ToString(),
                            avatar.rotation.x.ToString(),
                            avatar.rotation.y.ToString(),
                            avatar.rotation.z.ToString()
                        }));
                }
            }

            stats.multiplayerConnectedClients++;
        }

        private byte[] GetInitialData(MultiplayerClient client, Player player)
        {
            return world.SaveMultiplayer(player);
        }

        public void ClientDisconnected(MultiplayerClient client)
        {
            stats.multiplayerConnectedClients--;

            DestroyAvatar(world.avatarManager.GetAvatarByObjectId(client.id));
        }

        public void ClientActionReceived(MultiplayerClient client, MultiplayerAction action)
        {
            switch (action.action)
            {
                case MultiplayerAction.Action.AVATAR_MOVE:
                {
                    int objectId = Int32.Parse(action.GetParameter(0));
                    Vector3 pos = new Vector3(
                        Single.Parse(action.GetParameter(1)),
                        Single.Parse(action.GetParameter(2)),
                        Single.Parse(action.GetParameter(3)));
                    Vector3 rot = new Vector3(
                        Single.Parse(action.GetParameter(4)),
                        Single.Parse(action.GetParameter(5)),
                        Single.Parse(action.GetParameter(6)));

                    if (objectId == client.id)
                    {
                        Avatar avatar = world.avatarManager.GetAvatarByObjectId(objectId);

                        avatar.position = pos;
                        avatar.rotation = rot;
                        
                        server.SendToEveryone(action, client.id);
                    }
                    break;
                }

                case MultiplayerAction.Action.TILE_CREATE:
                {
                    byte tileType = byte.Parse(action.GetParameter(0));
                    TilePosition pos = new TilePosition(
                        Int32.Parse(action.GetParameter(1)),
                        Int32.Parse(action.GetParameter(2)),
                        Int32.Parse(action.GetParameter(3)));

                    CreateTile(pos, tileType);
                    break;
                }

                case MultiplayerAction.Action.TILE_CLICKED:
                {
                    TilePosition pos = new TilePosition(
                        Int32.Parse(action.GetParameter(0)),
                        Int32.Parse(action.GetParameter(1)),
                        Int32.Parse(action.GetParameter(2)));
                    this.TileClicked(pos);
                    break;
                }

                case MultiplayerAction.Action.TILE_HIT:
                {
                    string itemId = action.GetParameter(0);

                    TilePosition pos = new TilePosition(
                        Int32.Parse(action.GetParameter(1)),
                        Int32.Parse(action.GetParameter(2)),
                        Int32.Parse(action.GetParameter(3)));

                    this.TileHit(pos, world.itemManager.GetItemDefinitionById(itemId));
                    break;
                }
            }
        }
    }
}
