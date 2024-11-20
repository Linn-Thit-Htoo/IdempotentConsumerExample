namespace IdempotentConsumerExample.Config;

public class RabbitMqConfiguration
{
    public string? HostName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int RetriesCount { get; set; }
    public IList<Queues>? QueueList { get; set; }
}

public class Queues
{
    public string? Exchange { get; set; }
    public string? Queue { get; set; }
    public string? RoutingKey { get; set; }
}
