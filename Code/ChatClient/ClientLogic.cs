using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClientLogic
{
    private Socket client;
    private string username;
    private bool isRunning = true;

    public ClientLogic(Socket socket, string user)
    {
        client = socket;
        username = user;
    }

    // Bắt đầu luồng nhận
    public void Start()
    {
        Thread receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // Luồng nhận dữ liệu từ Server
    private void ReceiveData()
    {
        byte[] buffer = new byte[1024];

        while (isRunning)
        {
            try
            {
                int received = client.Receive(buffer);

                if (received == 0)
                {
                    Console.WriteLine("Server đã đóng kết nối!");
                    break;
                }

                string msg = Encoding.UTF8.GetString(buffer, 0, received);
                HandleMessage(msg);
            }
            catch
            {
                Console.WriteLine("Mất kết nối tới Server!");
                isRunning = false;
                break;
            }
        }

        client.Close();
    }

    // Xử lý message theo protocol
    private void HandleMessage(string msg)
    {
        /*FORMAT: COMMAND|SENDER|RECEIVER|CONTENT */

        string[] parts = msg.Split('|');

        if (parts.Length < 4) return;

        string command = parts[0];
        string sender = parts[1];
        string content = parts[3];

        string time = DateTime.Now.ToString("HH:mm:ss");

        switch (command)
        {
            case "MSG":
                Console.WriteLine($"[{time}]{sender}: {content}");
                break;

            case "ALL":
                Console.WriteLine($"[{time}]{sender} (ALL): {content}");
                break;

            case "OK":
                Console.WriteLine($"[{time}]{content}");
                break;

            case "ERROR":
                Console.WriteLine($"[{time}]{content}");
                break;

            default:
                Console.WriteLine($"[{time}]{msg}");
                break;
        }
    }

    // Gửi dữ liệu
    public void Send(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data);
        }
        catch
        {
            Console.WriteLine("Không gửi được dữ liệu!");
        }
    }
}
