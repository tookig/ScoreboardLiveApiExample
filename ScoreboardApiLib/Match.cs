using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace ScoreboardLiveApi {
  [DataContract(Name="match")]
  public class Match {
    
    [DataContract(Name="matchResponse")]
    public class MatchResponse : ScoreboardResponse {
      [DataMember(Name = "match")]
      public Match Match { get; set; }
    }

    [DataContract(Name = "matchesResponse")]
    public class MatchesResponse : ScoreboardResponse {
      [DataMember(Name = "matches")]
      public List<Match> Matches { get; set; }

      public MatchesResponse() {
        Matches = new List<Match>();
      }
    }

    [DataMember(Name="matchid")]
    public int MatchID { get; set; }

    [DataMember(Name = "sequencenumber")]
    public int TournamentMatchNumber { get; set; }

    [DataMember(Name="team1player1name")]
    public string Team1Player1Name { get; set; }
    [DataMember(Name = "team1player1team")]
    public string Team1Player1Team { get; set; }

    [DataMember(Name = "team1player2name")]
    public string Team1Player2Name { get; set; }
    [DataMember(Name = "team1player2team")]
    public string Team1Player2Team { get; set; }

    [DataMember(Name = "team2player1name")]
    public string Team2Player1Name { get; set; }
    [DataMember(Name = "team2player1team")]
    public string Team2Player1Team { get; set; }

    [DataMember(Name = "team2player2name")]
    public string Team2Player2Name { get; set; }
    [DataMember(Name = "team2player2team")]
    public string Team2Player2Team { get; set; }

    [DataMember(Name = "status")]
    public string Status { get; set; }

    [DataMember(Name = "category")]
    public string Category { get; set; }

    [DataMember(Name = "starttime")]
    private string JsonStartTime { get; set; }

    [IgnoreDataMember]
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
