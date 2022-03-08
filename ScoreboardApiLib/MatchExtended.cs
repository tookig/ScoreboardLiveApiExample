using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text;

namespace ScoreboardLiveApi {
  public class MatchExtended: Match {
    [JsonPropertyName("tournamentid")] //, JsonConverter(typeof(Converters.IntToString))]
    public int TournamentID { get; set; }

    [JsonPropertyName("classid")] //, JsonConverter(typeof(Converters.IntToString))]
    public int ClassID { get; set; }

    [JsonPropertyName("classdescription")]
    public string ClassDescription { get; set; }

    [JsonPropertyName("scoresystem")]
    public string Scoresystem { get; set; }

    [JsonPropertyName("team1player1id")]
    public int Team1Player1ID { get; set; }
    [JsonPropertyName("team1player2id")]
    public int Team1Player2ID { get; set; }
    [JsonPropertyName("team2player1id")]
    public int Team2Player1ID { get; set; }
    [JsonPropertyName("team2player2id")]
    public int Team2Player2ID { get; set; }

    [JsonPropertyName("team1set1")]
    public int Team1Set1 { get; set; }
    [JsonPropertyName("team1set2")]
    public int Team1Set2 { get; set; }
    [JsonPropertyName("team1set3")]
    public int Team1Set3 { get; set; }
    [JsonPropertyName("team1set4")]
    public int Team1Set4 { get; set; }
    [JsonPropertyName("team1set5")]
    public int Team1Set5 { get; set; }

    [JsonPropertyName("team2set1")]
    public int Team2Set1 { get; set; }
    [JsonPropertyName("team2set2")]
    public int Team2Set2 { get; set; }
    [JsonPropertyName("team2set3")]
    public int Team2Set3 { get; set; }
    [JsonPropertyName("team2set4")]
    public int Team2Set4 { get; set; }
    [JsonPropertyName("team2set5")]
    public int Team2Set5 { get; set; }

    [JsonPropertyName("server")]
    public int Server { get; set; }
    [JsonPropertyName("ballcount")]
    public int BallCount { get; set; }
    [JsonPropertyName("serversequence")]
    public int ServerSequence { get; set; }

    [JsonPropertyName("special")]
    public string Special { get; set; }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.AppendFormat("{0, -2} {1, -2} {2, -2} {3, -2} {4, -2}{5}", Team1Set1, Team1Set2, Team1Set3, Team1Set4, Team1Set5, Environment.NewLine);
      sb.AppendFormat("{0, -2} {1, -2} {2, -2} {3, -2} {4, -2}{5}", Team2Set1, Team2Set2, Team2Set3, Team2Set4, Team2Set5, Environment.NewLine);
      return sb.ToString();
    }
  }
}
