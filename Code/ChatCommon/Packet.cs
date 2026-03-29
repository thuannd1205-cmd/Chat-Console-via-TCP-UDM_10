using System;

namespace ChatCommon
{
    public class Packet
    {
        public string Command { get; set; }  
        public string Sender { get; set; }   
        public string Receiver { get; set; } 
        public string Content { get; set; }  
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}