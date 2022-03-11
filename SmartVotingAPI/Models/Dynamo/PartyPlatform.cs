using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Dynamo
{
    [DynamoDBTable("partyPlatforms")]
    public class PartyPlatform
    {
        [DynamoDBHashKey("partyId")]
        public int PartyId { get; set; }

        [DynamoDBRangeKey("topicId")]
        public int TopicId { get; set; }

        [DynamoDBProperty("dateModified")]
        public string? DateModified { get; set; }

        [DynamoDBProperty("topicBody")]
        public string? TopicBody { get; set; }
    }
}
