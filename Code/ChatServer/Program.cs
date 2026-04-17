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
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {username}: {message}");
                    Broadcast($"{username}: {message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Loi voi user {username}: {ex.Message}");
        }
        finally
        {
            lock (lockObj)
            {
                if (!string.IsNullOrEmpty(username)) clients.Remove(username);
                foreach (var group in groups.Values) group.Remove(username);
            }

            if (!string.IsNullOrEmpty(username))
            {
                string leaveMsg = $"[HE THONG]: {username} da roi phong chat.";
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {leaveMsg}");
                Console.ResetColor();

                Broadcast(leaveMsg);
            }
            tcpClient.Close();
        }
    }
      // ===== PHẦN 3 + 4: COMMAND + GROUP =====
    static void HandleCommand(string sender, string fullCommand, StreamWriter writer)
    {
        string[] parts = fullCommand.Split(' ', 3);
        string cmd = parts[0].ToLower();

        lock (lockObj)
        {
            switch (cmd)
            {
                // ===== PHẦN 3: LỆNH CƠ BẢN =====
                case "/list":
                    writer.WriteLine($"[HE THONG]: Online: {string.Join(", ", clients.Keys)}");
                    break;

                case "/check":
                    if (parts.Length < 2) return;
                    string target = parts[1];

                    // Kiểm tra tồn tại user / group
                    if (target.ToUpper() == "ALL" ||
                        clients.ContainsKey(target) ||
                        groups.ContainsKey(target))
                        writer.WriteLine($"CHECK_OK|{target}");
                    else
                        writer.WriteLine($"CHECK_ERR|Khong tim thay '{target}'!");
                    break;

                case "/send":
                    if (parts.Length < 3) return;
                    string to = parts[1], msg = parts[2];

                    // Gửi nhóm
                    if (groups.ContainsKey(to))
                    {
                        if (groups[to].Contains(sender))
                            GroupBroadcast(to, $"[{to}] {sender}: {msg}");
                        else
                            writer.WriteLine($"[LOI]: Ban phai /join {to} truoc!");
                    }
                    // Gửi riêng
                    else if (clients.ContainsKey(to))
                    {
                        clients[to].WriteLine($"[RIENG tu {sender}]: {msg}");
                        writer.WriteLine($"[RIENG toi {to}]: {msg}");
                    }
                    break;

                // ===== PHẦN 4: GROUP =====
                case "/create":
                    if (parts.Length < 2) return;
                    string gCreate = parts[1];

                    if (!groups.ContainsKey(gCreate))
                    {
                        groups[gCreate] = new List<string> { sender };
                        writer.WriteLine($"[HE THONG]: Da tao nhom '{gCreate}'");
                    }
                    break;

                case "/join":
                    if (parts.Length < 2) return;
                    string gJoin = parts[1];

                    if (groups.ContainsKey(gJoin) && !groups[gJoin].Contains(sender))
                    {
                        groups[gJoin].Add(sender);
                        writer.WriteLine($"[HE THONG]: Da vao nhom '{gJoin}'");
                    }
                    break;

                case "/leave":
                    if (parts.Length < 2) return;
                    string gLeave = parts[1];

                    if (groups.ContainsKey(gLeave))
                    {
                        groups[gLeave].Remove(sender);
                        writer.WriteLine($"[HE THONG]: Da roi nhom '{gLeave}'");
                    }
                    break;
            }
        }
    }

    // Gửi cho tất cả
    static void Broadcast(string msg)
    {
        lock (lockObj)
        {
            foreach (var c in clients.Values)
                try { c.WriteLine(msg); } catch { }
        }
    }

    // Gửi trong nhóm
    static void GroupBroadcast(string gName, string msg)
    {
        foreach (var m in groups[gName])
            if (clients.ContainsKey(m))
                clients[m].WriteLine(msg);
    }

    // Log server
    static void LogServer(string log, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {log}");
        Console.ResetColor();
    }
}
