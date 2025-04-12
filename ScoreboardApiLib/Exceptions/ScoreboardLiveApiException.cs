using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ScoreboardLiveApi {
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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="statusCode">Http status code</param>
    /// <param name="response">Scoreboard live response</param>
    public ScoreboardLiveApiException(HttpStatusCode statusCode, ScoreboardResponse? response) : base(messageText(statusCode, response)) {
      StatusCode = statusCode;
      ScoreboardApiErrors = [];
      if ((response != null) && (response.Errors != null)) {
        ScoreboardApiErrors.AddRange(response.Errors);
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="statusCode">Http status code</param>
    public ScoreboardLiveApiException(HttpStatusCode statusCode) : this(statusCode, null) {
    }

    private static string messageText(HttpStatusCode statusCode, ScoreboardResponse? response) {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("The request finished with status code {0} ({1}). ", statusCode, (int)statusCode);
      if ((response == null) || (response.Errors.Count == 0)) {
        sb.Append("No additional info was given.");
      } else {
        sb.AppendFormat("The following reason{0} given:{1}", response.Errors.Count > 1 ? "s were" : " was", Environment.NewLine);
        sb.Append(string.Join(string.Format(" - {0}", Environment.NewLine), response.Errors));
      }
      return sb.ToString();
    }
  }
}
