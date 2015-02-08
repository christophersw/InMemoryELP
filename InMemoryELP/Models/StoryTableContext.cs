using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace InMemoryELP.Models
{

    public class StoryTableContext
    {
        public StoryTableContext(string StorageConnectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
        }

        private CloudStorageAccount storageAccount;
               
        private CloudTable getTable(){
           
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference("stories");
            table.CreateIfNotExists();

            return table;
        }

        public void AddStory(Story story){
            
            var table = getTable();

            story = new ImageBlobContext(this.storageAccount).CommittStoryImages(story);

            //We need to trap for Private Stories marked as "Not Approved" - Those stories don't need to go through approval.
            if (story.PartitionKey == "Private")
            {
                story.Approved = true;
            }

            // Create the TableOperation that inserts story.
            TableOperation insertOperation = TableOperation.Insert(story);

            // Execute the insert operation.
            table.Execute(insertOperation);
        }

        public void AddStory(StoryViewModel svm){
            AddStory(new Story(svm));
        }

        public bool RemoveStory(Story story)
        {
            var table = getTable();

            // Create a retrieve operation that expects a story.
            TableOperation retrieveOperation = TableOperation.Retrieve<Story>(story.PartitionKey, story.RowKey);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity.
            Story deleteEntity = (Story)retrievedResult.Result;

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {                
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);

                deleteEntity.PartitionKey = "Deleted";

                // Create the TableOperation that inserts the story with new partition.
                TableOperation insertOperation = TableOperation.Insert(deleteEntity);

                // Execute the insert operation.
                table.Execute(insertOperation);

                return true;
            }

            else
            {
                return false;
            }
        }

        public bool RemoveStory(StoryViewModel svm)
        {
            return RemoveStory(new Story(svm));
        }

        public bool UpdateStory(Story story)
        {
            var table = getTable();

            // Create a retrieve operation that takes a story.
            TableOperation retrieveOperation = TableOperation.Retrieve<Story>(story.PartitionKey, story.RowKey);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity object.
            Story updateEntity = (Story)retrievedResult.Result;

            if (updateEntity != null)
            {
                updateEntity.AuthorEmail = story.AuthorEmail;
                updateEntity.AuthorName = story.AuthorName;
                updateEntity.Title = story.Title;
                updateEntity.HTMLBodyText = story.HTMLBodyText;
                updateEntity.ImageUrls = story.ImageUrls;
                updateEntity.Approved = story.Approved;

                // Create the InsertOrReplace TableOperation
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                table.Execute(updateOperation);

                return true;
            }

            else
            {
                return false;
            }
        }

        public bool UpdateStory(StoryViewModel svm)
        {
            return UpdateStory(new Story(svm));
        }

        /// <summary>
        /// This will show PUBLIC stories
        /// </summary>
        /// <param name="showOnlyApproved">Do you want to see non-approved results?</param>
        /// <returns>A list of story view models from the relevant rows.</returns>
    
        public List<StoryViewModel> GetPublicStories(bool showOnlyApproved = true)
        {
            var table = getTable();
            var result = new List<StoryViewModel>();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            string queryString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Public");
            
            if(showOnlyApproved){
                queryString = TableQuery.CombineFilters(queryString, TableOperators.And ,TableQuery.GenerateFilterConditionForBool("Approved", QueryComparisons.Equal, true));
            }

            TableQuery<Story> query = new TableQuery<Story>().Where(queryString);
 
            result = StoryToSVM(table.ExecuteQuery(query));

            return result;
        }

        public List<StoryViewModel> GetPrivateStories(bool showOnlyApproved = true)
        {
            var table = getTable();
            var result = new List<StoryViewModel>();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            string queryString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Private");

            if (showOnlyApproved)
            {
                queryString = TableQuery.CombineFilters(queryString, TableOperators.And, TableQuery.GenerateFilterConditionForBool("Approved", QueryComparisons.Equal, true));
            }

            TableQuery<Story> query = new TableQuery<Story>().Where(queryString);

            result = StoryToSVM(table.ExecuteQuery(query));

            return result;
        }

        public List<StoryViewModel> GetUnapprovedStories()
        {
            var table = getTable();
            var result = new List<StoryViewModel>();

            string queryString = TableQuery.GenerateFilterConditionForBool("Approved", QueryComparisons.Equal, false); // !== Approved

            string PublicQueryString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Public"); // in Public
            string PrivateQueryString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Private"); // in Private
            string CombinedPublicAndPrivate = TableQuery.CombineFilters(PublicQueryString, TableOperators.Or, PrivateQueryString); // in Public OR Private
            queryString = TableQuery.CombineFilters(queryString, TableOperators.And, CombinedPublicAndPrivate); // !== Approved AND (in Public OR Private)
            
            TableQuery<Story> query = new TableQuery<Story>().Where(queryString);

            result = StoryToSVM(table.ExecuteQuery(query));

            return result;
        }

        public StoryViewModel GetPublicStory(Guid id)
        {
            var table = getTable();
            var query = TableOperation.Retrieve<Story>("Public", id.ToString());
            TableResult retrievedResult = table.Execute(query);

            if (retrievedResult.Result != null)
            {
                return StoryToSVM((Story)retrievedResult.Result);
            }
            else
            {
                return null;
            }
        }

        public StoryViewModel GetPrivateStory(Guid id)
        {
            var table = getTable();
            var query = TableOperation.Retrieve<Story>("Private", id.ToString());
            TableResult retrievedResult = table.Execute(query);

            if (retrievedResult.Result != null)
            {
                return StoryToSVM((Story)retrievedResult.Result);
            }
            else
            {
                return null;
            }
        }

        private StoryViewModel StoryToSVM(Story row)
        {
           
            var result = new StoryViewModel()
            {
                Id = Guid.Parse(row.RowKey),
                AuthorEmail = row.AuthorEmail,
                AuthorName = row.AuthorName,
                Title = row.Title,
                HTMLBodyText = row.HTMLBodyText,
                ImageUrls = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(row.ImageUrls),
                Preview = row.Preview,
                Approved = row.Approved
            };

            result.Public = (row.PartitionKey == "Public");

            return result;
        }

        private List<StoryViewModel> StoryToSVM(IEnumerable<Story> StoryRows){
            var result = new List<StoryViewModel>();

            // Print the fields for each customer.
            foreach (Story row in StoryRows)
            { 
                result.Add(new StoryViewModel()
                {
                    Id = Guid.Parse(row.RowKey),
                    AuthorEmail = row.AuthorEmail,
                    AuthorName = row.AuthorName,
                    Title = row.Title,
                    HTMLBodyText = row.HTMLBodyText,
                    ImageUrls = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(row.ImageUrls),
                    Public = true,
                    Preview = row.Preview, 
                    Approved = row.Approved
                });   
            }

            return result;
        }
    }
}