using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class MessageHandler
    {
        // Danh sách này sẽ được Thành viên 2 cập nhật khi có người Login
        public static Dictionary<string, TcpClient> OnlineClients = new Dictionary<string, TcpClient>();

        // 1. Xử lý gửi tin nhắn riêng (Private)
        public void SendPrivateMessage(string sender, string receiver, string content)
        {
            if (OnlineClients.ContainsKey(receiver))
            {
                byte[] data = Encoding.UTF8.GetBytes($"[PRIVATE] {sender}: {content}");
                OnlineClients[receiver].GetStream().Write(data, 0, data.Length);
            }
        }

        // 2. Xử lý gửi tin nhắn nhóm (Broadcast)
        public void BroadcastMessage(string sender, string content)
        {
            byte[] data = Encoding.UTF8.GetBytes($"[GROUP] {sender}: {content}");
            foreach (var client in OnlineClients.Values)
            {
                client.GetStream().Write(data, 0, data.Length);
            }
        }

        // 3. Trả về danh sách user online
        public string GetListOnline()
        {
            return "Online: " + string.Join(", ", OnlineClients.Keys);
        }
    }
}