using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Mime;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

var connection = await factory.CreateConnectionAsync();
connection.ConnectionShutdownAsync += Connection_ConnectionShutdownAsync;

var options = new CreateChannelOptions(false, false);
var json = "{ \"A\" : \"X\"}";
var body = Encoding.UTF8.GetBytes(json);
var bp = new BasicProperties
{
    ContentType = MediaTypeNames.Application.Json,
    ContentEncoding = "UTF-8",
};

IChannel channel = await connection.CreateChannelAsync(options);
channel.ChannelShutdownAsync += Channel_ChannelShutdownAsync;

while (true)
{
    try
    {
        await channel.BasicPublishAsync("NotExistsExchange", String.Empty, true, bp, body);
        Console.WriteLine("after BasicPublishAsync");
    }
    catch (AlreadyClosedException)
    {
        Console.WriteLine("caught AlreadyClosedException");
    }

    if (channel.IsClosed)
    {
        Console.WriteLine("channel.IsClosed is true");
        channel = await connection.CreateChannelAsync(options);  // The error occurs here
        Console.WriteLine("channel recreated successfully");
        channel.ChannelShutdownAsync += Channel_ChannelShutdownAsync;
    }
}

static Task Connection_ConnectionShutdownAsync(object sender, ShutdownEventArgs @event)
{
    Console.WriteLine("Connection shutdown: " + @event.ReplyCode);
    return Task.CompletedTask;
}

static Task Channel_ChannelShutdownAsync(object sender, ShutdownEventArgs @event)
{
    Console.WriteLine("Channel shutdown: " + @event.ReplyCode);
    return Task.CompletedTask;
}
