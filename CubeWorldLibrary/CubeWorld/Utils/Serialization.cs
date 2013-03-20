using System;
using System.Collections.Generic;
using System.IO;
using CubeWorld.Tiles;
using CubeWorld.Utils;
using CubeWorld.World.Generator;

namespace SourceCode.CubeWorld.Utils
{
    public static class SerializationUtils
    {
        public static void Write(BinaryWriter bw, Vector3 pos)
        {
            bw.Write(pos.x);
            bw.Write(pos.y);
            bw.Write(pos.z);
        }

        public static void Write(BinaryWriter bw, TilePosition pos)
        {
            bw.Write(pos.x);
            bw.Write(pos.y);
            bw.Write(pos.z);
        }

        public static void Write(BinaryWriter bw, Color color)
        {
            bw.Write(color.r);
            bw.Write(color.g);
            bw.Write(color.b);
            bw.Write(color.a);
        }

        public static Vector3 ReadVector3(BinaryReader br)
        {
            Vector3 v = new Vector3();
            v.x = br.ReadSingle();
            v.y = br.ReadSingle();
            v.z = br.ReadSingle();
            return v;
        }

        public static TilePosition ReadTilePosition(BinaryReader br)
        {
            TilePosition v = new TilePosition();
            v.x = br.ReadInt32();
            v.y = br.ReadInt32();
            v.z = br.ReadInt32();
            return v;
        }

        public static Color ReadColor(BinaryReader br)
        {
            Color color = new Color();
            color.r = br.ReadSingle();
            color.g = br.ReadSingle();
            color.b = br.ReadSingle();
            color.a = br.ReadSingle();

            return color;
        }


        public static void Write(BinaryWriter bw, WorldSizeRelativeValue wsRelativeValue)
        {
            bw.Write(wsRelativeValue.Expression);
        }

        public static WorldSizeRelativeValue ReadWorldSizeRelativeValue(BinaryReader br)
        {
            return new WorldSizeRelativeValue(br.ReadString());
        }
    }
}
