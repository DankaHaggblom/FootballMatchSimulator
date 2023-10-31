using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MatchCalculator.DatabaseItemTypes
{
    public class Team
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("MatchId")]
        public string MatchId { get; set; } = string.Empty;

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("AttackingTeam")]
        public bool AttackingTeam { get; set; } = false;
    }
}
