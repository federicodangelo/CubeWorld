using System;
using System.Collections.Generic;
using CubeWorld.World.Objects;
using CubeWorld.Utils;
using CubeWorld.Serialization;

namespace CubeWorld.Avatars
{
    public class AvatarPartDefinition : ISerializable
    {
        public string id;
        public Vector3 offset;
        public Vector3 rotation;
        public CWVisualDefinition visualDefinition;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref id, "id");
            serializer.Serialize(ref offset, "offset");
            serializer.Serialize(ref rotation, "rotation");
            serializer.Serialize(ref visualDefinition, "visualDefinition");
        }
    }
}
