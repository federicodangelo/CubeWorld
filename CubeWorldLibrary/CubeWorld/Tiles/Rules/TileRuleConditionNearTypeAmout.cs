using CubeWorld.Tiles;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleConditionNearTypeAmout : TileRuleCondition
    {
        public int minValue;
        public byte tileType;

        public TileRuleConditionNearTypeAmout()
        {
        }

        public TileRuleConditionNearTypeAmout(int minValue, byte tileType)
        {
            this.minValue = minValue;
            this.tileType = tileType;
        }

        public override bool Validate(TileManager tileManager, Tile tile, TilePosition pos)
        {
            tileManager.world.stats.checkedConditions++;
            int amount = 0;

            foreach (TilePosition delta in Manhattan.GetTilesAtDistance(1))
                if (tileManager.IsValidTile(pos + delta) && tileManager.GetTileType(pos + delta) == tileType)
                    amount++;

            return amount >= minValue;
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref minValue, "minValue");
            serializer.Serialize(ref tileType, "tileType");
        }
    }
}