namespace NoteApp_API.NoteModel
{
    public class Response
    {
        public int success { get; set; }
        public string message { get; set; }
        public string body { get; set; }
        public object data { get; set; }
        public string token { get; set; }
    }
}
