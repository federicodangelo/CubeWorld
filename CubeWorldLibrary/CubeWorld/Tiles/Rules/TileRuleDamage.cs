using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleDamage : TileRule
    {
        public TilePosition delta;
        public int damage;

        public TileRuleDamage()
        {
        }

        public TileRuleDamage(TilePosition delta, int damage, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.damage = damage;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
                tileManager.DamageTile(pos, damage);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref damage, "damage");
        }
    }
}