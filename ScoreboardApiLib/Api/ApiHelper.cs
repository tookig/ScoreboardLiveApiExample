using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace ScoreboardLiveApi {
  public class ApiHelper {
    // The client object to make all server requests
    private readonly HttpClient m_client  = new HttpClient();

    /// <summary>
    /// Gets or sets the base URL for the Scoreboard Live server (ex: http://www.scoreboardlive.se).
    /// </summary>
    /// <value>The base URL.</value>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ScoreboardLiveApi.Api.ApiHelper"/> class.
    /// </summary>
    /// <param name="baseUrl">Base URL for the Scoreboard Live server.</param>
    public ApiHelper(string baseUrl) {
      BaseUrl = baseUrl;
      // Set the default headers to be used for all requests
      m_client.DefaultRequestHeaders.Accept.Clear();
      m_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      m_client.DefaultRequestHeaders.Add("User-Agent", "Scoreboard Live API Tester");
    }

    /// <summary>
    /// Get all units available on the server
    /// </summary>
    /// <returns>The server response.</returns>
    public async Task<Unit.UnitResponse> GetUnits() {
      // Create the serializer used to decode the json server response
      var serializer = new DataContractJsonSerializer(typeof(Unit.UnitResponse));
      // Since this is a simple GET request, no data needs to be appended to the
      // request.
      HttpResponseMessage response = await m_client.GetAsync(string.Format("{0}api/unit/get_units", AppendSlash(BaseUrl)));
      // If an error occured, throw an exception
      response.EnsureSuccessStatusCode();
      // Parse the json and return
      var unitResponse = serializer.ReadObject(await response.Content.ReadAsStreamAsync()) as Unit.UnitResponse;
      return unitResponse;
    }

    /// <summary>
    /// Helper class to append a slash to a string if it doesn't end with one
    /// </summary>
    /// <returns>String with slash appended</returns>
    /// <param name="url">URL to append slash to.</param>
    private string AppendSlash(string url) {
      if (url.EndsWith("/")) {
        return url;
      }
      return url + "/";
    }
  }
}
