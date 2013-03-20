using System;
using System.Collections.Generic;
using System.IO;
using CubeWorld.Utils;
using CubeWorld.Tiles;

namespace CubeWorld.Serialization
{
    public class Serializer
    {
        private const byte TYPE_INT = 0;
        private const byte TYPE_BYTE = 1;
        private const byte TYPE_FLOAT = 2;
        private const byte TYPE_STRING = 3;
        private const byte TYPE_VECTOR3 = 4;
        private const byte TYPE_TILEPOSITION = 5;
        private const byte TYPE_BOOL = 6;
        
        private const byte TYPE_BYTE_ARRAY = 50;
        private const byte TYPE_INT_ARRAY = 51;

        private const byte TYPE_OBJECT_ARRAY = 80;

        private const byte TYPE_OBJECT_START = 100;
        private const byte TYPE_OBJECT_END = 101;

        private const byte TYPE_NULL = 200;

        private const string TYPE_TAG = "__TYPE__";
        private const string ID_TAG = "__ID__";

        private bool serialization;
        static private Dictionary<Type, String> serializablesTypesFromType = new Dictionary<Type, string>();
        static private Dictionary<String, Type> serializablesTypesFromName = new Dictionary<string, Type>();

        private Dictionary<ISerializable, int> serializedObjects = new Dictionary<ISerializable, int>();
        private Dictionary<int, Dictionary<string, object>> deserializedObjects = new Dictionary<int, Dictionary<string, object>>();
        private Dictionary<int, ISerializable> deserializedObjectsReal = new Dictionary<int, ISerializable>();

        private MemoryStream memoryStream;
        private BinaryWriter binaryWriter;

        public bool IsSerialization
        {
            get { return serialization == true; }
        }

        public bool IsDeserialization
        {
            get { return serialization == false; }
        }

        public Serializer(bool serialization)
        {
            this.serialization = serialization;
        }

        static public void AddType(Type type)
        {
            AddType(type, type.Name);
        }

        static public void AddType(Type type, String name)
        {
            serializablesTypesFromName[name] = type;
            serializablesTypesFromType[type] = name;
        }

        public byte[] Serialize(ISerializable obj)
        {
            memoryStream = new MemoryStream();
            binaryWriter = new BinaryWriter(memoryStream);

            serialization = true;

            Serialize(ref obj, "");

            byte[] data = memoryStream.ToArray();

            binaryWriter.Close();
            memoryStream.Dispose();

            binaryWriter = null;
            memoryStream = null;
            serializedObjects.Clear();

            return data;
        }

        private Dictionary<string, object> currentObject = null;

        public ISerializable Deserialize(byte[] data)
        {
            MemoryStream memoryStream = new MemoryStream(data);
            BinaryReader binaryReader = new BinaryReader(memoryStream);

            ISerializable obj = null;

            if (binaryReader.ReadByte() == TYPE_OBJECT_START)
            {
                string fieldName = binaryReader.ReadString();

                currentObject = new Dictionary<string, object>();
                currentObject[fieldName] = InternalObjectDeserialize(binaryReader);

                serialization = false;

                Serialize(ref obj, "");
            }

            deserializedObjects.Clear();
            deserializedObjectsReal.Clear();

            return obj;
        }

        #region Internal

        private Dictionary<string, object> InternalObjectDeserialize(BinaryReader br)
        {
            Dictionary<string, object> currentObject;

            int objectId = br.ReadInt32();

            if (deserializedObjects.ContainsKey(objectId) == false)
            {
                currentObject = new Dictionary<string, object>();

                deserializedObjects[objectId] = currentObject;

                currentObject[TYPE_TAG] = br.ReadString();
                currentObject[ID_TAG] = objectId;

                byte objType = br.ReadByte();

                while(objType != TYPE_OBJECT_END)
                {
                    string fieldName = br.ReadString();

                    switch (objType)
                    {
                        case TYPE_INT:
                            currentObject[fieldName] = br.ReadInt32();
                            break;
                        case TYPE_BYTE:
                            currentObject[fieldName] = br.ReadByte();
                            break;
                        case TYPE_FLOAT:
                            currentObject[fieldName] = br.ReadSingle();
                            break;
                        case TYPE_STRING:
                            currentObject[fieldName] = br.ReadString();
                            break;
                        case TYPE_NULL:
                            currentObject[fieldName] = null;
                            break;
                        case TYPE_VECTOR3:
                        {
                            float x = br.ReadSingle();
                            float y = br.ReadSingle();
                            float z = br.ReadSingle();
                            currentObject[fieldName] = new Vector3(x, y, z);
                            break;
                        }
                        case TYPE_TILEPOSITION:
                        {
                            int x = br.ReadInt32();
                            int y = br.ReadInt32();
                            int z = br.ReadInt32();
                            currentObject[fieldName] = new TilePosition(x, y, z);
                            break;
                        }
                        case TYPE_BOOL:
                            currentObject[fieldName] = br.ReadBoolean();
                            break;
                        case TYPE_BYTE_ARRAY:
                        {
                            int n = br.ReadInt32();
                            byte[] bytes = br.ReadBytes(n);
                            currentObject[fieldName] = bytes;
                            break;
                        }
                        case TYPE_INT_ARRAY:
                        {
                            int n = br.ReadInt32();
                            int[] ints = new int[n];
                            for (int i = 0; i < n; i++)
                                ints[i] = br.ReadInt32();
                            currentObject[fieldName] = ints;
                            break;
                        }
                        case TYPE_OBJECT_ARRAY:
                        {
                            int n = br.ReadInt32();
                            Dictionary<string, object>[] objects = new Dictionary<string, object>[n];
                            for (int i = 0; i < n; i++)
                            {
                                byte b = br.ReadByte();
                                if (b == TYPE_OBJECT_START)
                                    objects[i] = InternalObjectDeserialize(br);
                                else if (b == TYPE_NULL)
                                    objects[i] = null;
                            }
                            currentObject[fieldName] = objects;
                            break; 
                        }
                        case TYPE_OBJECT_START:
                        {
                            currentObject[fieldName] = InternalObjectDeserialize(br);
                            break;
                        }
                    }

                    objType = br.ReadByte();
                }
            }
            else
            {
                currentObject = deserializedObjects[objectId];
            }

            return currentObject;
        }

        #endregion

        #region Serialization Methods

        public void Serialize(ref int v, string fieldName)
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_INT);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v);
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (int) currentObject[fieldName];
            }
        }

        public void SerializeEnum<T>(ref T v, string fieldName) where T : struct, IConvertible
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_STRING);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v.ToString());
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                {
                    string s = (string) currentObject[fieldName];

                    v = (T) Enum.Parse(typeof(T), s, true);
                }
            }
        }

        public void Serialize(ref byte v, string fieldName)
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_BYTE);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v);
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (byte) currentObject[fieldName];
            }
        }

        public void Serialize(ref float v, string fieldName)
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_FLOAT);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v);
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (float)currentObject[fieldName];
            }
        }

        public void Serialize(ref string v, string fieldName)
        {
            if (serialization)
            {
                if (v != null)
                {
                    binaryWriter.Write(TYPE_STRING);
                    binaryWriter.Write(fieldName);
                    binaryWriter.Write(v);
                }
                else
                {
                    binaryWriter.Write(TYPE_NULL);
                    binaryWriter.Write(fieldName);
                }
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (string) currentObject[fieldName];
            }
        }

        public void Serialize(ref Vector3 v, string fieldName)
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_VECTOR3);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v.x);
                binaryWriter.Write(v.y);
                binaryWriter.Write(v.z);
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (Vector3) currentObject[fieldName];
            }
        }

        public void Serialize(ref TilePosition v, string fieldName)
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_TILEPOSITION);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v.x);
                binaryWriter.Write(v.y);
                binaryWriter.Write(v.z);
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (TilePosition) currentObject[fieldName];
            }
        }


        public void Serialize(ref bool v, string fieldName)
        {
            if (serialization)
            {
                binaryWriter.Write(TYPE_BOOL);
                binaryWriter.Write(fieldName);
                binaryWriter.Write(v);
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (bool)currentObject[fieldName];
            }
        }

        public void Serialize(ref byte[] v, string fieldName)
        {
            if (serialization)
            {
                if (v != null)
                {
                    binaryWriter.Write(TYPE_BYTE_ARRAY);
                    binaryWriter.Write(fieldName);
                    binaryWriter.Write(v.Length);
                    binaryWriter.Write(v);
                }
                else
                {
                    binaryWriter.Write(TYPE_NULL);
                    binaryWriter.Write(fieldName);
                }
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (byte[]) currentObject[fieldName];
            }
        }

        public void Serialize(ref int[] v, string fieldName)
        {
            if (serialization)
            {
                if (v != null)
                {
                    binaryWriter.Write(TYPE_INT_ARRAY);
                    binaryWriter.Write(fieldName);
                    binaryWriter.Write(v.Length);
                    for (int i = 0; i < v.Length; i++)
                        binaryWriter.Write(v[i]);
                }
                else
                {
                    binaryWriter.Write(TYPE_NULL);
                    binaryWriter.Write(fieldName);
                }
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                    v = (int[])currentObject[fieldName];
            }
        }

        public void Serialize<T>(ref T[] v, string fieldName) where T : ISerializable
        {
            if (serialization)
            {
                if (v != null)
                {
                    binaryWriter.Write(TYPE_OBJECT_ARRAY);
                    binaryWriter.Write(fieldName);

                    binaryWriter.Write(v.Length);
                    foreach (T s in v)
                    {
                        if (s != null)
                        {
                            binaryWriter.Write(TYPE_OBJECT_START);
                            if (serializedObjects.ContainsKey(s) == false)
                            {
                                if (serializablesTypesFromType.ContainsKey(s.GetType()) == false)
                                    throw new ArgumentException("Type [" + s.GetType() + "] not registered in Serializer");
                                binaryWriter.Write(serializedObjects.Count);
                                serializedObjects[s] = serializedObjects.Count;
                                binaryWriter.Write(serializablesTypesFromType[s.GetType()]);
                                s.Serialize(this);
                                binaryWriter.Write(TYPE_OBJECT_END);
                            }
                            else
                            {
                                binaryWriter.Write(serializedObjects[s]);
                            }
                        }
                        else
                        {
                            binaryWriter.Write(TYPE_NULL);
                        }
                    }
                }
                else
                {
                    binaryWriter.Write(TYPE_NULL);
                    binaryWriter.Write(fieldName);
                }
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                {
                    Dictionary<string, object> oldCurrentObject = currentObject;

                    Dictionary<string, object>[] objects = (Dictionary<string, object>[])currentObject[fieldName];

                    if (objects != null)
                    {
                        v = new T[objects.Length];
                        for (int i = 0; i < objects.Length; i++)
                        {
                            currentObject = objects[i];

                            if (currentObject != null)
                            {
                                int objectId = (int)currentObject[ID_TAG];

                                if (deserializedObjectsReal.ContainsKey(objectId) == false)
                                {
                                    string typeName = (string)currentObject[TYPE_TAG];
                                    if (serializablesTypesFromName.ContainsKey(typeName) == false)
                                        throw new ArgumentException("Type with name '" + typeName + " ' not registered in Serializer");
                                    Type type = serializablesTypesFromName[typeName];
									v[i] = (T) Activator.CreateInstance(type);
                                    //v[i] = (T)type.GetConstructor(Type.EmptyTypes).Invoke(null);
                                    deserializedObjectsReal[objectId] = v[i];
                                    v[i].Serialize(this);
                                }
                                else
                                {
                                    v[i] = (T) deserializedObjectsReal[objectId];
                                }
                            }
                            else
                            {
                                v[i] = default(T);
                            }
                        }

                        currentObject = oldCurrentObject;
                    }
                    else
                    {
                        v = default(T[]);
                    }
                }

            }
        }

        public void Serialize<T>(ref T v, string fieldName) where T : ISerializable
        {
            if (serialization)
            {
                if (v != null)
                {
                    binaryWriter.Write(TYPE_OBJECT_START);
                    binaryWriter.Write(fieldName);
                    if (serializedObjects.ContainsKey(v) == false)
                    {
                        if (serializablesTypesFromType.ContainsKey(v.GetType()) == false)
                            throw new ArgumentException("Type [" + v.GetType() + "] not registered in Serializer");
                        binaryWriter.Write(serializedObjects.Count);
                        serializedObjects[v] = serializedObjects.Count;
                        binaryWriter.Write(serializablesTypesFromType[v.GetType()]);
                        v.Serialize(this);
                        binaryWriter.Write(TYPE_OBJECT_END);
                    }
                    else
                    {
                        binaryWriter.Write(serializedObjects[v]);
                    }
                }
                else
                {
                    binaryWriter.Write(TYPE_NULL);
                    binaryWriter.Write(fieldName);
                }
            }
            else
            {
                if (currentObject.ContainsKey(fieldName))
                {
                    Dictionary<string, object> oldCurrentObject = currentObject;
                    currentObject = (Dictionary<string, object>)currentObject[fieldName];

                    if (currentObject != null)
                    {
                        int objectId = (int)currentObject[ID_TAG];

                        if (deserializedObjectsReal.ContainsKey(objectId) == false)
                        {
                            string typeName = (string)currentObject[TYPE_TAG];
                            if (serializablesTypesFromName.ContainsKey(typeName) == false)
                                throw new ArgumentException("Type with name '" + typeName + " ' not registered in Serializer");
                            Type type = serializablesTypesFromName[typeName];
							v = (T) Activator.CreateInstance(type);
                            //v = (T)type.GetConstructor(Type.EmptyTypes).Invoke(null);
                            deserializedObjectsReal[objectId] = v;
                            v.Serialize(this);
                        }
                        else
                        {
                            v = (T)deserializedObjectsReal[objectId];
                        }
                    }
                    else
                    {
                        v = default(T);
                    }
                    currentObject = oldCurrentObject;
                }
            }
        }

        #endregion
    }
}
