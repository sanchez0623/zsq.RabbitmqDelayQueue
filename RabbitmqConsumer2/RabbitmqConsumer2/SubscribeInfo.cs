﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitmqConsumer2
{
    public class SubscribeInfo : ConsumerInfo
    {
        //public string SubscribeId { get; set; }

        public SubscribeType Type { get; set; }
    }

    public enum SubscribeType
    {
        Init = 0,
        Day = 1,
        Week = 2,
        Month = 3
    }

    public class ConsumerInfo
    {
        public string UserId { get; set; }
    }
}
