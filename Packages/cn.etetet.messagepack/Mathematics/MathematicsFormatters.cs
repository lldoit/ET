using System;
using MessagePack.Formatters;

namespace MessagePack.Unity
{
    public sealed class MathematicsFloat3Formatter : IMessagePackFormatter<global::Unity.Mathematics.float3>
    {
        public void Serialize(ref MessagePackWriter writer, global::Unity.Mathematics.float3 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public global::Unity.Mathematics.float3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            int length = reader.ReadArrayHeader();
            float x = default;
            float y = default;
            float z = default;
            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        x = reader.ReadSingle();
                        break;
                    case 1:
                        y = reader.ReadSingle();
                        break;
                    case 2:
                        z = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new global::Unity.Mathematics.float3(x, y, z);
        }
    }

    public sealed class MathematicsQuaternionFormatter : IMessagePackFormatter<global::Unity.Mathematics.quaternion>
    {
        public void Serialize(ref MessagePackWriter writer, global::Unity.Mathematics.quaternion value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.value.x);
            writer.Write(value.value.y);
            writer.Write(value.value.z);
            writer.Write(value.value.w);
        }

        public global::Unity.Mathematics.quaternion Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            int length = reader.ReadArrayHeader();
            float x = default;
            float y = default;
            float z = default;
            float w = default;
            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        x = reader.ReadSingle();
                        break;
                    case 1:
                        y = reader.ReadSingle();
                        break;
                    case 2:
                        z = reader.ReadSingle();
                        break;
                    case 3:
                        w = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new global::Unity.Mathematics.quaternion(x, y, z, w);
        }
    }
}
