using Azure;
using Azure.Data.Tables;

namespace OfBot.TableStorage.Models
{
    public class TrackedPubgPlayer : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string AddedBy { get; set; }
        public string Name { get; set; }
        public long? Id { get; set; }
    }
}