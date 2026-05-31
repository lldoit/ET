using System;
using System.Collections.Generic;
using MessagePack.Formatters;

namespace MessagePack.Unity
{
    public sealed class MathematicsResolver : IFormatterResolver
    {
        public static readonly MathematicsResolver Instance = new MathematicsResolver();

        private MathematicsResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter =
                    (IMessagePackFormatter<T>)MathematicsResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }

    internal static class MathematicsResolverGetFormatterHelper
    {
        private static readonly Dictionary<Type, object> FormatterMap = new Dictionary<Type, object>
        {
            { typeof(global::Unity.Mathematics.float3), new MathematicsFloat3Formatter() },
            { typeof(global::Unity.Mathematics.quaternion), new MathematicsQuaternionFormatter() },
            { typeof(global::Unity.Mathematics.float3?), new StaticNullableFormatter<global::Unity.Mathematics.float3>(new MathematicsFloat3Formatter()) },
            { typeof(global::Unity.Mathematics.quaternion?), new StaticNullableFormatter<global::Unity.Mathematics.quaternion>(new MathematicsQuaternionFormatter()) },
            { typeof(global::Unity.Mathematics.float3[]), new ArrayFormatter<global::Unity.Mathematics.float3>() },
            { typeof(global::Unity.Mathematics.quaternion[]), new ArrayFormatter<global::Unity.Mathematics.quaternion>() },
            { typeof(global::Unity.Mathematics.float3?[]), new ArrayFormatter<global::Unity.Mathematics.float3?>() },
            { typeof(global::Unity.Mathematics.quaternion?[]), new ArrayFormatter<global::Unity.Mathematics.quaternion?>() },
            { typeof(List<global::Unity.Mathematics.float3>), new ListFormatter<global::Unity.Mathematics.float3>() },
            { typeof(List<global::Unity.Mathematics.quaternion>), new ListFormatter<global::Unity.Mathematics.quaternion>() },
            { typeof(List<global::Unity.Mathematics.float3?>), new ListFormatter<global::Unity.Mathematics.float3?>() },
            { typeof(List<global::Unity.Mathematics.quaternion?>), new ListFormatter<global::Unity.Mathematics.quaternion?>() },
        };

        internal static object GetFormatter(Type t)
        {
            FormatterMap.TryGetValue(t, out object formatter);
            return formatter;
        }
    }
}
