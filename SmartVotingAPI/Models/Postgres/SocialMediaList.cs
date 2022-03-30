using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class SocialMediaList
    {
        public Guid SocialId { get; set; }
        public string? TwitterId { get; set; }
        public string? InstagramId { get; set; }
        public string? FacebookId { get; set; }
        public string? YoutubeId { get; set; }
        public string? SnapchatId { get; set; }
        public string? FlickrId { get; set; }
        public string? TiktokId { get; set; }
        public DateTime Updated { get; set; }
        // Type ID 1 for parties and 2 for candidates
        public int TypeId { get; set; }
    }
}
