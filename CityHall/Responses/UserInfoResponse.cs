using System.Collections.Generic;

namespace CityHall.Responses
{
    public class UserInfoResponse : BaseResponse
    {
        public Dictionary<string, int> Environments { get; set; }
    }
}
