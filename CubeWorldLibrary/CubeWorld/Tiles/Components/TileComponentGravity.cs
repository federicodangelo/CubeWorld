using System;

using CubeWorld.World.Objects;
using CubeWorld.Utils;

namespace CubeWorld.Tiles.Components
{
    public class TileComponentGravity : CWComponent
    {
        private DynamicTile tile;

        protected override void OnAddedToObject(CWObject cwobject)
        {
            tile = (DynamicTile) cwobject;
        }

        protected override void OnRemovedFromObject()
        {
            tile = null;
        }

        public const float MAX_DELTA_TIME = Avatars.Components.AvatarComponentPhysics.MAX_DELTA_TIME;
        public const float MAX_DELTA_POSITION = Avatars.Components.AvatarComponentPhysics.MAX_DELTA_POSITION;

        public const float GRAVITY = Avatars.Components.AvatarComponentPhysics.GRAVITY;
        public const float GRAVITY_INSIDE_WATER = Avatars.Components.AvatarComponentPhysics.GRAVITY_INSIDE_WATER;
        public const float MAX_FALL_SPEED = Avatars.Components.AvatarComponentPhysics.MAX_FALL_SPEED;
        public const float MAX_INSIDE_WATER_SPEED = Avatars.Components.AvatarComponentPhysics.MAX_INSIDE_WATER_SPEED;

        private float verticalSpeed = 0.0f;

        private bool falling = true;

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
            TilePosition tilePos = Graphics.Vector3ToTilePosition(tile.position);
            int yBelow = Graphics.FloatToTilePosition(tile.position.y - Graphics.HALF_TILE_SIZE - 0.01f);

            if (yBelow >= 0)
            {
                bool floorBelow = IsFloorBelow(yBelow);
                bool avatarBelow = IsAvatarBelow(yBelow);

                if (floorBelow == false && avatarBelow == false)
                {
                    falling = true;

                    bool insideWater = (tile.world.tileManager.IsValidTile(tilePos) && tile.world.tileManager.GetTileLiquid(tilePos) == true);

                    if (insideWater == false)
                    {
                        verticalSpeed += GRAVITY * deltaTime;

                        if (verticalSpeed < MAX_FALL_SPEED)
                            verticalSpeed = MAX_FALL_SPEED;
                    }
                    else
                    {
                        verticalSpeed += GRAVITY_INSIDE_WATER * deltaTime;

                        if (verticalSpeed < -MAX_INSIDE_WATER_SPEED)
                            verticalSpeed = -MAX_INSIDE_WATER_SPEED;
                    }

                    float deltaY = verticalSpeed * deltaTime;

                    if (Math.Abs(deltaY) > MAX_DELTA_POSITION)
                        deltaY = MAX_DELTA_POSITION * Math.Sign(deltaY);

                    tile.position += new Vector3(0, deltaY, 0);

                    TilePosition newTilePos = Graphics.Vector3ToTilePosition(tile.position);

                    if (tile.world.tileManager.IsValidTile(newTilePos) && newTilePos != tilePos)
                        tile.world.tileManager.MoveTile(tilePos, newTilePos);

                }
                else
                {
                    if (falling)
                    {
                        verticalSpeed = 0.0f;
                        tile.position = Graphics.TilePositionToVector3(tilePos); 
                        falling = false;

                        if (avatarBelow == false)
                            tile.world.tileManager.TileHitFloor(tilePos);
                    }
                }
            }
            else
            {
                verticalSpeed = 0.0f;
                tile.position = Graphics.TilePositionToVector3(tilePos);
                falling = false;

                tile.world.tileManager.TileHitFloor(tilePos);
            }
        }

        private bool IsAvatarBelow(int yBelow)
        {
            return tile.world.avatarManager.IsTileBlockedByAnyAvatar(new TilePosition(tile.tilePosition.x, yBelow, tile.tilePosition.z));
        }

        private bool IsFloorBelow(int yBelow)
        {
            if (yBelow == tile.tilePosition.y)
                return false;

            TilePosition posBelow = tile.tilePosition;
            posBelow.y = yBelow;

            if (tile.world.tileManager.IsValidTile(posBelow) &&
                tile.world.tileManager.GetTileSolid(posBelow))
                return true;

            return false;
        }
    }
}
