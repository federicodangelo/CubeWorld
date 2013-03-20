using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleConditionIsType : TileRuleCondition
    {
        public TilePosition delta;
        public byte tileType;

        public TileRuleConditionIsType()
        {
        }

        public TileRuleConditionIsType(TilePosition delta, byte tileType)
        {
            this.delta = delta;
            this.tileType = tileType;
        }

        public override bool Validate(TileManager tileManager, Tile tile, TilePosition pos)
        {
            tileManager.world.stats.checkedConditions++;

            pos += delta;

            return tileManager.IsValidTile(pos) && 
				tileManager.GetTileType(pos) == tileType;
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref tileType, "tileType");
        }
    }
}