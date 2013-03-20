using CubeWorld;
using CubeWorld.Tiles;
using CubeWorld.Tiles.Rules;
using CubeWorld.World.Generator;
using System.Xml;
using System.Collections.Generic;
using CubeWorld.World.Lights;
using CubeWorld.Configuration;
using CubeWorld.Items;
using CubeWorld.Avatars;
using CubeWorld.World.Objects;
using CubeWorld.Utils;

namespace CubeWorld.Configuration
{
    public class ConfigParserXML
    {
        private AvailableConfigurations availableConfigs;

        public AvailableConfigurations Parse(
            string miscConfigTxt,
            string tilesConfigTxt,
            string avatarsConfigTxt,
            string itemsConfigTxt,
            string generatorsConfigTxt)
        {
            availableConfigs = new AvailableConfigurations();

            XmlElement miscConfig = GetConfigXmlElement(miscConfigTxt);
            XmlElement tilesConfig = GetConfigXmlElement(tilesConfigTxt);
            XmlElement avatarsConfig = GetConfigXmlElement(avatarsConfigTxt);
            XmlElement itemsConfig = GetConfigXmlElement(itemsConfigTxt);
            XmlElement generatorsConfig = GetConfigXmlElement(generatorsConfigTxt);

            List<ConfigWorldSize> sizes = new List<ConfigWorldSize>();
            foreach (XmlElement sizeXML in GetChildElements(miscConfig["Sizes"]))
                sizes.Add(ParseSize(GetAttributeStringValue(sizeXML, "name"), sizeXML));

            List<ConfigDayInfo> days = new List<ConfigDayInfo>();
            foreach (XmlElement dayInfoXML in GetChildElements(miscConfig["DayInfos"]))
                days.Add(ParseDayInfo(GetAttributeStringValue(dayInfoXML, "name"), dayInfoXML));

            availableConfigs.extraMaterials = ParseExtraMaterials(tilesConfig["ExtraMaterials"]);

            availableConfigs.tileDefinitions = ParseTileDefinitions(tilesConfig["Tiles"]);
            availableConfigs.itemDefinitions = ParseItemDefinitions(itemsConfig["Items"]);
            availableConfigs.avatarDefinitions = ParseAvatarDefinitions(avatarsConfig["Avatars"]);

            LoadTileDefinitionsAsItemTileDefinitions();

            ParseTileUpdateRules(tilesConfig["TileRules"]);

            List<ConfigWorldGenerator> generators = new List<ConfigWorldGenerator>();
            foreach (XmlElement generatorXML in GetChildElements(generatorsConfig["Generators"]))
                generators.Add(ParseGenerator(GetAttributeStringValue(generatorXML, "name"), generatorXML));

            availableConfigs.worldGenerators = generators.ToArray();
            availableConfigs.worldSizes = sizes.ToArray();
            availableConfigs.dayInfos = days.ToArray();

            return availableConfigs;
        }

        private XmlElement GetConfigXmlElement(string configTxt)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(configTxt);
            XmlElement configXML = xDoc["Config"];

            return configXML;
        }

        private void LoadTileDefinitionsAsItemTileDefinitions()
        {
            List<ItemDefinition> itemDefinitions = new List<ItemDefinition>(availableConfigs.itemDefinitions);

            foreach (TileDefinition tileDefinition in availableConfigs.tileDefinitions)
            {
                if (tileDefinition.tileType != TileDefinition.EMPTY_TILE_TYPE)
                {
                    ItemTileDefinition itemTileDefinition = new ItemTileDefinition();
                    itemTileDefinition.id = tileDefinition.id;
                    itemTileDefinition.description = "--.--";
                    itemTileDefinition.tileDefinition = tileDefinition;

                    itemDefinitions.Add(itemTileDefinition);
                }
            }

            availableConfigs.itemDefinitions = itemDefinitions.ToArray();
        }

        private ConfigExtraMaterials ParseExtraMaterials(XmlElement extraMaterialsXML)
        {
            ConfigExtraMaterials extra = new ConfigExtraMaterials();

            string[] damageMaterials = extraMaterialsXML["DamageMaterials"].InnerText.Split(',');
            extra.damageMaterials = new int[damageMaterials.Length];
            for (int i = 0; i < damageMaterials.Length; i++)
                extra.damageMaterials[i] = int.Parse(damageMaterials[i]);

            string[] fireMaterials = extraMaterialsXML["FireMaterials"].InnerText.Split(',');
            extra.fireMaterials = new int[fireMaterials.Length];
            for (int i = 0; i < fireMaterials.Length; i++)
                extra.fireMaterials[i] = int.Parse(fireMaterials[i]);

            return extra;
        }

        static int GetElementIntValue(XmlElement x, string name)
        {
            return int.Parse(x[name].InnerText);
        }

        static int GetAttributeIntValue(XmlElement x, string name)
        {
            return int.Parse(x.GetAttribute(name));
        }

        static float GetAttributeFloatValue(XmlElement x, string name)
        {
            return float.Parse(x.GetAttribute(name), System.Globalization.CultureInfo.InvariantCulture);
        }


        static private Vector3 GetAttributeVector3Value(XmlElement x, string name, Vector3 defaultValue)
        {
            if (string.IsNullOrEmpty(x.GetAttribute(name)) == false)
            {
                string[] s = x.GetAttribute(name).Split(',');
                return new Vector3(
                    float.Parse(s[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(s[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(s[2], System.Globalization.CultureInfo.InvariantCulture));
            }

            return defaultValue;
        }

        static private Vector3 GetAttributeVector3Value(XmlElement x, string name)
        {
            string[] s = x.GetAttribute(name).Split(',');
            return new Vector3(
                float.Parse(s[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(s[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(s[2], System.Globalization.CultureInfo.InvariantCulture));
        }

        static private TilePosition GetAttributeTilePositionValue(XmlElement x, string name)
        {
            string[] s = x.GetAttribute(name).Split(',');
            return new TilePosition(
                int.Parse(s[0]),
                int.Parse(s[1]),
                int.Parse(s[2]));
        }

        static private TilePosition GetAttributeDeltaValue(XmlElement x)
        {
            return new TilePosition(
                    GetAttributeIntValue(x, "dx", 0),
                    GetAttributeIntValue(x, "dy", 0),
                    GetAttributeIntValue(x, "dz", 0));
        }

        static float GetAttributeFloatValue(XmlElement x, string name, float defaultValue)
        {
            if (string.IsNullOrEmpty(x.GetAttribute(name)) == false)
                return float.Parse(x.GetAttribute(name), System.Globalization.CultureInfo.InvariantCulture);

            return defaultValue;
        }

        static int GetAttributeIntValue(XmlElement x, string name, int defaultValue)
        {
            if (string.IsNullOrEmpty(x.GetAttribute(name)) == false)
                return int.Parse(x.GetAttribute(name));

            return defaultValue;
        }

        static WorldSizeRelativeValue GetAttributeRelativeValue(XmlElement x, string name, int defaultValue)
        {
            if (string.IsNullOrEmpty(x.GetAttribute(name)) == false)
                return new WorldSizeRelativeValue(x.GetAttribute(name));

            return new WorldSizeRelativeValue(defaultValue.ToString());
        }

        static WorldSizeRelativeValue GetAttributeRelativeValue(XmlElement x, string name)
        {
            return new WorldSizeRelativeValue(x.GetAttribute(name));
        }

        static XmlElement[] GetChildElements(XmlElement x)
        {
            List<XmlElement> childs = new List<XmlElement>();

            foreach (XmlNode node in x)
                if (node is XmlElement)
                    childs.Add((XmlElement)node);

            return childs.ToArray();
        }

        static string GetAttributeStringValue(XmlElement x, string name)
        {
            return x.GetAttribute(name);
        }

        static string GetAttributeStringValue(XmlElement x, string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(x.GetAttribute(name)) == false)
                return x.GetAttribute(name);

            return defaultValue;
        }

        static bool GetAttributeBoolValue(XmlElement x, string name)
        {
            return bool.Parse(x.GetAttribute(name));
        }

        static bool GetAttributeBoolValue(XmlElement x, string name, bool defaultValue)
        {
            if (string.IsNullOrEmpty(x.GetAttribute(name)) == false)
                return bool.Parse(x.GetAttribute(name));

            return defaultValue;
        }

        private ConfigWorldSize ParseSize(string name, XmlElement sizeXML)
        {
            ConfigWorldSize size = new ConfigWorldSize();

            size.name = name;
            size.worldSizeBitsX = GetElementIntValue(sizeXML, "WorldSizeBitsX");
            size.worldSizeBitsY = GetElementIntValue(sizeXML, "WorldSizeBitsY");
            size.worldSizeBitsZ = GetElementIntValue(sizeXML, "WorldSizeBitsZ");

            return size;
        }

        private ConfigDayInfo ParseDayInfo(string name, XmlElement dayInfoXML)
        {
            ConfigDayInfo dayInfo = new ConfigDayInfo();
            dayInfo.name = name;

            dayInfo.dayDuration = GetElementIntValue(dayInfoXML, "Duration");
            dayInfo.dayTimeLuminances = ParseLuminances(dayInfoXML["Luminances"]);

            return dayInfo;
        }

        private DayTimeLuminanceInfo[] ParseLuminances(XmlElement luminancesXML)
        {
            List<DayTimeLuminanceInfo> luminances = new List<DayTimeLuminanceInfo>();

            foreach (XmlElement luminanceXML in GetChildElements(luminancesXML))
                luminances.Add(ParseLuminance(luminanceXML));

            return luminances.ToArray();
        }

        private DayTimeLuminanceInfo ParseLuminance(XmlElement luminanceXML)
        {
            DayTimeLuminanceInfo luminance = new DayTimeLuminanceInfo(
                GetAttributeFloatValue(luminanceXML, "normalizedTime"),
                GetAttributeIntValue(luminanceXML, "luminancePercent"));

            return luminance;
        }

        private TileDefinition[] ParseTileDefinitions(XmlElement tilesDefinitionsXML)
        {
            List<TileDefinition> tiles = new List<TileDefinition>();

            TileDefinition emptyTile = new TileDefinition();
            emptyTile.tileType = TileDefinition.EMPTY_TILE_TYPE;
            emptyTile.castShadow = false;
            emptyTile.description = "Empty";
            emptyTile.drawMode = TileDefinition.DrawMode.NONE;
            emptyTile.energy = 0;
            emptyTile.id = "empty";
            //emptyTile.liquid = false;
            emptyTile.materials = null;
            emptyTile.solid = false;

            tiles.Add(emptyTile);

            byte tileId = 1;

            foreach (XmlElement tileDefinitionXML in GetChildElements(tilesDefinitionsXML))
                tiles.Add(ParseTileDefinition(tileId++, tileDefinitionXML));

            return tiles.ToArray();
        }

        private TileDefinition ParseTileDefinition(byte type, XmlElement tileDefinitionXML)
        {
            TileDefinition tile = new TileDefinition();

            tile.tileType = type;
            tile.id = GetAttributeStringValue(tileDefinitionXML, "id");
            tile.solid = GetAttributeBoolValue(tileDefinitionXML, "solid", true);
            tile.liquid = GetAttributeBoolValue(tileDefinitionXML, "liquid", false);
            tile.burns = GetAttributeBoolValue(tileDefinitionXML, "burns", false);
            tile.animated = GetAttributeBoolValue(tileDefinitionXML, "animated", false);
            tile.energy = (byte)GetAttributeIntValue(tileDefinitionXML, "energy", Tile.MAX_ENERGY);
            tile.description = GetAttributeStringValue(tileDefinitionXML, "description");
            tile.drawMode = (TileDefinition.DrawMode)System.Enum.Parse(typeof(TileDefinition.DrawMode),
                                                                            GetAttributeStringValue(tileDefinitionXML, "drawMode"),
                                                                            true);

            tile.castShadow = GetAttributeBoolValue(tileDefinitionXML, "castShadow", true);
            tile.lightSourceIntensity = (byte)GetAttributeIntValue(tileDefinitionXML, "lightSourceIntensity", 0);

            string[] materials = GetAttributeStringValue(tileDefinitionXML, "materials").Split(',');
            tile.materials = new int[6];

            if (materials.Length == 1)
            {
                for (int i = 0; i < 6; i++)
                    tile.materials[i] = int.Parse(materials[0]);
            }
            else if (materials.Length == 6)
            {
                for (int i = 0; i < materials.Length; i++)
                    tile.materials[i] = int.Parse(materials[i]);
            }
            else
            {
                throw new System.Exception("Invalid number of materials in tile " + tile.id);
            }

            return tile;
        }

        private ItemDefinition[] ParseItemDefinitions(XmlElement itemsDefinitionsXML)
        {
            List<ItemDefinition> items = new List<ItemDefinition>();

            foreach (XmlElement itemDefinitionXML in GetChildElements(itemsDefinitionsXML))
                items.Add(ParseItemDefinition(itemDefinitionXML));

            return items.ToArray();
        }

        private ItemDefinition ParseItemDefinition(XmlElement itemDefinitionXML)
        {
            ItemDefinition item = new ItemDefinition();

            item.id = GetAttributeStringValue(itemDefinitionXML, "id");
            item.description = GetAttributeStringValue(itemDefinitionXML, "description");
            item.visualDefinition = ParseVisualDefinition(itemDefinitionXML);
            item.energy = GetAttributeIntValue(itemDefinitionXML, "energy", 0);

            item.durability = GetAttributeIntValue(itemDefinitionXML, "durability", 0);
            item.damage = GetAttributeIntValue(itemDefinitionXML, "damage", 0);
            item.setOnFire = GetAttributeBoolValue(itemDefinitionXML, "setOnFire", false);

            return item;
        }

        private CWVisualDefinition ParseVisualDefinition(XmlElement visualDefinitionXML)
        {
            CWVisualDefinition visualDefinition = new CWVisualDefinition();

            visualDefinition.material = GetAttributeStringValue(visualDefinitionXML, "material");
            visualDefinition.materialCount = GetAttributeIntValue(visualDefinitionXML, "materialCount", 1);
            visualDefinition.scale = GetAttributeIntValue(visualDefinitionXML, "scale", 16);
            visualDefinition.pivot = GetAttributeVector3Value(visualDefinitionXML, "pivot", new Vector3(0, 0, 0));
            visualDefinition.plane = GetAttributeStringValue(visualDefinitionXML, "plane", "z");

            return visualDefinition;
        }

        private AvatarDefinition[] ParseAvatarDefinitions(XmlElement avatarsDefinitionsXML)
        {
            List<AvatarDefinition> avatars = new List<AvatarDefinition>();

            foreach (XmlElement avatarDefinitionXML in GetChildElements(avatarsDefinitionsXML))
                avatars.Add(ParseAvatarDefinition(avatarDefinitionXML));

            return avatars.ToArray();
        }

        private AvatarDefinition ParseAvatarDefinition(XmlElement avatarDefinitionXML)
        {
            AvatarDefinition avatar = new AvatarDefinition();

            avatar.id = GetAttributeStringValue(avatarDefinitionXML, "id");
            avatar.description = GetAttributeStringValue(avatarDefinitionXML, "description");
            avatar.energy = GetAttributeIntValue(avatarDefinitionXML, "energy", 0);
            avatar.sizeInTiles = GetAttributeTilePositionValue(avatarDefinitionXML, "size");

            List<AvatarPartDefinition> parts = new List<AvatarPartDefinition>();

            foreach (XmlElement partXML in GetChildElements(avatarDefinitionXML["Parts"]))
                parts.Add(ParseAvatarPartDefinition(partXML));

            avatar.parts = parts.ToArray();

            return avatar;
        }

        private AvatarPartDefinition ParseAvatarPartDefinition(XmlElement partXML)
        {
            AvatarPartDefinition part = new AvatarPartDefinition();
            part.visualDefinition = ParseVisualDefinition(partXML);
            part.id = GetAttributeStringValue(partXML, "id");
            part.offset = GetAttributeVector3Value(partXML, "offset", new Vector3(0, 0, 0));
            part.rotation = GetAttributeVector3Value(partXML, "rotation", new Vector3(0, 0, 0));

            return part;
        }

        private void ParseTileUpdateRules(XmlElement tileUpdateRulesXML)
        {
            foreach (XmlElement tileUpdateRuleXML in GetChildElements(tileUpdateRulesXML))
                AddTileUpdateRule(tileUpdateRuleXML);
        }

        private void AddTileUpdateRule(XmlElement tileUpdateRuleXML)
        {
            List<TileDefinition> tileDefinitions = new List<TileDefinition>();

            foreach (string tileTypeId in GetAttributeStringValue(tileUpdateRuleXML, "tileType").Split(','))
                tileDefinitions.Add(TileIdToTileDefinition(tileTypeId));

            foreach (TileDefinition tileDefinition in tileDefinitions)
            {
                //Slow.. i'm parsing all the rules for each tile definition to update..
                switch (tileUpdateRuleXML.Name)
                {
                    case "TileRule":
                        {
                            TileUpdateRules tileUpdateRule = tileDefinition.tileUpdateRules;

                            if (tileUpdateRule == null)
                            {
                                tileUpdateRule = new TileUpdateRules();
                                tileDefinition.tileUpdateRules = tileUpdateRule;
                            }

                            foreach (XmlElement ruleXML in GetChildElements(tileUpdateRuleXML))
                                tileUpdateRule.AddRule(ParseTileRule(ruleXML));
                            break;
                        }

                    case "TileActionRule":
                        {
                            TileActionRules tileActionRules = tileDefinition.tileActionRules;

                            if (tileActionRules == null)
                            {
                                tileActionRules = new TileActionRules();
                                tileDefinition.tileActionRules = tileActionRules;
                            }

                            TileActionRule.ActionType tileActionType = (TileActionRule.ActionType)System.Enum.Parse(typeof(TileActionRule.ActionType),
                                                                            GetAttributeStringValue(tileUpdateRuleXML, "action"),
                                                                            true);

                            TileActionRule tileActionRule = tileActionRules.GetRulesForAction(tileActionType);

                            if (tileActionRule == null)
                            {
                                tileActionRule = new TileActionRule();
                                tileActionRule.action = tileActionType;

                                tileActionRules.AddActionRule(tileActionRule);
                            }

                            foreach (XmlElement ruleXML in GetChildElements(tileUpdateRuleXML))
                                tileActionRule.AddRule(ParseTileRule(ruleXML));
                            break;
                        }

                    default:
                        throw new System.Exception("Unknown type of tile rule: " + tileUpdateRuleXML.Name);
                }
            }
        }

        private TileRule ParseTileRule(XmlElement ruleXML)
        {
            TileRule rule;

            switch (ruleXML.Name)
            {
                case "Multiple":
                    {
                        List<TileRule> otherRules = new List<TileRule>();
                        foreach (XmlElement otherRuleXML in GetChildElements(ruleXML["Rules"]))
                            otherRules.Add(ParseTileRule(otherRuleXML));

                        TileRuleCondition condition = null;
                        if (ruleXML["Condition"] != null && GetChildElements(ruleXML["Condition"]).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML["Condition"])[0]);

                        rule = new TileRuleMultiple(
                            otherRules.ToArray(),
                            condition);
                        break;
                    }

                case "MultipleOnlyOne":
                    {
                        List<TileRule> otherRules = new List<TileRule>();
                        foreach (XmlElement otherRuleXML in GetChildElements(ruleXML["Rules"]))
                            otherRules.Add(ParseTileRule(otherRuleXML));

                        TileRuleCondition condition = null;
                        if (ruleXML["Condition"] != null && GetChildElements(ruleXML["Condition"]).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML["Condition"])[0]);

                        rule = new TileRuleMultipleOnlyOne(
                            GetAttributeBoolValue(ruleXML, "useRandom", false),
                            otherRules.ToArray(),
                            condition);
                        break;
                    }

                case "SetTileType":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleSetTileType(
                            GetAttributeDeltaValue(ruleXML),
                            TileIdToTileType(GetAttributeStringValue(ruleXML, "tileType")),
                            condition);
                        break;
                    }

                case "Destroy":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleDestroy(
                            GetAttributeDeltaValue(ruleXML),
                            condition);
                        break;
                    }

                case "Damage":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleDamage(
                            GetAttributeDeltaValue(ruleXML),
                            GetAttributeIntValue(ruleXML, "damage"),
                            condition);
                        break;
                    }

                case "Explode":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleExplode(
                            GetAttributeIntValue(ruleXML, "radius"),
                            GetAttributeIntValue(ruleXML, "damage"),
                            GetAttributeBoolValue(ruleXML, "setOnFire", false),
                            condition);
                        break;
                    }

                case "SetOnFire":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleSetOnFire(
                            GetAttributeDeltaValue(ruleXML),
                            GetAttributeBoolValue(ruleXML, "value", true),
                            condition);
                        break;
                    }

                case "SetDynamic":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleSetDynamic(
                            GetAttributeDeltaValue(ruleXML),
                            GetAttributeBoolValue(ruleXML, "value", true),
                            GetAttributeBoolValue(ruleXML, "gravity", true),
                            GetAttributeIntValue(ruleXML, "timeout", -1),
                            condition);
                        break;
                    }

                case "PlayEffect":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRulePlayEffect(
                            GetAttributeDeltaValue(ruleXML),
                            GetAttributeStringValue(ruleXML, "id"),
                            condition);
                        break;
                    }

                case "ApplyEffect":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleApplyEffect(
                            GetAttributeDeltaValue(ruleXML),
                            GetAttributeStringValue(ruleXML, "id"),
                            condition);
                        break;
                    }

                case "PlaySound":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRulePlaySound(
                            GetAttributeDeltaValue(ruleXML),
                            GetAttributeStringValue(ruleXML, "id"),
                            condition);
                        break;
                    }

                case "Invalidate":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleInvalidate(
                            GetAttributeDeltaValue(ruleXML),
                            condition);
                        break;
                    }

                case "CreateItem":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleCreateItem(
                            GetAttributeDeltaValue(ruleXML),
                            ItemIdToItemDefinition(GetAttributeStringValue(ruleXML, "item")),
                            condition);
                        break;
                    }

                case "DropSameTileItem":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleDropSameTileItem(
                            condition);
                        break;
                    }

                case "Liquid":
                    {
                        TileRuleCondition condition = null;
                        if (GetChildElements(ruleXML).Length > 0)
                            condition = ParseTileRuleCondition(GetChildElements(ruleXML)[0]);

                        rule = new TileRuleLiquid(
                            GetAttributeIntValue(ruleXML, "max"),
                            GetAttributeIntValue(ruleXML, "speed"),
                            condition);
                        break;
                    }

                default:
                    throw new System.Exception("Unknown tile rule: " + ruleXML.Name);
            }

            return rule;
        }

        private TileRuleCondition[] ParseTileRuleConditions(XmlElement tileRuleConditionsXML)
        {
            List<TileRuleCondition> conditions = new List<TileRuleCondition>();

            foreach (XmlElement tileRuleConditionXML in GetChildElements(tileRuleConditionsXML))
                conditions.Add(ParseTileRuleCondition(tileRuleConditionXML));

            return conditions.ToArray();
        }

        private TileRuleCondition ParseTileRuleCondition(XmlElement tileRuleConditionXML)
        {
            TileRuleCondition condition = null;

            switch (tileRuleConditionXML.Name)
            {
                case "Or":
                    condition = new TileRuleConditionOr(
                        ParseTileRuleConditions(tileRuleConditionXML));
                    break;

                case "And":
                    condition = new TileRuleConditionAnd(
                        ParseTileRuleConditions(tileRuleConditionXML));
                    break;

                case "Not":
                    condition = new TileRuleConditionNot(
                            ParseTileRuleCondition(GetChildElements(tileRuleConditionXML)[0]));
                    break;

                case "IsType":
                    condition = new TileRuleConditionIsType(
                        GetAttributeDeltaValue(tileRuleConditionXML),
                        TileIdToTileType(GetAttributeStringValue(tileRuleConditionXML, "tileType")));
                    break;

                case "IsBurnable":
                    condition = new TileRuleConditionIsBurnable(
                        GetAttributeDeltaValue(tileRuleConditionXML));
                    break;

                case "IsOnFire":
                    condition = new TileRuleConditionIsOnFire(
                        GetAttributeDeltaValue(tileRuleConditionXML));
                    break;

                case "NearTypeAmount":
                    condition = new TileRuleConditionNearTypeAmout(
                        GetAttributeIntValue(tileRuleConditionXML, "min", 0),
                        TileIdToTileType(GetAttributeStringValue(tileRuleConditionXML, "tileType")));
                    break;

                default:
                    throw new System.Exception("Unknown tile rule condition: " + tileRuleConditionXML.Name);
            }

            return condition;
        }

        private byte TileIdToTileType(string id)
        {
            if (id == "empty")
                return 0;

            return TileIdToTileDefinition(id).tileType;
        }

        private ItemDefinition ItemIdToItemDefinition(string id)
        {
            foreach (ItemDefinition itemDefinition in availableConfigs.itemDefinitions)
                if (string.Equals(itemDefinition.id, id, System.StringComparison.InvariantCultureIgnoreCase))
                    return itemDefinition;

            throw new System.Exception("Invalid item definition id: " + id);
        }

        private TileDefinition TileIdToTileDefinition(string id)
        {
            foreach (TileDefinition tile in availableConfigs.tileDefinitions)
                if (string.Equals(tile.id, id, System.StringComparison.InvariantCultureIgnoreCase))
                    return tile;

            throw new System.Exception("Invalid tile id: " + id);
        }

        private ConfigWorldGenerator ParseGenerator(string name, XmlElement generatorXML)
        {
            ConfigWorldGenerator generator = new ConfigWorldGenerator();
            generator.name = name;

            generator.surroundings.surroundingMaterial = GetAttributeIntValue(generatorXML, "surroundingMaterial", -1);
            generator.surroundings.surroundingLevel = GetAttributeRelativeValue(generatorXML, "surroundingLevel", 0);
            generator.surroundings.surroundingOffsetY = GetAttributeFloatValue(generatorXML, "surroundingOffsetY", 0);

            generator.generator = CreateGenerator(GetChildElements(generatorXML)[0]);
            return generator;
        }

        private CubeWorldGenerator CreateGenerator(XmlElement generatorXML)
        {
            CubeWorldGenerator generator = null;

            switch (generatorXML.Name)
            {
                case "Chained":
                    generator = new ChainedWorldGenerator();
                    foreach (XmlElement childGeneratorXML in GetChildElements(generatorXML))
                        ((ChainedWorldGenerator)generator).AddGenertor(CreateGenerator(childGeneratorXML));
                    break;

                case "Plain":
                    generator = new PlainWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")));
                    break;

                case "PlainRandom":
                    generator = new PlainRandomWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        GetTileTypeProbabilities(GetChildElements(generatorXML)));
                    break;

                case "ParticleDeposition":
                    generator = new ParticleDepositionWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "minParticles"),
                        GetAttributeRelativeValue(generatorXML, "maxParticles"),
                        GetAttributeRelativeValue(generatorXML, "minDrops"),
                        GetAttributeRelativeValue(generatorXML, "maxDrops"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")));
                    break;

                case "Water":
                    generator = new WaterWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "waterTileType")));
                    break;

                case "TopTileTransformation":
                    generator = new TopTileTransformationWorldGenerator(
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "fromTile")),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "toTile")),
                        GetAttributeFloatValue(generatorXML, "probability"));
                    break;

                case "Tree":
                    generator = new TreeWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "min"),
                        GetAttributeRelativeValue(generatorXML, "max"),
                        GetAttributeIntValue(generatorXML, "minTrunkHeight"),
                        GetAttributeIntValue(generatorXML, "maxTrunkHeight"),
                        GetAttributeIntValue(generatorXML, "minLeavesHeight"),
                        GetAttributeIntValue(generatorXML, "maxLeavesHeight"),
                        GetAttributeIntValue(generatorXML, "minLeavesRadius"),
                        GetAttributeIntValue(generatorXML, "maxLeavesRadius"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "overTile")),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "trunkTile")),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "leavesTile")));
                    break;

                case "Smooth":
                    generator = new SmoothWorldGenerator(
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")),
                        GetAttributeRelativeValue(generatorXML, "iterations"),
                        GetAttributeRelativeValue(generatorXML, "minRadius"),
                        GetAttributeRelativeValue(generatorXML, "maxRadius"));
                    break;

                case "Hole":
                    generator = new HoleWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "iterations"),
                        GetAttributeRelativeValue(generatorXML, "minRadius"),
                        GetAttributeRelativeValue(generatorXML, "maxRadius"),
                        GetAttributeRelativeValue(generatorXML, "minDepth"),
                        GetAttributeRelativeValue(generatorXML, "maxDepth"));
                    break;

                case "Cave":
                    generator = new CaveWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "iterations"),
                        GetAttributeRelativeValue(generatorXML, "minRadius"),
                        GetAttributeRelativeValue(generatorXML, "maxRadius"),
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"));
                    break;

                case "CaveCell":
                    generator = new CaveCellWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "iterations"),
                        GetAttributeRelativeValue(generatorXML, "minRadius"),
                        GetAttributeRelativeValue(generatorXML, "maxRadius"),
                        GetAttributeRelativeValue(generatorXML, "minDepth"),
                        GetAttributeRelativeValue(generatorXML, "maxDepth"));
                    break;

                case "MidpointDisplacement":
                    generator = new MidpointDisplacementWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        GetAttributeFloatValue(generatorXML, "roughness"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")));
                    break;

                case "PerlinNoise":
                    generator = new PerlinNoiseWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        GetAttributeIntValue(generatorXML, "octaves"),
                        GetAttributeFloatValue(generatorXML, "freq"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")));
                    break;

                case "PerlinNoise2":
                    generator = new PerlinNoise2WorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        GetAttributeIntValue(generatorXML, "octaves"),
                        GetAttributeFloatValue(generatorXML, "freq"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")));
                    break;

                case "Deposit":
                    generator = new DepositWorldGenerator(
                        GetAttributeRelativeValue(generatorXML, "fromY"),
                        GetAttributeRelativeValue(generatorXML, "toY"),
                        GetAttributeRelativeValue(generatorXML, "iterations"),
                        GetAttributeRelativeValue(generatorXML, "minRadius"),
                        GetAttributeRelativeValue(generatorXML, "maxRadius"),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "overTile")),
                        TileIdToTileType(GetAttributeStringValue(generatorXML, "tileType")),
                        GetAttributeBoolValue(generatorXML, "allowEmptyAbove", false));
                    break;


                default:
                    throw new System.Exception("Unknown generator type: " + generatorXML.Name);
            }

            return generator;
        }

        private PlainRandomWorldGenerator.TileTypeProbability[] GetTileTypeProbabilities(XmlElement[] tileTypeProbabilitiesXML)
        {
            List<PlainRandomWorldGenerator.TileTypeProbability> ps = new List<PlainRandomWorldGenerator.TileTypeProbability>();

            foreach (XmlElement x in tileTypeProbabilitiesXML)
            {
                int p = GetAttributeIntValue(x, "probability");
                byte t = TileIdToTileType(GetAttributeStringValue(x, "tileType"));

                ps.Add(new PlainRandomWorldGenerator.TileTypeProbability(p, t));
            }

            return ps.ToArray();
        }
    }
}
