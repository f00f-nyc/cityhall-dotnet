using System;
using System.Collections.Generic;

namespace CityHall.Responses
{
    public class LogEntry
    {
        public bool active { get; set; }
        public string @override { get; set; }
        public int id { get; set; }
        public string value { get; set; }
        public DateTime datetime { get; set; }
        public bool protect { get; set; }
        public string name { get; set; }
        public string author { get; set; }
    }

    public class HistoryResponse : BaseResponse
    {
        public List<LogEntry> History { get; set; }
    }
}
