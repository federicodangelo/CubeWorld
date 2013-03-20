using System;
using System.Collections.Generic;
using CubeWorld.Sectors;
using CubeWorld.Tiles;
using CubeWorld.Avatars;
using CubeWorld.Utils;

namespace Unity.CubeWorld.VisibleSectorsStrategies
{
    public class VisibleSectorsStrategyRadius : VisibleSectorsStrategy
    {
        private SectorManagerUnity sectorManagerUnity;
        private SectorManager sectorManager;
        private Player player;
        private PlayerUnity playerUnity;

        private Sector currentPlayerSector;

        private List<Sector> visibleSectors = new List<Sector>();

        public VisibleSectorsStrategyRadius(SectorManagerUnity sectorManagerUnity, SectorManager sectorManager, Player player, PlayerUnity playerUnity)
        {
            this.sectorManager = sectorManager;
            this.player = player;
            this.playerUnity = playerUnity;
            this.sectorManagerUnity = sectorManagerUnity;
        }

        private List<Sector> FindSectorsNearPlayer()
        {
            TilePosition playerTilePosition = Graphics.Vector3ToTilePosition(player.position);

            TilePosition playerSectorTilePosition = sectorManager.GetSectorTile(playerTilePosition).sectorPosition;

            List<Sector> sectorsNear = new List<Sector>();

            int visibileSectorsRadius = (int)Math.Ceiling(playerUnity.mainCamera.farClipPlane / SectorManager.SECTOR_SIZE) + 1;

            for (int d = 0; d <= visibileSectorsRadius; d++)
            {
                foreach (TilePosition tilePosition in Manhattan.GetTilesAtDistance(d))
                {
                    TilePosition t = tilePosition + playerSectorTilePosition;

                    if (t.x >= 0 && t.x < sectorManager.xSectors &&
                        t.y >= 0 && t.y < sectorManager.ySectors &&
                        t.z >= 0 && t.z < sectorManager.zSectors)
                    {
                        sectorsNear.Add(sectorManager.GetSector(t));
                    }
                }
            }

            return sectorsNear;
        }

        public override void Update()
        {
            TilePosition playerTilePosition = Graphics.Vector3ToTilePosition(player.position);

            Sector playerSector = sectorManager.GetSectorTile(playerTilePosition);

            if (playerSector != currentPlayerSector)
            {
                currentPlayerSector = playerSector;

                List<Sector> newVisibleVectors = FindSectorsNearPlayer();

                //Disable sectors far away
                for (int i = visibleSectors.Count - 1; i >= 0; i--)
                {
                    Sector sector = visibleSectors[i];

                    int pos = newVisibleVectors.IndexOf(sector);

                    if (pos < 0)
                    {
                        sectorManagerUnity.ReturnSectorUnityToCache((SectorUnity) sector.GetSectorGraphics());
                        visibleSectors.RemoveAt(i);
                    }
                    else
                    {
                        newVisibleVectors.RemoveAt(pos);
                    }
                }

                //Enable sectors near
                for (int i = newVisibleVectors.Count - 1; i >= 0; i--)
                {
                    Sector sector = newVisibleVectors[i];

                    visibleSectors.Add(sector);

                    sectorManagerUnity.GetSectorUnityFromCache(sector);
                }
            }
        }

        public override void Clear()
        {
            visibleSectors.Clear();

            currentPlayerSector = null;
        }
    }
}
