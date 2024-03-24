public class MessageDTO
{
    public long MessageId { get; set; }
    public long AuthorId { get; set; }
    public string? Text { get; set; }
    public int? PubDate { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
}