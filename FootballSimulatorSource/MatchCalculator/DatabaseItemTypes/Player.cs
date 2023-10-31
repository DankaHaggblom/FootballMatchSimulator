using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MatchCalculator.DatabaseItemTypes
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("TeamId")]
        public string TeamId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("MatchId")]
        public string MatchId { get; set; } = string.Empty;

        [BsonElement("PosX")]
        public float PosX { get; set; } = 0;

        [BsonElement("PosY")]
        public float PosY { get; set; } = 0;
    }
}
