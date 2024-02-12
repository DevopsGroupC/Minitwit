public class MessageModel
{
    public int MessageId { get; set; }
    public int AuthorId { get; set; }
    public string? Text { get; set; } // Nullable string
    public long PubDate { get; set; }
    public int Flagged { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; } // Nullable string
    public string? Email { get; set; } // Nullable string
    public string? PwHash { get; set; } // Nullable string
}
