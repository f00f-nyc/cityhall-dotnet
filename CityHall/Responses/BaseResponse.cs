namespace CityHall.Responses
{
    public class BaseResponse
    {
        public string Response { get; set; }
        public string Message { get; set; }
        public bool IsValid { get { return this.Response == "Ok"; } }
    }
}
