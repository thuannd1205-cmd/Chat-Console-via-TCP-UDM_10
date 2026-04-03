using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks; // Cần thêm cái này để dùng Task

namespace MyApp
{
    internal class Program
    {
        static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 9090);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);
            Console.WriteLine("Server ĐA LUỒNG đang chạy tại port 9090...");

            while (true)
            {
                // 1. Đợi người kết nối
                Socket client = serverSocket.Accept();
                Console.WriteLine("Co ket noi moi tu: " + client.RemoteEndPoint);

                // 2. Chạy một luồng riêng để xử lý client này, không làm nghẽn vòng lặp Accept
                Task.Run(() => HandleClient(client));
            }
        }

        // Hàm xử lý riêng cho từng Client
        static void HandleClient(Socket client)
        {
            byte[] buffer = new byte[1024];
            string username = "";

            try
            {
                // Giai đoạn đăng ký Username
                while (true)
                {
                    client.Send(Encoding.ASCII.GetBytes("NHAP_USER"));
                    int rec = client.Receive(buffer);
                    if (rec == 0) return;

                    username = Encoding.ASCII.GetString(buffer, 0, rec).Trim();

                    if (clients.ContainsKey(username))
                    {
                        client.Send(Encoding.ASCII.GetBytes("TRUNG_TEN"));
                    }
                    else
                    {
                        clients.Add(username, client);
                        client.Send(Encoding.ASCII.GetBytes("OK_WELCOME"));
                        Console.WriteLine($"[SYSTEM] {username} da gia nhap.");
                        break;
                    }
                }

                // Giai đoạn Chat
                while (true)
                {
                    int rec = client.Receive(buffer);
                    if (rec == 0) break;

                    string msg = Encoding.ASCII.GetString(buffer, 0, rec).Trim();
                    Console.WriteLine($"[{username}]: {msg}");

                    if (msg.ToUpper() == "EXIT") break;

                    // Phản hồi lại cho chính người gửi
                    client.Send(Encoding.ASCII.GetBytes("Server: " + msg.ToUpper()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi voi {username}: {ex.Message}");
            }
            finally
            {
                // Dọn dẹp khi thoát
                if (!string.IsNullOrEmpty(username)) clients.Remove(username);
                client.Close();
                Console.WriteLine($"[SYSTEM] {username} da thoat.");
            }
        }
    }
}