using System;
using CubeWorld.Tiles;
using CubeWorld.World.Objects;
using CubeWorld.Utils;

namespace CubeWorld.World
{
	public interface ICWFxListener
	{
        void PlaySound(String soundId, Vector3 position);
        void PlaySound(String soundId, CWObject fromObject);

        void PlayEffect(String effectId, Vector3 position);
        void PlayEffect(String effectId, CWObject fromObject);
	}
}

