using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitmqConsumer
{
    public class SendInfo
    {
        public string SubscribeId { get; set; }

        public int StartDate { get; set; }

        public int EndDate { get; set; }

        public string TeamId { get; set; }
    }
}
