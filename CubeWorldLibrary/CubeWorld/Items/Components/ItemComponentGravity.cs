using System;

using CubeWorld.World.Objects;
using CubeWorld.Utils;
using CubeWorld.Items;
using CubeWorld.Tiles;

namespace CubeWorld.Items.Components
{
    public class ItemComponentGravity : CWComponent
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

        public const float MAX_DELTA_TIME = Avatars.Components.AvatarComponentPhysics.MAX_DELTA_TIME;
        public const float MAX_DELTA_POSITION = Avatars.Components.AvatarComponentPhysics.MAX_DELTA_POSITION;

        public const float GRAVITY = Avatars.Components.AvatarComponentPhysics.GRAVITY;
        public const float MAX_FALL_SPEED = Avatars.Components.AvatarComponentPhysics.MAX_FALL_SPEED;

        public const float MIN_RANDOM_VERTICAL_SPEED = 3.0f;
        public const float MAX_RANDOM_VERTICAL_SPEED = 6.0f;

        public const float MIN_RANDOM_HORIZONTAL_SPEED = -1.0f;
        public const float MAX_RANDOM_HORIZONTAL_SPEED = 1.0f;

        private float verticalSpeed = 0.0f;
        private float xSpeed = 0.0f;
        private float zSpeed = 0.0f;

        private bool falling = true;

        public ItemComponentGravity()
        {
            Random rnd = new Random();

            verticalSpeed = MIN_RANDOM_VERTICAL_SPEED + (float) rnd.NextDouble() * (MAX_RANDOM_VERTICAL_SPEED - MIN_RANDOM_VERTICAL_SPEED);
            xSpeed = MIN_RANDOM_HORIZONTAL_SPEED + (float)rnd.NextDouble() * (MAX_RANDOM_HORIZONTAL_SPEED - MIN_RANDOM_HORIZONTAL_SPEED);
            zSpeed = MIN_RANDOM_HORIZONTAL_SPEED + (float)rnd.NextDouble() * (MAX_RANDOM_HORIZONTAL_SPEED - MIN_RANDOM_HORIZONTAL_SPEED);
        }

        public override void Update(float deltaTime)
        {
            while (deltaTime > 0.0f)
            {
                float delta;

                if (deltaTime > MAX_DELTA_TIME)
                {
                    delta = MAX_DELTA_TIME;
                    deltaTime -= MAX_DELTA_TIME;
                }
                else
                {
                    delta = deltaTime;
                    deltaTime = 0.0f;
                }

                Fall(delta);
            }
        }
       
        private void Fall(float deltaTime)
        {
            TilePosition tilePos = Graphics.Vector3ToTilePosition(item.position);
            int yBelow = Graphics.FloatToTilePosition(item.position.y - Graphics.HALF_TILE_SIZE - 0.01f);

            if (yBelow >= 0)
            {
                if (IsFloorBelow(tilePos, yBelow) == false || verticalSpeed > 0.0f)
                {
                    falling = true;

                    verticalSpeed += GRAVITY * deltaTime;

                    if (verticalSpeed < MAX_FALL_SPEED)
                        verticalSpeed = MAX_FALL_SPEED;

                    Vector3 delta = new Vector3(xSpeed * deltaTime, verticalSpeed * deltaTime, zSpeed * deltaTime);

                    if (delta.magnitude > MAX_DELTA_POSITION)
                        delta = delta.normalized * MAX_DELTA_POSITION;

                    item.position += delta;
                }
                else
                {
                    if (falling)
                    {
                        verticalSpeed = 0.0f;
                        xSpeed = 0.0f;
                        zSpeed = 0.0f;
                        falling = false;
                    }
                }
            }
            else
            {
                verticalSpeed = 0.0f;
                xSpeed = 0.0f;
                zSpeed = 0.0f;
                falling = false;
            }
        }

        private bool IsFloorBelow(TilePosition tilePosition, int yBelow)
        {
            tilePosition.y = yBelow;

            if (item.world.tileManager.IsValidTile(tilePosition) &&
                item.world.tileManager.GetTileSolid(tilePosition))
                return true;

            return false;
        }
    }
}
