using System;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.Avatars;
using CubeWorld.Utils;
using CubeWorld.Sectors;

namespace Unity.CubeWorld.VisibleSectorsStrategies
{
    public class VisibleSectorsStrategyAll : VisibleSectorsStrategy
    {
        public VisibleSectorsStrategyAll(SectorManagerUnity sectorManagerUnity, SectorManager sectorManager)
        {
            foreach (Sector sector in sectorManager.sectors)
                sectorManagerUnity.GetSectorUnityFromCache(sector);
        }

        public override void Update()
        {
        }

        public override void Clear()
        {
        }
    }
}
