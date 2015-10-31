using System;

namespace CityHall.Data
{
    public struct Entry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Author { get; set; }
        public DateTime DateTime { get; set; }
        public bool Active { get; set; }
        public bool Protect { get; set; }
        public string Override { get; set; }
    }

    public struct History
    {
        public Entry[] Entries { get; set; }
    }
}
