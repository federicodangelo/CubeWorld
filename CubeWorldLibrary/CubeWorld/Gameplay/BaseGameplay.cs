using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeWorld.Configuration;
using CubeWorld.World.Generator;
using System.IO;
using CubeWorld.Tiles;
using CubeWorld.Tiles.Rules;
using CubeWorld.Items;
using CubeWorld.Utils;
using CubeWorld.Avatars;
using CubeWorld.World.Objects;

namespace CubeWorld.Gameplay
{
    public abstract class BaseGameplay
    {
        private string id;
        protected CubeWorld.World.CubeWorld world;

        public bool useEnqueueTileUpdates = true;
        protected int lastObjectId;

        public String Id
        {
            get { return id; }
        }

        public BaseGameplay(string id)
        {
            this.id = id;
        }

        public virtual void Init(CubeWorld.World.CubeWorld world)
        {
            this.world = world;
        }

        public virtual GeneratorProcess Generate(Config config)
        {
            return world.tileManager.Generate(config.worldGenerator.generator);
        }

        public virtual void WorldCreated()
        {
            CalculateLastObjectId();

            Player player = (Player) CreateAvatar("player", new Vector3());
            FillPlayerInventory(player.inventory);
            player.resetPosition = GetPlayerResetPosition();
            player.ResetPosition();
        }

        public virtual TilePosition GetPlayerResetPosition()
        {
            return new TilePosition(
                    world.sizeX / 2,
                    world.tileManager.GetTopPosition(world.sizeX / 2, world.sizeZ / 2),
                    world.sizeZ / 2);
        }

        public virtual void WorldLoaded()
        {
            CalculateLastObjectId();
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void Save(BinaryWriter bw)
        {
        }

        public virtual void Load(BinaryReader br)
        {
        }

        public virtual void Clear()
        {

        }

        public virtual void FillPlayerInventory(Inventory inventory)
        {
            foreach (ItemDefinition itemDefinition in world.itemManager.itemDefinitions)
            {
                if (itemDefinition.type != CubeWorld.World.Objects.CWDefinition.DefinitionType.ItemTile)
                {
                    Item item = new Item(world, itemDefinition, -1);

                    InventoryEntry ie = new InventoryEntry();
                    ie.position = 0;
                    ie.quantity = 1;
                    ie.cwobject = item;

                    inventory.entries.Add(ie);
                }
            }

            foreach (TileDefinition tileDefinition in world.tileManager.tileDefinitions)
            {
                if (tileDefinition.tileType != TileDefinition.EMPTY_TILE_TYPE)
                {
                    DynamicTile dynamicTile = new DynamicTile(world, tileDefinition, -1);

                    InventoryEntry ie = new InventoryEntry();
                    ie.position = 0;
                    ie.quantity = 10;
                    ie.cwobject = dynamicTile;

                    inventory.entries.Add(ie);
                }
            }
        }

        public virtual bool ProcessTileDamage(TilePosition pos, Tile tile, TileDefinition tileDefinition, int damage)
        {
            if (tileDefinition.energy != Tile.MAX_ENERGY)
            {
                if (tile.Energy > damage)
                {
                    world.tileManager.SetTileEnergy(pos, (byte)(tile.Energy - damage));

                    world.tileManager.TileDamaged(pos);

                    return false;
                }
                else
                {
                    world.tileManager.DestroyTile(pos);

                    return true;
                }
            }
            else
            {
                //world.tileManager.DestroyTile(pos);

                //return true;

                return false;
            }
        }

        public virtual Avatar CreateAvatar(string id, Vector3 pos)
        {
            return world.avatarManager.CreateAvatar(world.avatarManager.GetAvatarDefinitionById(id), NextObjectId(), pos, true);
        }

        public virtual void DestroyAvatar(Avatar avatar)
        {
            world.avatarManager.DestroyAvatar(avatar);
        }

        public virtual void TileClicked(TilePosition tilePosition)
        {
            world.tileManager.TileClicked(tilePosition);
        }

        public virtual void DamageTile(TilePosition tilePosition, int damage)
        {
            world.tileManager.DamageTile(tilePosition, damage);
        }

        public virtual void TileHit(TilePosition tilePosition, ItemDefinition itemDefinition)
        {
            if (itemDefinition.damage > 0)
                world.gameplay.DamageTile(tilePosition, itemDefinition.damage);

            if (itemDefinition.setOnFire)
                if (world.tileManager.GetTileBurns(tilePosition))
                    world.tileManager.SetTileOnFire(tilePosition, true);
        }

        public virtual void CreateTile(TilePosition tileCreatePosition, byte tileType)
        {
            if (world.tileManager.GetTileType(tileCreatePosition) == TileDefinition.EMPTY_TILE_TYPE)
            {
                world.tileManager.SetTileType(
                    tileCreatePosition,
                    tileType);
            }
        }

        protected void AddRandomEnemies(int count)
        {
            Random rnd = new Random();

            if (world.avatarManager.avatarDefinitions.Length > 1)
            {
                for (int i = 0; i < count; i++)
                {
                    int avatarIndex = rnd.Next(0, world.avatarManager.avatarDefinitions.Length);
                    while (world.avatarManager.avatarDefinitions[avatarIndex].id.IndexOf("player") == 0)
                        avatarIndex = (avatarIndex + 1) % world.avatarManager.avatarDefinitions.Length;

                    AvatarDefinition avatarDefinition = world.avatarManager.avatarDefinitions[avatarIndex];

                    Vector3 pos = new Vector3(
                        world.sizeX * ((float)rnd.NextDouble() * 0.8f + 0.1f),
                        world.sizeY * 0.9f,
                        world.sizeZ * ((float)rnd.NextDouble() * 0.8f + 0.1f));

                    world.avatarManager.CreateAvatar(avatarDefinition, NextObjectId(), pos, true);
                }
            }
        }

        public virtual Item CreateItem(ItemDefinition itemDefinition, Vector3 position)
        {
            return world.itemManager.CreateItem(itemDefinition, NextObjectId(), position, true);
        }

        public virtual int NextObjectId()
        {
            return lastObjectId++;
        }

        public void CalculateLastObjectId()
        {
            lastObjectId = -1;

            foreach (CWObject cwobject in world.avatarManager.Avatars)
                if (cwobject.objectId > lastObjectId)
                    lastObjectId = cwobject.objectId;

            foreach (CWObject cwobject in world.itemManager.Items)
                if (cwobject.objectId > lastObjectId)
                    lastObjectId = cwobject.objectId;

            foreach (CWObject cwobject in world.tileManager.DynamicTiles)
                if (cwobject.objectId > lastObjectId)
                    lastObjectId = cwobject.objectId;

            lastObjectId++;
        }

        public virtual void TileInvalidated(TilePosition pos, bool lightRelated)
        {
        }
    }
}
