using System;
using System.Collections.Generic;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.World.Objects
{
    public class CWVisualDefinition : ISerializable
    {
        public Vector3 pivot;

        public string material;
        public int scale;
        public string plane;

        public int materialCount;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref pivot, "pivot");
            serializer.Serialize(ref material, "material");
            serializer.Serialize(ref scale, "scale");
            serializer.Serialize(ref plane, "plane");
            serializer.Serialize(ref materialCount, "materialCount");
        }
    }
}
