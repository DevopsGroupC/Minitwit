
namespace csharp_minitwit.Utils

{
    public static class MessageHelper
    {
        public static List<MessageModel> MessageConverter(IEnumerable<dynamic>? queryResult){
        
        var messages = queryResult.Select(row =>
                    {
                        var dict = (IDictionary<string, object>)row;
                        return new MessageModel
                        {
                            MessageId = (long)dict["message_id"],
                            AuthorId = (long)dict["author_id"],
                            Text = (string)dict["text"],
                            PubDate = (long)dict["pub_date"],
                            Flagged = (long)dict["flagged"],
                            Username = (string)dict["username"],
                            Email = (string)dict["email"],
                        };
                    }).ToList();
        
        return messages;

        }
    }
}