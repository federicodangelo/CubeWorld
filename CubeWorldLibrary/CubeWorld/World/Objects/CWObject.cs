using System;
using CubeWorld.Utils;
using System.Collections.Generic;
using SourceCode.CubeWorld.Utils;

namespace CubeWorld.World.Objects
{
	public class CWObject
	{
        public int objectId;
		public CubeWorld world;
		public CWDefinition definition;

		public Vector3 position;
        public Vector3 rotation;
        public byte energy;
		public bool destroyed;

        private List<CWComponent> components;

        public CWObject(int objectId)
        {
            this.objectId = objectId;
        }

        public void AddComponent(CWComponent component)
        {
            if (components == null)
                components = new List<CWComponent>();

            components.Add(component);

            component.AddedToObject(this);
        }

        public void RemoveComponent(CWComponent component)
        {
            components.Remove(component);

            component.RemovedFromObject();
        }

        public virtual void Clear()
        {
            if (components != null)
            {
                foreach (CWComponent component in components)
                    component.RemovedFromObject();

                components.Clear();
            }

            world = null;
        }

        public virtual void Update(float deltaTime)
        {
            if (components != null)
                foreach (CWComponent component in components)
                    component.Update(deltaTime);
        }

        public virtual void Save(System.IO.BinaryWriter bw)
        {
            SerializationUtils.Write(bw, position);
            SerializationUtils.Write(bw, rotation);
            bw.Write(energy);
            bw.Write(destroyed);
        }

        public virtual void Load(System.IO.BinaryReader br)
        {
            position = SerializationUtils.ReadVector3(br);
            rotation = SerializationUtils.ReadVector3(br);
            energy = br.ReadByte();
            destroyed = br.ReadBoolean();
        }
    }
}

