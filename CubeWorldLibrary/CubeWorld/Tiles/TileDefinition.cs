using CubeWorld.Tiles.Rules;
using CubeWorld.World;
using CubeWorld.World.Objects;
using CubeWorld.Serialization;
namespace CubeWorld.Tiles
{
    public class TileDefinition : CWDefinition
    {
		public TileDefinition()
			: base(DefinitionType.Tile)
		{
			
		}
		
        public const byte EMPTY_TILE_TYPE = 0;

        public enum DrawMode
        {
            NONE,
            SOLID,
            SOLID_ALPHA,
            TRANSPARENT,
            LIQUID
        }

        public byte tileType;

        //back, front, bottom, top, right, left
        public int[] materials;
        public bool animated;
        public DrawMode drawMode;
        public bool castShadow;
        public byte lightSourceIntensity;

        public bool solid;
		public bool liquid;
        public bool burns;

        public TileUpdateRules tileUpdateRules;
        public TileActionRules tileActionRules;

        public TileActionRule GetRulesForAction(TileActionRule.ActionType action)
        {
            if (tileActionRules != null)
                return tileActionRules.GetRulesForAction(action);

            return null;
        }

        public override void Serialize(Serializer serializer)
        {
			base.Serialize(serializer);
			
            serializer.Serialize(ref tileType, "tileType");
            serializer.Serialize(ref materials, "materials");
            serializer.Serialize(ref animated, "animated");
            serializer.SerializeEnum(ref drawMode, "drawMode");
            serializer.Serialize(ref castShadow, "castShadow");
            serializer.Serialize(ref lightSourceIntensity, "lightSourceIntensity");
            serializer.Serialize(ref solid, "solid");
            serializer.Serialize(ref liquid, "liquid");
            serializer.Serialize(ref burns, "burns");
            serializer.Serialize(ref tileUpdateRules, "tileUpdateRules");
            serializer.Serialize(ref tileActionRules, "tileActionRules");
        }
    }
}