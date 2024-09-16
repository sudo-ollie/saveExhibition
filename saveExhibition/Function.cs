using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaveExhibition;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace saveExhibition;

public class Function
{
    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var log = context.Logger;
        log.LogInformation($"context = {JsonConvert.SerializeObject(context, Formatting.Indented)}");
        log.LogInformation($"request = {JsonConvert.SerializeObject(request, Formatting.Indented)}");

        try
        {
            JObject jsonObject = JObject.Parse(request.Body);
            if (jsonObject["userID"] != null &&
                jsonObject["exhibName"] != null &&
                jsonObject["exhibImage"] != null &&
                jsonObject["exhibPublic"] != null &&
                jsonObject.ContainsKey("exhibItems"))
            {
                IAmazonDynamoDB _dbClient = new AmazonDynamoDBClient(RegionEndpoint.EUWest2);
                DynamoDBContext _dbContext = new DynamoDBContext(_dbClient);

                var itemsToConvert = (JArray)jsonObject["exhibItems"];
                var exhibitContent = ConvertJSONObjs(itemsToConvert);

                Random random = new Random();

                var exhibit = Exhibit.CreateExhibit(
                    exhibitionID: random.Next(100001),
                    exhibitionPublic: jsonObject["exhibPublic"].Value<bool>(),
                    exhibitionName: jsonObject["exhibName"].ToString(),
                    exhibitionImage: jsonObject["exhibImage"].ToString(),
                    exhibitContent: exhibitContent,
                    exhibitionLength: exhibitContent.Count,
                    userID: jsonObject["userID"].ToString()
                );

                // Splitting DB save between public & private exhibition tables
                if (exhibit.ExhibitionPublic)
                {
                    await _dbContext.SaveAsync(exhibit, new DynamoDBOperationConfig { OverrideTableName = "PublicExhibitions" });

                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 200,
                        Body = JsonConvert.SerializeObject(new { message = "Exhibit saved successfully - [Public]", exhibitId = exhibit.ExhibitionID })
                    };
                }
                else
                {
                    await _dbContext.SaveAsync(exhibit, new DynamoDBOperationConfig { OverrideTableName = "PrivateExhibitions" });

                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 200,
                        Body = JsonConvert.SerializeObject(new { message = "Exhibit saved successfully - [Private]", exhibitId = exhibit.ExhibitionID })
                    };
                }

            }
            else
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new { message = "Incomplete request body. Missing one or more required fields." })
                };
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing request: {ex.Message}");
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "An error occurred while processing the request." })
            };
        }
    }

    private static List<ExhibitItem> ConvertJSONObjs(JArray jsonObject)
    {
        var exhibitItems = new List<ExhibitItem>();
        foreach (var item in jsonObject)
        {
            var exhibitItem = ExhibitItem.CreateExhibitItem(
                itemCreditline: item["itemCreditline"]?.ToString(),
                itemDepartment: item["itemDepartment"]?.ToString(),
                itemID: item["itemID"]?.Value<int>() ?? 0,
                itemClassification: item["itemClassification"]?.ToString(),
                itemTechnique: item["itemTechnique"]?.ToString(),
                itemTitle: item["itemTitle"]?.ToString(),
                creationDate: item["creationDate"]?.Value<int>() ?? 0,
                artistBirthplace: item["artistBirthplace"]?.ToString(),
                artistName: item["artistName"]?.ToString(),
                itemObjectlink: item["itemObjectLink"]?.ToString(),
                itemCentury: item["itemCentury"]?.ToString()
            );
            exhibitItems.Add(exhibitItem);
        }
        return exhibitItems;
    }
}