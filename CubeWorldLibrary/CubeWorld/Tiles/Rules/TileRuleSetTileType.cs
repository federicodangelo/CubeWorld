using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleSetTileType : TileRule
    {
        public TilePosition delta;
        public byte tileType;

        public TileRuleSetTileType()
        {
        }

        public TileRuleSetTileType(TilePosition delta, byte tileType, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.tileType = tileType;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
                tileManager.SetTileType(pos, tileType);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref tileType, "tileType");
        }
    }
}