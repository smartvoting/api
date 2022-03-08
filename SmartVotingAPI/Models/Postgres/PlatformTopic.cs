using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PlatformTopic
    {
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = null!;
    }
}
