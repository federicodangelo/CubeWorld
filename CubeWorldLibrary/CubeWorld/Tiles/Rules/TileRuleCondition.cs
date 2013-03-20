using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleCondition : ISerializable
    {
        public virtual bool Validate(TileManager tileManager, Tile tile, TilePosition pos)
        {
            return true;
        }

        public virtual void Serialize(Serializer serializer)
        {
        }
    }
}