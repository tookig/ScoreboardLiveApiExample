using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text;
using System.Linq;

namespace ScoreboardLiveApi {
  public class MatchExtended: Match {
    public class MatchSet {
      public int Team1Score { get; set; }
      public int Team2Score { get; set; }

      public int this[int index] {
        get {
          if (index < 1 || index > 2) throw new IndexOutOfRangeException("Team index can be a 1 or a 2");
          return index == 1 ? Team1Score : Team2Score;
        }
        set {
          if (index < 1 || index > 2) throw new IndexOutOfRangeException("Team index can be a 1 or a 2");
          if (index == 1) {
            Team1Score = value;
          } else {
            Team2Score = value;
          }
        }
      }
    }

    public class MatchSets {
      private MatchSet[] sets = new MatchSet[5];

      public MatchSets() {
        for (int i=0; i<sets.Length; i++) {
          sets[i] = new MatchSet();
        }
      }

      public MatchSet this[int index] {
        get {
          if (index < 1 || index > 5) throw new IndexOutOfRangeException("Set index can be between 1 and 5");
          return sets[index - 1];
        }
      }
    }

    [JsonIgnore]
    public MatchSets Sets { get; } = new MatchSets();

    [JsonPropertyName("tournamentid")] //, JsonConverter(typeof(Converters.IntToString))]
    public int TournamentID { get; set; }

    [JsonPropertyName("classid")] //, JsonConverter(typeof(Converters.IntToString))]
    public int ClassID { get; set; }

    [JsonPropertyName("classdescription")]
    public string? ClassDescription { get; set; }

    [JsonPropertyName("scoresystem")]
    public string? Scoresystem { get; set; }

    [JsonPropertyName("team1player1id")]
    public int Team1Player1ID { get; set; }
    [JsonPropertyName("team1player2id")]
    public int Team1Player2ID { get; set; }
    [JsonPropertyName("team2player1id")]
    public int Team2Player1ID { get; set; }
    [JsonPropertyName("team2player2id")]
    public int Team2Player2ID { get; set; }

    [JsonPropertyName("team1set1")]
    public int Team1Set1 { 
      get { 
        return Sets[1][1];
      }
      set {
        Sets[1][1] = value;
      }
    }

    [JsonPropertyName("team1set2")]
    public int Team1Set2 {
      get {
        return Sets[2][1];
      }
      set {
        Sets[2][1] = value;
      }
    }

    [JsonPropertyName("team1set3")]
    public int Team1Set3 {
      get {
        return Sets[3][1];
      }
      set {
        Sets[3][1] = value;
      }
    }

    [JsonPropertyName("team1set4")]
    public int Team1Set4 {
      get {
        return Sets[4][1];
      }
      set {
        Sets[4][1] = value;
      }
    }

    [JsonPropertyName("team1set5")]
    public int Team1Set5 {
      get {
        return Sets[5][1];
      }
      set {
        Sets[5][1] = value;
      }
    }

    [JsonPropertyName("team2set1")]
    public int Team2Set1 {
      get {
        return Sets[1][2];
      }
      set {
        Sets[1][2] = value;
      }
    }

    [JsonPropertyName("team2set2")]
    public int Team2Set2 {
      get {
        return Sets[2][2];
      }
      set {
        Sets[2][2] = value;
      }
    }

    [JsonPropertyName("team2set3")]
    public int Team2Set3 {
      get {
        return Sets[3][2];
      }
      set {
        Sets[3][2] = value;
      }
    }

    [JsonPropertyName("team2set4")]
    public int Team2Set4 {
      get {
        return Sets[4][2];
      }
      set {
        Sets[4][2] = value;
      }
    }

    [JsonPropertyName("team2set5")]
    public int Team2Set5 {
      get {
        return Sets[5][2];
      }
      set {
        Sets[5][2] = value;
      }
    }

    [JsonPropertyName("server")]
    public int Server { get; set; }
    [JsonPropertyName("ballcount")]
    public int BallCount { get; set; }
    [JsonPropertyName("serversequence")]
    public int ServerSequence { get; set; }

    [JsonPropertyName("special")]
    public string? Special { get; set; }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.AppendFormat("{0, -2} {1, -2} {2, -2} {3, -2} {4, -2}{5}", Team1Set1, Team1Set2, Team1Set3, Team1Set4, Team1Set5, Environment.NewLine);
      sb.AppendFormat("{0, -2} {1, -2} {2, -2} {3, -2} {4, -2}{5}", Team2Set1, Team2Set2, Team2Set3, Team2Set4, Team2Set5, Environment.NewLine);
      return sb.ToString();
    }
  }
}
