using UnityEngine;
using System.Collections.Generic;
using CubeWorld.World.Objects;
using CubeWorld.Tiles;

public class GUIStatePlayerInventory : GUIState
{
    private PlayerGUI playerGUI;
    private Dictionary<CWDefinition, Texture2D> inventoryTextures;

    public GUIStatePlayerInventory(PlayerGUI playerGUI)
    {
        this.playerGUI = playerGUI;
    }

    public override void ProcessKeys()
    {
        if (Input.GetKeyDown(KeyCode.B))
            playerGUI.ExitInventory();
    }


    private List<GUIContent> inventoryContents;
    private int inventoryItemSelected;

    public override void Draw()
    {
        if (inventoryTextures == null)
            InitItemTextures();

        int w = 600;
        int h = 400;

        GUI.BeginGroup(new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h));
        GUI.Box(new Rect(0, 0, w, h), "Select Tile");

        int gridWidth = w - 40;
        int gridHeight = h - 40;

        if (inventoryContents == null)
        {
            inventoryContents = new List<GUIContent>();

            foreach (CubeWorld.Items.InventoryEntry inventoryEntry in playerGUI.playerUnity.player.inventory.entries)
            {
                GUIContent itemContent = new GUIContent(
                    inventoryEntry.cwobject.definition.description + " [" + inventoryEntry.quantity + "]",
                    inventoryTextures[inventoryEntry.cwobject.definition]);

                inventoryContents.Add(itemContent);
            }
        }

        inventoryItemSelected = GUI.SelectionGrid(new Rect(20, 20, gridWidth, gridHeight), inventoryItemSelected, inventoryContents.ToArray(), 4);

        if (GUI.changed)
        {
            if (inventoryItemSelected >= 0 && inventoryItemSelected < playerGUI.playerUnity.player.inventory.entries.Count)
                playerGUI.playerUnity.objectInHand = playerGUI.playerUnity.player.inventory.entries[inventoryItemSelected].cwobject;

            playerGUI.ExitInventory();
        }

        GUI.EndGroup();
    }

    public override void OnDeactivated()
    {
        inventoryContents = null;
    }

    private void InitItemTextures()
    {
        Texture2D tilesetTexture = (Texture2D) playerGUI.playerUnity.gameManagerUnity.material.mainTexture;

        inventoryTextures = new Dictionary<CWDefinition, Texture2D>();

        foreach (CubeWorld.Items.ItemDefinition itemDefinition in playerGUI.playerUnity.gameManagerUnity.world.itemManager.itemDefinitions)
        {
            if (itemDefinition.type == CWDefinition.DefinitionType.Item)
            {
                string materialName = "Items/" + itemDefinition.visualDefinition.material;

                Texture2D texture = (Texture2D) Resources.Load(materialName, typeof(Texture2D));

                inventoryTextures[itemDefinition] = texture;
            }
        }

        foreach (TileDefinition tileDefinition in playerGUI.playerUnity.gameManagerUnity.world.tileManager.tileDefinitions)
        {
            if (tileDefinition.tileType != TileDefinition.EMPTY_TILE_TYPE)
            {
                Texture2D texture = null;

                foreach (int material in tileDefinition.materials)
                    if (material >= 0)
                    {
                        texture = GraphicsUnity.GetTilesetTexture(tilesetTexture, material);
                        break;
                    }

                inventoryTextures[tileDefinition] = texture;
            }
        }
    }
}

