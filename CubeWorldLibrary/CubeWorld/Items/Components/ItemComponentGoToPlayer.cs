using System;

using CubeWorld.World.Objects;
using CubeWorld.Utils;
using CubeWorld.Items;
using CubeWorld.Tiles;
using CubeWorld.Avatars;

namespace CubeWorld.Items.Components
{
    public class ItemComponentGoToPlayer : CWComponent
    {
        private Item item;

        protected override void OnAddedToObject(CWObject cwobject)
        {
            item = (Item)cwobject;
        }

        protected override void OnRemovedFromObject()
        {
            item = null;
        }

        private const float PLAYER_ATTRACT_DISTANCE = 2.5f;
        private const float PLAYER_CATCH_DISTANCE = 0.25f;
        private const float MOVE_SPEED = 10.0f;
        private const float MAX_DELTA_TIME = 1.0f / MOVE_SPEED;

        public override void Update(float deltaTime)
        {
            if (deltaTime > MAX_DELTA_TIME)
                deltaTime = MAX_DELTA_TIME;

            Vector3 vectorToPlayer = item.position - (item.world.avatarManager.player.position + new Vector3(0, Player.HEAD_POSITION * 0.5f, 0));
            float distanceToPlayer = vectorToPlayer.magnitude;

            if (distanceToPlayer < PLAYER_CATCH_DISTANCE)
            {
                CWObject objectToAdd = this.item;

                if (objectToAdd.definition.type == CWDefinition.DefinitionType.ItemTile)
                    objectToAdd = new DynamicTile(item.world, ((ItemTileDefinition) item.definition).tileDefinition, -1);

                if (item.world.avatarManager.player.inventory.Add(objectToAdd))
                    item.world.itemManager.RemoveItem(item);
            }
            else if (distanceToPlayer < PLAYER_ATTRACT_DISTANCE)
            {
                Vector3 dirToPlayer = vectorToPlayer.normalized;

                item.position -= dirToPlayer * MOVE_SPEED * deltaTime;
            }
        }
    }
}
