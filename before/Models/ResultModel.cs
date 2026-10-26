namespace Library.Before.Models
{
    public class ResultModel
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
