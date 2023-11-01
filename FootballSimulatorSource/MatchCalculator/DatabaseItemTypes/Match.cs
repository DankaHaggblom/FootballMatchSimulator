using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatchCalculator.DatabaseItemTypes
{
    public class Match
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("ScoreHome")]
        public int ScoreHome { get; set; } = 0;

        [BsonElement("ScoreAway")]
        public int ScoreAway { get; set; } = 0;

        [BsonElement("TimeLeft")]
        public int TimeLeft { get; set; } = 20 * 60;

        [BsonElement("BallPosX")]
        public float BallPosX { get; set; } = 0;

        [BsonElement("BallPosY")]
        public float BallPosY { get; set; } = 0;

        [BsonElement("BallPossessionPlayerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? BallPossessionPlayerId { get; set; } = null;

        [BsonElement("HomeTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? HomeTeamId { get; set; } = null;

        [BsonElement("AwayTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? AwayTeamId { get; set; } = null;
    }
}
