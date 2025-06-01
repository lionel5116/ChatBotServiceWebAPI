using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Mvc;

using Newtonsoft.Json;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors; // For CORS
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/directline")]
    public class DirectLineTokenController : ApiController
    {
        private static readonly string DirectLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private const string DirectLineTokenEndpoint = "https://directline.botframework.com/v3/directline/tokens/generate";

        // Using a static HttpClient is generally recommended for performance in .NET Framework
        // as it avoids socket exhaustion issues that can occur with frequent creation/disposal.
        private static readonly HttpClient httpClient = new HttpClient();

        public DirectLineTokenController()
        {
            if (string.IsNullOrEmpty(DirectLineSecret))
            {
                // Log this error appropriately in a real application
                System.Diagnostics.Debug.WriteLine("FATAL ERROR: DirectLineSecret is not configured in Web.config.");
                // Consider throwing a specific configuration exception if appropriate for your app's startup
            }
        }

        [HttpPost]
        [Route("token")]
        public async Task<IHttpActionResult> GenerateToken()
        {
            if (string.IsNullOrEmpty(DirectLineSecret))
            {
                return InternalServerError(new Exception("DirectLineSecret is not configured on the server."));
            }

            try
            {
                // The request to Direct Line API
                var request = new HttpRequestMessage(HttpMethod.Post, DirectLineTokenEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", DirectLineSecret);

                // Optional: If you want to associate the token with a specific user ID for the bot
                // var userId = $"dl_{Guid.NewGuid()}"; // Generate a unique ID for the user
                // var requestBody = new { User = new { Id = userId } };
                // request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                // If no user ID is needed, an empty body is fine for basic token generation.
                // The Direct Line service will still generate a conversationId.
                // An empty JSON object can also be sent as content if required by an API,
                // but for this specific endpoint, it's often optional if not providing user context.
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");


                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<DirectLineTokenResponse>(responseString);

                    // Return only what the client needs (token and conversationId are common)
                    return Ok(new { token = tokenResponse.token, conversationId = tokenResponse.conversationId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Log the detailed errorContent on the server for debugging
                    System.Diagnostics.Debug.WriteLine($"Error from Direct Line Service: {response.StatusCode} - {errorContent}");
                    return Content(response.StatusCode, $"Failed to generate Direct Line token. Status: {response.StatusCode}. Details: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception ex on the server
                System.Diagnostics.Debug.WriteLine($"Exception generating Direct Line token: {ex.ToString()}");
                return InternalServerError(new Exception("An unexpected error occurred while generating the Direct Line token."));
            }
        }
    }
}