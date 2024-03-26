using System;
using System.Collections.Generic;

namespace csharp_minitwit;

public partial class Message
{
    public int MessageId { get; set; }

    public int AuthorId { get; set; }

    public string Text { get; set; } = null!;

    public int PubDate { get; set; }

    public int Flagged { get; set; }

    public User Author { get; set; } = null!;
}