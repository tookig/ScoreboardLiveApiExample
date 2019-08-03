using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
    public async Task<List<Unit>> GetUnits() {
      // Since this is a simple GET request, no data needs to be appended to the
      // request.
      HttpResponseMessage response = await m_client.GetAsync(string.Format("{0}api/unit/get_units", AppendSlash(BaseUrl)));
      // Parse the json
      var unitResponse = await TryReadResponse<Unit.UnitResponse>(response);
      // If an error occured, throw an exception
      if (!response.IsSuccessStatusCode) {
        throw (new ScoreboardLiveApiException(response.StatusCode, unitResponse));
      }
      // Return response
      return unitResponse.Units;
    }

    /// <summary>
    /// Register a device with the server
    /// </summary>
    /// <param name="activationCode">Activation code</param>
    /// <returns>The newly created credentials</returns>
    public async Task<Device> RegisterDevice(string activationCode) {
      // Create the http data
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "activationCode", activationCode }
      };
      HttpContent content = new FormUrlEncodedContent(formData);
      // Send request
      HttpResponseMessage response = await m_client.PostAsync(string.Format("{0}api/device/register_device", AppendSlash(BaseUrl)), content);
      // Check response
      Device.DeviceResponse deviceInfo = await TryReadResponse<Device.DeviceResponse>(response);
      // If an error occured, throw an exception
      if (!response.IsSuccessStatusCode) {
        throw (new ScoreboardLiveApiException(response.StatusCode, deviceInfo));
      }
      // Return the new device credentials
      return deviceInfo.Device;
    }

    /// <summary>
    /// Check if a set of credentials are still valid on the server.
    /// </summary>
    /// <param name="device">Device credentials to check</param>
    /// <returns>True if still valid, false if not.</returns>
    public async Task<bool> CheckCredentials(Device device) {
      // Create some random http data. Some random content is needed on the request so that
      // the same generated HMAC isn't sent multiple times (this could be a security issue). 
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "randomStuff", Guid.NewGuid().ToString("n") }
      };
      HttpContent content = new FormUrlEncodedContent(formData);
      // Create the HMAC from the http content
      string authentication = await CalculateHMAC(device, content);
      // Send the request
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}api/device/check_registration", AppendSlash(BaseUrl)));
      request.Headers.Add("Authorization", authentication);
      request.Content = content;
      HttpResponseMessage response = await m_client.SendAsync(request);
      // If the response is a 403 - forbidden, these credentials are no longer valid and should be forgotten.
      // If the response is a 200 - OK, all is well. Any other response means the server was unable to check
      // the credentials for some reason. Check the response exception error messages to find clues.
      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        return true;
      }
      if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) {
        return false;
      }
      // Try and get the reasons for the failure. If the serializer throws, it's probably due to an internal
      // server error, since this will not generate valid json.
      ScoreboardResponse scoreboardResponse = await TryReadResponse<ScoreboardResponse>(response);
      throw (new ScoreboardLiveApiException(response.StatusCode, scoreboardResponse));
    }

    public async Task<List<Tournament>> GetTournaments(Device device, int limit) {
      // Create the request content. Limit is the max number of the most recent
      // tournaments to return.
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "randomStuff", Guid.NewGuid().ToString("n") },
        { "limit", limit.ToString() }
      };
      HttpContent content = new FormUrlEncodedContent(formData);
      // Create the HMAC from the http content
      string authentication = await CalculateHMAC(device, content);
      // Send the request
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}api/unit/get_tournaments", AppendSlash(BaseUrl)));
      request.Headers.Add("Authorization", authentication);
      request.Content = content;
      HttpResponseMessage response = await m_client.SendAsync(request);
      // Try and parse the tournament json
      Tournament.TournamentResponse tournamentResponse = await TryReadResponse<Tournament.TournamentResponse>(response);
      // Throw error if request is not successfull
      if (!response.IsSuccessStatusCode) {
        throw (new ScoreboardLiveApiException(response.StatusCode, tournamentResponse));
      }
      // Return the tournaments
      return tournamentResponse.Tournaments;
    }

    /// <summary>
    /// Helper function that tries to read a json response. If it fails, it returns null unless the
    /// request itself succeeded; in that case there should be valid json available, and this function
    /// throws a json serialization exception.
    /// </summary>
    /// <typeparam name="T">ScoreboardResponse type</typeparam>
    /// <param name="httpResponse">The http request response to try and parse</param>
    /// <returns></returns>
    private static async Task<T> TryReadResponse<T>(HttpResponseMessage httpResponse) where T:ScoreboardResponse {
      T scoreboardResponse = null;
      try {
        var serializer = new DataContractJsonSerializer(typeof(T));
        scoreboardResponse = serializer.ReadObject(await httpResponse.Content.ReadAsStreamAsync()) as T;
      } catch (Exception e) {
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK) throw e;
      }
      return scoreboardResponse;
    }

    /// <summary>
    /// Helper function to append a slash to a string if it doesn't end with one
    /// </summary>
    /// <returns>String with slash appended</returns>
    /// <param name="url">URL to append slash to.</param>
    private static string AppendSlash(string url) {
      if (url.EndsWith("/")) {
        return url;
      }
      return url + "/";
    }

    /// <summary>
    /// Create a hex string from a byte array
    /// </summary>
    /// <param name="ba"></param>
    /// <returns></returns>
    private static string ByteArrayToHexString(byte[] ba) {
      StringBuilder hex = new StringBuilder(ba.Length * 2);
      foreach (byte b in ba)
        hex.AppendFormat("{0:x2}", b);
      return hex.ToString();
    }

    /// <summary>
    /// Create a HMAC signature for a HttpContent object, using device credentials.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private static async Task<string> CalculateHMAC(Device device, HttpContent content) {
      string hash;
      using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(device.ClientToken))) {
        hash = device.DeviceCode + ByteArrayToHexString(hmac.ComputeHash(await content.ReadAsByteArrayAsync()));
      }
      return hash;
    }
  }
}
