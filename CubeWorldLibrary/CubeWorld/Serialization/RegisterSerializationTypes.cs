using System;
using CubeWorld.Configuration;
using CubeWorld.Items;
using CubeWorld.Tiles;
using CubeWorld.Tiles.Rules;
using CubeWorld.World.Objects;
using CubeWorld.Avatars;

namespace CubeWorld.Serialization
{
	public class RegisterSerializationTypes
	{
		static public void Register()
		{
			Serializer.AddType(typeof(ConfigExtraMaterials));
			Serializer.AddType(typeof(AvatarDefinition));
			Serializer.AddType(typeof(AvatarPartDefinition));
			Serializer.AddType(typeof(ItemDefinition));
			Serializer.AddType(typeof(ItemTileDefinition));
			Serializer.AddType(typeof(TileActionRules));
			Serializer.AddType(typeof(TileDefinition));
			Serializer.AddType(typeof(TileUpdateRules));
			Serializer.AddType(typeof(TileActionRule));
			Serializer.AddType(typeof(TileRule));
			Serializer.AddType(typeof(TileRuleApplyEffect));
			Serializer.AddType(typeof(TileRuleCondition));
			Serializer.AddType(typeof(TileRuleConditionAnd));
			Serializer.AddType(typeof(TileRuleConditionIsBurnable));
			Serializer.AddType(typeof(TileRuleConditionIsOnFire));
			Serializer.AddType(typeof(TileRuleConditionIsType));
			Serializer.AddType(typeof(TileRuleConditionNearTypeAmout));
			Serializer.AddType(typeof(TileRuleConditionNot));
			Serializer.AddType(typeof(TileRuleConditionOr));
			Serializer.AddType(typeof(TileRuleCreateItem));
			Serializer.AddType(typeof(TileRuleDamage));
			Serializer.AddType(typeof(TileRuleDestroy));
			Serializer.AddType(typeof(TileRuleDropSameTileItem));
			Serializer.AddType(typeof(TileRuleExplode));
			Serializer.AddType(typeof(TileRuleInvalidate));
			Serializer.AddType(typeof(TileRuleLiquid));
			Serializer.AddType(typeof(TileRuleMultiple));
			Serializer.AddType(typeof(TileRuleMultipleOnlyOne));
			Serializer.AddType(typeof(TileRulePlayEffect));
			Serializer.AddType(typeof(TileRulePlaySound));
			Serializer.AddType(typeof(TileRuleSetDynamic));
			Serializer.AddType(typeof(TileRuleSetOnFire));
			Serializer.AddType(typeof(TileRuleSetTileType));
			Serializer.AddType(typeof(CubeWorld.World.CubeWorld.MultiplayerConfig));
			Serializer.AddType(typeof(CWDefinition));
			Serializer.AddType(typeof(CWVisualDefinition));
		}
	}
}

