using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardLiveApi {
  public class Special {
    public string Value { get; set; }
    private Special(string value) {
      Value = value;
    }

    public static Special None { get { return new Special("none"); } }
    public static Special WalkOver { get { return new Special("walkover"); } }
    public static Special Disqualified { get { return new Special("disqualified"); } }
    public static Special Retired { get { return new Special("retired"); } }

    public static Special FromString(string value) {
      if (string.IsNullOrEmpty(value)) return None;
      switch (value) {
        case "none": return None;
        case "walkover": return WalkOver;
        case "disqualified": return Disqualified;
        case "retired": return Retired;
        default: throw new ArgumentException("Invalid special value");
      }
    }

    public override string ToString() {
      return Value;
    }

    public override bool Equals(object? obj) {
      if (obj == null || GetType() != obj.GetType()) {
        return false;
      }
      Special s = (Special)obj;
      return Value == s.Value;
    }

    public override int GetHashCode() {
      return Value.GetHashCode();
    }

    public static bool operator ==(Special s1, Special s2) {
      return s1.Equals(s2);
    }

    public static bool operator !=(Special s1, Special s2) {
      return !s1.Equals(s2);
    }

    public static implicit operator string(Special s) {
      return s.Value;
    }

    public static implicit operator Special(string s) {
      return FromString(s);
    }
  }
}
