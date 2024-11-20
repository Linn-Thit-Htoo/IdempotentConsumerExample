namespace IdempotentConsumerExample.Models
{
    public class BlogRequestModel
    {
        public string MessageId { get; set; }
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogAuthor { get; set; }
        public string BlogContent { get; set; }
    }
}
