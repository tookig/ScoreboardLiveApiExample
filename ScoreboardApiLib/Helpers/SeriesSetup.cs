using ScoreboardLiveApi;
using System;

namespace ScoreboardApiLib.Helpers {
  public class SeriesSetup {
    public string Preset { get; init; } = string.Empty;
    private int[] MatchCounts { get; init; } = [1, 1, 1, 1, 1];

    public SeriesSetup() { }

    public SeriesSetup(int mensSingles, int womensSingles, int mensDoubles, int womensDoubles, int mixedDoubles) {
      Validate(mensSingles);
      Validate(womensSingles);
      Validate(mensDoubles);
      Validate(womensDoubles);
      Validate(mixedDoubles);
      MatchCounts = [mensSingles, womensSingles, mensDoubles, womensDoubles, mixedDoubles];
    }

    public void Set(Category category, int count) {
      Validate(count);
      int index = 4;
      if (category == Category.MensSingles) {
        index = 0;
      } else if (category == Category.WomensSingles) {
        index = 1;
      } else if (category == Category.MensDoubles) {
        index = 2;
      } else if (category == Category.WomensDoubles) {
        index = 3;
      }
      MatchCounts[index] = count;
    }

    private void Validate(int count) {
      ArgumentOutOfRangeException.ThrowIfLessThan(count, 0, nameof(count));
      ArgumentOutOfRangeException.ThrowIfGreaterThan(count, 9, nameof(count));
    }

    public override string ToString() {
      if (!string.IsNullOrEmpty(Preset)) {
        return Preset;
      }
      return $"{MatchCounts[0]}{MatchCounts[1]}{MatchCounts[2]}{MatchCounts[3]}{MatchCounts[4]}";
    }
  }
}
