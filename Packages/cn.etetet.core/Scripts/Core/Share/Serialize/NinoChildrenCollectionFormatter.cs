using Nino.Core;

namespace ET
{
    public sealed class NinoChildrenCollectionFormatter : NinoFormatter<ChildrenCollection>
    {
        public override void Serialize(ChildrenCollection value, ref Writer writer)
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

        public override void Deserialize(out ChildrenCollection value, ref Reader reader)
        {
            value = null;
            this.DeserializeRef(ref value, ref reader);
        }

        public override void DeserializeRef(ref ChildrenCollection value, ref Reader reader)
        {
            reader.Read(out int count);

            value ??= ChildrenCollection.Create(true);
            value.Clear();

            for (int i = 0; i < count; ++i)
            {
                Entity entity = (Entity)NinoDeserializer.DeserializeBoxed(ref reader, typeof(Entity));
                entity.IsSerializeWithParent = true;
                value.Add(entity.Id, entity);
            }
        }
    }
}
