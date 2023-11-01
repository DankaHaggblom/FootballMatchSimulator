using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MatchCalculator.DatabaseItemTypes
{
    public class Team
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;
    }
}
