using CubeWorld.Tiles;
using System.Collections.Generic;
using System;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleMultipleOnlyOne : TileRule
    {
        public TileRule[] otherRules;
        public bool useRandom;

        private Random random = new Random();

        public TileRuleMultipleOnlyOne()
        {
        }

        public TileRuleMultipleOnlyOne(bool useRandom, TileRule[] otherRules, TileRuleCondition condition)
            : base(condition)
        {
            this.otherRules = otherRules;
            this.useRandom = useRandom;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            int offset = 0;
            
            if (useRandom)
                offset = random.Next(otherRules.Length);

            for (int i = otherRules.Length - 1; i >= 0; i--)
            {
                if (otherRules[(i + offset) % otherRules.Length].CheckConditions(tileManager, tile, pos) == true)
                {
                    otherRules[(i + offset) % otherRules.Length].Execute(tileManager, tile, pos);
                    break;
                }
            }
        }
        
        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref otherRules, "otherRules");
            serializer.Serialize(ref useRandom, "useRandom");
        }

    }
}