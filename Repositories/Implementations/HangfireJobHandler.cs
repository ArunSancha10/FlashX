using Hangfire;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using outofoffice.App_code;
using outofoffice.Models;
using outofoffice.Repositories.Interfaces;

namespace outofoffice.Repositories.Implementations
{
    public class HangfireJobHandler(OOODbContext _dbContext) : IHangfireJobHandler
    {
        public async Task AddJobIds(Dictionary<string, string> jobIds, Guid AppId)
        {
            var appList = await _dbContext.UserAppMessages.Where(u => u.UAID == AppId).FirstOrDefaultAsync();

            if (appList is null)
            {
                throw new InvalidOperationException($"Item {AppId} Not Found!");
            }

            var appJsonStr = appList.Apps_To_Publish;

            var appJson = JsonConvert.DeserializeObject<List<JObject>>(appJsonStr);

            if (appJson == null)
            {
                throw new InvalidOperationException("Apps_To_Publish content is invalid or null.");
            }

            foreach (var app in appJson)
            {
                var applicationName = app["Application"]?.ToString();
                if (applicationName != null && jobIds.TryGetValue(applicationName, out var jobId))
                {
                    app["JobId"] = jobId; 
                }
            }

            appList.Apps_To_Publish = JsonConvert.SerializeObject(appJson, Formatting.Indented);

            await _dbContext.SaveChangesAsync();
        }
        public async Task<string> DeleteJob(Guid jobId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var results = new List<string>();

            try
            {
                // Fetch the user application message with associated HangFireJobs
                var userAppMessage = await _dbContext.UserAppMessages
                    .FirstOrDefaultAsync(u => u.UAID == jobId);

                if (userAppMessage == null)
                {
                    throw new InvalidOperationException("User not found!");
                }

                userAppMessage.IsDeleted = true;

                // Parse the Apps_To_Publish JSON string
                var appJson = JsonConvert.DeserializeObject<List<JObject>>(userAppMessage.Apps_To_Publish);
                if (appJson == null)
                {
                    throw new InvalidOperationException("Apps_To_Publish content is invalid or null.");
                }

                foreach (var app in appJson)
                {
                    var jobIdsToDelete = app["JobId"]?.ToString();
                    if (string.IsNullOrWhiteSpace(jobIdsToDelete)) continue;

                    try
                    {
                        var jobIdArray = jobIdsToDelete.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var jobIdToDelete in jobIdArray)
                        {
                            var trimmedJobId = jobIdToDelete.Trim(); 
                            if (string.IsNullOrWhiteSpace(trimmedJobId)) continue;

                            BackgroundJob.Delete(trimmedJobId);
                            results.Add(trimmedJobId);
                        }

                        app["JobId"] = "";
                    }
                    catch (Exception ex)
                    {
                        // Log the error and continue with the next app
                        Console.WriteLine($"Error processing JobId(s): {jobIdsToDelete}. Exception: {ex.Message}");
                    }
                }


                if (!results.Any())
                {
                    throw new InvalidOperationException("There are no Job IDs to delete!");
                }

                // Update the Apps_To_Publish JSON and save changes
                userAppMessage.Apps_To_Publish = JsonConvert.SerializeObject(appJson, Formatting.Indented);

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return the list of deleted Job IDs
                return string.Join(",", results);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Log the exception (optional)
                Console.WriteLine($"Error in DeleteJob: {ex.Message}");

                // Rethrow the exception with an informative message
                throw new Exception("An error occurred while deleting the job. See inner exception for details.", ex);
            }
        }

        public async Task DeleteSingleJob(Guid appId, string appName)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var userAppMessage = await _dbContext.UserAppMessages
                    .FirstOrDefaultAsync(u => u.UAID == appId);

                if (userAppMessage == null)
                {
                    throw new InvalidOperationException("User not found!");
                }

                var appJson = JsonConvert.DeserializeObject<List<JObject>>(userAppMessage.Apps_To_Publish)
                    ?? throw new InvalidOperationException("Apps_To_Publish content is invalid or null.");

                var appToRemove = appJson.FirstOrDefault(app => app["Application"]?.ToString() == appName)
                    ?? throw new InvalidOperationException($"Application '{appName}' not found.");

                var jobIds = appToRemove["JobId"]?.ToString();

                if (!string.IsNullOrWhiteSpace(jobIds))
                {
                    var jobIdArray = jobIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var jobId in jobIdArray)
                    {
                        var trimmedJobId = jobId.Trim(); 
                        if (string.IsNullOrWhiteSpace(trimmedJobId)) continue;

                        BackgroundJob.Delete(trimmedJobId);
                    }
                }

                appJson.Remove(appToRemove);

                userAppMessage.Apps_To_Publish = JsonConvert.SerializeObject(appJson, Formatting.Indented);
                _dbContext.UserAppMessages.Update(userAppMessage);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();

                Console.WriteLine($"Error in DeleteSingleJob: {ex.Message}");

                // Throw a new exception to indicate an error occurred
                throw new Exception("An error occurred while deleting the application.", ex);
            }
        }

    }
}
