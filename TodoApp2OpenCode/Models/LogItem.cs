namespace TodoApp2OpenCode.Models
{
    public class LogItem
    {
        public long Id { get; set; } = new Random().NextInt64();
        public DatabaseAction Action{ get; set; }
        public string User { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string BoardId { get; set; } = string.Empty;
    }
}
