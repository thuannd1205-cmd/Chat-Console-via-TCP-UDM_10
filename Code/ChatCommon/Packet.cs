using System;

namespace ChatCommon
{
    public enum CommandType
    {
        Login,
        Message,
        Private,
        Disconnect,
        Error
    }

    public class Packet
    {
        public CommandType Command { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public Packet() { }

        public Packet(CommandType cmd, string sender, string content, string receiver = "All")
        {
            Command = cmd;
            Sender = sender;
            Content = content;
            Receiver = receiver;
        }
    }
}