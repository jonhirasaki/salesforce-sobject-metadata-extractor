using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class SalesforceConfig
{
    public string InstanceUrl { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string InitialSObject { get; set; } = "Opportunity";
    public int DepthLevel { get; set; } = 2;
    public int MaxChildRelationships { get; set; } = 0; // 0 = Process all
    public string ApiVersion { get; set; } = "v59.0";
    public string OutputFilePath { get; set; } = "final_output.json";
}

public class SalesforceDataProcessor
{
    private readonly HttpClient _httpClient;
    private readonly SalesforceConfig _config;

    public SalesforceDataProcessor(SalesforceConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public async Task ProcessChildSObjectsAsync()
    {
        try
        {
            Console.WriteLine($"Fetching metadata for initial sObject: {_config.InitialSObject}");

            // Query Salesforce to get metadata for the initial sObject
            var rootData = await QuerySalesforceObject(_config.InitialSObject, 1);

            // Create the final structured output
            var result = new
            {
                sObject = _config.InitialSObject,
                childRelationships = rootData
            };

            string outputJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_config.OutputFilePath, outputJson);

            Console.WriteLine($"✅ Processed JSON file saved successfully: {_config.OutputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing JSON: {ex.Message}");
        }
    }

    private async Task<Dictionary<string, object>> QuerySalesforceObject(string sObjectName, int currentDepth)
    {
        if (currentDepth > _config.DepthLevel)
            return new Dictionary<string, object>(); // Stop processing if max depth is reached

        Console.WriteLine($"🔍 Querying object: {sObjectName} (Depth: {currentDepth})");

        try
        {
            string requestUrl = $"{_config.InstanceUrl}/services/data/{_config.ApiVersion}/sobjects/{sObjectName}/describe/";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.AccessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"⚠️ 404 Not Found: {sObjectName} does not exist in Salesforce.");
                }
                else
                {
                    Console.WriteLine($"⚠️ Error fetching {sObjectName}: {response.StatusCode}");
                }
                return new Dictionary<string, object>();
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            Dictionary<string, object> childRelationships = new();
            int processedCount = 0;

            // Extract only childRelationships with "childSObject" and "field"
            if (root.TryGetProperty("childRelationships", out JsonElement childRelationshipsElement))
            {
                foreach (JsonElement child in childRelationshipsElement.EnumerateArray())
                {
                    if (_config.MaxChildRelationships > 0 && processedCount >= _config.MaxChildRelationships)
                        break; // Stop processing if max child relationships reached

                    if (child.TryGetProperty("childSObject", out JsonElement childSObject) &&
                        child.TryGetProperty("field", out JsonElement field))
                    {
                        string childObjectName = childSObject.GetString() ?? "";
                        string fieldName = field.GetString() ?? "";

                        // Extract additional properties
                        string associateEntityType = child.TryGetProperty("associateEntityType", out JsonElement assocType) ? assocType.GetString() ?? "" : "";
                        string associateParentEntity = child.TryGetProperty("associateParentEntity", out JsonElement assocParent) ? assocParent.GetString() ?? "" : "";

                        // 🚨 Stop recursion if the childSObject is the same as its parent 🚨
                        if (childObjectName == sObjectName)
                        {
                            childRelationships[childObjectName] = new Dictionary<string, object>
                            {
                                { "field", fieldName },
                                { "associateEntityType", associateEntityType },
                                { "associateParentEntity", associateParentEntity }
                            };
                            continue; // Do not go deeper for this branch
                        }

                        // Recursively fetch child relationships if within depth limit
                        Dictionary<string, object> nestedChildRelationships = await QuerySalesforceObject(childObjectName, currentDepth + 1);

                        // Build the structured hierarchy
                        var childData = new Dictionary<string, object>
                        {
                            { "field", fieldName },
                            { "associateEntityType", associateEntityType },
                            { "associateParentEntity", associateParentEntity }
                        };

                        // Only add nested relationships if they exist
                        if (nestedChildRelationships.Count > 0)
                        {
                            childData["childRelationships"] = nestedChildRelationships;
                        }

                        childRelationships[childObjectName] = childData;
                        processedCount++;
                    }
                }
            }

            return childRelationships;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error querying Salesforce for {sObjectName}: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public static async Task<SalesforceConfig> LoadConfigAsync(string configFilePath)
    {
        try
        {
            string json = await File.ReadAllTextAsync(configFilePath);
            return JsonSerializer.Deserialize<SalesforceConfig>(json) ?? new SalesforceConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error reading config file: {ex.Message}");
            return new SalesforceConfig();
        }
    }
}
