public class MessageModel
{
    public long MessageId { get; set; }
    public long AuthorId { get; set; }
    public string? Text { get; set; }
    public long PubDate { get; set; }
    public long Flagged { get; set; }
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PwHash { get; set; }
}
