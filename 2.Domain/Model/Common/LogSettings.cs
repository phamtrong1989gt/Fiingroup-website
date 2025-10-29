namespace PT.Domain.Model
{
    public class LogSettings
    {
        public bool Is { get; set; }
        public string MongoClient { get; set; }
        public string MongoDataBase { get; set; }
        public string MongoCollection { get; set; }
        public bool IsUseMongo { get; set; }
    }
}
