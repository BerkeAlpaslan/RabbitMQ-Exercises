using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory connectionFactory = new();
connectionFactory.Uri = new("amqps://nqyazcjg:2PTQKDbBt9-Tqh8EctuXFWdzwpkOH_ni@gull.rmq.cloudamqp.com/nqyazcjg");

using IConnection connection = await connectionFactory.CreateConnectionAsync();
using IChannel channel = await connection.CreateChannelAsync();




#region Header Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "header-exchange", type: ExchangeType.Headers);
Console.Write("Enter header value: ");
string? header_value = Console.ReadLine();

var queue = await channel.QueueDeclareAsync();
string queue_name = queue.QueueName;

await channel.QueueBindAsync(
    queue: queue_name,
    exchange: "header-exchange",
    routingKey: string.Empty,
    arguments: new Dictionary<string, object>
{
    ["x-match"] = "all",
    ["header"] = header_value ?? string.Empty
});

AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(
    queue: queue,
    autoAck: true,
    consumer: consumer);

consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
};
*/
#endregion


#region Topic Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "topic-exchange", type: ExchangeType.Topic);
Console.Write("Select Topic: ");
string? topic = Console.ReadLine();
var queue_declare = await channel.QueueDeclareAsync();
string queue_name = queue_declare.QueueName;
await channel.QueueBindAsync(queue: queue_name, exchange: "topic-exchange", routingKey: topic ?? string.Empty);

AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(queue: queue_name, autoAck: true, consumer: consumer);
consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await Task.CompletedTask;
};
*/
#endregion


#region Fanout Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "fanout-exchange", type: ExchangeType.Fanout);
Console.Write("Queue Name: ");
string? queue_name = Console.ReadLine();
await channel.QueueDeclareAsync(queue: queue_name, exclusive: false);
await channel.QueueBindAsync(queue: queue_name, exchange: "fanout-exchange", routingKey: string.Empty);

AsyncEventingBasicConsumer consumer = new(channel);
consumer.ReceivedAsync += async (sender, e) =>
await channel.BasicConsumeAsync(queue: queue_name, autoAck: true, consumer: consumer);
consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await Task.CompletedTask;
};
*/
#endregion


#region Direct Exchange
/*
await channel.ExchangeDeclareAsync(exchange: "direct-exchange", type: ExchangeType.Direct);
var queue_declare = await channel.QueueDeclareAsync();
string queue_name = queue_declare.QueueName;
await channel.QueueBindAsync(queue: queue_name, exchange: "direct-exchange", routingKey: "direct-queue");

AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(queue: queue_name, autoAck: true, consumer: consumer);
consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await Task.CompletedTask;

};
*/
#endregion


#region Basic Consumer
/*
await channel.QueueDeclareAsync(queue: "example-queue", exclusive: false);

AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(queue: "example-queue", autoAck: false, consumer);
consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await Task.CompletedTask;
};
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
AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(queue: "p2p-queue",
    autoAck: false,
    consumer: consumer);

consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await Task.CompletedTask;
};
*/
#endregion


#region Publish/Subscribe
/*
string exchangeName = "pubsub-exchange";
await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: ExchangeType.Fanout);

var queue = await channel.QueueDeclareAsync();
string queueName = queue.QueueName;

await channel.QueueBindAsync(
    queue: queueName,
    exchange: exchangeName,
    routingKey: string.Empty);

AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: false,
    consumer: consumer);

consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await channel.BasicAckAsync(deliveryTag: e.DeliveryTag, multiple: false);
};
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

AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: true,
    consumer: consumer);

await channel.BasicQosAsync(
    prefetchCount: 1,
    prefetchSize: 0,
    global: false);

consumer.ReceivedAsync += async (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
    await Task.CompletedTask;
};
*/
#endregion


#region Request/Response
string requestQueueName = "request-queue";
await channel.QueueDeclareAsync(
    queue: requestQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

AsyncEventingBasicConsumer consumer = new(channel);
consumer.ReceivedAsync += async (sender, e) =>
{
    byte[] responseMessage = Encoding.UTF8.GetBytes($"Response to: {Encoding.UTF8.GetString(e.Body.Span)}");
    BasicProperties responseProperties = new()
    {
        CorrelationId = e.BasicProperties.CorrelationId,
        ReplyTo = e.BasicProperties.ReplyTo
    };

    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: e.BasicProperties.ReplyTo ?? string.Empty,
        body: responseMessage,
        basicProperties: responseProperties,
        mandatory: false);
};
#endregion




Console.Read();