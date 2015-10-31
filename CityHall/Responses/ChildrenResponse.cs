using System.Collections.Generic;

namespace CityHall.Responses
{
    public class ChildResponse
    {
        public string @override { get; set; }
        public string path { get; set; }
        public int id { get; set; }
        public string value { get; set; }
        public bool protect { get; set; }
        public string name { get; set; }
    }

    public class ChildrenResponse : BaseResponse
    {
        public string path { get; set; }
        public List<ChildResponse> children { get; set; }
    }
}
