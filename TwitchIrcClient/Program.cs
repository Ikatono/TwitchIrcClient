using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Threading.Channels;
using TwitchIrcClient.IRC;
using TwitchIrcClient.IRC.Messages;

RateLimiter limiter = new(20, 30);
bool ssl = true;
async Task<IrcConnection> CreateConnection(string channel)
{
    IrcConnection connection;
    if (ssl)
        connection = new IrcConnection("irc.chat.twitch.tv", 6697, limiter, true, true);
    else
        connection = new IrcConnection("irc.chat.twitch.tv", 6667, limiter, true, false);
    connection.AddCallback(new MessageCallbackItem(
        (o, m) =>
        {
            if (m is Privmsg priv)
            {
                if (priv.Bits > 0)
                    lock (Console.Out)
                        Console.WriteLine($"{priv.DisplayName}: {priv.Bits}{Environment.NewLine}");
            }
            else
                throw new ArgumentException("Received an unrequested message type", nameof(m));
        }, [IrcMessageType.PRIVMSG]));
    connection.onUserChange += (object? o, UserChangeEventArgs args) =>
    {
        lock (Console.Out)
        {
            var resetColor = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(string.Join(", ", args.Joined.Order()));
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(string.Join(", ", args.Left.Order()));
            Console.BackgroundColor = resetColor;
            Console.WriteLine();
        }
    };
    if (!await connection.ConnectAsync())
    {
        Console.WriteLine("failed to connect");
        Environment.Exit(-1);
    }
    connection.Authenticate(null, null);
    connection.SendLine("CAP REQ :twitch.tv/commands twitch.tv/membership twitch.tv/tags");
    connection.JoinChannel(channel);
    return connection;
}
Console.Write("Channel: ");
var channelName = Console.ReadLine();
ArgumentNullException.ThrowIfNull(channelName, nameof(Channel));
var connection = CreateConnection(channelName);
while (true)
{
    //all the work happens in other threads
    //specifically the threadpool used by Task.Run for
    //the tasks owned by the IrcConnection
    await Task.Delay(1000);
}
