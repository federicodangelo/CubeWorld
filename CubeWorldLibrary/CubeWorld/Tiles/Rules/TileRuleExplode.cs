using CubeWorld.Tiles;
using System;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleExplode : TileRule
    {
        public int radius;
        public int damage;
        public bool setOnFire;

        public TileRuleExplode()
        {
        }

        public TileRuleExplode(int radius, int damage, bool setOnFire, TileRuleCondition condition)
            : base(condition)
        {
            this.radius = radius;
            this.damage = damage;
            this.setOnFire = setOnFire;

            if (radius <= 0)
                throw new Exception("Invalid radius: " + radius);
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition center)
        {
            for (int d = 1; d <= radius; d++)
            {
	            int rDamage = damage * (radius - d + 1) / radius;
				
				TilePosition[] tiles = Manhattan.GetTilesAtDistance(d);
				
				for(int i = 0; i < tiles.Length; i++)
				{
					TilePosition p = center + tiles[i];
					
	                if (tileManager.IsValidTile(p))
						if (tileManager.DamageTile(p, rDamage) == false)
							if (setOnFire && tileManager.GetTileDefinition(tileManager.GetTileType(p)).burns)
		                        tileManager.SetTileOnFire(p, true);
				}
            }
			
			if (setOnFire)
			{
				TilePosition[] tiles = Manhattan.GetTilesAtDistance(radius + 1);
				
				for(int i = 0; i < tiles.Length; i++)
				{
					TilePosition p = center + tiles[i];
					
	                if (tileManager.IsValidTile(p))
						if (setOnFire && tileManager.GetTileDefinition(tileManager.GetTileType(p)).burns)
	                        tileManager.SetTileOnFire(p, true);
				}
			}
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref radius, "radius");
            serializer.Serialize(ref damage, "damage");
            serializer.Serialize(ref setOnFire, "setOnFire");
        }
    }
}