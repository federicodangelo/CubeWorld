using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleInvalidate : TileRule
    {
        public TilePosition delta;

        public TileRuleInvalidate()
        {
        }

        public TileRuleInvalidate(TilePosition delta, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
                tileManager.EnqueueInvalidatedTile(pos);
        }
        
        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
        }
    }
}