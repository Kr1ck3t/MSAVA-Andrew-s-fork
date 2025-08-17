using System;
using System.Collections.Generic;

namespace M_SAVA_Core.Models
{
    public class AccessCodeDTO
    {
        public Guid Id { get; set; }
        public required Guid OwnerId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public required int MaxUses { get; set; }
        public required Guid AccessGroupId { get; set; }
        public required AccessGroupDTO AccessGroup { get; set; }
        public int UsageCount { get; set; }
    }
}