using UnityEngine;
using System.Collections;
using CubeWorld.Gameplay;
using CubeWorld.Configuration;

public class MainMenu
{
    private GameManagerUnity gameManagerUnity;

    public MainMenu(GameManagerUnity gameManagerUnity)
    {
        this.gameManagerUnity = gameManagerUnity;
    }

    public enum MainMenuState
    {
        NORMAL,
        GENERATOR,
        OPTIONS,
        JOIN_MULTIPLAYER,
#if !UNITY_WEBPLAYER
        LOAD,
        SAVE,
#endif
        ABOUT
    }

    public MainMenuState state = MainMenuState.NORMAL;

    public void Draw()
    {
        MenuSystem.useKeyboard = false;

        switch (state)
        {
            case MainMenuState.NORMAL:
                DrawMenuNormal();
                break;

            case MainMenuState.GENERATOR:
                DrawGenerator();
                break;

            case MainMenuState.OPTIONS:
                DrawOptions();
                break;

            case MainMenuState.ABOUT:
                DrawMenuAbout();
                break;

            case MainMenuState.JOIN_MULTIPLAYER:
                DrawJoinMultiplayer();
                break;

#if !UNITY_WEBPLAYER
            case MainMenuState.LOAD:
                DrawMenuLoadSave(true);
                break;

            case MainMenuState.SAVE:
                DrawMenuLoadSave(false);
                break;
#endif
        }
    }

    public void DrawPause()
    {
        MenuSystem.useKeyboard = false;

        switch (state)
        {
            case MainMenuState.NORMAL:
                DrawMenuPause();
                break;

            case MainMenuState.OPTIONS:
                DrawOptions();
                break;

#if !UNITY_WEBPLAYER
            case MainMenuState.LOAD:
                DrawMenuLoadSave(true);
                break;

            case MainMenuState.SAVE:
                DrawMenuLoadSave(false);
                break;
#endif
        }
    }

#if !UNITY_WEBPLAYER
    void DrawMenuLoadSave(bool load)
    {
        if (load)
            MenuSystem.BeginMenu("Load");
        else
            MenuSystem.BeginMenu("Save");

        for (int i = 0; i < 5; i++)
        {
            System.DateTime fileDateTime = WorldManagerUnity.GetWorldFileInfo(i);

            if (fileDateTime != System.DateTime.MinValue)
            {
                string prefix;
                if (load)
                    prefix = "Load World ";
                else
                    prefix = "Overwrite World";

                MenuSystem.Button(prefix + (i + 1).ToString() + " [ " + fileDateTime.ToString() + " ]", delegate()
                {
                    if (load)
                    {
                        gameManagerUnity.worldManagerUnity.LoadWorld(i);
                        state = MainMenuState.NORMAL;
                    }
                    else
                    {
                        gameManagerUnity.worldManagerUnity.SaveWorld(i);
                        state = MainMenuState.NORMAL;
                    }
                }
                );
            }
            else
            {
                MenuSystem.Button("-- Empty Slot --", delegate()
                {
                    if (load == false)
                    {
                        gameManagerUnity.worldManagerUnity.SaveWorld(i);
                        state = MainMenuState.NORMAL;
                    }
                }
                );
            }
        }

        MenuSystem.LastButton("Return", delegate()
        {
            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }
#endif

    private CubeWorld.Configuration.Config lastConfig; 

    void DrawMenuPause()
    {
        MenuSystem.BeginMenu("Pause");

        if (lastConfig != null)
        {
            MenuSystem.Button("Re-create Random World", delegate()
            {
                gameManagerUnity.worldManagerUnity.CreateRandomWorld(lastConfig);
            }
            );
        }

#if !UNITY_WEBPLAYER
        MenuSystem.Button("Save World", delegate()
        {
            state = MainMenuState.SAVE;
        }
        );
#endif

        MenuSystem.Button("Options", delegate()
        {
            state = MainMenuState.OPTIONS;
        }
        );

        MenuSystem.Button("Exit to Main Menu", delegate()
        {
            gameManagerUnity.ReturnToMainMenu();
        }
        );

        MenuSystem.LastButton("Return to Game", delegate()
        {
            gameManagerUnity.Unpause();
        }
        );

        MenuSystem.EndMenu();
    }

    void DrawMenuNormal()
    {
        MenuSystem.BeginMenu("Main Menu");

        MenuSystem.Button("Create Random World", delegate()
        {
            state = MainMenuState.GENERATOR;
        }
        );

#if !UNITY_WEBPLAYER
        MenuSystem.Button("Load Saved World", delegate()
        {
            state = MainMenuState.LOAD;
        }
        );
#endif

        MenuSystem.Button("Join Multiplayer", delegate()
        {
            state = MainMenuState.JOIN_MULTIPLAYER;
        }
        );


        MenuSystem.Button("Options", delegate()
        {
            state = MainMenuState.OPTIONS;
        }
        );

        MenuSystem.Button("About", delegate()
        {
            state = MainMenuState.ABOUT;
        }
        );

        if (Application.platform != RuntimePlatform.WebGLPlayer && Application.isEditor == false)
        {
            MenuSystem.LastButton("Exit", delegate()
            {
                Application.Quit();
            }
            );
        }

        MenuSystem.EndMenu();
    }

    public const string CubeworldWebServerServerList = "http://cubeworldweb.appspot.com/list";
    public const string CubeworldWebServerServerRegister = "http://cubeworldweb.appspot.com/register?owner={owner}&description={description}&port={port}";

    private WWW wwwRequest;
    private string[] servers;

    void DrawJoinMultiplayer()
    {
        MenuSystem.BeginMenu("Join Multiplayer");

        if (wwwRequest == null && servers == null)
            wwwRequest = new WWW(CubeworldWebServerServerList);

        if (servers == null && wwwRequest != null && wwwRequest.isDone)
            servers = wwwRequest.text.Split(';');

        if (wwwRequest != null && wwwRequest.isDone)
        {
            foreach (string s in servers)
            {
                string[] ss = s.Split(',');

                if (ss.Length >= 2)
                {
                    MenuSystem.Button("Join [" + ss[0] + ":" + ss[1] + "]", delegate()
                    {
                        gameManagerUnity.worldManagerUnity.JoinMultiplayerGame(ss[0], System.Int32.Parse(ss[1]));

                        availableConfigurations = null;

                        wwwRequest = null;
                        servers = null;

                        state = MainMenuState.NORMAL;
                    }
                    );
                }
            }

            MenuSystem.Button("Refresh List", delegate()
            {
                wwwRequest = null;
                servers = null;
            }
            );
        }
        else
        {
            MenuSystem.TextField("Waiting data from server..");
        }

        MenuSystem.LastButton("Back", delegate()
        {
            wwwRequest = null;
            servers = null;
            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }

    void DrawOptions()
    {
        MenuSystem.BeginMenu("Options");

        MenuSystem.Button("Draw Distance: " + CubeWorldPlayerPreferences.farClipPlanes[CubeWorldPlayerPreferences.viewDistance], delegate()
        {
            CubeWorldPlayerPreferences.viewDistance = (CubeWorldPlayerPreferences.viewDistance + 1) % CubeWorldPlayerPreferences.farClipPlanes.Length;

            if (gameManagerUnity.playerUnity)
                gameManagerUnity.playerUnity.mainCamera.farClipPlane = CubeWorldPlayerPreferences.farClipPlanes[CubeWorldPlayerPreferences.viewDistance];
        }
        );

        MenuSystem.Button("Show Help: " + CubeWorldPlayerPreferences.showHelp, delegate()
        {
            CubeWorldPlayerPreferences.showHelp = !CubeWorldPlayerPreferences.showHelp;
        }
        );

        MenuSystem.Button("Show FPS: " + CubeWorldPlayerPreferences.showFPS, delegate()
        {
            CubeWorldPlayerPreferences.showFPS = !CubeWorldPlayerPreferences.showFPS;
        }
        );

        MenuSystem.Button("Show Engine Stats: " + CubeWorldPlayerPreferences.showEngineStats, delegate()
        {
            CubeWorldPlayerPreferences.showEngineStats = !CubeWorldPlayerPreferences.showEngineStats;
        }
        );

        MenuSystem.Button("Visible Strategy: " + System.Enum.GetName(typeof(SectorManagerUnity.VisibleStrategy), CubeWorldPlayerPreferences.visibleStrategy), delegate()
        {
            if (System.Enum.IsDefined(typeof(SectorManagerUnity.VisibleStrategy), (int)CubeWorldPlayerPreferences.visibleStrategy + 1))
            {
                CubeWorldPlayerPreferences.visibleStrategy = CubeWorldPlayerPreferences.visibleStrategy + 1;
            }
            else
            {
                CubeWorldPlayerPreferences.visibleStrategy = 0;
            }
        }
        );

        MenuSystem.LastButton("Back", delegate()
        {
            CubeWorldPlayerPreferences.StorePreferences();

            gameManagerUnity.PreferencesUpdated();

            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }

    private AvailableConfigurations availableConfigurations;
    private int currentSizeOffset = 0;
    private int currentGeneratorOffset = 0;
    private int currentDayInfoOffset = 0;
    private int currentGameplayOffset = 0;
#if !UNITY_WEBPLAYER
    private bool multiplayer = false;
#endif

    void DrawGenerator()
    {
        if (availableConfigurations == null)
        {
            availableConfigurations = GameManagerUnity.LoadConfiguration();
            currentDayInfoOffset = 0;
            currentGeneratorOffset = 0;
            currentSizeOffset = 0;
            currentGameplayOffset = 0;
        }

        MenuSystem.BeginMenu("Random World Generator");

        MenuSystem.Button("Gameplay: " + GameplayFactory.AvailableGameplays[currentGameplayOffset].name, delegate()
        {
            currentGameplayOffset = (currentGameplayOffset + 1) % GameplayFactory.AvailableGameplays.Length;
        }
        );

        MenuSystem.Button("World Size: " + availableConfigurations.worldSizes[currentSizeOffset].name, delegate()
        {
            currentSizeOffset = (currentSizeOffset + 1) % availableConfigurations.worldSizes.Length;
        }
        );

        MenuSystem.Button("Day Length: " + availableConfigurations.dayInfos[currentDayInfoOffset].name, delegate()
        {
            currentDayInfoOffset = (currentDayInfoOffset + 1) % availableConfigurations.dayInfos.Length;
        }
        );

        if (GameplayFactory.AvailableGameplays[currentGameplayOffset].hasCustomGenerator == false)
        {
            MenuSystem.Button("Generator: " + availableConfigurations.worldGenerators[currentGeneratorOffset].name, delegate()
            {
                currentGeneratorOffset = (currentGeneratorOffset + 1) % availableConfigurations.worldGenerators.Length;
            }
            );
        }

#if !UNITY_WEBPLAYER
        MenuSystem.Button("Host Multiplayer: " + (multiplayer ? "Yes" : "No") , delegate()
        {
            multiplayer = !multiplayer;
        }
        );
#endif

        MenuSystem.LastButton("Generate!", delegate()
        {
            lastConfig = new CubeWorld.Configuration.Config();
            lastConfig.tileDefinitions = availableConfigurations.tileDefinitions;
			lastConfig.itemDefinitions = availableConfigurations.itemDefinitions;
			lastConfig.avatarDefinitions = availableConfigurations.avatarDefinitions;
            lastConfig.dayInfo = availableConfigurations.dayInfos[currentDayInfoOffset];
            lastConfig.worldGenerator = availableConfigurations.worldGenerators[currentGeneratorOffset];
            lastConfig.worldSize = availableConfigurations.worldSizes[currentSizeOffset];
            lastConfig.extraMaterials = availableConfigurations.extraMaterials;
            lastConfig.gameplay = GameplayFactory.AvailableGameplays[currentGameplayOffset];

#if !UNITY_WEBPLAYER
            if (multiplayer)
            {
                MultiplayerServerGameplay multiplayerServerGameplay = new MultiplayerServerGameplay(lastConfig.gameplay.gameplay, true);

                GameplayDefinition g = new GameplayDefinition("", "", multiplayerServerGameplay, false);

                lastConfig.gameplay = g;

                gameManagerUnity.RegisterInWebServer();
            }
#endif

            gameManagerUnity.worldManagerUnity.CreateRandomWorld(lastConfig);

            availableConfigurations = null;

            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }

    void DrawMenuAbout()
    {
        MenuSystem.BeginMenu("Author");

        GUI.TextArea(new Rect(10, 40 + 30 * 0, 380, 260),
                    "Work In Progress by Federico D'Angelo (lawebdefederico@gmail.com)");

        MenuSystem.LastButton("Back", delegate() { state = MainMenuState.NORMAL; });

        MenuSystem.EndMenu();
    }

    public void DrawGeneratingProgress(string description, int progress)
    {
        Rect sbPosition = new Rect(40,
                                    Screen.height / 2 - 20,
                                    Screen.width - 80,
                                    40);

        GUI.HorizontalScrollbar(sbPosition, 0, progress, 0, 100);

        Rect dPosition = new Rect(Screen.width / 2 - 200, sbPosition.yMax + 10, 400, 25);
        GUI.Box(dPosition, description);
    }
}
