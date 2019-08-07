using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

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
      Unit.UnitResponse response = await SendRequest<Unit.UnitResponse>("api/unit/get_units", null, null);
      return response.Units;
    }

    /// <summary>
    /// Register a device with the server
    /// </summary>
    /// <param name="activationCode">Activation code</param>
    /// <returns>The newly created credentials</returns>
    public async Task<Device> RegisterDevice(string activationCode) {
      // Create the post data
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "activationCode", activationCode }
      };
      // Send request
      Device.DeviceResponse deviceResponse = await SendRequest<Device.DeviceResponse>("api/device/register_device", null, formData);
      return deviceResponse.Device;
    }

    /// <summary>
    /// Check if a set of credentials are still valid on the server.
    /// This function can't use the generic SendRequest method since it needs to analyse the http
    /// response code.
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

    /// <summary>
    /// Get the tournaments available for a device
    /// </summary>
    /// <returns>Available tournaments</returns>
    /// <param name="device">The device to get tournaments for</param>
    /// <param name="limit">Max number of tournaments to get, with the most recent first. 0 if to get all of them.</param>
    public async Task<List<Tournament>> GetTournaments(Device device, int limit) {
      // Create the post data. Limit is the max number of the most recent
      // tournaments to return.
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "limit", limit.ToString() }
      };
      Tournament.TournamentResponse tournamentResponse = await SendRequest<Tournament.TournamentResponse>("api/unit/get_tournaments", device, formData);
      return tournamentResponse.Tournaments;
    }

    /// <summary>
    /// Create a on-the-fly match.
    /// </summary>
    /// <returns>The newly created match</returns>
    /// <param name="device">Device with server credentials</param>
    /// <param name="tournament">The tournament to add the match to. Can be null, in that case the server will
    ///                          try to figure out which tournament to use.</param>
    /// <param name="match">The match to add</param>
    public async Task<Match> CreateOnTheFlyMatch(Device device, Tournament tournament, Match match) {
      // Create the post data
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "category", match.Category },
        { "sequencenumber", match.TournamentMatchNumber.ToString() },
        { "starttime", match.StartTime.ToString("yyyy-MM-dd HH:mm") },
        { "team1player1name", match.Team1Player1Name },
        { "team1player1team", match.Team1Player1Team },
        { "team1player2name", match.Team1Player2Name },
        { "team1player2team", match.Team1Player2Team },
        { "team2player1name", match.Team2Player1Name },
        { "team2player1team", match.Team2Player1Team },
        { "team2player2name", match.Team2Player2Name },
        { "team2player2team", match.Team2Player2Team }
      };
      if (tournament != null) {
        formData.Add("tournamentid", tournament.TournamentID.ToString());
      }
      Match.MatchResponse matchResponse = await SendRequest<Match.MatchResponse>("api/match/create_onthefly_match", device, formData);
      return matchResponse.Match;
    }

    /// <summary>
    /// Get a list of all courts available for a device.
    /// </summary>
    /// <returns>Available courts</returns>
    /// <param name="device">Device to look up courts for.</param>
    public async Task<List<Court>> GetCourts(Device device) {
      // Create the post data.
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "addvenueinfo", "1" } // Makes sure venue info is added to the returned json
      };
      Court.CourtResponse courtResponse = await SendRequest<Court.CourtResponse>("api/court/get_courts", device, formData);
      return courtResponse.Courts;
    }

    /// <summary>
    /// Assign a match to a court.
    /// </summary>
    /// <returns></returns>
    /// <param name="device">Device credentials</param>
    /// <param name="match">Match to be assigned</param>
    /// <param name="court">Court for the match to be assigned to.</param>
    public async Task AssignMatchToCourt(Device device, Match match, Court court) {
      // Create the post data.
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "courtid", court.CourtID.ToString() },
        { "matchid", match.MatchID.ToString() }
      };
      ScoreboardResponse response = await SendRequest<ScoreboardResponse>("api/court/assign_match", device, formData);
    }

    /// <summary>
    /// Send a request to the scoreboard server
    /// </summary>
    /// <returns>Server response</returns>
    /// <param name="route">Server route</param>
    /// <param name="credentials">Device credentials. Null if no authorization should be used.</param>
    /// <param name="postParams">Post parameters.</param>
    /// <typeparam name="T">The type of response to expect from the server.</typeparam>
    private async Task<T> SendRequest<T>(string route, Device credentials, Dictionary<string, string> postParams) where T : ScoreboardResponse {
      // Create the request content.
      Dictionary<string, string> formData = postParams != null ? new Dictionary<string, string>(postParams) : new Dictionary<string, string>();
      formData.Add("randomStuff", Guid.NewGuid().ToString("n"));
      HttpContent content = new FormUrlEncodedContent(formData);
      // Create the request
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}{1}", AppendSlash(BaseUrl), route)) {
        Content = content
      };
      // Check if to use authentication
      if (credentials != null) {
        // Create the HMAC from the http content
        string authentication = await CalculateHMAC(credentials, content);
        request.Headers.Add("Authorization", authentication);
      }
      // Send the request
      HttpResponseMessage response = await m_client.SendAsync(request);
      // Try and parse the json
      T scoreboardResponse = await TryReadResponse<T>(response);
      // Throw error if request is not successfull
      if (!response.IsSuccessStatusCode) {
        throw (new ScoreboardLiveApiException(response.StatusCode, scoreboardResponse));
      }
      // Return the response
      return scoreboardResponse;
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
