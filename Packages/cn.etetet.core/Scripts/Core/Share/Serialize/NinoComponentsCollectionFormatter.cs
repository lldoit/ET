using Nino.Core;

namespace ET
{
    public sealed class NinoComponentsCollectionFormatter : NinoFormatter<ComponentsCollection>
    {
        public override void Serialize(ComponentsCollection value, ref Writer writer)
        {
            int count = 0;
            if (value != null)
            {
                foreach ((long _, Entity entity) in value)
                {
                    if (entity.IsSerializeWithParent)
                    {
                        ++count;
                    }
                }
            }

            writer.Write(count);
            if (value == null)
            {
                return;
            }

            foreach ((long _, Entity entity) in value)
            {
                if (entity.IsSerializeWithParent)
                {
                    NinoSerializer.SerializeBoxed(entity, ref writer, typeof(Entity));
                }
            }
        }

        public override void Deserialize(out ComponentsCollection value, ref Reader reader)
        {
            value = null;
            this.DeserializeRef(ref value, ref reader);
        }

        public override void DeserializeRef(ref ComponentsCollection value, ref Reader reader)
        {
            reader.Read(out int count);

            value ??= ComponentsCollection.Create(true);
            value.Clear();

            for (int i = 0; i < count; ++i)
            {
                Entity entity = (Entity)NinoDeserializer.DeserializeBoxed(ref reader, typeof(Entity));
                entity.IsSerializeWithParent = true;
                value.Add(entity.GetLongHashCode(), entity);
            }
        }
    }
}
