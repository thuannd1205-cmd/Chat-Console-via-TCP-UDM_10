
using System;
using System.Net;
using System.Net.Sockets;
using ChatClient;

class Program
{
    static void Main()
    {
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(IPAddress.Parse("127.0.0.1"), 5000);

        Console.Write("Nhập username: ");
        string username = Console.ReadLine();

        ClientLogic logic = new ClientLogic(client, username);

        // Gửi login
        logic.Send($"LOGIN|{username}||");

        // Bắt đầu luồng nhận
        logic.Start();

        // Luồng gửi (main thread)
        while (true)
        {
            string input = Console.ReadLine();
            if (input.ToLower() == "exit")
            {
                logic.Exit();
                break;
            }

            if (input.StartsWith("@"))
            {
                string[] parts = input.Split(' ');
                string receiver = parts[0].Substring(1);
                string message = input.Substring(parts[0].Length + 1);

                logic.Send($"MSG|{username}|{receiver}|{message}");
            }
            else
            {
                logic.Send($"ALL|{username}||{input}");
            }
        }
    }
}
