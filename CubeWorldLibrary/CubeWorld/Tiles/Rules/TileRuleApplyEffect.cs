using CubeWorld.Tiles;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleApplyEffect : TileRule
    {
        public TilePosition delta;
        public string effectId;

        public TileRuleApplyEffect()
        {
        }

        public TileRuleApplyEffect(TilePosition delta, string effectId, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.effectId = effectId;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
            {
                if (tileManager.GetTileDynamic(pos))
                {
                    DynamicTile dynamicTile = tileManager.GetDynamicTile(pos);
                    tileManager.world.fxListener.PlayEffect(effectId, dynamicTile);
                }
            }
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref effectId, "effectId");
        }
    }
}