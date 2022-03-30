using Amazon.DynamoDBv2.DataModel;

namespace SmartVotingAPI.Models.Dynamo
{
    [DynamoDBTable("partyBlogs")]
    public class PartyBlog
    {
        // Partition key
        [DynamoDBHashKey("partyId")]
        public int PartyId { get; set; }

        // Sort key
        [DynamoDBRangeKey("blogId")]
        public string BlogId { get; set; }

        [DynamoDBProperty("blogBody")]
        public string? BlogBody { get; set; }

        [DynamoDBProperty("datePosted")]
        public string? DatePosted { get; set; }

        [DynamoDBProperty("dateModified")]
        public string? DateModified { get; set; }

        [DynamoDBProperty("writerId")]
        public int PersonId { get; set; }
    }
}
