using CubeWorld.Tiles.Rules;
using System.Collections.Generic;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles
{
    public class TileUpdateRules : ISerializable
    {
        public TileRule[] rules;

        public void AddRule(TileRule rule)
        {
            List<TileRule> r = new List<TileRule>();
            if (this.rules != null)
                r.AddRange(this.rules);
            r.Add(rule);
            this.rules = r.ToArray();
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref rules, "rules");
        }
    }
}