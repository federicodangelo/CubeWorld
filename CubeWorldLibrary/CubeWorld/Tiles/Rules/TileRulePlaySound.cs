using CubeWorld.Tiles;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRulePlaySound : TileRule
    {
        public TilePosition delta;
        public string soundId;

        public TileRulePlaySound()
        {
        }

        public TileRulePlaySound(TilePosition delta, string soundId, TileRuleCondition condition)
            : base(condition)
        {
            this.delta = delta;
            this.soundId = soundId;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            pos += delta;

            tileManager.world.fxListener.PlaySound(soundId, Graphics.TilePositionToVector3(pos));
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref delta, "delta");
            serializer.Serialize(ref soundId, "soundId");
        }
    }
}