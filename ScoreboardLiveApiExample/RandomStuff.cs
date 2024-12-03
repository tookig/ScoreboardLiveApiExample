using System;
using ScoreboardLiveApi;

namespace ScoreboardLiveApiExample {
  public static class RandomStuff {
    private static string[] maleFirstnames = { "Eric", "Fez", "Michael", "Red", "Bob", "Steven" };
    private static string[] femaleFirstnames = { "Donna", "Kitty", "Laurie", "Jackie", "Midge" };
    private static string[] lastNames = { "Forman", "Pinciotti", "Hyde", "Kelso", "Burkhart" };
    private static string[] teamNames = { "Point Place", "Green Bay Packers", "Milton Wildcats", "Ripen Red Hawks", "Lakeland Muskies" };
    private static string[] categories = { "ms", "md", "ws", "wd", "xd" };
    private static Random randomizer = new Random();


    public static string MaleName() {
      return string.Format("{0} {1}",
        maleFirstnames[randomizer.Next(maleFirstnames.Length)],
        lastNames[randomizer.Next(lastNames.Length)]
      );
    }

    public static string FemaleName() {
      return string.Format("{0} {1}",
        femaleFirstnames[randomizer.Next(femaleFirstnames.Length)],
        lastNames[randomizer.Next(lastNames.Length)]
      );
    }

    public static string TeamName() {
      return teamNames[randomizer.Next(teamNames.Length)];
    }

    public static Match RandomMatch() {
      Match match = new Match();
      match.Category = "xd";  //categories[randomizer.Next(categories.Length)];
      match.Team1Player1Name = match.Category.StartsWith("m") || match.Category.StartsWith("x") ? MaleName() : FemaleName();
      match.Team2Player1Name = match.Category.StartsWith("m") || match.Category.StartsWith("x") ? MaleName() : FemaleName();
      match.Team1Player1Team = TeamName();
      match.Team2Player1Team = TeamName();
      if (match.Category.EndsWith("d")) {
        match.Team1Player2Team = TeamName();
        match.Team2Player2Team = TeamName();
        if (match.Category == "md") {
          match.Team1Player2Name = MaleName();
          match.Team2Player2Name = MaleName();
        }
        else if ((match.Category == "wd") || (match.Category == "xd")) {
          match.Team1Player2Name = FemaleName();
          match.Team2Player2Name = FemaleName();
        }
      }
      match.StartTime = DateTime.Now;
      match.TournamentMatchNumber = randomizer.Next(99) + 1;
      match.Umpire = randomizer.Next(2) == 0 ? MaleName() : FemaleName();
      match.ServiceJudge = randomizer.Next(2) == 0 ? MaleName() : FemaleName();
      return match;
    }
  }
}
