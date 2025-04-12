using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardLiveApi {
  public class TournamentType {
    public string Value { get; set; }
    private TournamentType(string value) {
      Value = value;
    }

    public static TournamentType Individual { get { return new TournamentType("individual"); } }
    public static TournamentType Series { get { return new TournamentType("series"); } }
    public static TournamentType MultiSeries { get { return new TournamentType("multiseries"); } }

    public static TournamentType FromString(string value) {
      switch (value) {
        case "individual": return Individual;
        case "series": return Series;
        case "multiseries": return MultiSeries;
        default: throw new ArgumentException("Invalid tournament type value");
      }
    }

    public override string ToString() {
      return Value;
    }

    public override bool Equals(object? obj) {
      if (obj == null || GetType() != obj.GetType()) {
        return false;
      }
      TournamentType s = (TournamentType)obj;
      return Value == s.Value;
    }

    public override int GetHashCode() {
      return Value.GetHashCode();
    }

    public static bool operator ==(TournamentType s1, TournamentType s2) {
      return s1.Equals(s2);
    }

    public static bool operator !=(TournamentType s1, TournamentType s2) {
      return !s1.Equals(s2);
    }

    public static implicit operator string(TournamentType s) {
      return s.Value;
    }

    public static implicit operator TournamentType(string s) {
      return FromString(s);
    }
  }
}
