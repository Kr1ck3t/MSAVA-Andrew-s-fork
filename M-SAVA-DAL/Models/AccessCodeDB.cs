using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class AccessCodeDB : IIdentifiableDB
{
    public Guid Id { get; set; }
    public required Guid OwnerId { get; set; }
    public UserDB Owner { get; set; } = null!;
    public required DateTime CreatedAt { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required int MaxUses { get; set; }
    public required Guid AccessGroupId { get; set; }
    public required AccessGroupDB AccessGroup { get; set; } = null!;
}

}
