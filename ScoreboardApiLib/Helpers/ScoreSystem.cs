using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardLiveApi {
  public class ScoreSystem {
    public string Value { get; set; }
    private ScoreSystem(string value) {
      Value = value;
    }

    public static ScoreSystem Standard   { get { return new ScoreSystem("standard"); } }
    public static ScoreSystem Elitserien { get { return new ScoreSystem("elitserien"); } }
    public static ScoreSystem FiveSet11 { get { return new ScoreSystem("5set11"); } }
    public static ScoreSystem FiveSet11Max15 { get { return new ScoreSystem("5set11max15"); } }

    public static ScoreSystem FromString(string value) {
      switch (value) {
        case "standard": return Standard;
        case "elitserien": return Elitserien;
        case "5set11": return FiveSet11;
        case "5set11max15": return FiveSet11Max15;
        default: throw new ArgumentException("Invalid score system value");
      }
    }

    public override string ToString() {
      return Value;
    }

    public override bool Equals(object obj) {
      if (obj == null || GetType() != obj.GetType()) {
        return false;
      }
      ScoreSystem s = (ScoreSystem)obj;
      return Value == s.Value;
    }

    public override int GetHashCode() {
      return Value.GetHashCode();
    }

    public static bool operator == (ScoreSystem s1, ScoreSystem s2) {
      return s1.Equals(s2);
    }

    public static bool operator != (ScoreSystem s1, ScoreSystem s2) {
      return !s1.Equals(s2);
    }

    public static implicit operator string(ScoreSystem s) {
      return s.Value;
    }

    public static implicit operator ScoreSystem(string s) {
      return FromString(s);
    }
  }
}
