using CubeWorld.Tiles;
using CubeWorld.Utils;
using System;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleLiquid : TileRule
    {
        public int maxLevel;
        public int spreadSpeed;

        public TileRuleLiquid()
        {
        }

        public TileRuleLiquid(int maxLevel, int spreadSpeed, TileRuleCondition condition)
            : base(condition)
        {
            this.maxLevel = maxLevel;
            this.spreadSpeed = spreadSpeed;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            int level = tile.ExtraData;

            if (tileManager.Ticks % spreadSpeed == 0)
            {
                //Update this tile level
                if (level != 0)
                    level = UpdateTileLevel(tileManager, tile, pos, level);

                //Propagate
                if (level <= maxLevel)
                    Propagate(tileManager, tile, pos, level);
                else if (level > maxLevel)
                    tileManager.SetTileType(pos, TileDefinition.EMPTY_TILE_TYPE);
            }
            else
            {
                tileManager.EnqueueInvalidatedTile(pos);
            }
        }

        private void Propagate(TileManager tileManager, Tile tile, TilePosition pos, int level)
        {
            TilePosition posBelow = pos + new TilePosition(0, -1, 0);
            bool belowIsSameLiquid = tileManager.IsValidTile(posBelow) && tileManager.GetTileType(posBelow) == tile.tileType;

            if (tileManager.IsValidTile(posBelow) && (tileManager.GetTileType(posBelow) == TileDefinition.EMPTY_TILE_TYPE) || belowIsSameLiquid)
            {
                if (belowIsSameLiquid == false)
                {
                    tileManager.SetTileType(posBelow, tile.tileType);
                    tileManager.SetTileExtraData(posBelow, 1);
                }
            }
            else
            {
                if (level < maxLevel)
                {
                    TilePosition[] nearDirection = new TilePosition[4];

                    int[] nearDistance = new int[] { maxLevel, maxLevel, maxLevel, maxLevel };

                    FindFall(new TilePosition(1, 0, 0), tileManager, tile, pos, ref nearDirection[0], ref nearDistance[0]);
                    FindFall(new TilePosition(-1, 0, 0), tileManager, tile, pos, ref nearDirection[1], ref nearDistance[1]);
                    FindFall(new TilePosition(0, 0, 1), tileManager, tile, pos, ref nearDirection[2], ref nearDistance[2]);
                    FindFall(new TilePosition(0, 0, -1), tileManager, tile, pos, ref nearDirection[3], ref nearDistance[3]);

                    int minNearDistance = Math.Min(Math.Min(Math.Min(nearDistance[0], nearDistance[1]), nearDistance[2]), nearDistance[3]);

                    if (minNearDistance != maxLevel)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (nearDistance[i] == minNearDistance)
                            {
                                TilePosition posNear = pos + nearDirection[i];

                                if (tileManager.IsValidTile(posNear) && tileManager.GetTileType(posNear) == TileDefinition.EMPTY_TILE_TYPE)
                                {
                                    TilePosition posNearBelow = posNear + new TilePosition(0, -1, 0);
                                    bool nearBelowIsSameLiquid = tileManager.IsValidTile(posNearBelow) && tileManager.GetTileType(posNearBelow) == tile.tileType;

                                    if (nearBelowIsSameLiquid == false || belowIsSameLiquid == false)
                                    {
                                        tileManager.SetTileType(posNear, tile.tileType);
                                        tileManager.SetTileExtraData(posNear, (byte)(level + 1));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (TilePosition deltaPos in Manhattan.GetTilesAtDistance(1))
                        {
                            if (deltaPos.y == 0)
                            {
                                TilePosition posNear = pos + deltaPos;
                                if (tileManager.IsValidTile(posNear) && tileManager.GetTileType(posNear) == TileDefinition.EMPTY_TILE_TYPE)
                                {
                                    TilePosition posNearBelow = posNear + new TilePosition(0, -1, 0);
                                    bool nearBelowIsSameLiquid = tileManager.IsValidTile(posNearBelow) && tileManager.GetTileType(posNearBelow) == tile.tileType;

                                    if (nearBelowIsSameLiquid == false || belowIsSameLiquid == false)
                                    {
                                        tileManager.SetTileType(posNear, tile.tileType);
                                        tileManager.SetTileExtraData(posNear, (byte)(level + 1));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private int UpdateTileLevel(TileManager tileManager, Tile tile, TilePosition pos, int level)
        {
            TilePosition posAbove = pos + new TilePosition(0, 1, 0);

            bool aboveIsSameLiquid = tileManager.IsValidTile(posAbove) && tileManager.GetTileType(posAbove) == tile.tileType;

            int nearLowestLevel = int.MaxValue;

            if (aboveIsSameLiquid == false)
            {
                nearLowestLevel = GetLowerLevel(new TilePosition(1, 0, 0), tileManager, tile, pos, nearLowestLevel);
                nearLowestLevel = GetLowerLevel(new TilePosition(-1, 0, 0), tileManager, tile, pos, nearLowestLevel);
                nearLowestLevel = GetLowerLevel(new TilePosition(0, 0, 1), tileManager, tile, pos, nearLowestLevel);
                nearLowestLevel = GetLowerLevel(new TilePosition(0, 0, -1), tileManager, tile, pos, nearLowestLevel);
            }
            else
            {
                nearLowestLevel = 0;
            }

            if (nearLowestLevel != int.MaxValue)
            {
                if (nearLowestLevel + 1 != level)
                {
                    if (nearLowestLevel + 1 > level)
                    {
                        level++;
                        tileManager.SetTileExtraData(pos, (byte)level);
                    }
                    else
                    {
                        if (level > 1)
                        {
                            level--;
                            tileManager.SetTileExtraData(pos, (byte)level);
                        }
                    }
                }
            }
            else
            {
                level = maxLevel + 1;
            }

            return level;
        }

        static private int GetLowerLevel(TilePosition dir, TileManager tileManager, Tile tile, TilePosition pos, int minLevel)
        {
            TilePosition posNear = pos + dir;
            if (tileManager.IsValidTile(posNear) && tileManager.GetTileType(posNear) == tile.tileType)
            {
                int levelNear = tileManager.GetTileExtraData(posNear);
                if (levelNear < minLevel)
                    minLevel = levelNear;
            }

            return minLevel;
        }

        static private void FindFall(TilePosition dir, TileManager tileManager, Tile tile, TilePosition pos, ref TilePosition nearDirection, ref int nearDistance)
        {
            TilePosition posNear = pos;

            for (int d = 1; d < nearDistance; d++)
            {
                posNear += dir;

                if (tileManager.IsValidTile(posNear) && 
                    (tileManager.GetTileType(posNear) == TileDefinition.EMPTY_TILE_TYPE ||
                     tileManager.GetTileType(posNear) == tile.tileType && tileManager.GetTileExtraData(posNear) != 0))
                {
                    TilePosition posNearBelow = posNear + new TilePosition(0, -1, 0);
                    if (tileManager.IsValidTile(posNearBelow) && 
                        (tileManager.GetTileType(posNearBelow) == TileDefinition.EMPTY_TILE_TYPE ||
                         tileManager.GetTileType(posNearBelow) == tile.tileType))
                    {
                        nearDirection = dir;
                        nearDistance = d;
                        break;
                    }
                }
                else
                    break;
            }
        }
        
        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref maxLevel, "maxLevel");
            serializer.Serialize(ref spreadSpeed, "spreadSpeed");
        }
    }
}