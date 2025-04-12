using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace ScoreboardLiveApi {
  public class ApiHelper {
    // The HTTP client handler to use for all requests
    private readonly HttpClientHandler m_clientHandler;
    // The client object to make all server requests
    private readonly HttpClient m_client;

    // Default json options to use for all json serialization
    private readonly JsonSerializerOptions m_jsonDefaultOptions = new() {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Gets or sets the base URL for the Scoreboard Live server (ex: https://www.scoreboardlive.se).
    /// </summary>
    /// <value>The base URL.</value>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Event that is triggered when an error occurs during a request to the server.
    /// This is sent on a different thread than the one that triggered the error, and in parallel
    /// with the throwing of the exception.
    /// </summary>
    public event EventHandler<ApiHelperErrorEventArgs>? Error;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ScoreboardLiveApi.Api.ApiHelper"/> class.
    /// </summary>
    /// <param name="baseUrl">Base URL for the Scoreboard Live server.</param>
    /// <param name="requestTimeout">Request timeout in seconds.</param>
    /// <param name="acceptAnyCertificates">If set to <c>true</c> accept any certificates, trusted or not.</param>
    public ApiHelper(string baseUrl, int requestTimeout = 30, bool acceptAnyCertificates = false) {
      BaseUrl = baseUrl;
      (m_clientHandler, m_client) = InitHttpClient(requestTimeout, acceptAnyCertificates);
    }

    protected static (HttpClientHandler, HttpClient) InitHttpClient(int requestTimeout, bool acceptAnyCertificates) {
      // Create the client handler
      HttpClientHandler handler = new();
      if (acceptAnyCertificates) {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
      }
      // Create the client
      HttpClient client = new(handler);
      // Set the default headers to be used for all requests
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      client.DefaultRequestHeaders.Add("User-Agent", "Scoreboard Live API Helper");
      // Set timeout
      client.Timeout = TimeSpan.FromSeconds(requestTimeout);
      return (handler, client);
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
    /// Get the socket URL for the server
    /// </summary>
    /// <returns>Socket URL</returns>
    public async Task<string> GetSocketURL() {
      SocketInfo.SocketInfoResponse response = await SendRequest<SocketInfo.SocketInfoResponse>("api/socketcontroller/get_uri", null, null);
      return response.URL ?? string.Empty;
    } 

    /// <summary>
    /// Register a device with the server
    /// </summary>
    /// <param name="activationCode">Activation code</param>
    /// <returns>The newly created credentials</returns>
    public async Task<Device> RegisterDevice(string activationCode) {
      // Create the post data
      Dictionary<string, string?> formData = new() {
        { "activationCode", activationCode }
      };
      // Send request
      Device.DeviceResponse deviceResponse = await SendRequest<Device.DeviceResponse>("api/device/register_device", null, formData);
      if (deviceResponse.Device == null) {
        throw new Exception("Device could not be created; null value returned from registration call.");
      }
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
      Dictionary<string, string> formData = new() {
        { "randomStuff", Guid.NewGuid().ToString("n") }
      };
      HttpContent content = new FormUrlEncodedContent(formData);
      // Create the HMAC from the http content
      string authentication = await CalculateHMAC(device, content);
      // Send the request
      HttpRequestMessage request = new(HttpMethod.Post, string.Format("{0}api/device/check_registration", AppendSlash(BaseUrl)));
      request.Headers.Add("Authorization", authentication);
      request.Content = content;
      HttpResponseMessage response = await TrySendAsync(request);
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
      OnError(ApiHelperErrorEventArgs.ErrorStage.HttpError, new ScoreboardLiveApiException(response.StatusCode, scoreboardResponse));
      throw new ScoreboardLiveApiException(response.StatusCode, scoreboardResponse);
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
      Dictionary<string, string?> formData = new() {
        { "limit", limit.ToString() }
      };
      Tournament.TournamentResponse tournamentResponse = await SendRequest<Tournament.TournamentResponse>("api/unit/get_tournaments", device, formData);
      return tournamentResponse.Tournaments;
    }

    /// <summary>
    /// Get a specific tournament
    /// </summary>
    /// <param name="tournamentID">ID of tournament to get</param>
    /// <param name="device">(optional) Device credentials. Not needed for published tournaments.</param>
    /// <returns>Tournament if found.</returns>
    public async Task<Tournament?> GetTournament(int tournamentID, Device? device = null) {
      // Create the post data. 
      Dictionary<string, string?> formData = new() {
        { "tournamentid", tournamentID.ToString() }
      };
      Tournament.SingleTournamentResponse tournamentResponse = await SendRequest<Tournament.SingleTournamentResponse>("api/tournament/get_tournament", device, formData);
      return tournamentResponse.Tournament;
    }

    /// <summary>
    /// Create a match.
    /// </summary>
    /// <returns>The newly created match</returns>
    /// <param name="device">Device with server credentials</param>
    /// <param name="tournament">The tournament to add the match to. Can be null, in that case the server will
    ///                          try to figure out which tournament to use.</param>
    /// <param name="tClass">If this match is to belong to a round-robin tournament class, add class data here. 
    ///                      Set as null otherwise</param>
    /// <param name="match">The match to add</param>
    public async Task<Match> CreateMatch(Device device, Tournament? tournament, TournamentClass? tClass, Match match) {
      // Create the post data
      Dictionary<string, string?> formData = new() {
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
        { "team2player2team", match.Team2Player2Team },
        { "umpire", match.Umpire },
        { "servicejudge", match.ServiceJudge },
        { "logo1", match.Logo1 },
        { "logo2", match.Logo2 }
      };
      if (tournament != null) {
        formData.Add("tournamentid", tournament.TournamentID.ToString());
      }
      if (tClass != null) {
        formData.Add("classid", tClass.ID.ToString());
      }
      if (!string.IsNullOrEmpty(match.Tag)) {
        formData.Add("tag", match.Tag);
      }
      Match.MatchResponse matchResponse = await SendRequest<Match.MatchResponse>("api/match/create_match", device, formData);
      if (matchResponse.Match != null) {
        return matchResponse.Match;
      }
      throw new Exception("Error when creating match; server returned null");
    }

    /// <summary>
    /// Create a match.
    /// </summary>
    /// <returns>The newly created match</returns>
    /// <param name="device">Device with server credentials</param>
    /// <param name="tournament">The tournament to add the match to. Can be null, in that case the server will
    ///                          try to figure out which tournament to use.</param>
    /// <param name="match">The match to add</param>
    public async Task<Match> CreateOnTheFlyMatch(Device device, Tournament tournament, Match match) {
      return await CreateMatch(device, tournament, null, match);
    }

    /// <summary>
    /// Update an existing match on the server
    /// </summary>
    /// <param name="device">Device with server credentials</param>
    /// <param name="match">Match to update.</param>
    /// <returns></returns>
    public async Task<Match> UpdateMatch(Device device, Match match) {
      // Create POST-data
      Dictionary<string, string?> formData = new() {
        { "matchid", match.MatchID.ToString() },
        { "sequencenumber", match.TournamentMatchNumber.ToString() },
        { "starttime", match.StartTime.ToString("yyyy-MM-dd HH:mm") },
        { "umpire", match.Umpire },
        { "servicejudge", match.ServiceJudge },
        { "logo1", match.Logo1 },
        { "logo2", match.Logo2 }
      };
      if (!string.IsNullOrEmpty(match.Tag)) {
        formData.Add("tag", match.Tag);
      }
      // Update match
      Match.MatchResponse matchResponse = await SendRequest<Match.MatchResponse>("api/match/update_match", device, formData);
      if (matchResponse.Match == null) {
        throw new Exception("Error when updating match; server returned null");
      }
      Match updatedMatch = matchResponse.Match;
      Category matchCategory = Category.FromString(updatedMatch.Category ?? string.Empty);
      // Assing players
      for (int i = 0; i <= 3; i += matchCategory.IsSingles() ? 2 : 1) {
        var playerStrings = match.GetPlayerAtIndex(i);
        if (string.IsNullOrEmpty(playerStrings.Item1)) {
          updatedMatch = await DetachPlayer(device, match.MatchID, i);
        } else {
          updatedMatch = await AttachPlayer(device, match.MatchID, i, playerStrings);
        }
      }
      return updatedMatch;
    }

    /// <summary>
    /// Attach (add) a player to a match
    /// </summary>
    /// <param name="device">Device with server credentials</param>
    /// <param name="matchid">ID of match to update</param>
    /// <param name="playerIndex">Zero-based index of the player (0 = player 1 of team 1, 1 = player 2 of team 1 etc.)</param>
    /// <param name="entry">Name and team of the player to attach</param>
    /// <returns></returns>
    public async Task<Match> AttachPlayer(Device device, int matchid, int playerIndex, (string playerName, string playerTeam) entry) {
      Dictionary<string, string?> playerData = new() {
          { "matchid", matchid.ToString() },
          { "playerIndex", playerIndex.ToString() },
          { "name", entry.playerName },
          { "team", entry.playerTeam }
        };
      Match.MatchResponse updatedMatch = await SendRequest<Match.MatchResponse>("api/player/attach_player", device, playerData);
      return updatedMatch?.Match ?? throw new Exception("Could not attach player; server returned null");
    }

    /// <summary>
    /// Detach (remove) a player from a match
    /// </summary>
    /// <param name="device">Device with server credentials</param>
    /// <param name="matchid">ID of match to detach player from</param>
    /// <param name="playerIndex">Zero-based index of the player (0 = player 1 of team 1, 1 = player 2 of team 1 etc.)</param>
    /// <returns></returns>
    public async Task<Match> DetachPlayer(Device device, int matchid, int playerIndex) {
      Dictionary<string, string?> playerData = new() {
          { "matchid", matchid.ToString() },
          { "playerIndex", playerIndex.ToString() }
        };
      Match.MatchResponse updatedMatch = await SendRequest<Match.MatchResponse>("api/player/detach_player", device, playerData);
      return updatedMatch?.Match ?? throw new Exception("Could not detach player; server returned null");
    }

    /// <summary>
    /// Upload the scores for a match to the server
    /// </summary>
    /// <param name="device">Device with server credentials</param>
    /// <param name="match">Match to set scores for.</param>
    public async Task<Match> SetScore(Device device, MatchExtended match) {
      // Create POST-data
      Dictionary<string, string?> formData = new() {
        { "matchid", match.MatchID.ToString() },
        { "team1set1", match.Team1Set1.ToString() },
        { "team1set2", match.Team1Set2.ToString() },
        { "team1set3", match.Team1Set3.ToString() },
        { "team1set4", match.Team1Set4.ToString() },
        { "team1set5", match.Team1Set5.ToString() },
        { "team2set1", match.Team2Set1.ToString() },
        { "team2set2", match.Team2Set2.ToString() },
        { "team2set3", match.Team2Set3.ToString() },
        { "team2set4", match.Team2Set4.ToString() },
        { "team2set5", match.Team2Set5.ToString() }
      };
      // Add any specials (w/o etc.)
      if (!String.IsNullOrEmpty(match.Special) && (match.Special != "none")) {
        if (match.Status == "team1won") {
          formData.Add("team2special", match.Special);
        }
        if (match.Status == "team2won") {
          formData.Add("team1special", match.Special);
        }
      }
      // Set score
      Match.MatchResponse matchResponse = await SendRequest<Match.MatchResponse>("api/match/set_score", device, formData);
      return matchResponse?.Match ?? throw new Exception("Could not set score; server returned null");

    }

    /// <summary>
    /// Get a list of all courts available for a device.
    /// </summary>
    /// <returns>Available courts</returns>
    /// <param name="device">Device to look up courts for.</param>
    public async Task<List<Court>> GetCourts(Device device) {
      // Create the post data.
      Dictionary<string, string?> formData = new() {
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
      Dictionary<string, string?> formData = new() {
        { "courtid", court.CourtID.ToString() },
        { "matchid", match.MatchID.ToString() }
      };
      await SendRequest<ScoreboardResponse>("api/court/assign_match", device, formData);
    }

    /// <summary>
    /// Clear a court (remove any assigned match)
    /// </summary>
    /// <returns></returns>
    /// <param name="device">Device credentials</param>
    /// <param name="court">Court to be cleared</param>
    public async Task ClearCourt(Device device, Court court) {
      // Create the post data.
      Dictionary<string, string?> formData = new() {
        { "courtid", court.CourtID.ToString() },
        { "matchid", "0" }
      };
      await SendRequest<ScoreboardResponse>("api/court/assign_match", device, formData);
    }

    /// <summary>
    /// Find match by sequence number.
    /// </summary>
    /// <returns></returns>
    /// <param name="device">Device credentials</param>
    /// <param name="tournament">Tournament to search match in</param>
    /// <param name="tournamentMatchNumber">Tournament match number</param>
    public async Task<List<Match>> FindMatchBySequenceNumber(Device device, Tournament tournament, int tournamentMatchNumber) {
      // Create the post data.
      Dictionary<string, string?> formData = new() {
        { "tournamentid", tournament.TournamentID.ToString() },
        { "sequencenumber", tournamentMatchNumber.ToString() }
      };
      Match.MatchesResponse response = await SendRequest<Match.MatchesResponse>("api/match/get_matches", device, formData);
      return response.Matches;
    }

    /// <summary>
    /// Find match by its tag.
    /// </summary>
    /// <param name="device">Device credentials</param>
    /// <param name="tag">Tag to search for. Should be a 64 character string.</param>
    /// <returns>A list with matches with the supplied tag.</returns>
    public async Task<List<Match>> FindMatchByTag(Device device, string tag) {
      // Create the post data
      Dictionary<string, string?> formData = new() {
        { "tag", tag }
      };
      Match.MatchesResponse response = await SendRequest<Match.MatchesResponse>("api/match/get_matches", device, formData);
      return response.Matches;
    }

    /// <summary>
    /// Get all matches for a specific tournament class
    /// </summary>
    /// <param name="device">Device credentials</param>
    /// <param name="classId">ID of class to find matches for</param>
    /// <returns></returns>
    public async Task<List<Match>> FindMatchesByClass(Device device, int classId) {
      // Create the post data.
      Dictionary<string, string?> formData = new() {
        { "classid", classId.ToString() }
      };
      Match.MatchesResponse response = await SendRequest<Match.MatchesResponse>("api/match/get_matches", device, formData);
      return response.Matches;
    }

    /// <summary>
    /// Create a tournament class on the server
    /// </summary>
    /// <param name="device">Device credentials</param>
    /// <param name="tournament">Tournament to add class to</param>
    /// <param name="tournamentClass">Class to add</param>
    public async Task<TournamentClass> CreateTournamentClass(Device device, Tournament tournament, TournamentClass tournamentClass) {
      // Create post data
      Dictionary<string, string?> formData = new() {
        { "description", tournamentClass.Description },
        { "tournamentid", tournament.TournamentID.ToString() },
        { "size", tournamentClass.Size.ToString() },
        { "classtype", tournamentClass.ClassType },
        { "category", tournamentClass.Category }
      };
      if (tournamentClass.ParentClassID > 0) {
        formData.Add("parentclass", tournamentClass.ParentClassID.ToString());
      }
      TournamentClass.TournamentClassResponse response = await SendRequest<TournamentClass.TournamentClassResponse>("api/tournament/add_class", device, formData);
      return response?.TournamentClass ?? throw new Exception("Could create class; server returned null");
    }

    public async Task<Link> CreateLink(Device device, Tournament tournament, Link link) {
      // Create post data
      Dictionary<string, string?> formData = new() {
        { "tournamentid", tournament.TournamentID.ToString() },
        { "sourceclass", link.SourceClassID.ToString() },
        { "sourceplace", link.SourcePlace.ToString() },
        { "targetmatch", link.TargetMatchID.ToString() },
        { "targetteam",  link.TargetTeam }
      };
      // Send
      Link.LinkResponse response = await SendRequest<Link.LinkResponse>("api/tournament/create_link", device, formData);
      return response?.Link ?? throw new Exception("Could create link; server returned null");
    }

    /// <summary>
    /// Send a request to the scoreboard server
    /// </summary>
    /// <returns>Server response</returns>
    /// <param name="route">Server route</param>
    /// <param name="credentials">Device credentials. Null if no authorization should be used.</param>
    /// <param name="postParams">Post parameters. Can be null if no parameters.</param>
    /// <typeparam name="T">The type of response to expect from the server.</typeparam>
    private async Task<T> SendRequest<T>(string route, Device? credentials, Dictionary<string, string?>? postParams) where T : ScoreboardResponse {
      // Create the request content.
      Dictionary<string, string?> formData = postParams != null ? new Dictionary<string, string?>(postParams) : [];
      formData.Add("randomStuff", Guid.NewGuid().ToString("n"));
      HttpContent content = new FormUrlEncodedContent(formData);
      // Create the request
      HttpRequestMessage request = new(HttpMethod.Post, string.Format("{0}{1}", AppendSlash(BaseUrl), route)) {
        Content = content
      };
      // Check if to use authentication
      if (credentials != null) {
        // Create the HMAC from the http content
        string authentication = await CalculateHMAC(credentials, content);
        request.Headers.Add("Authorization", authentication);
      }
      // Send the request
      HttpResponseMessage response = await TrySendAsync(request);
      // Try and parse the json
      T scoreboardResponse = await TryReadResponse<T>(response);
      // Throw error if request is not successfull
      if (!response.IsSuccessStatusCode) {
        OnError(ApiHelperErrorEventArgs.ErrorStage.HttpError, new ScoreboardLiveApiException(response.StatusCode, scoreboardResponse));
        throw (new ScoreboardLiveApiException(response.StatusCode, scoreboardResponse));
      }
      // Return the response
      return scoreboardResponse;
    }

    /// <summary>
    /// Helper function that tries to send a request. If it fails, it sends an error event and throws an exception.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<HttpResponseMessage> TrySendAsync(HttpRequestMessage request) {
      try {
        return await m_client.SendAsync(request);
      } catch (Exception e) {
        OnError(ApiHelperErrorEventArgs.ErrorStage.ConnectionError, e);
        throw;
      }
    }

    /// <summary>
    /// Helper function that tries to read a json response. If it fails, it returns null unless the
    /// request itself succeeded; in that case there should be valid json available, and this function
    /// throws a json serialization exception.
    /// </summary>
    /// <typeparam name="T">ScoreboardResponse type</typeparam>
    /// <param name="httpResponse">The http request response to try and parse</param>
    /// <returns></returns>
    private async Task<T> TryReadResponse<T>(HttpResponseMessage httpResponse) where T : ScoreboardResponse {
      T? scoreboardResponse = null;
      try {
        scoreboardResponse = await JsonSerializer.DeserializeAsync<T>(await httpResponse.Content.ReadAsStreamAsync(), m_jsonDefaultOptions) as T;
      } catch (Exception e) {
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK) {
          OnError(ApiHelperErrorEventArgs.ErrorStage.ParseError, e);
          throw;
        }
      }
      if (scoreboardResponse != null) {
        return scoreboardResponse;
      }
      throw new Exception("Message deserialization returned null value");
    }

    /// <summary>
    /// Send an error event to all listeners using a different thread
    /// </summary>
    /// <param name="stage">The error type</param>
    /// <param name="exception">Exception thrown</param>
    private void OnError(ApiHelperErrorEventArgs.ErrorStage stage, Exception exception) {
      Task.Run(() => Error?.Invoke(this, new ApiHelperErrorEventArgs(this, stage, exception)));
    }

    /// <summary>
    /// Helper function to append a slash to a string if it doesn't end with one
    /// </summary>
    /// <returns>String with slash appended</returns>
    /// <param name="url">URL to append slash to.</param>
    private static string AppendSlash(string url) {
      if (url.EndsWith('/')) {
        return url;
      }
      return url + "/";
    }

    /// <summary>
    /// Create a hex string from a byte array
    /// </summary>
    /// <param name="ba"></param>
    /// <returns></returns>
    public static string ByteArrayToHexString(byte[] ba) {
      StringBuilder hex = new(ba.Length * 2);
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
    public static async Task<string> CalculateHMAC(Device device, HttpContent content) {
      ArgumentNullException.ThrowIfNull(device.ClientToken);
      string hash;
      using (HMACSHA256 hmac = new(Encoding.ASCII.GetBytes(device.ClientToken))) {
        hash = device.DeviceCode + ByteArrayToHexString(hmac.ComputeHash(await content.ReadAsByteArrayAsync()));
      }
      return hash;
    }

    /// <summary>
    /// Create a HMAC signature for a string, using device credentials.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string CalculateHMAC(Device device, string content) {
      ArgumentNullException.ThrowIfNull(device.ClientToken);
      string hash;
      using (HMACSHA256 hmac = new(Encoding.ASCII.GetBytes(device.ClientToken))) {
        hash = device.DeviceCode + ByteArrayToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(content)));
      }
      return hash;
    }
  }
}
