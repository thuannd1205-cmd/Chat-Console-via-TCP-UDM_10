using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChatCommon
{
    public static class Utils
    {
        public static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        public static string HashPassword(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        public static byte[] Serialize(Packet packet)
        {
            string json = JsonSerializer.Serialize(packet);
            return Encoding.UTF8.GetBytes(json);
        }

        public static Packet Deserialize(byte[] data, int size)
        {
            string json = Encoding.UTF8.GetString(data, 0, size);
            return JsonSerializer.Deserialize<Packet>(json);
        }
    }
}