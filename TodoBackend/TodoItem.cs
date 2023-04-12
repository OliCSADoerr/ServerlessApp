using Azure;
using Azure.Data.Tables;
using System;

namespace TodoBackend
{
    public class TodoItem : ITableEntity
    {
        public string Id { get; set; }
        public string ItemName { get; set; }
        public string ItemOwner { get; set; }
        public DateTime? ItemCreateDate { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get => Id; set => Id = value; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
