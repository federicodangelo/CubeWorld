using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleDestroy : TileRule
    {
        public TilePosition delta;

        public TileRuleDestroy()
        {
        }

        public TileRuleDestroy(TilePosition delta, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
                tileManager.DestroyTile(pos);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
        }
    }
}