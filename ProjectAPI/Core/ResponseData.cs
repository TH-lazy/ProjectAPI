namespace ProjectAPI.Core
{
    public class ResponseData
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public object Data { get; set; }
    }
}