using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static string currentTarget = "ALL";
    static StringBuilder inputBuffer = new StringBuilder();
    static object consoleLock = new object(); 
    static bool isRunning = true;

    static void Main()
    {
        try
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 9090);
            StreamReader reader = new StreamReader(tcpClient.GetStream());
            StreamWriter writer = new StreamWriter(tcpClient.GetStream()) { AutoFlush = true };

            // 1. Đăng nhập
            while (isRunning)
            {
                string msg = reader.ReadLine();
                if (msg == "NHAP_USER")
                {
                    Console.Write("Nhap Ten (MAX 20): ");
                    writer.WriteLine(Console.ReadLine());
                }
                else if (msg.StartsWith("ERR_")) Console.WriteLine("!!! " + msg.Split('|')[1]);
                else if (msg.StartsWith("OK_WELCOME"))
                {
                    Console.Clear();
                    Console.WriteLine($"=== DA KET NOI: {msg.Split('|')[1].ToUpper()} ===");
                    ShowGuide();
                    break;
                }
            }

            // 2. Luồng nhận tin nhắn (Xử lý chống nhảy dòng ở đây)
            new Thread(() => {
                try
                {
                    string s;
                    while (isRunning && (s = reader.ReadLine()) != null)
                    {
                        if (s == "BYE") break;

                        lock (consoleLock)
                        {
                            ClearCurrentLine();

                            if (s.StartsWith("CHECK_OK|")) currentTarget = s.Split('|')[1];
                            else if (s.StartsWith("CHECK_ERR|")) Console.WriteLine("\n!!! " + s.Split('|')[1]);
                            else Console.WriteLine(s);

                            PrintPrompt();
                        }
                    }
                }
                catch { }
            }).Start();

            // 3. Vòng lặp nhập liệu chính
            while (isRunning)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    lock (consoleLock)
                    {
                        if (key.Key == ConsoleKey.Enter)
                        {
                            string input = inputBuffer.ToString().Trim();
                            inputBuffer.Clear();
                            Console.WriteLine(); 

                            if (input.ToLower() == "/exit")
                            {
                                writer.WriteLine("/exit");
                                isRunning = false;
                                break;
                            }
if (input.StartsWith("/to ")) writer.WriteLine($"/check {input.Substring(4)}");
                            else if (input.StartsWith("/")) writer.WriteLine(input);
                            else
                            {
                                if (currentTarget == "ALL") writer.WriteLine(input);
                                else writer.WriteLine($"/send {currentTarget} {input}");
                            }
                            PrintPrompt();
                        }
                        else if (key.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                        {
                            inputBuffer.Remove(inputBuffer.Length - 1, 1);
                            Console.Write("\b \b"); 
                        }
                        else if (!char.IsControl(key.KeyChar))
                        {
                            inputBuffer.Append(key.KeyChar);
                            Console.Write(key.KeyChar);
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
        catch { Console.WriteLine("Mat ket noi!"); }
        Environment.Exit(0);
    }

    static void ClearCurrentLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, currentLineCursor);
    }

    static void PrintPrompt()
    {
        Console.Write($"[{currentTarget}] > " + inputBuffer.ToString());
    }

    static void ShowGuide()
    {
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("- /list: Danh sach   | /to <ten>: Ghim Chat");
        Console.WriteLine("- /create <nhom>     | /join <nhom> | /leave <nhom>");
        Console.WriteLine("- /exit: Thoat");
        Console.WriteLine("--------------------------------------------------");
        PrintPrompt();
    }
}
