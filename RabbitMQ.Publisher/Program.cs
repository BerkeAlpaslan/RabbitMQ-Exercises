using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory connectionFactory = new();
connectionFactory.Uri = new("amqps://nqyazcjg:2PTQKDbBt9-Tqh8EctuXFWdzwpkOH_ni@gull.rmq.cloudamqp.com/nqyazcjg");

using IConnection connection =  await connectionFactory.CreateConnectionAsync();
using IChannel channel = await connection.CreateChannelAsync();




#region Header Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "header-exchange", type: ExchangeType.Headers);

for (int i = 0; i < 10; i++)
{
    byte[] message = Encoding.UTF8.GetBytes($"Header Message {i}");
    Console.Write("Enter header value: ");
    string? header_value = Console.ReadLine();
    var properties = new BasicProperties()
    {
        Headers = new Dictionary<string, object>()
        {
            ["header"] = header_value ?? string.Empty
        }
    };

    await channel.BasicPublishAsync(
        exchange: "header-exchange",
        routingKey: string.Empty,
        body: message,
        basicProperties: properties,
        mandatory: false
    );
    await Task.Delay(200);
}
*/
#endregion


#region Topic Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "topic-exchange", type: ExchangeType.Topic);

for (int i = 0; i < 10; i++)
{
    byte[] message = Encoding.UTF8.GetBytes($"Topic Message {i}");
    Console.Write("Select Topic: ");
    string? topic = Console.ReadLine();
    await channel.BasicPublishAsync(exchange: "topic-exchange", routingKey: topic ?? string.Empty, body: message);
}
*/
#endregion


#region Fanout Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "fanout-exchange", type: ExchangeType.Fanout);

for (int i = 0; i < 10; i++)
{
    byte[] message = Encoding.UTF8.GetBytes($"Fanout Message {i}");
    await channel.BasicPublishAsync(exchange: "fanout-example", routingKey: string.Empty, body: message);
    await Task.Delay(200);
}*/
#endregion


#region Direct Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "direct-exchange", type: ExchangeType.Direct);

while (true)
{
    Console.Write("Direct Message: ");
    string? direct_message = Console.ReadLine();
    byte[] message = Encoding.UTF8.GetBytes($"Direct Message: {direct_message}");

    await channel.BasicPublishAsync(exchange: "direct-exchange", routingKey: "direct-queue", body: message);
}
*/
#endregion


#region Basic Publisher
/*
await channel.QueueDeclareAsync(queue: "example-queue", exclusive: false); 

byte[] message = Encoding.UTF8.GetBytes("Hello RabbitMQ!");
await channel.BasicPublishAsync(exchange: "", routingKey: "example-queue", body: message);
Console.Read();
*/
#endregion


#region Round Robin
/*
await channel.QueueDeclareAsync(queue: "example-queue", exclusive: false);
for (int i = 1; i <= 100; i++)
{
    byte[] message = Encoding.UTF8.GetBytes($"Message {i}");
    await channel.BasicPublishAsync(exchange: "", routingKey: "example-queue", message);
    await Task.Delay(500);
}
Console.Read();
*/
#endregion




// Message Broker Design Patterns
#region P2P (Point-to-Point)
/*
string queueName = "p2p-queue";
await channel.QueueDeclareAsync(
    queue: queueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

byte[] p2pMessage = Encoding.UTF8.GetBytes("Hello P2P!");
await channel.BasicPublishAsync(
    exchange: string.Empty,
    routingKey: queueName,
    body: p2pMessage);
*/
#endregion


#region Publish/Subscribe
/*
string exchangeName = "pubsub-exchange";
await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: ExchangeType.Fanout);

byte[] pubsubMessage = Encoding.UTF8.GetBytes("Hello Pub/Sub!");
await channel.BasicPublishAsync(
    exchange: exchangeName,
    routingKey: string.Empty,
    body: pubsubMessage);
*/
#endregion


#region Work Queue
/*
string queueName = "work-queue";
await channel.QueueDeclareAsync(
    queue: queueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

for (int i = 0; i < 100; i++)
{
    byte[] message = Encoding.UTF8.GetBytes($"Hello Work! {i}");
    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: queueName,
        body: message);
    await Task.Delay(100);
}
*/
#endregion


#region Request/Response
string requestQueueName = "request-queue";
await channel.QueueDeclareAsync(
    queue: requestQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

string responseQueueName = "response-queue";
await channel.QueueDeclareAsync(
    queue: responseQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

string correlationId = Guid.NewGuid().ToString();

#region Request
BasicProperties requestProperties = new()
{
    CorrelationId = correlationId,
    ReplyTo = responseQueueName
};

for (int i = 0; i < 100; i++)
{
    byte[] requestMessage = Encoding.UTF8.GetBytes($"Request {i}");
    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: requestQueueName,
        body: requestMessage,
        basicProperties: requestProperties,
        mandatory: false);
    await Task.Delay(200);
}
#endregion

#region Response
AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(
    queue: responseQueueName,
    autoAck: true,
    consumer: consumer);

consumer.ReceivedAsync += async (sender, e) =>
{
    if (e.BasicProperties.CorrelationId == correlationId)
    {
        string responseMessage = Encoding.UTF8.GetString(e.Body.Span);
        Console.WriteLine($"Received Response: {responseMessage}");
    }
    await Task.CompletedTask;
};
#endregion
#endregion




Console.Read();