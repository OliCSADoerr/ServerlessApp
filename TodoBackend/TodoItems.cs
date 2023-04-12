using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api
{
	public static class TodoItems
	{
		private const string PartitionKey = "partition";
		private const string TableName = "ToDoTable";

        //private static AuthorizedUser GetCurrentUserName(TraceWriter log)
        //{
        //	// On localhost claims will be empty
        //	string name = "Dev User";
        //	string upn = "dev@localhost";

        //	foreach (Claim claim in ClaimsPrincipal.Current.Claims)
        //	{
        //		if (claim.Type == "name")
        //		{
        //			name = claim.Value;
        //		}
        //		if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")
        //		{
        //			upn = claim.Value;
        //		}
        //		//Uncomment to print all claims to log output for debugging
        //		//log.Verbose("Claim: " + claim.Type + " Value: " + claim.Value);
        //	}
        //	return new AuthorizedUser() {DisplayName = name, UniqueName = upn };
        //}

        // Add new item
        [FunctionName("TodoItemAdd")]
        [return: Table(TableName, Connection = "AzureStorage")] // This syntax comes from the docs, alternatively you can add a Table input binding and use todoTable.UpsertAsync
        public static async Task<TodoItem> AddItem(
			[HttpTrigger(
				AuthorizationLevel.Anonymous, 
				"post",
				Route = "todoitem")]
			HttpRequestMessage req,
			ILogger log)
		{
			// TODO ERROR HANDLING

			try
			{
                var json = await req.Content.ReadAsStringAsync();
                var newItem = JsonConvert.DeserializeObject<TodoItem>(json); // Getting the content from the request

                // Setting the additional required properties
                newItem.PartitionKey = PartitionKey;
                newItem.Id = DateTime.Now.Ticks.ToString();
                newItem.ItemCreateDate = DateTime.UtcNow;

                return newItem;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

		// Get all items
		[FunctionName("TodoItemGetAll")]
		public static async Task<IActionResult> GetAll(
			[HttpTrigger(
				AuthorizationLevel.Anonymous, 
				"get", 
				Route = "todoitem")]
			HttpRequestMessage req,
			[Table(TableName, PartitionKey, Connection = "AzureStorage")]
			TableClient todoTable, 
			ILogger log)
		{
			var list = todoTable.QueryAsync<TodoItem>();

			var result = new List<TodoItem>();

			await foreach (var item in list) // The loop is executing the paged query
			{
				result.Add(item);
			}

			return new OkObjectResult(result);
		}

		// Delete item by id
		[FunctionName("TodoItemDelete")]
		public static async Task<IActionResult> DeleteItem(
		   [HttpTrigger(
			AuthorizationLevel.Anonymous,
			"delete",
			Route = "todoitem/{id}")]
		   HttpRequestMessage req,
		   [Table(TableName, Connection = "AzureStorage")]
		   TableClient todoTable, 
		   string id,
		   ILogger log)
		{
			try
			{
                await todoTable.DeleteEntityAsync(PartitionKey, id);
				return new OkObjectResult(HttpStatusCode.NoContent);
            }
			catch (Exception ex)
			{
				return new UnprocessableEntityObjectResult(ex.Message);
			}

            //var currentUser = GetCurrentUserName(log);
            //log.Info("Deleting document with ID " + id + " for user " + currentUser.UniqueName);

            //Uri documentUri = UriFactory.CreateDocumentUri("ServerlessTodo", "TodoItems", id);

            //try
            //{
            //	// Verify the user owns the document and can delete it
            // 			await client.DeleteDocumentAsync(documentUri, new RequestOptions() { PartitionKey = new PartitionKey(currentUser.UniqueName) });
            //}
            //catch (DocumentClientException ex)
            //{
            //	if (ex.StatusCode == HttpStatusCode.NotFound)
            //	{
            //		// Document does not exist, is not owned by the current user, or was already deleted
            //		log.Warning("Document with ID: " + id + " not found.");
            //	}
            //	else
            //	{
            //		// Something else happened
            //		throw ex;
            //	}
            //}

            //return req.CreateResponse(HttpStatusCode.NoContent);
        }
	}
}
