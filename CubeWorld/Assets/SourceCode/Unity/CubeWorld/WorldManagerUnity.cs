using UnityEngine;
using System.Collections.Generic;
using CubeWorld.World.Generator;
using CubeWorld.Gameplay;
using CubeWorld.Configuration;

public class WorldManagerUnity
{
    private GameManagerUnity gameManagerUnity;

    public WorldManagerUnity(GameManagerUnity gameManagerUnity)
    {
        this.gameManagerUnity = gameManagerUnity;
    }

#if !UNITY_WEBPLAYER

    static public string GetWorldFilePath(int n)
    {
        string exePath = System.IO.Directory.GetParent(Application.dataPath).FullName;
        return System.IO.Path.Combine(exePath, "world" + n + ".map");
    }

    static private Dictionary<int, System.DateTime> worldFileInfoCache = new Dictionary<int, System.DateTime>();

    static public System.DateTime GetWorldFileInfo(int n)
    {
        if (worldFileInfoCache.ContainsKey(n) == false)
        {
            try
            {
                string path = GetWorldFilePath(n);

                if (System.IO.File.Exists(path))
                {
                    System.IO.FileStream fs = System.IO.File.OpenRead(path);

                    try
                    {
                        System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

                        if (br.ReadString() == CubeWorld.World.CubeWorld.VERSION_INFO)
                            worldFileInfoCache[n] = System.IO.File.GetLastWriteTime(path);
                        else
                            worldFileInfoCache[n] = System.DateTime.MinValue;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                else
                {
                    worldFileInfoCache[n] = System.DateTime.MinValue;
                }
            }
            catch (System.Exception)
            {
                worldFileInfoCache[n] = System.DateTime.MinValue;
            }
        }

        return worldFileInfoCache[n];
    }

    public void LoadWorld(int n)
    {
        if (System.IO.File.Exists(GetWorldFilePath(n)))
        {
            gameManagerUnity.DestroyWorld();

            AvailableConfigurations configurations = GameManagerUnity.LoadConfiguration();

            gameManagerUnity.LoadCustomTextures();

            byte[] data = System.IO.File.ReadAllBytes(GetWorldFilePath(n));

            CubeWorld.Configuration.Config config = new CubeWorld.Configuration.Config();
            config.tileDefinitions = configurations.tileDefinitions;
			config.itemDefinitions = configurations.itemDefinitions;
			config.avatarDefinitions = configurations.avatarDefinitions;
            config.extraMaterials = configurations.extraMaterials;

            gameManagerUnity.extraMaterials = config.extraMaterials;

            gameManagerUnity.world = new CubeWorld.World.CubeWorld(gameManagerUnity.objectsManagerUnity, gameManagerUnity.fxManagerUnity);
            gameManagerUnity.world.Load(config, data);
            worldGeneratorProcess = null;

            gameManagerUnity.surroundingsUnity.CreateSurroundings(gameManagerUnity.world.configSurroundings);

            gameManagerUnity.State = GameManagerUnity.GameManagerUnityState.GENERATING;
        }
    }

    public void SaveWorld(int n)
    {
        byte[] map = gameManagerUnity.world.Save();

        System.IO.File.WriteAllBytes(GetWorldFilePath(n), map);

        worldFileInfoCache.Clear();
    }

#endif

    public void JoinMultiplayerGame(string server, int port)
    {
        gameManagerUnity.DestroyWorld();

        worldGeneratorProcess = new GeneratorProcess(new MultiplayerGameLoaderGenerator(this, server, port), null);

        gameManagerUnity.State = GameManagerUnity.GameManagerUnityState.GENERATING;
    }

    private class MultiplayerGameLoaderGenerator : CubeWorldGenerator
    {
        private MultiplayerClientGameplay mutiplayerClientGameplay;
        private WorldManagerUnity worldManagerUnity;

        public MultiplayerGameLoaderGenerator(WorldManagerUnity worldManagerUnity, string server, int port)
        {
#if UNITY_WEBPLAYER
            Security.PrefetchSocketPolicy(server, port);
#endif
            this.worldManagerUnity = worldManagerUnity;
            mutiplayerClientGameplay = new MultiplayerClientGameplay(server, port);
        }

        public override bool Generate(CubeWorld.World.CubeWorld world)
        {
            mutiplayerClientGameplay.Update(0.0f);

            if (mutiplayerClientGameplay.initializationDataReceived)
            {
                GameManagerUnity gameManagerUnity = worldManagerUnity.gameManagerUnity;

                gameManagerUnity.world = new CubeWorld.World.CubeWorld(gameManagerUnity.objectsManagerUnity, gameManagerUnity.fxManagerUnity);
                gameManagerUnity.world.gameplay = mutiplayerClientGameplay;

                CubeWorld.World.CubeWorld.MultiplayerConfig config = gameManagerUnity.world.LoadMultiplayer(mutiplayerClientGameplay.initializationData);

                mutiplayerClientGameplay.initializationData = null;

                gameManagerUnity.extraMaterials = config.extraMaterials;
                gameManagerUnity.surroundingsUnity.CreateSurroundings(gameManagerUnity.world.configSurroundings);

                return true;
            }

            return false;
        }
    }


    public CubeWorld.World.Generator.GeneratorProcess worldGeneratorProcess;

    public void CreateRandomWorld(CubeWorld.Configuration.Config config)
    {
        gameManagerUnity.DestroyWorld();

        gameManagerUnity.LoadCustomTextures();

        gameManagerUnity.extraMaterials = config.extraMaterials;

        gameManagerUnity.world = new CubeWorld.World.CubeWorld(gameManagerUnity.objectsManagerUnity, gameManagerUnity.fxManagerUnity);
        worldGeneratorProcess = gameManagerUnity.world.Generate(config);

        gameManagerUnity.surroundingsUnity.CreateSurroundings(gameManagerUnity.world.configSurroundings);

        gameManagerUnity.State = GameManagerUnity.GameManagerUnityState.GENERATING;
    }


}
