using System;
using CubeWorld.Tiles;
using System.Collections.Generic;

namespace CubeWorld.Utils
{
	public class Manhattan
	{
		private const int MAX_INIT = 32;
		static private TilePosition[][] tilesAtDistance;
		
		static public int Distance(TilePosition t1, TilePosition t2)
		{
			TilePosition diff = t1 - t2;
			return Math.Abs(diff.x) + Math.Abs(diff.y) + Math.Abs(diff.z);
		}
		
		static private void InitValues()
		{
			TilePosition center = new TilePosition();
			Dictionary<int, List<TilePosition>> values = new Dictionary<int, List<TilePosition>>();
			
			for (int x = -MAX_INIT; x <= MAX_INIT; x++)
				for (int y = -MAX_INIT; y <= MAX_INIT; y++)
					for (int z = -MAX_INIT; z <= MAX_INIT; z++)
					{
						TilePosition p = new TilePosition(x, y, z);
					
						int d = Distance(center, p);
					
						List<TilePosition> l;
						if (values.ContainsKey(d))
						{
							l = values[d];
						}
						else
						{
							l = new List<TilePosition>();
							values[d] = l;
						}
					
						l.Add(p);
					}
			
			tilesAtDistance = new TilePosition[values.Count][];
			for (int i = 0; i < values.Count; i++)
				tilesAtDistance[i] = values[i].ToArray();
		}	
		
		
		static public TilePosition[] GetTilesAtDistance(int n)
		{
			if (tilesAtDistance == null)
				InitValues();
			
			return tilesAtDistance[n];
		}
	}
}

