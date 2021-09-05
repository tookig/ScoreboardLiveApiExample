using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text;

namespace ScoreboardLiveApi {
  public class Match {
    public class MatchResponse : ScoreboardResponse {
      [JsonPropertyName("match")]
      public Match Match { get; set; }
    }

    public class MatchesResponse : ScoreboardResponse {
      [JsonPropertyName("matches")]
      public List<Match> Matches { get; set; }

      public MatchesResponse() {
        Matches = new List<Match>();
      }
    }

    [JsonPropertyName("matchid"), JsonConverter(typeof(Converters.IntToString))]
    public int MatchID { get; set; }

    [JsonPropertyName("sequencenumber"), JsonConverter(typeof(Converters.IntToString))]
    public int TournamentMatchNumber { get; set; }

    [JsonPropertyName("place"), JsonConverter(typeof(Converters.IntToString))]
    public int Place { get; set; }

    [JsonPropertyName("team1player1name")]
    public string Team1Player1Name { get; set; }
    [JsonPropertyName("team1player1team")]
    public string Team1Player1Team { get; set; }

    [JsonPropertyName("team1player2name")]
    public string Team1Player2Name { get; set; }
    [JsonPropertyName("team1player2team")]
    public string Team1Player2Team { get; set; }

    [JsonPropertyName("team2player1name")]
    public string Team2Player1Name { get; set; }
    [JsonPropertyName("team2player1team")]
    public string Team2Player1Team { get; set; }

    [JsonPropertyName("team2player2name")]
    public string Team2Player2Name { get; set; }
    [JsonPropertyName("team2player2team")]
    public string Team2Player2Team { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("starttime")]
    public string JsonStartTime { get; set; }

    [JsonIgnore]
    public DateTime StartTime {
      get {
        return DateTime.ParseExact(JsonStartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
      }
      set {
        JsonStartTime = value.ToString("yyyy-MM-dd HH:mm:00");
      }
    }

    private static Dictionary<string, string> categoriesDescription = new Dictionary<string, string> {
      { "ms", "Men's singles" },
      { "md", "Men's doubles" },
      { "ws", "Women's singles" },
      { "wd", "Women's doubles" },
      { "xd", "Mixed doubles" }
    };

    public (string, string) GetPlayerAtIndex(int i) {
      if ((i < 0) || (i > 3)) throw new ArgumentOutOfRangeException("i", "Player index must be in range 0..3");
      int t = (i / 2) + 1;
      int p = (i % 2) + 1;
      string name = this.GetType().GetProperty(string.Format("Team{0}Player{1}Name", t, p)).GetValue(this, null) as string;
      string team = this.GetType().GetProperty(string.Format("Team{0}Player{1}Team", t, p)).GetValue(this, null) as string;
      return (name, team);
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("{0} {1} {2}{3}",
        categoriesDescription[this.Category],
        TournamentMatchNumber > 0 ? string.Format("({0})", TournamentMatchNumber) : "",
        string.IsNullOrEmpty(JsonStartTime) ? "" : JsonStartTime,
        Environment.NewLine
      );
      sb.AppendLine("----------------------------------------------------");
      sb.AppendFormat("{0, -20}    {1, -20}{2}", Team1Player1Name, Team2Player1Name, Environment.NewLine);
      sb.AppendFormat("{0, -20} vs {1, -20}{2}", Team1Player1Team, Team2Player1Team, Environment.NewLine);
      if (Category.EndsWith("d")) {
        sb.AppendFormat("{0, -20}    {1, -20}{2}", Team1Player2Name, Team2Player2Name, Environment.NewLine);
        sb.AppendFormat("{0, -20}    {1, -20}{2}", Team1Player2Team, Team2Player2Team, Environment.NewLine);
      }
      return sb.ToString();
    }
  }
}
