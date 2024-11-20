namespace IdempotentConsumerExample.Db;

[Table("Tbl_Messages")]
public class Tbl_Messages
{
    [Key]
    public string MessageId { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
