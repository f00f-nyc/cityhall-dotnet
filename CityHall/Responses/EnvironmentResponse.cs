using System.Collections.Generic;

namespace CityHall.Responses
{
    public class EnvironmentResponse : BaseResponse
    {
        public Dictionary<string, int> Users { get; set; }
    }
}
