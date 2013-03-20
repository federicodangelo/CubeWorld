using CubeWorld.Tiles;
using System.Collections.Generic;
namespace CubeWorld.World.Lights
{
    public class LightModelAmbient
    {
        static private long PositionToInt(long x, long y, long z)
        {
            return x | (y << 16) | (z << 32);
        }

        static private TilePosition IntToPosition(long v)
        {
            return new TilePosition((int)(v & 0xFFFF), (int)((v >> 16) & 0xFFFF), (int)(v >> 32));
        }

        static private HashSet<long> updatedTiles = new HashSet<long>();
        static private HashSet<long> lightsToRecalculate = new HashSet<long>();
        static private List<long> pendingUpdateLights = new List<long>();
        static private List<long> nextPendingUpdateLights = new List<long>();

        static public void InitLuminance(TileManager tileManager)
        {
            for (int x = 0; x < tileManager.sizeX; x++)
            {
                for (int z = 0; z < tileManager.sizeZ; z++)
                {
                    for (int y = tileManager.sizeY - 1; y >= 0; y--)
                    {
                        TilePosition pos = new TilePosition(x, y, z);

                        tileManager.SetTileAmbientLuminance(pos, Tile.MAX_LUMINANCE);

                        if (tileManager.GetTileCastShadow(pos))
                            break;
                    }
                }
            }

            for (int x = 0; x < tileManager.sizeX; x++)
            {
                for (int z = 0; z < tileManager.sizeZ; z++)
                {
                    for (int y = tileManager.sizeY - 1; y >= 0; y--)
                    {
                        TilePosition pos = new TilePosition(x, y, z);

                        if (tileManager.GetTileAmbientLuminance(pos) == 0)
                            break;

                        if (x > 0 && tileManager.GetTileAmbientLuminance(pos + new TilePosition(-1, 0, 0)) == 0 ||
                            x < tileManager.sizeX - 1 && tileManager.GetTileAmbientLuminance(pos + new TilePosition(1, 0, 0)) == 0 ||
                            z > 0 && tileManager.GetTileAmbientLuminance(pos + new TilePosition(0, 0, -1)) == 0 ||
                            z < tileManager.sizeZ - 1 && tileManager.GetTileAmbientLuminance(pos + new TilePosition(0, 0, 1)) == 0)
                        {
                            pendingUpdateLights.Add(PositionToInt(x, y, z));
                        }
                    }
                }
            }

            UpdateLuminanceLightVector(tileManager);
        }

        static public void UpdateLuminanceDark(TileManager tileManager, TilePosition from)
		{
            if (tileManager.GetTileAmbientLuminance(from) == 0)
                return;

            updatedTiles.Clear();

            pendingUpdateLights.Add(PositionToInt(from.x, from.y, from.z));

            for (int y = from.y - 1; y >= 0; y--)
            {
                pendingUpdateLights.Add(PositionToInt(from.x, y, from.z));

                if (tileManager.GetTileCastShadow(new TilePosition(from.x, y, from.z)) == true)
                    break;
            }
			
			while(pendingUpdateLights.Count > 0)
			{
				while(pendingUpdateLights.Count > 0)
				{
                    long pos = pendingUpdateLights[pendingUpdateLights.Count - 1];
                    pendingUpdateLights.RemoveAt(pendingUpdateLights.Count - 1);

                    TilePosition tilePos = IntToPosition(pos);
					Tile tile = tileManager.GetTile(tilePos);
					
					if ((tilePos.x == from.x && tilePos.z == from.z) || (tile.AmbientLuminance != 0 && tile.AmbientLuminance != Tile.MAX_LUMINANCE))
					{
                        byte oldLuminance = tile.AmbientLuminance;

                        tileManager.SetTileAmbientLuminance(tilePos, 0);

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
								int nearLuminance = tileManager.GetTileAmbientLuminance(tilePos);

                                if (nearLuminance <= oldLuminance && nearLuminance != Tile.MAX_LUMINANCE)
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
			
			tileManager.SetTileAmbientLuminance(from, luminance);
			
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
						byte luminance = tile.AmbientLuminance;

                        if (luminance > 0)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        tilePos.x -= 1;
                                        luminance--;
                                        break;
                                    case 1:
                                        tilePos.x += 2;
                                        break;
                                    case 2:
                                        tilePos.x -= 1;
                                        tilePos.z -= 1;
                                        break;
                                    case 3:
                                        tilePos.z += 2;
                                        break;
                                    case 4:
                                        luminance++;
                                        tilePos.z -= 1;
                                        tilePos.y -= 1;
                                        break;
                                    case 5:
                                        tilePos.y += 2;
                                        break;
                                }

                                if (tileManager.IsValidTile(tilePos))
                                {
                                    if (tileManager.GetTileAmbientLuminance(tilePos) < luminance)
                                    {
                                        tileManager.SetTileAmbientLuminance(tilePos, luminance);
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
