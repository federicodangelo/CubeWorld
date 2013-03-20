using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleSetDynamic : TileRule
    {
        public TilePosition delta;
        public bool value;
        public bool gravity;
        public int timeout;

        public TileRuleSetDynamic()
        {
        }

        public TileRuleSetDynamic(TilePosition delta, bool value, bool gravity, int timeout, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.value = value;
            this.gravity = gravity;
            this.timeout = timeout;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            if (tileManager.IsValidTile(pos))
            {
                if (value == true)
                {
                    tileManager.SetTileDynamic(pos, value, gravity, timeout);
                }
                else if (tileManager.GetTileDynamic(pos))
                {
                    //Call DynamicTile.MakeStatic because if the tile is inside it's update cycle
                    //when this rule is executed, then the array of components would become invalid
                    DynamicTile dynamicTile = tileManager.GetDynamicTile(pos);

                    dynamicTile.MakeStatic();
                }
            }
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref value, "value");
            serializer.Serialize(ref gravity, "gravity");
            serializer.Serialize(ref timeout, "timeout");
        }
    }
}