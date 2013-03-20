using UnityEngine;
using CubeWorld.Gameplay;
using CubeWorld.Gameplay.Multiplayer;
using CubeWorld.Console;

public class GUIStatePlayerNormal : GUIState
{
    private PlayerGUI playerGUI;
    private int itemShortcutSelected;
    private CubeWorld.Items.InventoryEntry[] inventoryShortcuts = new CubeWorld.Items.InventoryEntry[0];
    private bool showLog;

    public GUIStatePlayerNormal(PlayerGUI playerGUI)
    {
        this.playerGUI = playerGUI;
    }

    public override void ProcessKeys()
    {
        for (int i = (int)KeyCode.Alpha1; i <= (int)KeyCode.Alpha9; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                itemShortcutSelected = i - (int)KeyCode.Alpha1;

                if (itemShortcutSelected >= 0 && itemShortcutSelected < inventoryShortcuts.Length)
                    playerGUI.playerUnity.objectInHand = inventoryShortcuts[itemShortcutSelected].cwobject;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            playerGUI.playerUnity.gameManagerUnity.Pause();
        }

        if (Input.GetKeyDown(KeyCode.I))
            playerGUI.EnterInventory();

        if (Input.GetKeyDown(KeyCode.L))
            showLog = !showLog;

        if (Input.GetKeyDown(KeyCode.E))
        {
            itemShortcutSelected++;
            if (itemShortcutSelected >= inventoryShortcuts.Length)
                itemShortcutSelected = 0;

            if (itemShortcutSelected >= 0 && itemShortcutSelected < inventoryShortcuts.Length)
                playerGUI.playerUnity.objectInHand = inventoryShortcuts[itemShortcutSelected].cwobject;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            itemShortcutSelected--;
            if (itemShortcutSelected <= 0)
                itemShortcutSelected = inventoryShortcuts.Length - 1;

            if (itemShortcutSelected >= 0 && itemShortcutSelected < inventoryShortcuts.Length)
                playerGUI.playerUnity.objectInHand = inventoryShortcuts[itemShortcutSelected].cwobject;
        }

        if (playerGUI.playerUnity.objectInHand == null)
        {
            if (playerGUI.playerUnity.player.inventory.entries.Count > 0)
                playerGUI.playerUnity.objectInHand = playerGUI.playerUnity.player.inventory.entries[0].cwobject;
        }
    }

    public override void Draw()
    {
        int offsetY = 0;

        if (CubeWorldPlayerPreferences.showFPS)
            GUI.Label(new Rect(0, offsetY++ * 20, Screen.width, 25), "FPS: " + playerGUI.lastFps);

        if (CubeWorldPlayerPreferences.showHelp)
        {
            GUI.Box(new Rect(0, offsetY * 20, 300, 100), "");
            GUI.Label(new Rect(0, offsetY * 20, 300, 100), "Press 'I' to enter Inventory\nRight click to add objects\nLeft click to activate / destroy them\nYou can disable this dialog from the options screen! (press ESCAPE)");
            offsetY += 5;
        }

        if (CubeWorldPlayerPreferences.showEngineStats)
        {
            CubeWorld.World.CubeWorldStats stats = playerGUI.playerUnity.player.world.stats;
            string memStats = "Memory used: " + (System.GC.GetTotalMemory(false) / (1024 * 1024)) + " MB";
            string multiplayerStats = (MultiplayerStats.Singleton.connected) ? MultiplayerStats.Singleton.ToString() : "";

            GUI.Label(new Rect(0, offsetY * 20, Screen.width, 60), "Stats: " + stats.ToString() + "\n" + memStats + "\n" + multiplayerStats);

            offsetY += 3;
        }

        if (showLog)
        {
            GUI.TextArea(new Rect(0, offsetY * 20, Screen.width, 200), CWConsole.Singleton.TextLog);
            offsetY += 10;
        }

        GUI.Box(new Rect((Screen.width - 10) / 2, (Screen.height - 10) / 2, 10, 10), "+");
    }
}

