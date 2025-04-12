using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardLiveApi {
  public class Category {
    public string Value { get; set; }
    private Category(string value) {
      Value = value;
    }

    public static Category MensSingles   { get { return new Category("ms"); } }
    public static Category WomensSingles { get { return new Category("ws"); } }
    public static Category MensDoubles   { get { return new Category("md"); } }
    public static Category WomensDoubles { get { return new Category("wd"); } }
    public static Category MixedDoubles  { get { return new Category("xd"); } }


    public static Category FromString(string value) {
      switch (value) {
        case "ms": return MensSingles;
        case "ws": return WomensSingles;
        case "md": return MensDoubles;
        case "wd": return WomensDoubles;
        case "xd": return MixedDoubles;
        default: throw new ArgumentException("Invalid category type value");
      }
    }

    public bool IsSingles() {
      return Value == MensSingles || Value == WomensSingles;
    }

    public override string ToString() {
      return Value;
    }

    public override bool Equals(object? obj) {
      if (obj == null || GetType() != obj.GetType()) {
        return false;
      }
      Category s = (Category)obj;
      return Value == s.Value;
    }

    public override int GetHashCode() {
      return Value.GetHashCode();
    }

    public static bool operator ==(Category s1, Category s2) {
      return s1.Equals(s2);
    }

    public static bool operator !=(Category s1, Category s2) {
      return !s1.Equals(s2);
    }

    public static implicit operator string(Category s) {
      return s.Value;
    }

    public static implicit operator Category(string s) {
      return FromString(s);
    }
  }
}
