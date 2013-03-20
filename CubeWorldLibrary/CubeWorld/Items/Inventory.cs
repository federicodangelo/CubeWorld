using System;
using System.Collections.Generic;
using CubeWorld.World.Objects;
using CubeWorld.Tiles;

namespace CubeWorld.Items
{
	public class Inventory
	{
        private CWObject owner;

        public Inventory(CWObject owner)
        {
            this.owner = owner;
        }

		public List<InventoryEntry> entries = new List<InventoryEntry>();

        /**
         * Adds an object to the inventory.
         * 
         * @returns true if the object was added, false if there was no space for the object
         */
        public bool Add(CWObject cwobject)
        {
            InventoryEntry existingEntry = FindInventoryEntryFromDefinition(cwobject.definition);

            if (existingEntry != null)
            {
                existingEntry.quantity++;
            }
            else
            {
                existingEntry = new InventoryEntry();
                existingEntry.quantity = 1;
                existingEntry.cwobject = cwobject;

                entries.Add(existingEntry);
            }

            return true;
        }

        public InventoryEntry FindInventoryEntryFromDefinition(CWDefinition definition)
        {
            foreach (InventoryEntry entry in entries)
                if (entry.cwobject.definition == definition)
                    return entry;

            return null;
        }

        public bool HasMoreOfDefinition(CWDefinition definition)
        {
            return FindInventoryEntryFromDefinition(definition) != null;
        }

        public bool RemoveFromDefinition(CWDefinition definition, int quantity)
        {
            InventoryEntry existingInventoryEntry = FindInventoryEntryFromDefinition(definition);

            while (quantity > 0 && existingInventoryEntry != null)
            {
                if (existingInventoryEntry.quantity >= quantity)
                {
                    int temp = existingInventoryEntry.quantity;
                    existingInventoryEntry.quantity -= quantity;
                    quantity -= temp;
                }
                else
                {
                    int temp = existingInventoryEntry.quantity;
                    existingInventoryEntry.quantity = 0;
                    quantity -= temp;
                }

                if (existingInventoryEntry.quantity == 0)
                    entries.Remove(existingInventoryEntry);

                existingInventoryEntry = FindInventoryEntryFromDefinition(definition);
            }

            return quantity <= 0;
        }

        public void Save(System.IO.BinaryWriter bw)
        {
            bw.Write(entries.Count);

            foreach (InventoryEntry ie in entries)
            {
                bw.Write(ie.cwobject.definition.id);
                bw.Write(ie.position);
                bw.Write(ie.quantity);
                ie.cwobject.Save(bw);
            }
        }

        public void Load(System.IO.BinaryReader br)
        {
            entries.Clear();

            int n = br.ReadInt32();

            for (int i = 0; i < n; i++)
            {
                string id = br.ReadString();
                int position = br.ReadInt32();
                int quantity = br.ReadInt32();

                TileDefinition tileDefinition = owner.world.tileManager.GetTileDefinitionById(id);
                CWObject cwobject = null;

                if (tileDefinition != null)
                {
                    //It's a tile
                    DynamicTile dynamicTile = new DynamicTile(owner.world, tileDefinition, -1);

                    cwobject = dynamicTile;
                }
                else
                {
                    ItemDefinition itemDefinition = owner.world.itemManager.GetItemDefinitionById(id);

                    if (itemDefinition != null)
                    {
                        //It's an item
                        Item item;

                        if (itemDefinition.type == CubeWorld.World.Objects.CWDefinition.DefinitionType.ItemTile)
                            item = new ItemTile(owner.world, (ItemTileDefinition)itemDefinition, -1);
                        else
                            item = new Item(owner.world, itemDefinition, -1);

                        cwobject = item;
                    }
                }

                cwobject.Load(br);

                InventoryEntry ie = new InventoryEntry();
                ie.position = position;
                ie.quantity = quantity;
                ie.cwobject = cwobject;

                entries.Add(ie);
            }
        }
    }
}

