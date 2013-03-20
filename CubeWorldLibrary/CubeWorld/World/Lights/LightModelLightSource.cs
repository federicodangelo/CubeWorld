using CubeWorld.Tiles;
using System.Collections.Generic;
namespace CubeWorld.World.Lights
{
    public class LightModelLightSource
    {
        static private long PositionToInt(long x, long y, long z)
		{
			return x | (y << 16) | (z << 32);
		}

        static private TilePosition IntToPosition(long v)
		{
			return new TilePosition((int) (v & 0xFFFF), (int) ((v >> 16) & 0xFFFF), (int) (v >> 32));
		}

        static private HashSet<long> updatedTiles = new HashSet<long>();
        static private HashSet<long> lightsToRecalculate = new HashSet<long>();
        static private List<long> pendingUpdateLights = new List<long>();
        static private List<long> nextPendingUpdateLights = new List<long>();

        static public void InitLuminance(TileManager tileManager)
        {
            for (int x = 0; x < tileManager.sizeX; x++)
            {
                for (int y = 0; y < tileManager.sizeY; y++)
                {
                    for (int z = 0; z < tileManager.sizeZ; z++)
                    {
                        TilePosition pos = new TilePosition(x, y, z);
                        if (tileManager.GetTileLightSource(pos))
                            pendingUpdateLights.Add(PositionToInt(x, y, z));
                    }
                }
            }

            UpdateLuminanceLightVector(tileManager);
        }

        static public void UpdateLuminanceDark(TileManager tileManager, TilePosition from)
		{
            updatedTiles.Clear();

            pendingUpdateLights.Add(PositionToInt(from.x, from.y, from.z));
			
			while(pendingUpdateLights.Count > 0)
			{
				while(pendingUpdateLights.Count > 0)
				{
                    long pos = pendingUpdateLights[pendingUpdateLights.Count - 1];
                    pendingUpdateLights.RemoveAt(pendingUpdateLights.Count - 1);

                    TilePosition tilePos = IntToPosition(pos);
					Tile tile = tileManager.GetTile(tilePos);
					
					if (tile.LightSourceLuminance > 0)
					{
						byte oldLuminance = tile.LightSourceLuminance;
                        tileManager.SetTileLightSourceLuminance(tilePos, 0);
						
						updatedTiles.Add(pos);
						
						for (int i = 0; i < 6; i++)
						{
							switch(i)
							{
								case 0:
									tilePos.x -= 1;
									break;
								case 1:
									tilePos.x += 2;
									break;
								case 2:
									tilePos.x -= 1;
									tilePos.y -= 1;
									break;
								case 3:
									tilePos.y += 2;
									break;
								case 4:
                                    tilePos.y -= 1;
									tilePos.z -= 1;
									break;
								case 5:
									tilePos.z += 2;
									break;
							}

                            long nearPos = PositionToInt(tilePos.x, tilePos.y, tilePos.z);
							
							if (tileManager.IsValidTile(tilePos) && 
								updatedTiles.Contains(nearPos) == false)
							{
								int nearLuminance = tileManager.GetTileLightSourceLuminance(tilePos);
								
								if (nearLuminance < oldLuminance)
									nextPendingUpdateLights.Add(nearPos);
								else if (lightsToRecalculate.Contains(nearPos) == false)
									lightsToRecalculate.Add(nearPos);
							}							
						}
					}
				}

                List<long> old = pendingUpdateLights;
				pendingUpdateLights = nextPendingUpdateLights;
				nextPendingUpdateLights = old;
			}

            pendingUpdateLights.AddRange(lightsToRecalculate);
            lightsToRecalculate.Clear();

            UpdateLuminanceLightVector(tileManager);
		}		
		
		static public void UpdateLuminanceLight(TileManager tileManager, TilePosition from, byte luminance)
		{
			pendingUpdateLights.Add(PositionToInt(from.x, from.y, from.z));
			
			tileManager.SetTileLightSourceLuminance(from, luminance);
			
			UpdateLuminanceLightVector(tileManager);
		}

        static private void UpdateLuminanceLightVector(TileManager tileManager)
		{
			while(pendingUpdateLights.Count > 0)
			{
				while(pendingUpdateLights.Count > 0)
				{
					long pos = pendingUpdateLights[pendingUpdateLights.Count - 1];
                    pendingUpdateLights.RemoveAt(pendingUpdateLights.Count - 1);

                    TilePosition tilePos = IntToPosition(pos);
					Tile tile = tileManager.GetTile(tilePos);
					
					if (tile.CastShadow == false)
					{
						byte luminance = tile.LightSourceLuminance;

                        if (luminance > 0)
                        {
                            luminance --;

                            for (int i = 0; i < 6; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        tilePos.x -= 1;
                                        break;
                                    case 1:
                                        tilePos.x += 2;
                                        break;
                                    case 2:
                                        tilePos.x -= 1;
                                        tilePos.y -= 1;
                                        break;
                                    case 3:
                                        tilePos.y += 2;
                                        break;
                                    case 4:
                                        tilePos.y -= 1;
                                        tilePos.z -= 1;
                                        break;
                                    case 5:
                                        tilePos.z += 2;
                                        break;
                                }

                                if (tileManager.IsValidTile(tilePos))
                                {
                                    if (tileManager.GetTileLightSourceLuminance(tilePos) < luminance)
                                    {
                                        tileManager.SetTileLightSourceLuminance(tilePos, luminance);
                                        nextPendingUpdateLights.Add(PositionToInt(tilePos.x, tilePos.y, tilePos.z));
                                    }
                                }
                            }
						}
					}
				}

                List<long> old = pendingUpdateLights;
                pendingUpdateLights = nextPendingUpdateLights;
                nextPendingUpdateLights = old;
            }
		}
    }
}
