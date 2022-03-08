using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text;

namespace ScoreboardLiveApi {
  public class Tournament {
    public class TournamentResponse : ScoreboardResponse {
      [JsonPropertyName("tournaments")]
      public List<Tournament> Tournaments { get; set; }
      public TournamentResponse() {
        Tournaments = new List<Tournament>();
      }
    }

    public class SingleTournamentResponse : ScoreboardResponse {
      [JsonPropertyName("tournament")]
      public Tournament Tournament { get; set; }
    }

    [JsonPropertyName("tournamentid"), JsonConverter(typeof(Converters.IntToString))]
    public int TournamentID { get; set; }

    [JsonPropertyName("parenttournamentid"), JsonConverter(typeof(Converters.IntToString))]
    public int ParentTournamentID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string TournamentType { get; set; }

    [JsonPropertyName("team1")]
    public string Team1 { get; set; }

    [JsonPropertyName("team2")]
    public string Team2 { get; set; }

    [JsonPropertyName("startdate")]
    public string JsonStartDate { get; set; }
    [JsonIgnore]
    public DateTime StartDate {
      get {
        return DateTime.ParseExact(JsonStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
      }
    }

    [JsonPropertyName("enddate")]
    public string JsonEndDate { get; set; }
    [JsonIgnore]
    public DateTime EndDate {
      get {
        return DateTime.ParseExact(JsonEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
      }
    }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("scoresystem")]
    public string ScoreSystem { get; set; }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append(Name);
      if (!string.IsNullOrEmpty(Team1) && !string.IsNullOrEmpty(Team2)) {
        sb.AppendFormat(" {0} - {1}", Team1, Team2);
      }
      sb.AppendFormat(" ({0})", StartDate.ToShortDateString());
      return sb.ToString();
    }
  }
}
