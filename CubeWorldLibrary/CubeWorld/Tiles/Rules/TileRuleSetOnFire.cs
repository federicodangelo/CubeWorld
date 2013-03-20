using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleSetOnFire : TileRule
    {
        public TilePosition delta;
        public bool value;

        public TileRuleSetOnFire()
        {
        }

        public TileRuleSetOnFire(TilePosition delta, bool value, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.value = value;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
                tileManager.SetTileOnFire(pos, value);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref value, "value");
        }
    }
}