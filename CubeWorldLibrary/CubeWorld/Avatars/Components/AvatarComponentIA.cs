using CubeWorld.World.Objects;
using CubeWorld.Utils;
using CubeWorld.Tiles;
using System;

namespace CubeWorld.Avatars.Components
{
    public class AvatarComponentIA : CWComponent
    {
        static private Random random = new Random();

        private Avatar avatar;

        protected override void OnAddedToObject(CWObject cwobject)
        {
            this.avatar = (Avatar) cwobject;
        }

        protected override void OnRemovedFromObject()
        {
            avatar = null;
        }

        private Vector3 randomDir;
        private float actionTimer;
        private Action currentAction;
        private float jumpTimer;
        private enum Action
        {
            Walk,
            Wait,
            Max
        }

        public override void Update(float deltaTime)
        {
            if (actionTimer <= 0)
            {
                currentAction = (Action)random.Next(0, (int)Action.Max);

                actionTimer = (float)random.NextDouble() * 3.0f + 1.0f;

                switch (currentAction)
                {
                    case Action.Walk:
                        randomDir = new Vector3(
                        (float)random.NextDouble() * 2.0f - 1.0f,
                        0,
                        (float)random.NextDouble() * 2.0f - 1.0f);
                        randomDir = randomDir.normalized;
                        jumpTimer = (float)random.NextDouble();

                        avatar.input.moveDirection = randomDir;
                        avatar.input.jump = false;
                        break;

                    case Action.Wait:
                        avatar.input.moveDirection = new Vector3();
                        avatar.input.jump = false;
                        break;
                }
            }
            else
            {
                switch (currentAction)
                {
                    case Action.Walk:
                        jumpTimer -= deltaTime;
                        if (jumpTimer <= 0.0f)
                        {
                            jumpTimer = (float)random.NextDouble();
                            avatar.input.jump = true;
                        }
                        else
                        {
                            avatar.input.jump = false;
                        }
                        break;
                }
            }

            actionTimer -= deltaTime;

            if (avatar.input.moveDirection.magnitude > 0)
                avatar.rotation.y = (float) Math.Atan2(avatar.input.moveDirection.x, avatar.input.moveDirection.z) * 57.2958f;
        }
    }
}
