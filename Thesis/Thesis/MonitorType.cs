namespace Thesis
{
    public class MonitorType
    {
        public int MonitorId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string SourceT { get; set; }
        public string ServerT { get; set; }

        //public MonitorType(int id,string name,string value, string source,string server)
        //{
        //    MonitorId = id;
        //    Name = name;
        //    Value = value;
        //    SourceT = source;
        //    ServerT = server;
        //}
        public MonitorType()
        {
        }
    }
}
