using System;
using System.Collections;
using CubeWorld.Utils;
using CubeWorld.World;
using CubeWorld.World.Objects;
using CubeWorld.Avatars.Components;

namespace CubeWorld.Avatars
{
    public class Avatar : CWObject
    {
        public AvatarInput input;

        public Avatar(CubeWorld.World.CubeWorld world, AvatarDefinition avatarDefinition, int objectId)
            : base(objectId)
        {
            this.world = world;
			this.definition = avatarDefinition;

            this.input = new AvatarInput();

            AddComponent(new AvatarComponentPhysics());
        }
    }
}
