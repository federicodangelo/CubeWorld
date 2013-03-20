using CubeWorld.World.Objects;
using CubeWorld.Utils;
using CubeWorld.Tiles;
using System;

namespace CubeWorld.Avatars.Components
{
    public class AvatarComponentPhysics : CWComponent
    {
        private Avatar avatar;

        protected override void OnAddedToObject(CWObject cwobject)
        {
            this.avatar = (Avatar) cwobject;
        }

        protected override void OnRemovedFromObject()
        {
            avatar = null;
        }

        public const float MAX_DELTA_TIME = 0.05f;
        public const float MAX_DELTA_POSITION = Graphics.HALF_TILE_SIZE;

        public const float PLAYER_BORDER_TOLERANCE = 0.5f;

        public const float GRAVITY = -20.0f;
        public const float GRAVITY_INSIDE_WATER = -5.0f;
        public const float MAX_FALL_SPEED = -20.0f;
        public const float JUMP_SPEED = 6.5f;
        public const float MAX_INSIDE_WATER_SPEED = 3.0f;
        public const float WALK_SPEED = 4.0f;

        private float verticalSpeed = 0.0f;

        public bool falling = true;

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

                ExecuteHorizontalMovement(delta);
                ExecuteVerticalMovement(delta);
            }
        }


        private void ExecuteHorizontalMovement(float deltaTime)
        {
            TilePosition avatarActual = Graphics.Vector3ToTilePosition(avatar.position);
            AvatarDefinition avatarDefinition = (AvatarDefinition) avatar.definition;

            int yActualHead = avatarActual.y + avatarDefinition.sizeInTiles.y - 1;

            if (yActualHead >= avatar.world.sizeY)
                yActualHead = avatarActual.y;

            Vector3 totalDeltaMov = avatar.input.moveDirection * WALK_SPEED * deltaTime;
            float totalDeltaLen = totalDeltaMov.magnitude;

            while (totalDeltaLen > 0.0f)
            {
                float actualDeltaLen;

                if (totalDeltaLen > MAX_DELTA_POSITION)
                {
                    totalDeltaLen -= MAX_DELTA_POSITION;
                    actualDeltaLen = MAX_DELTA_POSITION;
                }
                else
                {
                    actualDeltaLen = totalDeltaLen;
                    totalDeltaLen = 0.0f;
                }

                Vector3 deltaMov = avatar.input.moveDirection * actualDeltaLen;

                if (avatarActual.y < avatar.world.sizeY)
                {
                    //Validate horizontal
                    if (deltaMov.x != 0.0f)
                    {
                        TilePosition avatarNew = avatarActual;

                        if (deltaMov.x < 0.0f)
                            avatarNew.x = Graphics.FloatToTilePosition(avatar.position.x + deltaMov.x - Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);
                        else
                            avatarNew.x = Graphics.FloatToTilePosition(avatar.position.x + deltaMov.x + Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                        TilePosition avatarHeadNew = avatarNew;
                        avatarHeadNew.y = yActualHead;

                        if (avatar.world.tileManager.IsValidTile(avatarNew) == false ||
                            avatar.world.tileManager.GetTileSolid(avatarNew) ||
                            avatar.world.tileManager.IsValidTile(avatarHeadNew) == false ||
                            avatar.world.tileManager.GetTileSolid(avatarHeadNew))
                        {
                            deltaMov.x = 0.0f;
                        }
                    }

                    //Validate forward
                    if (deltaMov.z != 0.0f)
                    {
                        TilePosition avatarNew = avatarActual;

                        if (deltaMov.z < 0.0f)
                            avatarNew.z = Graphics.FloatToTilePosition(avatar.position.z + deltaMov.z - Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);
                        else
                            avatarNew.z = Graphics.FloatToTilePosition(avatar.position.z + deltaMov.z + Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                        TilePosition avatarHeadNew = avatarNew;
                        avatarHeadNew.y = yActualHead;

                        if (avatar.world.tileManager.IsValidTile(avatarNew) == false ||
                            avatar.world.tileManager.GetTileSolid(avatarNew) ||
                            avatar.world.tileManager.IsValidTile(avatarHeadNew) == false ||
                            avatar.world.tileManager.GetTileSolid(avatarHeadNew))
                        {
                            deltaMov.z = 0.0f;
                        }
                    }

                    //Validate the combination
                    if (deltaMov.x != 0.0f && deltaMov.z != 0.0f)
                    {
                        TilePosition avatarNew = avatarActual;

                        if (deltaMov.x < 0.0f)
                            avatarNew.x = Graphics.FloatToTilePosition(avatar.position.x + deltaMov.x - Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);
                        else
                            avatarNew.x = Graphics.FloatToTilePosition(avatar.position.x + deltaMov.x + Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                        if (deltaMov.z < 0.0f)
                            avatarNew.z = Graphics.FloatToTilePosition(avatar.position.z + deltaMov.z - Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);
                        else
                            avatarNew.z = Graphics.FloatToTilePosition(avatar.position.z + deltaMov.z + Graphics.HALF_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                        TilePosition avatarHeadNew = avatarNew;
                        avatarHeadNew.y = yActualHead;

                        if (avatar.world.tileManager.IsValidTile(avatarNew) == false ||
                            avatar.world.tileManager.GetTileSolid(avatarNew) ||
                            avatar.world.tileManager.IsValidTile(avatarHeadNew) == false ||
                            avatar.world.tileManager.GetTileSolid(avatarHeadNew))
                        {
                            //Both horizontal and forward movements where ok.. but not the combination, cancel both!
                            deltaMov.z = 0.0f;
                            deltaMov.x = 0.0f;
                        }
                    }

                }
                else
                {
                    if (deltaMov.x < 0.0f && avatarActual.x < 0 || deltaMov.x > 0.0f && avatarActual.x >= avatar.world.sizeX)
                        deltaMov.x = 0;

                    if (deltaMov.z < 0.0f && avatarActual.z < 0 || deltaMov.z > 0.0f && avatarActual.z >= avatar.world.sizeZ)
                        deltaMov.z = 0;
                }

                avatar.position += deltaMov;
            }
        }

        private bool IsFloorBelow()
        {
            int yBelow = Graphics.FloatToTilePosition(avatar.position.y - Graphics.HALF_TILE_SIZE - 0.01f);
            if (yBelow < 0)
                return true;

            TilePosition near = new TilePosition(0, yBelow, 0);

            for (int x = -1; x <= 1; x++)
            {
                near.x = Graphics.FloatToTilePosition(avatar.position.x + x * Graphics.QUART_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                for (int z = -1; z <= 1; z++)
                {
                    near.z = Graphics.FloatToTilePosition(avatar.position.z + z * Graphics.QUART_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                    if (avatar.world.tileManager.IsValidTile(near) &&
                        avatar.world.tileManager.GetTileSolid(near))
                        return true;
                }
            }

            return false;
        }

        private bool IsCeilingAbove(float deltaY)
        {
            int yAbove = Graphics.FloatToTilePosition(avatar.position.y + Graphics.TILE_SIZE * 1.3f);
            int yAboveDelta = Graphics.FloatToTilePosition(avatar.position.y + Graphics.TILE_SIZE * 1.3f + deltaY);

            TilePosition near = new TilePosition(0, yAbove, 0);
            TilePosition nearDelta = new TilePosition(0, yAboveDelta, 0);

            for (int x = -1; x <= 1; x++)
            {
                nearDelta.x = near.x = Graphics.FloatToTilePosition(avatar.position.x + x * Graphics.QUART_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                for (int z = -1; z <= 1; z++)
                {
                    nearDelta.z = near.z = Graphics.FloatToTilePosition(avatar.position.z + z * Graphics.QUART_TILE_SIZE * PLAYER_BORDER_TOLERANCE);

                    if (avatar.world.tileManager.IsValidTile(near) && avatar.world.tileManager.GetTileSolid(near) ||
                        avatar.world.tileManager.IsValidTile(nearDelta) && avatar.world.tileManager.GetTileSolid(nearDelta))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ExecuteVerticalMovement(float deltaTime)
        {
            TilePosition playerPosition = Graphics.Vector3ToTilePosition(avatar.position);
            int yBelow = Graphics.FloatToTilePosition(avatar.position.y - Graphics.HALF_TILE_SIZE - 0.01f);

            bool insideWater = (avatar.world.tileManager.IsValidTile(playerPosition) && avatar.world.tileManager.GetTileLiquid(playerPosition) == true);

            if (avatar.input.jump)
            {
                if (insideWater)
                {
                    verticalSpeed = MAX_INSIDE_WATER_SPEED;
                }
                else if (falling == false)
                {
                    verticalSpeed = JUMP_SPEED;
                    falling = true;
                }
            }

            if (verticalSpeed > 0.0f || yBelow >= 0)
            {
                bool floorBelow = IsFloorBelow();

                if (verticalSpeed > 0.0f || floorBelow == false)
                {
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

                    if (verticalSpeed > 0.0f && IsCeilingAbove(deltaY))
                    {
                        verticalSpeed = 0.0f;
                        deltaY = 0.0f;
                    }

                    avatar.position += new Vector3(0, deltaY, 0);

                    falling = true;
                }
                else
                {
                    if (falling)
                    {
                        verticalSpeed = 0.0f;
                        avatar.position = new Vector3(
                            avatar.position.x,
                            Graphics.TilePositionToFloat(Graphics.FloatToTilePosition(avatar.position.y)),
                            avatar.position.z);

                        falling = false;
                    }
                }
            }
            else
            {
                verticalSpeed = 0.0f;
                avatar.position = new Vector3(avatar.position.x, 0.0f, avatar.position.z);
                falling = false;
            }
        }
    }
}
