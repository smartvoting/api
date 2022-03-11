using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Dynamo
{
    [DynamoDBTable("agencyInfo")]
    public class AgencyInfo
    {
        // Partition key
        [DynamoDBHashKey("agencyCode")]
        public string AgencyCode { get; set; }
        
        // Sort key
        [DynamoDBRangeKey("docType")]
        public string DocType { get; set; }
        
        [DynamoDBProperty("bodyText")]
        public string? BodyText { get; set; }

        [DynamoDBProperty("dateModified")]
        public string? DateModified { get; set; }
    }
}
