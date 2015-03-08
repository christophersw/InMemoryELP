using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InMemoryELP.Models
{
    public class GuestbookTableContext
    {
        public GuestbookTableContext(string StorageConnectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
        }

        private CloudStorageAccount storageAccount;
               
        private CloudTable getTable(){
           
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference("guestbook");
            table.CreateIfNotExists();

            return table;
        }

        public void AddEntry(GuestbookRow row)
        {
            var table = getTable();

            // Create the TableOperation that inserts story.
            TableOperation insertOperation = TableOperation.Insert(row);

            // Execute the insert operation.
            table.Execute(insertOperation);
        }

        public void AddEntry(GusetbookViewModel gvm)
        {
            AddEntry(new GuestbookRow(gvm));
        }

        public List<GusetbookViewModel> GetEntries()
        {
            var table = getTable();
            var result = new List<GusetbookViewModel>();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<GuestbookRow> query = new TableQuery<GuestbookRow>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "guestbook"));

            result = RowToGVM(table.ExecuteQuery(query));

            return result;
        }

        private List<GusetbookViewModel> RowToGVM(IEnumerable<GuestbookRow> Rows)
        {
            var result = new List<GusetbookViewModel>();

            // Print the fields for each customer.
            foreach (GuestbookRow row in Rows)
            {
                result.Add(new GusetbookViewModel()
                {
                    Comment = row.Comment,
                    Name = row.Name,
                    Date = row.Date
                });
            }

            return result;
        }


    }
}