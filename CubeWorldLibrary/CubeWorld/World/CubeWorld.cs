using System;
using System.Collections;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.World.Generator;
using CubeWorld.Tiles.Rules;
using CubeWorld.Sectors;
using CubeWorld.Utils;
using CubeWorld.World.Lights;
using CubeWorld.Items;
using CubeWorld.Configuration;
using CubeWorld.Avatars;
using System.IO;
using Ionic.Zlib;
using CubeWorld.Gameplay;
using CubeWorld.Serialization;

namespace CubeWorld.World
{
    public class CubeWorld
    {
        public const string VERSION_INFO = "CW-V1";

        public SectorManager sectorManager;
		public TileManager tileManager;
		public ItemManager itemManager;
		public AvatarManager avatarManager;
		public DayCycleManager dayCycleManager;
        public BaseGameplay gameplay;

        public ConfigSurroundings configSurroundings;
        public ConfigExtraMaterials configExtraMaterials;
		
        public CubeWorldStats stats = new CubeWorldStats();

		public ICWListener cwListener;
        public ICWFxListener fxListener;

        public int sizeXbits;
        public int sizeYbits;
        public int sizeZbits;
        public int sizeX;
        public int sizeY;
        public int sizeZ;
		
        public CubeWorld(ICWListener cwListener, ICWFxListener fxListener)
        {
			this.cwListener = cwListener;
            this.fxListener = fxListener;
            this.sectorManager = new SectorManager(this);
			this.tileManager = new TileManager(this);
			this.itemManager = new ItemManager(this);
			this.avatarManager = new AvatarManager(this);
			this.dayCycleManager = new DayCycleManager(this);
        }

        public GeneratorProcess Generate(Config config)
        {
            this.sizeXbits = config.worldSize.worldSizeBitsX;
            this.sizeYbits = config.worldSize.worldSizeBitsY;
            this.sizeZbits = config.worldSize.worldSizeBitsZ;

            sizeX = 1 << sizeXbits;
            sizeY = 1 << sizeYbits;
            sizeZ = 1 << sizeZbits;

            gameplay = config.gameplay.gameplay;
            gameplay.Init(this);
			
			tileManager.Create(config.tileDefinitions, sizeXbits, sizeYbits, sizeZbits);
			itemManager.Create(config.itemDefinitions);
			
            avatarManager.Create(config.avatarDefinitions);

			sectorManager.Create();
			dayCycleManager.Create(config.dayInfo);

            configSurroundings = config.worldGenerator.surroundings;
            configExtraMaterials = config.extraMaterials;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            return gameplay.Generate(config);
		}

        public void Update(float deltaTime)
        {
			avatarManager.Update(deltaTime);
			
			tileManager.Update(deltaTime);
			
			itemManager.Update(deltaTime);
			
			dayCycleManager.Update(deltaTime);

            sectorManager.Update(deltaTime);

            gameplay.Update(deltaTime);
        }

        public byte[] Save()
        {
			//Save world to memory
			MemoryStream bytesStream = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(bytesStream);
			
            Save(bw, false);
            tileManager.Save(bw);
            itemManager.Save(bw);
            avatarManager.Save(bw);
            sectorManager.Save(bw);
            dayCycleManager.Save(bw);

            bw.Flush();
			
			byte[] bytes = bytesStream.ToArray();
			
			//Write header
            MemoryStream bytesStreamFinal = new MemoryStream();
            BinaryWriter bwFinal = new BinaryWriter(bytesStreamFinal);

            bwFinal.Write(VERSION_INFO);
			bwFinal.Write(bytes.Length);
			bwFinal.Flush();
			
			//Compress and write world
			GZipStream gzipStream = new GZipStream(bytesStreamFinal, CompressionMode.Compress, CompressionLevel.BestSpeed, true);
			gzipStream.Write(bytes, 0, bytes.Length);
			gzipStream.Close();

            return bytesStreamFinal.ToArray();
        }

        public byte[] SaveMultiplayer(Player player)
        {
            //Save world to memory
            MemoryStream bytesStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(bytesStream);

            //Basic world info
            SaveMultiplayer(bw);

            //Managers
            tileManager.Save(bw);
            itemManager.Save(bw);
            sectorManager.Save(bw);
            dayCycleManager.Save(bw);

            //Player avatar
            bw.Write(player.objectId);
            player.Save(bw);

            bw.Flush();

            byte[] bytes = bytesStream.ToArray();

            //Write header
            MemoryStream bytesStreamFinal = new MemoryStream();
            BinaryWriter bwFinal = new BinaryWriter(bytesStreamFinal);

            bwFinal.Write(bytes.Length);
            bwFinal.Flush();

            //Compress and write world
            GZipStream gzipStream = new GZipStream(bytesStreamFinal, CompressionMode.Compress, CompressionLevel.BestSpeed, true);
            gzipStream.Write(bytes, 0, bytes.Length);
            gzipStream.Close();

            return bytesStreamFinal.ToArray();
        }

        public MultiplayerConfig LoadMultiplayer(byte[] data)
        {
            MemoryStream memoryStream = new MemoryStream(data);
            BinaryReader br = new BinaryReader(memoryStream);

            //Read compressed bytes
            int uncompressedSize = br.ReadInt32();

            //Uncompress bytes
            byte[] uncompressedBytes = new byte[uncompressedSize];
            GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            int bytesRead = 0;

            while (true)
            {
                int read = gzipStream.Read(uncompressedBytes, bytesRead, uncompressedSize - bytesRead);

                if (read == 0)
                    break;

                bytesRead += read;
            }

            br = new BinaryReader(new MemoryStream(uncompressedBytes));

            //Load world from uncompressed bytes
            MultiplayerConfig config = LoadMultiplayer(br);

            tileManager.Create(config.tileDefinitions, sizeXbits, sizeYbits, sizeZbits);
            itemManager.Create(config.itemDefinitions);
            avatarManager.Create(config.avatarDefinitions);
            sectorManager.Create();
            dayCycleManager.Create(null);

            tileManager.Load(br);
            itemManager.Load(br);
            sectorManager.Load(br);
            dayCycleManager.Load(br);

            //Player avatar
            int playerObjectId = br.ReadInt32();
            Player player = (Player) avatarManager.CreateAvatar(avatarManager.GetAvatarDefinitionById("player"), playerObjectId, new Vector3(), false);
            player.Load(br);

            cwListener.CreateObject(player);

            gameplay.WorldLoaded();

            return config;
        }

        private void SaveMultiplayer(BinaryWriter bw)
        {
            SaveDefinitions(bw);
            Save(bw, true);
        }

        private void Save(BinaryWriter bw, bool isMultiplayer)
        {
            bw.Write(sizeXbits);
            bw.Write(sizeYbits);
            bw.Write(sizeZbits);

            if (isMultiplayer == false)
            {
                bw.Write(gameplay.Id);
                gameplay.Save(bw);
            }

            configSurroundings.Save(bw);
        }
		
        public void Load(Config config, byte[] data)
        {
			MemoryStream memoryStream = new MemoryStream(data);
			
            BinaryReader br = new BinaryReader(memoryStream);
			
            //Read version info
            string version = br.ReadString();
            if (version != VERSION_INFO)
                return;
			
			//Read compressed bytes
			int uncompressedSize = br.ReadInt32();
			
			//Uncompress bytes
			byte[] uncompressedBytes = new byte[uncompressedSize];
			GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
			int bytesRead = 0;
			
			while(true)
			{
				int read = gzipStream.Read(uncompressedBytes, bytesRead, uncompressedSize - bytesRead);
				
				if (read == 0)
					break;
				
				bytesRead += read;
			}
			
			br = new BinaryReader(new MemoryStream(uncompressedBytes));

			//Load world from uncompressed bytes
            Load(br, false);
			
            tileManager.Create(config.tileDefinitions, sizeXbits, sizeYbits, sizeZbits);
            itemManager.Create(config.itemDefinitions);
            avatarManager.Create(config.avatarDefinitions);
            sectorManager.Create();
            dayCycleManager.Create(config.dayInfo);

            tileManager.Load(br);
            itemManager.Load(br);
            avatarManager.Load(br);
            sectorManager.Load(br);
            dayCycleManager.Load(br);

            gameplay.WorldLoaded();
        }

        private MultiplayerConfig LoadMultiplayer(BinaryReader br)
        {
            MultiplayerConfig config = LoadDefinitions(br);
            Load(br, true);
            return config;
        }

        private void Load(BinaryReader br, bool isMultiplayer)
        {
            this.sizeXbits = br.ReadInt32();
            this.sizeYbits = br.ReadInt32();
            this.sizeZbits = br.ReadInt32();

            sizeX = 1 << sizeXbits;
            sizeY = 1 << sizeYbits;
            sizeZ = 1 << sizeZbits;

            if (isMultiplayer == false)
            {
                string gameplayId = br.ReadString();

                gameplay = GameplayFactory.GetGameplayById(gameplayId);
                gameplay.Init(this);
                gameplay.Load(br);
            }
            else
            {
                gameplay.Init(this);
            }

            configSurroundings = new ConfigSurroundings();
            configSurroundings.Load(br);
        }

        public void Clear()
        {
            stats = null;
			
			sectorManager.Clear();
			sectorManager = null;
			
			tileManager.Clear();
			tileManager = null;
			
			itemManager.Clear();
			itemManager = null;
			
			avatarManager.Clear();
			avatarManager = null;
			
			dayCycleManager.Clear();
			dayCycleManager = null;

            gameplay.Clear();
            gameplay = null;
		}

        public class MultiplayerConfig : ISerializable
        {
            public TileDefinition[] tileDefinitions;
            public ItemDefinition[] itemDefinitions;
            public AvatarDefinition[] avatarDefinitions;
            public ConfigExtraMaterials extraMaterials;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref tileDefinitions, "tileDefinitions");
                serializer.Serialize(ref itemDefinitions, "itemDefinitions");
                serializer.Serialize(ref avatarDefinitions, "avatarDefinitions");
                serializer.Serialize(ref extraMaterials, "extraMaterials");
            }
        }

        private void SaveDefinitions(BinaryWriter bw)
        {
            MultiplayerConfig config = new MultiplayerConfig();
            config.tileDefinitions = tileManager.tileDefinitions;
            config.itemDefinitions = itemManager.itemDefinitions;
            config.avatarDefinitions = avatarManager.avatarDefinitions;
            config.extraMaterials = this.configExtraMaterials;
			
			RegisterSerializationTypes.Register();
            byte[] data = new Serializer(true).Serialize(config);

            bw.Write(data.Length);
            bw.Write(data);
        }

        private MultiplayerConfig LoadDefinitions(BinaryReader br)
        {
            int n = br.ReadInt32();
            byte[] data = br.ReadBytes(n);

			RegisterSerializationTypes.Register();
            MultiplayerConfig config = (MultiplayerConfig) new Serializer(false).Deserialize(data);

            return config;
        }
    }
}