using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeWorld.Tiles;
using CubeWorld.Avatars;
using CubeWorld.Items;
using CubeWorld.Gameplay.Multiplayer;
using CubeWorld.World.Generator;
using System.Net.Sockets;
using CubeWorld.Utils;
using System.IO;
using SourceCode.CubeWorld.Utils;

namespace CubeWorld.Gameplay
{
    public class MultiplayerClientGameplay : BaseGameplay, IMultiplayerClientListener
    {
        private MultiplayerStats stats;
        private MultiplayerClient client;
        public byte[] initializationData;
        public bool initializationDataReceived;

        public MultiplayerClientGameplay(string server, int port) :
            base("multiplayerClient")
        {
            useEnqueueTileUpdates = false;

            stats = MultiplayerStats.Singleton;
            stats.Reset();
            stats.connected = true;

            client = new MultiplayerClient(server, port, this);
        }

        public override void Init(CubeWorld.World.CubeWorld world)
        {
            base.Init(world);
        }

        public override void WorldCreated()
        {
        }

        public override void TileClicked(TilePosition tilePosition)
        {
            //base.TileClicked(tilePosition);

            client.AddAction(
                           new MultiplayerAction(MultiplayerAction.Action.TILE_CLICKED,
                               new String[] {
                    tilePosition.x.ToString(),
                    tilePosition.y.ToString(),
                    tilePosition.z.ToString()
                }));
        }

        public override void TileHit(TilePosition tilePosition, ItemDefinition itemDefinition)
        {
            //base.TileHit(tilePosition, item);

            client.AddAction(
                new MultiplayerAction(MultiplayerAction.Action.TILE_HIT,
                    new String[] {
                    itemDefinition.id,
                    tilePosition.x.ToString(),
                    tilePosition.y.ToString(),
                    tilePosition.z.ToString()
                }));
        }

        public override void CreateTile(TilePosition tileCreatePosition, byte tileType)
        {
            //base.CreateTile(tileCreatePosition, tileType);

            client.AddAction(
                new MultiplayerAction(MultiplayerAction.Action.TILE_CREATE,
                    new String[] {
                    tileType.ToString(),
                    tileCreatePosition.x.ToString(),
                    tileCreatePosition.y.ToString(),
                    tileCreatePosition.z.ToString()
                }));
        }

        private float timerSendPosition;

        public override void Update(float deltaTime)
        {
            if (initializationDataReceived)
            {
                if (world.avatarManager.player != null)
                {
                    timerSendPosition += deltaTime;

                    if (timerSendPosition > 0.1f)
                    {
                        client.AddAction(new MultiplayerAction(
                            MultiplayerAction.Action.AVATAR_MOVE,
                            new String[] {
                                    world.avatarManager.player.objectId.ToString(),
                                    world.avatarManager.player.position.x.ToString(),
                                    world.avatarManager.player.position.y.ToString(),
                                    world.avatarManager.player.position.z.ToString(),
                                    world.avatarManager.player.rotation.x.ToString(),
                                    world.avatarManager.player.rotation.y.ToString(),
                                    world.avatarManager.player.rotation.z.ToString()
                                }));
								
						timerSendPosition = 0.0f;
                    }
                }
            }

            if (initializationDataReceived)
                client.Update(999);
            else
                client.Update(1);
        }

        public void ClientActionReceived(MultiplayerClient client, MultiplayerAction action)
        {
            switch (action.action)
            {
                case MultiplayerAction.Action.INITIAL_DATA:
                    initializationDataReceived = true;
                    initializationData = action.extraData;
                    break;

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

                    Avatar avatar = world.avatarManager.GetAvatarByObjectId(objectId);
                    avatar.position = pos;
                    avatar.rotation = rot;
                    break;
                }

                case MultiplayerAction.Action.AVATAR_CREATE:
                {
                    int objectId = Int32.Parse(action.GetParameter(0));
                    string avatarDefinitionId = action.GetParameter(1);
                    Vector3 pos = new Vector3(
                        Single.Parse(action.GetParameter(2)),
                        Single.Parse(action.GetParameter(3)),
                        Single.Parse(action.GetParameter(4)));
                    Vector3 rot = new Vector3(
                        Single.Parse(action.GetParameter(5)),
                        Single.Parse(action.GetParameter(6)),
                        Single.Parse(action.GetParameter(7)));

                    Avatar avatar = world.avatarManager.CreateAvatar(world.avatarManager.GetAvatarDefinitionById(avatarDefinitionId), objectId, pos, true);
                    avatar.rotation = rot;
                    break;
                }

                case MultiplayerAction.Action.AVATAR_DESTROY:
                {
                    int objectId = Int32.Parse(action.GetParameter(0));
                    world.avatarManager.DestroyAvatar(world.avatarManager.GetAvatarByObjectId(objectId));
                    break;
                }

                case MultiplayerAction.Action.TILE_INVALIDATED:
                {
                    MemoryStream ms = new MemoryStream(action.extraData);
                    BinaryReader br = new BinaryReader(ms);

                    int n = br.ReadInt32();
                    Tile tile = new Tile();

                    for (int i = 0; i < n; i++)
                    {
                        TilePosition pos = SerializationUtils.ReadTilePosition(br);
                        tile.Deserialize(br.ReadUInt32());

                        if (tile.tileType != world.tileManager.GetTileType(pos))
                            world.tileManager.SetTileType(pos, tile.tileType);
                        if (tile.OnFire != world.tileManager.GetTileOnFire(pos))
                            world.tileManager.SetTileOnFire(pos, tile.OnFire);
                        if (tile.ExtraData != world.tileManager.GetTileExtraData(pos))
                            world.tileManager.SetTileExtraData(pos, tile.ExtraData);
                        if (tile.Energy != world.tileManager.GetTileEnergy(pos))
                            world.tileManager.SetTileEnergy(pos, tile.Energy);
                    }
                    break;
                }
            }
        }
    }
}
