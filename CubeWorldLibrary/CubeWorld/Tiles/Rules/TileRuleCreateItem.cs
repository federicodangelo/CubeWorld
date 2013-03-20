using CubeWorld.Tiles;
using CubeWorld.Items;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleCreateItem : TileRule
    {
        public TilePosition delta;
        public ItemDefinition itemDefinition;

        public TileRuleCreateItem()
        {
        }

        public TileRuleCreateItem(TilePosition delta, ItemDefinition itemDefinition, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.itemDefinition = itemDefinition;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
                tileManager.world.gameplay.CreateItem(itemDefinition, Graphics.TilePositionToVector3(pos));
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref itemDefinition, "itemDefinition");
        }
    }
}