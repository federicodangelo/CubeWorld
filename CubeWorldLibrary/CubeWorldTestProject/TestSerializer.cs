using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeWorld.Serialization;

namespace CubeWorldTestProject
{
    public class TestSerializer
    {
        public class Class2 : ISerializable
        {
            public int k;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref k, "k");
            }
        }

        public class Class1 : ISerializable
        {
            public enum mye
            {
                aaaa,
                bbbb
            }

            public int a;
            public float b;
            public byte c;
            public string d;
            public byte[] e;
            public Class2 f;
            public Class2[] g;
            public mye h;
            public int[] i;
            public Class2 ff;
            //public ISerializable[] f;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref a, "a");
                serializer.Serialize(ref b, "b");
                serializer.Serialize(ref c, "c");
                serializer.Serialize(ref d, "d");
                serializer.Serialize(ref e, "e");
                serializer.Serialize(ref f, "f");
                serializer.Serialize(ref ff, "ff");
                serializer.Serialize(ref g, "g");
                serializer.SerializeEnum(ref h, "h");
                serializer.Serialize(ref i, "i");
            }
        }

        public TestSerializer()
        {
            Serializer.AddType(typeof(Class1));
            Serializer.AddType(typeof(Class2));

            Serializer ser = new Serializer(true);

            Class1 c1t = new Class1();
            c1t.a = 3;
            c1t.b = 4;
            c1t.c = 5;
            c1t.d = "6";
            c1t.e = new byte[] { 1, 2, 3, 4 };
            c1t.g = new Class2[2];
            c1t.g[1] = new Class2();
            c1t.g[0] = c1t.g[1];
            c1t.h = Class1.mye.bbbb;
            c1t.i = new int[] { 3, 2, 1 };

            c1t.f = new Class2();
            c1t.ff = c1t.f;

            c1t.f.k = 22;

            byte[] data = ser.Serialize(c1t);

            Class1 c1 = (Class1)ser.Deserialize(data);

            c1.f.k++;

            if (c1.f.k != c1.ff.k)
                throw new Exception("no funciona la reconstrucción de la serialización de objetos serializados mas de una vez :-(");

            c1.g[0].k++;

            if (c1.g[0].k != c1.g[1].k)
                throw new Exception("no funciona la reconstrucción de la serialización de objetos serializados mas de una vez :-(");

            
        }
    }
}
