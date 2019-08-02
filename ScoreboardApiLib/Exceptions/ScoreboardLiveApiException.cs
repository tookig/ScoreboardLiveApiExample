using System;
using System.Net;
using System.Collections.Generic;

namespace ScoreboardLiveApi.Exceptions {
  /// <summary>
  /// Scoreboard live API exception.
  /// </summary>
  public class ScoreboardLiveApiException: Exception {
    /// <summary>
    /// The http request status code.
    /// The Scoreboard Live API can return 4 different status codes:
    /// 500 - Internal server error. Not much to do about this, other than contacting the Scoreboard Live admins.
    /// 400 - Bad request. Some parameters was missing in the api request. More info on what is missing can be found
    ///       in the ScoreboardApiErrors message list.
    /// 403 - Forbidden. Something is wrong with the authentication process. The HMAC might be wrongly generated,
    ///       or the client token may have been revoked or expired. Sometimes there is more info in the 
    ///       ScoreboardApiErrors message list.
    /// 200 - OK. Request was sent and processed ok.
    /// </summary>
    /// <value>The status code.</value>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// A list with error descriptions. For example, if the api was expecting parameters that was not in the request,
    /// a list with descriptive texts of what was missing will be returned by the server.
    /// </summary>
    /// <value>The scoreboard API errors.</value>
    public List<string> ScoreboardApiErrors { get; set; }

    public ScoreboardLiveApiException(HttpStatusCode statusCode, ScoreboardResponse response) {
    }
  }
}
