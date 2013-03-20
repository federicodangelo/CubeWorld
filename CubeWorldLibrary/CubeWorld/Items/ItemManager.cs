using System;
using System.Collections.Generic;
using CubeWorld.Utils;
using CubeWorld.Items.Components;
using SourceCode.CubeWorld.Utils;

namespace CubeWorld.Items
{
	public class ItemManager
	{
		public CubeWorld.World.CubeWorld world;
			
		public ItemDefinition[] itemDefinitions;

        private List<Item> items = new List<Item>();

        public Item[] Items
        {
            get { return items.ToArray(); }
        }

		public ItemManager (CubeWorld.World.CubeWorld world)
		{
			this.world = world;
		}
		
		public void Create(ItemDefinition[] itemDefinitions)
		{
			this.itemDefinitions = itemDefinitions;
		}
		
        public ItemDefinition GetItemDefinitionById(string id)
        {
            foreach (ItemDefinition itemDefinition in itemDefinitions)
                if (itemDefinition.id == id)
                    return itemDefinition;

            return null;
        }

        public Item CreateItem(ItemDefinition itemDefinition, int objectId, Vector3 position, bool dispatchWorldListener)
        {
            Item item;

            if (itemDefinition.type == CubeWorld.World.Objects.CWDefinition.DefinitionType.ItemTile)
                item = new ItemTile(world, (ItemTileDefinition) itemDefinition, objectId);
            else
                item = new Item(world, itemDefinition, objectId);

            item.position = position;
            item.AddComponent(new ItemComponentGravity());
            item.AddComponent(new ItemComponentGoToPlayer());
            item.AddComponent(new ItemComponentAutoDestroy());

            items.Add(item);

            if (dispatchWorldListener)
                world.cwListener.CreateObject(item);

            return item;
        }

        private bool insideUpdate;
        private List<Item> itemsToRemove = new List<Item>();

        public void RemoveItem(Item item)
        {
            if (insideUpdate)
            {
                itemsToRemove.Add(item);
            }
            else
            {
                if (item.destroyed == false)
                {
                    item.destroyed = true;
                    items.Remove(item);
                    world.cwListener.DestroyObject(item);
                    item.Clear();
                }
            }
        }

        public void Update(float deltaTime)
		{
            insideUpdate = true;

            foreach (Item item in items)
                item.Update(deltaTime);

            insideUpdate = false;

            if (itemsToRemove.Count > 0)
            {
                foreach (Item item in itemsToRemove)
                    RemoveItem(item);

                itemsToRemove.Clear();
            }
		}
		
		public void Clear()
		{
            foreach (Item item in items)
                item.Clear();

            items.Clear();
        }

        public void Save(System.IO.BinaryWriter bw)
        {
            bw.Write(items.Count);

            foreach (Item item in items)
            {
                bw.Write(item.definition.id);
                bw.Write(item.objectId);
                SerializationUtils.Write(bw, item.position);
                item.Save(bw);
            }
        }


        public void Load(System.IO.BinaryReader br)
        {
            int n = br.ReadInt32();

            for (int i = 0; i < n; i++)
            {
                string itemDefinitionId = br.ReadString();
                int objectId = br.ReadInt32();
                Vector3 position = SerializationUtils.ReadVector3(br);

                ItemDefinition itemDefinition = GetItemDefinitionById(itemDefinitionId);

                Item item = CreateItem(itemDefinition, objectId, position, false);

                item.Load(br);

                world.cwListener.CreateObject(item);
            }
        }
    }
}

