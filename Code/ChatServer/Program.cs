using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program
{
    // ===== PHẦN 1: SERVER CORE =====
    static Dictionary<string, StreamWriter> clients = new Dictionary<string, StreamWriter>();
    static Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
    static object lockObj = new object();
    const int MAX_CLIENTS = 10;

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 9090);
        server.Start();
        Console.WriteLine($">>> SERVER ONLINE (Max {MAX_CLIENTS} nguoi) <<<");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
        }
    }
    // ===== PHẦN 2: HANDLE CLIENT =====
    static void HandleClient(TcpClient tcpClient)
    {
        string username = "";
        StreamReader reader = new StreamReader(tcpClient.GetStream());
        StreamWriter writer = new StreamWriter(tcpClient.GetStream()) { AutoFlush = true };

        try
        {
            // ===== LOGIN (PHẦN 1) =====
            while (true)
            {
                writer.WriteLine("NHAP_USER");
                username = reader.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(username)) continue;

                lock (lockObj)
                {
                    if (clients.Count >= MAX_CLIENTS) writer.WriteLine("ERR_FULL|Server day!");
                    else if (username.Length > 20) writer.WriteLine("ERR_LENGTH|Ten khong qua 20 ky tu!");
                    else if (clients.ContainsKey(username)) writer.WriteLine("ERR_TRUNG|Ten da co!");
                    else { clients.Add(username, writer); break; }
                }
            }

            writer.WriteLine($"OK_WELCOME|{username}");
            LogServer($"{username} tham gia.", ConsoleColor.Green);
            Broadcast($"[HE THONG]: {username} online.");
            // ===== NHẬN TIN (PHẦN 2) =====
            string message;
            while ((message = reader.ReadLine()) != null)
            {
                if (message.ToLower() == "/exit") break;
                if (message.StartsWith("/")) HandleCommand(username, message, writer);
                else
                {
                    LogServer($"{username}: {message}");
                    Broadcast($"{username}: {message}");
                }
            }
        }
        catch { }
        finally
        {
            lock (lockObj)
            {
                if (!string.IsNullOrEmpty(username))
                {
                    clients.Remove(username);
                    foreach (var g in groups.Values) g.Remove(username);
                    LogServer($"{username} thoat.", ConsoleColor.Yellow);
                    Broadcast($"[HE THONG]: {username} da roi phong.");
                }
            }
            tcpClient.Close();
        }
    }