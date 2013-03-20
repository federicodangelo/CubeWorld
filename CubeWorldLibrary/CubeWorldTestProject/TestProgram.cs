using System;
using System.Collections.Generic;
using CubeWorld.Configuration;
using CubeWorld;
using CubeWorld.Gameplay;
using CubeWorld.World;
using CubeWorld.World.Generator;
using CubeWorld.Console;
using CubeWorldTestProject;

namespace CubeWorldTestProject
{
    public class TestProgram : ICWFxListener, ICWListener
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            new TestSerializer();
            new TestProgram();
        }

        public TestProgram()
        {
            string s = CWConsole.Singleton.TextLog;

            AvailableConfigurations availableConfigurations = LoadConfiguration();

            Config lastConfig;

            lastConfig = new CubeWorld.Configuration.Config();
            lastConfig.tileDefinitions = availableConfigurations.tileDefinitions;
            lastConfig.itemDefinitions = availableConfigurations.itemDefinitions;
            lastConfig.avatarDefinitions = availableConfigurations.avatarDefinitions;
            lastConfig.dayInfo = availableConfigurations.dayInfos[0];
            lastConfig.worldGenerator = availableConfigurations.worldGenerators[0];
            lastConfig.worldSize = availableConfigurations.worldSizes[0];
            lastConfig.extraMaterials = availableConfigurations.extraMaterials;
            lastConfig.gameplay = GameplayFactory.AvailableGameplays[0];

            CubeWorld.World.CubeWorld world = new CubeWorld.World.CubeWorld(this, this);
            GeneratorProcess worldGeneratorProcess = world.Generate(lastConfig);

            while (worldGeneratorProcess.Generate() == false)
            {

            }

            Console.WriteLine("World generated");
        }

        static public AvailableConfigurations LoadConfiguration()
        {
            AvailableConfigurations availableConfigurations =
                new ConfigParserXML().Parse(
                    GetConfigText("config_misc"),
                    GetConfigText("config_tiles"),
                    GetConfigText("config_avatars"),
                    GetConfigText("config_items"),
                    GetConfigText("config_generators"));

            return availableConfigurations;
        }

        static public string GetConfigText(string resourceName)
        {
            string exePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string fileConfigPath = System.IO.Path.Combine(exePath, resourceName + ".xml");

            String configText = System.IO.File.ReadAllText(fileConfigPath);

            return configText;
        }

        public void CreateObject(CubeWorld.World.Objects.CWObject cwobject)
        {
        }

        public void UpdateObject(CubeWorld.World.Objects.CWObject cwobject)
        {
        }

        public void DestroyObject(CubeWorld.World.Objects.CWObject cwobject)
        {
        }

        public void PlaySound(string soundId, CubeWorld.Utils.Vector3 position)
        {
        }

        public void PlaySound(string soundId, CubeWorld.World.Objects.CWObject fromObject)
        {
        }

        public void PlayEffect(string effectId, CubeWorld.Utils.Vector3 position)
        {
        }

        public void PlayEffect(string effectId, CubeWorld.World.Objects.CWObject fromObject)
        {
        }
    }
}
