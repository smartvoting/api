/*****************************************************************************************
 *     _________                      __       ____   ____     __  .__                   *
 *    /   _____/ _____ _____ ________/  |_     \   \ /   /____/  |_|__| ____    ____     *
 *    \_____  \ /     \\__  \\_  __ \   __\     \   Y   /  _ \   __\  |/    \  / ___\    *
 *    /        \  Y Y  \/ __ \|  | \/|  |        \     (  <_> )  | |  |   |  \/ /_/  >   *
 *   /_______  /__|_|  (____  /__|   |__|         \___/ \____/|__| |__|___|  /\___  /    *
 *           \/      \/     \/                                             \//_____/     *
 *****************************************************************************************
 *   Project Title: Smart Voting                                                         *
 *   Project Website: https://smartvoting.cc/                                            *
 *   API Url: https://api.smartvoting.cc/                                                *
 *   Project Source Code: https://github.com/smartvoting/                                *
 *****************************************************************************************
 *   Project License: GNU General Public License v3.0                                    *
 *   Project Authors: Stephen Davis, Michael Sirna, Matthew Campbell, Satabdi Sangma     *
 *   George Brown College - Computer Programmer Analyst (T127)                           *
 *   Capstone I & II - September 2021 to April 2022                                      *
 *****************************************************************************************/

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
        public string BlogId { get; set; } = null!;

        [DynamoDBProperty("blogBody")]
        public string? BlogBody { get; set; } = null!;

        [DynamoDBProperty("datePosted")]
        public string? DatePosted { get; set; } = null!;

        [DynamoDBProperty("dateModified")]
        public string? DateModified { get; set; } = null!;

        [DynamoDBProperty("writerId")]
        public int PersonId { get; set; }
    }
}
