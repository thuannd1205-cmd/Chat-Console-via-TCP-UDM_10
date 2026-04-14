using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Program
{
    static string myName = "";
    static string currentTarget = "ALL"; // Mặc định nhắn cho tất cả

    static void Main()
    {
        try
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 9090);
            StreamReader reader = new StreamReader(tcpClient.GetStream());
            StreamWriter writer = new StreamWriter(tcpClient.GetStream()) { AutoFlush = true };

            while (true)
            {
                string msg = reader.ReadLine();
                if (msg == "NHAP_USER")
                {
                    Console.Write("Nhập tên bạn: ");
                    writer.WriteLine(Console.ReadLine());
                }
                else if (msg.StartsWith("OK_WELCOME"))
                {
                    myName = msg.Split('|')[1];
                    Console.Clear();
                    Console.WriteLine($"=== CHÀO {myName.ToUpper()}! ===");
                    Console.WriteLine("LỆNH: /to <tên> (để ghim người nhận), /create <nhóm>, /join <nhóm>, /list");
                    break;
                }
            }

            new Thread(() => {
                try
                {
                    string s;
                    while ((s = reader.ReadLine()) != null)
                    {
                        Console.WriteLine("\n" + s);
                        Console.Write($"[{currentTarget}] > ");
                    }
                }
                catch { }
            }).Start();

            while (true)
            {
                Console.Write($"[{currentTarget}] > ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;

                if (input.StartsWith("/to "))
                {
                    currentTarget = input.Substring(4).Trim();
                    Console.WriteLine($">>> Đã chuyển chế độ nhắn cho: {currentTarget}");
                }
                else if (input.StartsWith("/"))
                {
                    writer.WriteLine(input); // Gửi lệnh trực tiếp (list, create, join...)
                }
                else
                {
                    // Nếu đang ghim một ai đó hoặc nhóm, tự động chuyển thành lệnh /send
                    if (currentTarget == "ALL") writer.WriteLine(input);
                    else writer.WriteLine($"/send {currentTarget} {input}");
                }
            }
        }
        catch (Exception ex) { Console.WriteLine("Lỗi: " + ex.Message); }
    }
}
