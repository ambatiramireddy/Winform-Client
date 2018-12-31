using AddAppAPI.Enums;
using AddAppAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWinFormsApp
{
    public class Message
    {
        public ScreenBooking ScreenBooking { get; set; }
        public MessageType Type { get; set; }
        public string Content { get; set; }
        public string Sender { get; set; }
        public int StartTime { get; set; }
        public int Duration { get; set; }

    }
}
