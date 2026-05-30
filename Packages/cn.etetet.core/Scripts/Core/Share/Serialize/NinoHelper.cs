using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Nino.Core;

namespace ET
{
    public static class NinoHelper
    {
        [StaticField]
        private static readonly object InitLock = new();
        [StaticField]
        private static readonly HashSet<string> InitializedAssemblies = new();

        public static void Init()
        {
            lock (InitLock)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = assembly.GetName().Name;
                    if (assemblyName == null || !InitializedAssemblies.Add(assemblyName))
                    {
                        continue;
                    }

                    InvokeNinoInit(assembly, $"{assemblyName}.NinoGen.Serializer");
                    InvokeNinoInit(assembly, $"{assemblyName}.NinoGen.Deserializer");
                    InvokeNinoInit(assembly, $"{assemblyName}.NinoGen.NinoBuiltInTypesRegistration");
                }
            }
        }

        private static void InvokeNinoInit(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName);
            MethodInfo methodInfo = type?.GetMethod("Init", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            methodInfo?.Invoke(null, null);
        }

        public static byte[] Serialize(object message)
        {
            Init();
            if (message is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }
            return NinoSerializer.Serialize(message);
        }

        public static void Serialize(object message, MemoryBuffer stream)
        {
            Init();
            if (message is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }
            NinoSerializer.Serialize(message, stream);
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            Init();
            object o = NinoDeserializer.Deserialize(bytes.AsSpan(index, count), type);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count, ref object o)
        {
            Init();
            NinoDeserializer.Deserialize(bytes.AsSpan(index, count), type, ref o);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static object Deserialize(Type type, MemoryBuffer stream)
        {
            Init();
            object o = NinoDeserializer.Deserialize(stream.GetSpan(), type);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static object Deserialize(Type type, MemoryBuffer stream, ref object o)
        {
            Init();
            NinoDeserializer.Deserialize(stream.GetSpan(), type, ref o);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}
