using System;
using System.Collections.Generic;
using UnityEngine;
using CubeWorld.Tiles;
using Unity.CubeWorld.VisibleSectorsStrategies;
using CubeWorld.Sectors;

public class SectorManagerUnity
{
    public enum VisibleStrategy
    {
        All,
        Radius
    }

	public GameManagerUnity gameManagerUnity;
	
	private List<SectorUnity> cacheSectors = new List<SectorUnity>();
	private Dictionary<TilePosition, SectorUnity> lastSectorAssignmentCache = new Dictionary<TilePosition, SectorUnity>();
	
    private List<SectorUnity> unitySectors = new List<SectorUnity>();

    private GameObject goContainer;

    private VisibleSectorsStrategy visibleStrategy;
	
	public SectorManagerUnity (GameManagerUnity gameManagerUnity)
	{
		this.gameManagerUnity = gameManagerUnity;
	}
	
	public void Clear()
	{
        foreach (SectorUnity unitySector in unitySectors)
            GameObject.DestroyImmediate(unitySector.gameObject);
		
        unitySectors.Clear();
		cacheSectors.Clear();
		lastSectorAssignmentCache.Clear();

        if (goContainer)
        {
            GameObject.DestroyImmediate(goContainer);
            goContainer = null;
        }

        if (visibleStrategy != null)
        {
            visibleStrategy.Clear();
            visibleStrategy = null;
        }
	}
	
	public void ReturnSectorUnityToCache(SectorUnity sectorUnity)
	{
		lastSectorAssignmentCache[sectorUnity.GetSector().sectorPosition] = sectorUnity;
		cacheSectors.Add(sectorUnity);
		sectorUnity.GetSector().SetSectorGraphics(null);
	}

    public SectorUnity GetSectorUnityFromCache(Sector sector)
    {
        if (goContainer == null)
        {
            goContainer = new GameObject();
            goContainer.name = "Sectors";
            goContainer.transform.position = new Vector3(0, 0, 0);
            goContainer.transform.rotation = Quaternion.identity;
            goContainer.transform.localScale = new Vector3(1, 1, 1);
        }

		SectorUnity sectorUnity = null;
		
		if (lastSectorAssignmentCache.ContainsKey(sector.sectorPosition))
		{
			sectorUnity = lastSectorAssignmentCache[sector.sectorPosition];
			if (sectorUnity.IsInUse())
				sectorUnity = null;
			else
				cacheSectors.Remove(sectorUnity);
		}
		
		if (sectorUnity == null)
		{
			if (cacheSectors.Count > 0)
			{
				sectorUnity = cacheSectors[cacheSectors.Count - 1];
				cacheSectors.RemoveAt(cacheSectors.Count - 1);
			}
			else
			{
	            GameObject g = new GameObject();
        		sectorUnity = (SectorUnity)g.AddComponent(typeof(SectorUnity));
	            sectorUnity.gameManagerUnity = gameManagerUnity;
                sectorUnity.transform.parent = goContainer.transform;
	
	            unitySectors.Add(sectorUnity);
			}
		}
		
        sectorUnity.transform.position = GraphicsUnity.TilePositionToVector3(sector.tileOffset);
		sector.SetSectorGraphics(sectorUnity);

        sectorUnity.name = sector.sectorPosition.y + "," + sector.sectorPosition.x + "," + sector.sectorPosition.z;
		
		return sectorUnity;
    }

    private VisibleStrategy activeVisibleStrategy;

    public void UpdateVisibleSectors()
    {
        if (activeVisibleStrategy != CubeWorldPlayerPreferences.visibleStrategy || visibleStrategy == null)
        {
            Clear();

            activeVisibleStrategy = CubeWorldPlayerPreferences.visibleStrategy;

            switch (activeVisibleStrategy)
            {
                case VisibleStrategy.Radius:
                    this.visibleStrategy = new VisibleSectorsStrategyRadius(this, gameManagerUnity.world.sectorManager, gameManagerUnity.world.avatarManager.player, gameManagerUnity.playerUnity);
                    break;

                case VisibleStrategy.All:
                    this.visibleStrategy = new VisibleSectorsStrategyAll(this, gameManagerUnity.world.sectorManager);
                    break;
            }
        }

        visibleStrategy.Update();
    }
}


