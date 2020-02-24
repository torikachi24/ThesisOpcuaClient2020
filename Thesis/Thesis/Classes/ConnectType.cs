namespace Thesis
{
    public class ConnectType
    {
        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionUrl { get; set; }

        public ConnectType(int id, string name, string url)
        {
            ConnectionId = id;
            ConnectionName = name;
            ConnectionUrl = url;
        }

        public ConnectType()
        {
        }
    }
}