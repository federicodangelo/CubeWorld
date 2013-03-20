using System;
using CubeWorld.Tiles;
using CubeWorld.World.Objects;

namespace CubeWorld.World
{
	public interface ICWListener
	{
		void CreateObject(CWObject cwobject);
		void UpdateObject(CWObject cwobject);
		void DestroyObject(CWObject cwobject);
	}
}

