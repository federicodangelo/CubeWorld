using System;
using System.Collections;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.Utils;

namespace CubeWorld.Sectors
{
    public class SectorManager
    {
        public const int SECTOR_SIZE_BITS = 4;
        public const int SECTOR_SIZE = 1 << SECTOR_SIZE_BITS;

        public CubeWorld.World.CubeWorld world;

        public int xSectors, ySectors, zSectors;
        public int xSectorsBits, ySectorsBits, zSectorsBits, xySectorsBits;
        
        public Sector[] sectors;

        private bool pendingSectorsUpdateOrderValid = false;
        private List<Sector> pendingSectorsUpdate = new List<Sector>();
        private bool pendingSectorsUpdateLightOrderValid = false;
        private List<Sector> pendingSectorsUpdateLight = new List<Sector>();

        public SectorManager(CubeWorld.World.CubeWorld world)
        {
            this.world = world;
        }

        public void Create()
        {
            xSectorsBits = world.tileManager.sizeXbits - SECTOR_SIZE_BITS;
            ySectorsBits = world.tileManager.sizeYbits - SECTOR_SIZE_BITS;
            zSectorsBits = world.tileManager.sizeZbits - SECTOR_SIZE_BITS;

            xySectorsBits = xSectorsBits + ySectorsBits;

            xSectors = 1 << xSectorsBits;
            ySectors = 1 << ySectorsBits;
            zSectors = 1 << zSectorsBits;

            sectors = new Sector[xSectors * ySectors * zSectors];

            for (int x = 0; x < xSectors; x++)
                for (int z = 0; z < zSectors; z++)
                    for (int y = ySectors - 1; y >= 0; y--)
                        CreateSector(new TilePosition(x, y, z), false);
        }

        private void CreateSector(TilePosition posSector, bool inmediate)
        {
            TilePosition sectorOffset = new TilePosition(posSector.x << SECTOR_SIZE_BITS, posSector.y << SECTOR_SIZE_BITS, posSector.z << SECTOR_SIZE_BITS);

            Sector sector = new Sector(world, posSector, sectorOffset);

            sectors[posSector.x | (posSector.y << xSectorsBits) | (posSector.z << xySectorsBits)] = sector;

            if (inmediate == false)
                pendingSectorsUpdate.Add(sector);
            else
                sector.UpdateMesh();
        }

        public Sector GetSectorTile(TilePosition pos)
        {
            pos.x >>= SECTOR_SIZE_BITS;
            pos.y >>= SECTOR_SIZE_BITS;
            pos.z >>= SECTOR_SIZE_BITS;

            return sectors[pos.x | (pos.y << xSectorsBits) | (pos.z << xySectorsBits)];
        }

        public Sector GetSector(TilePosition posSector)
        {
            return sectors[posSector.x | (posSector.y << xSectorsBits) | (posSector.z << xySectorsBits)];
        }

        public void TileInvalidated(TilePosition pos)
        {
            int xSector = pos.x >> SECTOR_SIZE_BITS;
            int ySector = pos.y >> SECTOR_SIZE_BITS;
            int zSector = pos.z >> SECTOR_SIZE_BITS;

            EnqueueInvalidatedSector(xSector, ySector, zSector);

            if (pos.x % SECTOR_SIZE == 0 && xSector > 0)
                EnqueueInvalidatedSector(xSector - 1, ySector, zSector);
            else if (pos.x % SECTOR_SIZE == SECTOR_SIZE - 1 && xSector < xSectors - 1)
                EnqueueInvalidatedSector(xSector + 1, ySector, zSector);

            if (pos.y % SECTOR_SIZE == 0 && ySector > 0)
                EnqueueInvalidatedSector(xSector, ySector - 1, zSector);
            else if (pos.y % SECTOR_SIZE == SECTOR_SIZE - 1 && ySector < ySectors - 1)
                EnqueueInvalidatedSector(xSector, ySector + 1, zSector);

            if (pos.z % SECTOR_SIZE == 0 && zSector > 0)
                EnqueueInvalidatedSector(xSector, ySector, zSector - 1);
            else if (pos.z % SECTOR_SIZE == SECTOR_SIZE - 1 && zSector < zSectors - 1)
                EnqueueInvalidatedSector(xSector, ySector, zSector + 1);
        }

        public void EnqueueInvalidatedSector(int xSector, int ySector, int zSector)
        {
            Sector sector = sectors[xSector | (ySector << xSectorsBits) | (zSector << xySectorsBits)];

            if (sector.insideInvalidateSectorQueue == false)
            {
                pendingSectorsUpdate.Add(sector);
                pendingSectorsUpdateOrderValid = false;

                sector.insideInvalidateSectorQueue = true;

                if (sector.insideInvalidateLightQueue)
                {
                    sector.insideInvalidateLightQueue = false;
                    pendingSectorsUpdateLight.Remove(sector);
                }
            }
        }

        public void EnqueueInvalidatedLight(int xSector, int ySector, int zSector)
        {
            Sector sector = sectors[xSector | (ySector << xSectorsBits) | (zSector << xySectorsBits)];

            if (sector.insideInvalidateLightQueue == false && sector.insideInvalidateSectorQueue == false)
            {
                pendingSectorsUpdateLight.Add(sector);
                pendingSectorsUpdateLightOrderValid = false;

                sector.insideInvalidateLightQueue = false;
            }
        }

        static private TilePosition currentPlayerPositionForSort;

        public void Update(float deltaTime)
        {
            long start = DateTime.Now.Ticks;
            long ticksInMillisecond = 10000;

            currentPlayerPositionForSort = Graphics.Vector3ToTilePosition(world.avatarManager.player.position);

            if (pendingSectorsUpdateOrderValid == false)
            {
                pendingSectorsUpdate.Sort(CompareSectorsByDistanceToPlayer);
                pendingSectorsUpdateOrderValid = true;
            }

            if (pendingSectorsUpdateLightOrderValid == false)
            {
                pendingSectorsUpdateLight.Sort(CompareSectorsByDistanceToPlayer);
                pendingSectorsUpdateLightOrderValid = true;
            }
			
			int updateRounds = 0;
			
			//Always update at leat 8 sectors (they could be the 8 sectors nearest to the player)
			
            while ((pendingSectorsUpdate.Count > 0 || pendingSectorsUpdateLight.Count > 0) && 
				(DateTime.Now.Ticks - start < ticksInMillisecond * 10 || updateRounds < 8))
            {
                if (pendingSectorsUpdate.Count > 0)
                {
                    Sector sectorToUpdate = pendingSectorsUpdate[pendingSectorsUpdate.Count - 1];
                    pendingSectorsUpdate.RemoveAt(pendingSectorsUpdate.Count - 1);
                    sectorToUpdate.UpdateMesh();
                    sectorToUpdate.insideInvalidateSectorQueue = false;
                }

                if (pendingSectorsUpdateLight.Count > 0)
                {
                    Sector sectorToUpdate = pendingSectorsUpdateLight[pendingSectorsUpdateLight.Count - 1];
                    pendingSectorsUpdateLight.RemoveAt(pendingSectorsUpdateLight.Count - 1);
                    sectorToUpdate.UpdateAmbientLight();
                    sectorToUpdate.insideInvalidateLightQueue = false;
                }
				
				updateRounds++;
            }
        }

        static private int CompareSectorsByDistanceToPlayer(Sector sector1, Sector sector2)
        {
            int dist1 = (currentPlayerPositionForSort - sector1.tileOffset).GetDistanceSquared();
            int dist2 = (currentPlayerPositionForSort - sector2.tileOffset).GetDistanceSquared();

            return dist2.CompareTo(dist1);
        }


        public void UpdateAllTilesLight()
        {
            for (int x = 0; x < xSectors; x++)
                for (int z = 0; z < zSectors; z++)
                    for (int y = ySectors - 1; y >= 0; y--)
                        EnqueueInvalidatedLight(x, y, z);
        }

        public void Clear()
        {
            world = null;

            pendingSectorsUpdate = null;
            pendingSectorsUpdateLight = null;

            foreach (Sector s in sectors)
                s.Clear();

            sectors = null;
        }

        public void Save(System.IO.BinaryWriter bw)
        {
            
        }

        public void Load(System.IO.BinaryReader br)
        {
        }
    }
}