using DisBotTelegram.BLL.DTO;
using System;

namespace DisBotTelegram.BLL.Logic.EventsArgs
{
    public class BotEventArgs : EventArgs
    {
        public DisBotMessage Message { get; set; }

        public BotEventArgs(DisBotMessage message)
        {
            Message = message;
        }
    }
}