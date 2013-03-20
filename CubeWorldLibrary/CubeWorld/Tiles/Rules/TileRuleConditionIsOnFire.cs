using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleConditionIsOnFire : TileRuleCondition
    {
        public TilePosition delta;

        public TileRuleConditionIsOnFire()
        {

        }

        public TileRuleConditionIsOnFire(TilePosition delta)
        {
            this.delta = delta;
        }

        public override bool Validate(TileManager tileManager, Tile tile, TilePosition pos)
        {
            tileManager.world.stats.checkedConditions++;

            pos += delta;

            return tileManager.IsValidTile(pos) && 
					tileManager.GetTileOnFire(pos);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
        }
    }
}