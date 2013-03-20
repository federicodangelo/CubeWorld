using System;
using System.Collections;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.World.Generator;
using CubeWorld.Tiles.Rules;
using CubeWorld.Sectors;
using CubeWorld.Utils;
using CubeWorld.World.Lights;
using CubeWorld.Tiles.Components;
using SourceCode.CubeWorld.Utils;

namespace CubeWorld.Tiles
{
    public class TileManager
    {
		public CubeWorld.World.CubeWorld world;
		
        private Tile[] tiles;
		private Dictionary<TilePosition, DynamicTile> dynamicTiles = new Dictionary<TilePosition, DynamicTile>();

        public DynamicTile[] DynamicTiles
        {
            get { return new List<DynamicTile>(dynamicTiles.Values).ToArray(); }
        }
		
        public TileDefinition[] tileDefinitions;

        public int sizeXbits;
        public int sizeYbits;
        public int sizeZbits;

        public int sizeX;
        public int sizeY;
        public int sizeZ;
		
        private int sizeXBitsYBits;
		
        private bool enqueueTileUpdates;
		private bool reportTileInvalidated;
        private bool updateLighting;
		
        private Queue<TilePosition> pendingTileUpdates;
        private Queue<TilePosition> pendingTileUpdates1 = new Queue<TilePosition>();
        private Queue<TilePosition> pendingTileUpdates2 = new Queue<TilePosition>();

        private byte[] topPositions;
        
        private Dictionary<TilePosition, DynamicTile> dynamicTilesTimeout = new Dictionary<TilePosition, DynamicTile>();

        private uint ticks;

        public uint Ticks
        {
            get { return ticks; }
        }

        public TileManager(CubeWorld.World.CubeWorld world)
        {
			this.world = world;
            pendingTileUpdates = pendingTileUpdates1;
        }

        public void Create(TileDefinition[] tileDefinitions, int sizeXbits, int sizeYbits, int sizeZbits)
        {
            this.sizeXbits = sizeXbits;
            this.sizeYbits = sizeYbits;
            this.sizeZbits = sizeZbits;

            sizeXBitsYBits = sizeXbits + sizeYbits;

            sizeX = 1 << sizeXbits;
            sizeY = 1 << sizeYbits;
            sizeZ = 1 << sizeZbits;

            tiles = new Tile[1 << (sizeXbits + sizeYbits + sizeZbits)];
            topPositions = new byte[1 << (sizeXbits + sizeZbits)];

            this.tileDefinitions = tileDefinitions;

            this.ticks = (uint) new Random().Next(0, 100);
        }

        public GeneratorProcess Generate(CubeWorldGenerator generator)
        {
            enqueueTileUpdates = false;
            reportTileInvalidated = false;
            updateLighting = false;

            ChainedWorldGenerator chained = new ChainedWorldGenerator();
            chained.AddGenertor(generator);
            chained.AddGenertor(new InternalWorldInitializationGenerator());

            return new GeneratorProcess(chained, this.world);
        }

        private class InternalWorldInitializationGenerator : CubeWorldGenerator
        {
            private int generationStep;
            private int initialIterations = 100;

            public override int GetTotalCost()
            {
                return 5;
            }

            public override int GetCurrentCost()
            {
                return generationStep;
            }

            public override bool Generate(CubeWorld.World.CubeWorld world)
            {
                switch (generationStep)
                {
                    case 0:
                        world.tileManager.EnqueueTilesWithRules();
                        world.tileManager.enqueueTileUpdates = true;
                        generationStep++;
                        break;

                    case 1:
                        if (world.tileManager.pendingTileUpdates.Count > 0 && initialIterations-- >= 0)
                            world.tileManager.Update(TILE_UPDATE_STEP);
                        else
                            generationStep++;
                        break;

                    case 2:
                        LightModelAmbient.InitLuminance(world.tileManager);
                        generationStep++;
                        break;

                    case 3:
                        LightModelLightSource.InitLuminance(world.tileManager);
                        generationStep++;
                        break;

                    case 4:
                        world.tileManager.updateLighting = true;
                        world.tileManager.reportTileInvalidated = true;

                        world.gameplay.WorldCreated();
                        return true;
                }

                return false;
            }

            public override string ToString()
            {
                switch (generationStep)
                {
                    case 0: return "Adding rules";
                    case 1: return "Updating rules";
                    case 2: return "Illuminating (1/2)";
                    case 3: return "Illuminating (2/2)";
                    case 4: return "Preparing";
                }

                return "Ready";
            }
        }

        public TileDefinition GetTileDefinition(byte type)
        {
			return tileDefinitions[type];
        }
		
        public TileDefinition GetTileDefinitionById(string id)
        {
            foreach (TileDefinition tileDefinition in tileDefinitions)
                if (tileDefinition.id == id)
                    return tileDefinition;

            return null;
        }

        public int GetTopPosition(int x, int z)
        {
            return topPositions[x | (z << sizeXbits)];
        }

        public Tile GetTile(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)];
        }

        public byte GetTileType(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].tileType;
        }

        public void SetTileType(TilePosition pos, byte type)
		{
            Tile oldTile = tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)];
			
			if (oldTile.Dynamic)
			{
				SetTileDynamic(pos, false, false, 0);
				oldTile.Dynamic = false;
			}

            if (oldTile.LightSource)
            {
                if (updateLighting)
                {
                    LightModelLightSource.UpdateLuminanceDark(this, pos);
                    oldTile.LightSourceLuminance = GetTileLightSourceLuminance(pos);
                }
                else
                {
                    oldTile.LightSourceLuminance = 0;
                }
            }

            Tile newTile = oldTile;
            TileDefinition tileDefinition = GetTileDefinition(type);

            newTile.tileType = type;
            newTile.Energy = (byte) tileDefinition.energy;
            newTile.Destroyed = false;
            newTile.OnFire = false;
            newTile.CastShadow = tileDefinition.castShadow;
            newTile.LightSource = (tileDefinition.lightSourceIntensity > 0);
            newTile.ExtraData = 0;

            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)] = newTile;

            if (updateLighting)
            {
                if (newTile.CastShadow != oldTile.CastShadow)
                {
                    if (newTile.CastShadow)
                    {
                        LightModelAmbient.UpdateLuminanceDark(this, pos);
                        LightModelLightSource.UpdateLuminanceDark(this, pos);
                    }
                    else
                    {
                        LightModelAmbient.UpdateLuminanceLight(this, pos, newTile.AmbientLuminance);
                        LightModelLightSource.UpdateLuminanceLight(this, pos, newTile.LightSourceLuminance);
                    }
                }

                if (tileDefinition.lightSourceIntensity > 0)
                    LightModelLightSource.UpdateLuminanceLight(this, pos, tileDefinition.lightSourceIntensity);
            }
            else
            {
                if (newTile.LightSource)
                    SetTileLightSourceLuminance(pos, tileDefinition.lightSourceIntensity);
            }

            ExecuteTileActions(pos, TileActionRule.ActionType.CREATED);

            if (enqueueTileUpdates)
                EnqueueInvalidatedTileAndNearTiles(pos);

            UpdateTopPosition(pos, type);

            if (reportTileInvalidated)
                ReportTileInvalidated(pos);
        }

		public void MoveTile(TilePosition oldTilePos, TilePosition newTilePos)
		{
            Tile oldTile = tiles[oldTilePos.x | (oldTilePos.y << sizeXbits) | (oldTilePos.z << sizeXBitsYBits)];

            Tile oldTileUpdated = oldTile;

            if (oldTile.LightSource)
            {
                if (updateLighting)
                {
                    LightModelLightSource.UpdateLuminanceDark(this, oldTilePos);
                    oldTileUpdated.LightSourceLuminance = GetTileLightSourceLuminance(oldTilePos);
                }
                else
                {
                    oldTile.LightSourceLuminance = 0;
                }
            }

            oldTileUpdated.tileType = TileDefinition.EMPTY_TILE_TYPE;
            oldTileUpdated.Energy = 0;
            oldTileUpdated.Destroyed = false;
            oldTileUpdated.OnFire = false;
            oldTileUpdated.CastShadow = false;
            oldTileUpdated.LightSource = false;
            oldTileUpdated.Dynamic = false;
            oldTileUpdated.ExtraData = 0;

            tiles[oldTilePos.x | (oldTilePos.y << sizeXbits) | (oldTilePos.z << sizeXBitsYBits)] = oldTileUpdated;

            if (updateLighting)
            {
                if (oldTileUpdated.CastShadow != oldTile.CastShadow)
                {
                    LightModelAmbient.UpdateLuminanceLight(this, oldTilePos, oldTileUpdated.AmbientLuminance);
                    LightModelLightSource.UpdateLuminanceLight(this, oldTilePos, oldTileUpdated.LightSourceLuminance);
                }
            }

            UpdateTopPosition(oldTilePos, TileDefinition.EMPTY_TILE_TYPE);

            Tile newTileOriginal = GetTile(newTilePos);

            Tile newTile = oldTile;

            if (newTile.LightSource == false)
                newTile.LightSourceLuminance = newTileOriginal.LightSourceLuminance;

            newTile.AmbientLuminance = newTileOriginal.AmbientLuminance;

            tiles[newTilePos.x | (newTilePos.y << sizeXbits) | (newTilePos.z << sizeXBitsYBits)] = newTile;

            if (newTile.Dynamic)
            {
                DynamicTile dynamicTile = dynamicTiles[oldTilePos];

                dynamicTiles.Remove(oldTilePos);
                dynamicTiles[newTilePos] = dynamicTile;

                dynamicTile.tilePosition = newTilePos;

                if (dynamicTilesTimeout.ContainsKey(oldTilePos))
                {
                    dynamicTilesTimeout.Remove(oldTilePos);
                    dynamicTilesTimeout[newTilePos] = dynamicTile;
                }
            }

            if (updateLighting)
            {
                if (newTile.CastShadow)
                {
                    LightModelAmbient.UpdateLuminanceDark(this, newTilePos);
                    LightModelLightSource.UpdateLuminanceDark(this, newTilePos);
                }

                if (newTile.LightSource)
                    LightModelLightSource.UpdateLuminanceLight(this, newTilePos, newTile.LightSourceLuminance);
            }
            else
            {
                if (newTile.LightSource)
                    SetTileLightSourceLuminance(newTilePos, newTile.LightSourceLuminance);
            }

            UpdateTopPosition(newTilePos, newTile.tileType);

            if (enqueueTileUpdates)
            {
                EnqueueInvalidatedTileAndNearTiles(oldTilePos);
                EnqueueInvalidatedTileAndNearTiles(newTilePos);
            }

            if (reportTileInvalidated)
            {
                ReportTileInvalidated(oldTilePos);
                ReportTileInvalidated(newTilePos);
            }
		}
		
        private void EnqueueInvalidatedTileAndNearTiles(TilePosition pos)
        {
            EnqueueInvalidatedTile(pos);

            if (pos.x > 0)
                EnqueueInvalidatedTile(pos + new TilePosition(-1, 0, 0));

            if (pos.x < sizeX - 1)
                EnqueueInvalidatedTile(pos + new TilePosition(1, 0, 0));

            if (pos.y > 0)
                EnqueueInvalidatedTile(pos + new TilePosition(0, -1, 0));

            if (pos.y < sizeY - 1)
                EnqueueInvalidatedTile(pos + new TilePosition(0, 1, 0));

            if (pos.z > 0)
                EnqueueInvalidatedTile(pos + new TilePosition(0, 0, -1));

            if (pos.z < sizeZ - 1)
                EnqueueInvalidatedTile(pos + new TilePosition(0, 0, 1));
        }

        public void SetTileOnFire(TilePosition pos, bool onFire)
        {
            if (GetTileType(pos) != TileDefinition.EMPTY_TILE_TYPE)
            {
                if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].OnFire != onFire)
                {
                    tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].OnFire = onFire;

                    if (onFire)
                    {
                        ExecuteTileActions(pos, TileActionRule.ActionType.ONFIRE);

                        if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSource == false)
                        {
                            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSource = true;

                            if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].CastShadow)
                            {
                                tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].CastShadow = false;
                                if (updateLighting)
                                    LightModelAmbient.UpdateLuminanceDark(this, pos);
                            }

                            if (updateLighting)
                                LightModelLightSource.UpdateLuminanceLight(this, pos, (byte)(Tile.MAX_LUMINANCE - 1));
                            else
                                SetTileLightSourceLuminance(pos, (byte)(Tile.MAX_LUMINANCE - 1));
                        }
                    }
                    else
                    {
                        TileDefinition def = GetTileDefinition(GetTileType(pos));

                        if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSource != (def.lightSourceIntensity > 0))
                        {
                            if (updateLighting)
                                LightModelLightSource.UpdateLuminanceDark(this, pos);

                            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSource = false;

                            if (def.castShadow != tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].CastShadow != def.castShadow)
                            {
                                tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].CastShadow = false;

                                if (updateLighting)
                                    LightModelAmbient.UpdateLuminanceLight(this, pos, GetTileAmbientLuminance(pos));
                            }
                        }
                    }

                    if (enqueueTileUpdates)
                        EnqueueInvalidatedTileAndNearTiles(pos);

                    if (reportTileInvalidated)
                        ReportTileInvalidated(pos);
                }
            }
        }
				
		public bool HasTileActions(TilePosition pos, TileActionRule.ActionType actionType)
		{
            TileDefinition tileDefinition = GetTileDefinition(GetTileType(pos));

            return tileDefinition.tileActionRules != null && tileDefinition.tileActionRules.GetRulesForAction(actionType) != null;
		}

        private bool ExecuteTileActions(TilePosition pos, TileActionRule.ActionType actionType)
        {
            TileDefinition tileDefinition = GetTileDefinition(GetTileType(pos));

            if (tileDefinition.tileActionRules != null && tileDefinition.tileActionRules.GetRulesForAction(actionType) != null)
            {
                TileActionRule actionRules = tileDefinition.tileActionRules.GetRulesForAction(actionType);

                ExecuteTileRules(pos, GetTile(pos), actionRules.rules);

                return true;
            }

            return false;
        }

        public bool GetTileOnFire(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].OnFire;
        }

        public void SetTileEnergy(TilePosition pos, byte energy)
        {
            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Energy = energy;

            if (reportTileInvalidated)
                ReportTileInvalidated(pos);
        }

        public byte GetTileEnergy(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Energy;
        }
		
        public void SetTileDynamic(TilePosition pos, bool dynamic, bool useGravity, int timeout)
        {
            if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Dynamic != dynamic)
            {
                if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Dynamic)
                {
                    DynamicTile dynamicTile = dynamicTiles[pos];
                    dynamicTiles.Remove(pos);
                    if (dynamicTile.timeout > 0)
                        dynamicTilesTimeout.Remove(pos);
                    world.cwListener.DestroyObject(dynamicTile);
                    dynamicTile.Clear();
                }

                tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Dynamic = dynamic;

                if (dynamic)
                {
                    DynamicTile dynamicTile = new DynamicTile(world, pos, true, world.gameplay.NextObjectId());
                    if (useGravity)
                        dynamicTile.AddComponent(new TileComponentGravity());
                    dynamicTiles[pos] = dynamicTile;
                    if (timeout > 0)
                    {
                        dynamicTile.timeout = timeout;
                        dynamicTilesTimeout[pos] = dynamicTile;
                    }

                    world.cwListener.CreateObject(dynamicTile);
                }

                if (reportTileInvalidated)
                    ReportTileInvalidated(pos);
            }
            else
            {
                if (dynamic && timeout >= 0)
                    SetTileDynamicTimeout(pos, timeout);
            }
        }

        public void SetTileDynamicTimeout(TilePosition pos, int timeout)
        {
            if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Dynamic)
            {
                DynamicTile dynamicTile = dynamicTiles[pos];

                if (dynamicTile.timeout > 0)
                {
                    //Dynamic tile already in the dynamicTilesTimeout dictionary, update timeout value or remove
                    if (timeout <= 0)
                    {
                        dynamicTile.timeout = 0;
                        dynamicTilesTimeout.Remove(pos);
                    }
                    else
                        dynamicTile.timeout = timeout;
                }
                else if (timeout > 0)
                {
                    //Dynamic tile not in the dynamicTilesTimeout dictionary, add it
                    dynamicTile.timeout = timeout;
                    dynamicTilesTimeout[pos] = dynamicTile;
                }
            }
        }

        public DynamicTile GetDynamicTile(TilePosition pos)
        {
            return dynamicTiles[pos];
        }

        public bool GetTileDynamic(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Dynamic;
        }

        public void SetTileAmbientLuminance(TilePosition pos, byte luminance)
        {
            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].AmbientLuminance = luminance;

            if (reportTileInvalidated)
                ReportTileInvalidated(pos, true);
        }

        public void SetTileLightSourceLuminance(TilePosition pos, byte luminance)
        {
            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSourceLuminance = luminance;

            if (reportTileInvalidated)
                ReportTileInvalidated(pos, true);
        }

        public void SetTileExtraData(TilePosition pos, byte extraData)
        {
            tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].ExtraData = extraData;

            if (reportTileInvalidated)
                ReportTileInvalidated(pos);

            if (enqueueTileUpdates) 
                EnqueueInvalidatedTileAndNearTiles(pos);
        }

        public byte GetTileExtraData(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].ExtraData;
        }

        private void UpdateTopPosition(TilePosition pos, byte type)
        {
            int top = GetTopPosition(pos.x, pos.z);

            if (type != TileDefinition.EMPTY_TILE_TYPE)
            {
                if (top < pos.y)
                    topPositions[pos.x | (pos.z << sizeXbits)] = (byte) pos.y;
            }
            else
            {
                if (top == pos.y)
                {
                    top = 0;
                    for (int i = sizeY - 1; i >= 0; i--)
                    {
                        if (GetTileType(new TilePosition(pos.x, i, pos.z)) != TileDefinition.EMPTY_TILE_TYPE)
                        {
                            top = i;
                            break;
                        }
                    }

                    topPositions[pos.x | (pos.z << sizeXbits)] = (byte) top;
                }
            }
        }

        public void EnqueueInvalidatedTile(TilePosition pos)
        {
            if (tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Enqueued == false)
            {
                tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Enqueued = true;

                pendingTileUpdates.Enqueue(pos);

                world.stats.invalidatedTiles++;
            }
        }

        public bool GetTileSolid(TilePosition pos)
        {
            return GetTileDefinition(tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].tileType).solid;
        }
		
        public bool GetTileLiquid(TilePosition pos)
        {
            return GetTileDefinition(tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].tileType).liquid;
        }
		
        public bool GetTileBurns(TilePosition pos)
        {
            return GetTileDefinition(tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].tileType).burns;
        }
		
        public TileDefinition.DrawMode GetTileDrawMode(TilePosition pos)
        {
            return GetTileDefinition(tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].tileType).drawMode;
        }

        public bool GetTileCastShadow(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].CastShadow;
        }

        public bool GetTileLightSource(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSource;
        }

        public byte GetTileAmbientLuminance(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].AmbientLuminance;
        }

        public byte GetTileLightSourceLuminance(TilePosition pos)
        {
            return tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].LightSourceLuminance;
        }

        public bool IsValidTile(TilePosition pos)
        {
            return pos.x >= 0 && pos.x < sizeX && pos.y >= 0 && pos.y < sizeY && pos.z >= 0 && pos.z < sizeZ;
        }
		
		public const float TILE_UPDATE_STEP = 0.1f;
		
        private float lastTilesUpdate = 0.0f;

        public void Update(float deltaTime)
        {
            lastTilesUpdate += deltaTime;

            if (lastTilesUpdate >= TILE_UPDATE_STEP)
            {
	            UpdateTiles();
				
                lastTilesUpdate = 0.0f;
            }

            if (dynamicTiles.Count > 0)
            {
                //TODO: I'm making a copy of the array because the update function
                // can change the amount of dynamic tiles.. see how to fix this!!
                foreach (DynamicTile dynamicTile in new List<DynamicTile>(dynamicTiles.Values))
                    dynamicTile.Update(deltaTime);
            }
        }

        public void TileHitFloor(TilePosition pos)
        {
            ExecuteTileActions(pos, TileActionRule.ActionType.HIT_FLOOR);
        }

        public void TileClicked(TilePosition pos)
        {
            ExecuteTileActions(pos, TileActionRule.ActionType.CLICKED);
        }

        public void TileDamaged(TilePosition pos)
        {
            ExecuteTileActions(pos, TileActionRule.ActionType.DAMAGED);
        }
		
        public bool DamageTile(TilePosition pos, int damage)
        {
            Tile tile = GetTile(pos);

            if (tile.tileType != TileDefinition.EMPTY_TILE_TYPE)
            {
                TileDefinition tileDefinition = GetTileDefinition(tile.tileType);

                return world.gameplay.ProcessTileDamage(pos, tile, tileDefinition, damage);
            }

            return false;
        }

        public void DestroyTile(TilePosition pos)
        {
            Tile tile = GetTile(pos);

            if (tile.tileType != TileDefinition.EMPTY_TILE_TYPE && tile.Destroyed == false)
            {
                tiles[pos.x | (pos.y << sizeXbits) | (pos.z << sizeXBitsYBits)].Destroyed = true;

                ExecuteTileActions(pos, TileActionRule.ActionType.DESTROYED);

                SetTileType(pos, TileDefinition.EMPTY_TILE_TYPE);
            }
        }

        private void UpdateTiles()
        {
            if (enqueueTileUpdates == false)
                return;

            if (pendingTileUpdates.Count > 0 || dynamicTilesTimeout.Count > 0)
            {
                if (dynamicTilesTimeout.Count > 0)
                {
                    List<DynamicTile> toExecute = new List<DynamicTile>();

                    //First select the dynamic tiles to execute, then execute all of them together, because if we execute them
                    //while iterating the dictionary, and one of the TIMEOUT handlers sets another's tile timeout timer, then the iterator
                    //will become invalid and everything fails
                    
                    foreach(DynamicTile pending in dynamicTilesTimeout.Values)
                        if (--pending.timeout <= 0)
                            toExecute.Add(pending);

                    foreach (DynamicTile pending in toExecute)
                        dynamicTilesTimeout.Remove(pending.tilePosition);

                    //The execution of the TIMEOUT handlers can set another's tile timeout value, so we validate once again
                    //that the tiles are still dynamic and that the timeout is valid
                    foreach (DynamicTile pending in toExecute)
                        if (pending.Dynamic && pending.timeout <= 0)
                            ExecuteTileActions(pending.tilePosition, TileActionRule.ActionType.TIMEOUT);
                }

                foreach (TilePosition update in pendingTileUpdates)
                    tiles[update.x | (update.y << sizeXbits) | (update.z << sizeXBitsYBits)].Enqueued = false;

                if (pendingTileUpdates == pendingTileUpdates1)
                {
                    pendingTileUpdates = pendingTileUpdates2;

                    while (pendingTileUpdates1.Count > 0)
                        UpdateTile(pendingTileUpdates1.Dequeue());
                }
                else
                {
                    pendingTileUpdates = pendingTileUpdates1;

                    while (pendingTileUpdates2.Count > 0)
                        UpdateTile(pendingTileUpdates2.Dequeue());
                }
            }

            ticks++;
        }

        private void UpdateTile(TilePosition pos)
        {
            Tile tile = GetTile(pos);

            TileDefinition tileDefinition = GetTileDefinition(tile.tileType);

            if (tileDefinition.tileUpdateRules != null)
            {
                world.stats.updatedTiles++;

                ExecuteTileRules(pos, tile, tileDefinition.tileUpdateRules.rules);
            }
        }

        private void ExecuteTileRules(TilePosition pos, Tile tile, TileRule[] rules)
        {
            world.stats.executedRules += rules.Length;

            foreach (TileRule rule in rules)
            {
                if (rule.CheckConditions(this, tile, pos))
                {
                    rule.Execute(this, tile, pos);

                    if (GetTileType(pos) != tile.tileType)
                        break;
                }
            }
        }

        private void ExecuteTileRule(TilePosition pos, Tile tile, TileRule rule)
        {
            world.stats.executedRules++;

            if (rule.CheckConditions(this, tile, pos))
                rule.Execute(this, tile, pos);
        }

        private void ReportTileInvalidated(TilePosition pos)
        {
            ReportTileInvalidated(pos, false);
        }

        private void ReportTileInvalidated(TilePosition pos, bool lightRelated)
        {
            world.sectorManager.TileInvalidated(pos);
            if (GetTileDynamic(pos))
                dynamicTiles[pos].Invalidate();

            world.gameplay.TileInvalidated(pos, lightRelated);
        }

        public void Save(System.IO.BinaryWriter bw)
        {
            //Write tile definition "type" - "id" mapper (used to load the map if the "type" of each tile changes)
            bw.Write((int)tileDefinitions.Length);
            foreach (TileDefinition def in tileDefinitions)
            {
                bw.Write((byte)def.tileType);
                bw.Write((string)def.id);
            }

            //Write map size
            //This is written in CubeWorld.Save()
            //bw.Write((int)sizeXbits);
            //bw.Write((int)sizeYbits);
            //bw.Write((int)sizeZbits);
			
			byte[] tileBytes = new byte[sizeX * sizeY * sizeZ * 4];
			
			//Write tiles
			for (int n = tiles.Length - 1; n >= 0; n--)
			{
                Tile tile = tiles[n];
				tile.Enqueued = false;
				
				tileBytes[(n << 2) | 0] = tile.tileType;
				tileBytes[(n << 2) | 1] = tile.luminance;
				tileBytes[(n << 2) | 2] = tile.extra;
				tileBytes[(n << 2) | 3] = tile.extra2;
			}
			
			bw.Write(tileBytes);
			
			//Write dynamic tiles
			bw.Write(dynamicTiles.Count);
			foreach(DynamicTile dynamicTile in dynamicTiles.Values)
			{
                bw.Write(dynamicTile.objectId);
				SerializationUtils.Write(bw, dynamicTile.tilePosition);
				bw.Write(dynamicTile.timeout);
            }

            //Write top positions
			bw.Write(topPositions);
        }
		
        public void Load(System.IO.BinaryReader br)
        {
            //Read tile definition mapper (oldId -> newId)
            byte[] mapTileDefinitionMapper = new byte[byte.MaxValue];
            
            int definitions = br.ReadInt32();
            for (int i = 0; i < definitions; i++)
            {
                byte oldType = br.ReadByte();
                string id = br.ReadString();
                byte newType = 0;

                foreach (TileDefinition td in tileDefinitions)
                {
                    if (td.id == id)
                    {
                        newType = td.tileType;
                        break;
                    }
                }

                mapTileDefinitionMapper[oldType] = newType;
            }

            //Read map size
            //This is read in CubeWorld.Load()
            //int sizeXbits = br.ReadInt32();
            //int sizeYbits = br.ReadInt32();
            //int sizeZbits = br.ReadInt32();
			
            //Create empty world
            //The world is already created in CubeLoad.Load()
            //Create(tileDefinitions, sizeXbits, sizeYbits, sizeZbits);
			
            //Read tiles
			byte[] tileBytes = br.ReadBytes(sizeX * sizeY * sizeZ * 4);
			Tile tile = new Tile();
			
			for (int n = tiles.Length - 1; n >= 0; n--)
			{
				tile.tileType = mapTileDefinitionMapper[tileBytes[(n << 2) | 0]];
				tile.luminance = tileBytes[(n << 2) | 1];
				tile.extra = tileBytes[(n << 2) | 2];
				tile.extra2 = tileBytes[(n << 2) | 3];
				
				tiles[n] = tile;
			}
			
			//Read dynamic tiles
			int nDyanamicTiles = br.ReadInt32();
			for (int n = 0; n < nDyanamicTiles; n++)
			{
                int objectId = br.ReadInt32();
				TilePosition pos = SerializationUtils.ReadTilePosition(br);
				int timeout = br.ReadInt32();
				
                DynamicTile dynamicTile = new DynamicTile(world, pos, true, objectId);

                //TODO: Get gravity attribute from somewhere
                if (true)
                    dynamicTile.AddComponent(new TileComponentGravity());

                dynamicTiles[pos] = dynamicTile;
				
                if (timeout > 0)
                {
                    dynamicTile.timeout = timeout;
                    dynamicTilesTimeout[pos] = dynamicTile;
                }

                world.cwListener.CreateObject(dynamicTile);
			}

            //Read top positions
			topPositions = br.ReadBytes(sizeX * sizeZ);

            if (world.gameplay.useEnqueueTileUpdates)
                EnqueueTilesWithRules();

            enqueueTileUpdates = world.gameplay.useEnqueueTileUpdates;
            updateLighting = true;
            reportTileInvalidated = true;

            if (world.gameplay.useEnqueueTileUpdates)
                UpdateTiles();
			
            //The read tiles already have the light information updated
            //LightModelAmbient.InitLuminance(this);
            //LightModelLightSource.InitLuminance(this);
        }


        private void EnqueueTilesWithRules()
        {
            bool[] mapTileDefinitionHasRule = new bool[byte.MaxValue];

            foreach (TileDefinition td in tileDefinitions)
                mapTileDefinitionHasRule[td.tileType] = (td.tileUpdateRules != null);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        TilePosition pos = new TilePosition(x, y, z);
                        byte tileType = GetTileType(pos);

                        if (mapTileDefinitionHasRule[tileType])
                            EnqueueInvalidatedTile(pos);
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (DynamicTile tile in dynamicTiles.Values)
                tile.Clear();

            tiles = null;
            tileDefinitions = null;
            topPositions = null;
			dynamicTiles = null;

            pendingTileUpdates = null;
            pendingTileUpdates1 = null;
            pendingTileUpdates2 = null;
        }
	}
}