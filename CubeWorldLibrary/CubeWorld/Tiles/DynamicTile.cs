using System;
using CubeWorld.Utils;
using CubeWorld.World;
using CubeWorld.World.Objects;

namespace CubeWorld.Tiles
{
	public class DynamicTile : CWObject
	{
		public TileDefinition tileDefinition;
		public TilePosition tilePosition;

        /*
         *  Dynamic tile timeout in ticks, NEVER modify this value directly,
         *  always use TileManager.SetTileDynamicTimeout()
         */
        public int timeout;
		
		private bool proxy;
		
		private bool onFire;
		private bool castShadow;
		private bool lightSource;
		private bool enqueued;
		private byte ambientLuminance;
		private byte lightSourceLuminance;
		private bool dynamic;
        private byte extraData;

        private bool invalidated;
		
		public bool IsProxy
		{
			get { return proxy; }
		}
		
		public bool OnFire
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).OnFire;
				else
					return onFire;
			}
		}
		
		public bool CastShadow
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).CastShadow;
				else
					return castShadow;
			}
		}
		
		public bool LightSource
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).LightSource;
				else
					return lightSource;
			}
		}
		
		public bool Enqueued
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).Enqueued;
				else
					return enqueued;
			}
		}


        public byte ExtraData
        {
            get
            {
                if (proxy)
                    return world.tileManager.GetTile(tilePosition).ExtraData;
                else
                    return extraData;
            }
        }
		
		public byte AmbientLuminance
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).AmbientLuminance;
				else
					return ambientLuminance;
			}
		}
		
		public byte LightSourceLuminance
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).LightSourceLuminance;
				else
					return lightSourceLuminance;
			}
		}
		
		public byte Energy
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).Energy;
				else
					return energy;
			}
		}
		
		public bool Destroyed
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).Destroyed;
				else
					return destroyed;
			}
		}
		
		public bool Dynamic
		{
			get 
			{ 
				if (proxy) 
					return world.tileManager.GetTile(tilePosition).Dynamic;
				else
					return dynamic;
			}
		}

        public bool Invalidated
        {
            get { return this.invalidated; }
            set { this.invalidated = value; }
        }

        public DynamicTile(CubeWorld.World.CubeWorld world, TileDefinition tileDefinition, int objectId)
            : base(objectId)
		{
			this.world = world;
			this.definition = tileDefinition;
			this.tileDefinition = tileDefinition;
			
			this.onFire = false;
			this.castShadow = tileDefinition.castShadow;
			this.lightSource = tileDefinition.lightSourceIntensity > 0;
			this.enqueued = false;
			this.ambientLuminance = 0;
			this.lightSourceLuminance = tileDefinition.lightSourceIntensity;
			this.energy = (byte) tileDefinition.energy;
			this.destroyed = false;
			this.dynamic = true;
            this.extraData = 0;
		}

        public DynamicTile(CubeWorld.World.CubeWorld world, TilePosition tilePosition, bool proxy, int objectId)
            : base(objectId)
		{
			this.world = world;
			this.tilePosition = tilePosition;
			this.position = Utils.Graphics.TilePositionToVector3(tilePosition);
			this.proxy = proxy;
			
			Tile tile = world.tileManager.GetTile(tilePosition);
			
			this.tileDefinition = world.tileManager.GetTileDefinition(tile.tileType);
			this.definition = this.tileDefinition;
			
			if (proxy == false)
			{
				this.onFire = tile.OnFire;
				this.castShadow = tile.CastShadow;
				this.lightSource = tile.LightSource;
				this.enqueued = tile.Enqueued;
				this.ambientLuminance = tile.AmbientLuminance;
				this.lightSourceLuminance = tile.LightSourceLuminance;
				this.energy = tile.Energy;
				this.destroyed = tile.Destroyed;
				this.dynamic = tile.Dynamic;
                this.extraData = tile.ExtraData;
			}
		}
		
		public Tile ToTile()
		{
			Tile tile = new Tile();
			
			tile.tileType = this.tileDefinition.tileType;
			tile.OnFire = this.OnFire;
			tile.CastShadow = this.CastShadow;
			tile.LightSource = this.LightSource;
			tile.Enqueued = this.Enqueued;
			tile.AmbientLuminance = this.AmbientLuminance;
			tile.LightSourceLuminance = this.LightSourceLuminance;
			tile.Energy = this.Energy;
			tile.Destroyed = this.Destroyed;
			tile.Dynamic = this.Dynamic;
            tile.ExtraData = this.extraData;
			
			return tile;
		}

        private bool insideUpdate;
        private bool makeStatic = false;

        public void MakeStatic()
        {
            if (insideUpdate)
                makeStatic = true;
            else
                world.tileManager.SetTileDynamic(tilePosition, false, false, 0);
        }

        public override void Update(float deltaTime)
        {
            insideUpdate = true;

            base.Update(deltaTime);

            insideUpdate = false;

            if (makeStatic)
                MakeStatic();
        }

        public void Invalidate()
        {
            invalidated = true;
        }
    }
}
