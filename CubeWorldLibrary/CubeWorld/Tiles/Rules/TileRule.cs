using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRule : ISerializable
    {
        public TileRuleCondition condition;

        public TileRule()
        {
        }

        public TileRule(TileRuleCondition condition)
        {
            this.condition = condition;
        }

        public bool CheckConditions(TileManager tileManager, Tile tile, TilePosition pos)
        {
            if (condition != null)
                return condition.Validate(tileManager, tile, pos);

            return true;
        }

        public virtual void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {

        }

        public virtual void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref condition, "condition");
        }
    }
}