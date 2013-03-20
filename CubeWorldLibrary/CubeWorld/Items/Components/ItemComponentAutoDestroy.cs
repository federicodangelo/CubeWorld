using System;

using CubeWorld.World.Objects;
using CubeWorld.Utils;
using CubeWorld.Items;
using CubeWorld.Tiles;

namespace CubeWorld.Items.Components
{
    public class ItemComponentAutoDestroy : CWComponent
    {
        private Item item;

        protected override void OnAddedToObject(CWObject cwobject)
        {
            item = (Item) cwobject;
        }

        protected override void OnRemovedFromObject()
        {
            item = null;
        }

        private const float AUTO_DESTROY_TIME = 6.0f;

        private float time;

        public override void Update(float deltaTime)
        {
            time += deltaTime;

            if (time > AUTO_DESTROY_TIME)
                item.world.itemManager.RemoveItem(item);
        }
    }
}
