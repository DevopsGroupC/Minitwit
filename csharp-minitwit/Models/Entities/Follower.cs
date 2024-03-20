using System;
using System.Collections.Generic;

namespace csharp_minitwit;

public partial class Follower
{
    public int FollowerId { get; set; }

    public int WhoId { get; set; }

    public int WhomId { get; set; }

    public User Who { get; set; } = null!;
    public User Whom { get; set; } = null!;
}
