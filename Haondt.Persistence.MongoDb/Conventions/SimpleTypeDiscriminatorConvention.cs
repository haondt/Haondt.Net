using Haondt.Identity.StorageKey;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Persistence.MongoDb.Discriminators
{
    public class SimpleTypeDiscriminatorConvention : IDiscriminatorConvention, IClassMapConvention, IConvention
    {
        public string ElementName => "_t";

        public string Name => nameof(SimpleTypeDiscriminatorConvention);

        private ObjectDiscriminatorConvention _default = new ObjectDiscriminatorConvention("_t");

        public Type GetActualType(IBsonReader bsonReader, Type nominalType)
        {
            if (bsonReader.GetCurrentBsonType() != BsonType.Document)
                return _default.GetActualType(bsonReader, nominalType);

            var bookmark = bsonReader.GetBookmark();
            bsonReader.ReadStartDocument();
            var actualType = nominalType;
            if (bsonReader.FindElement("_t"))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);
                var discriminator = BsonValueSerializer.Instance.Deserialize(context);
                if (discriminator.IsBsonArray)
                {
                    discriminator = discriminator.AsBsonArray.Last(); // last item is leaf class discriminator
                }

                actualType = SimpleTypeConverter.StringToType(discriminator.AsString);
            }
            bsonReader.ReturnToBookmark(bookmark);
            return actualType;

        }

        public BsonValue GetDiscriminator(Type nominalType, Type actualType)
        {
            return SimpleTypeConverter.TypeToString(actualType);
        }

        public void Apply(BsonClassMap classMap)
        {
            classMap.SetDiscriminator(SimpleTypeConverter.TypeToString(classMap.ClassType));
        }

    }
}
