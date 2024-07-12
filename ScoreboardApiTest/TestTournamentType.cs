using ScoreboardLiveApi;
using System.Reflection;

namespace ScoreboardApiTest {
  [TestClass]
  public class TestTournamentType {
    [TestMethod]
    public void TestTournamentTypeFromString() {
      Assert.AreEqual(TournamentType.Individual, TournamentType.FromString("individual"));
      Assert.AreEqual(TournamentType.MultiSeries, TournamentType.FromString("multiseries"));
      Assert.AreEqual(TournamentType.Series, TournamentType.FromString("series"));
      Assert.ThrowsException<ArgumentException>(() => TournamentType.FromString("invalid"));
    }

    [TestMethod]
    public void TestTournamentTypeToString() {
      Assert.AreEqual("individual", TournamentType.Individual.ToString());
      Assert.AreEqual("multiseries", TournamentType.MultiSeries.ToString());
      Assert.AreEqual("series", TournamentType.Series.ToString());
    }

    [TestMethod]
    public void TestTournamentTypeEquals() {
      Assert.AreEqual(TournamentType.Individual, TournamentType.Individual);
      Assert.AreEqual(TournamentType.Series, TournamentType.Series);
      Assert.AreEqual(TournamentType.MultiSeries, TournamentType.MultiSeries);
    }

    [TestMethod]
    public void TestTournamentTypeNotEquals() {
      Assert.AreNotEqual(TournamentType.Individual, TournamentType.Series);
      Assert.AreNotEqual(TournamentType.Individual, TournamentType.MultiSeries);
      Assert.AreNotEqual(TournamentType.Series, TournamentType.MultiSeries);
    }

    [TestMethod]
    public void TestTournamentTypeImplicitString() {
      TournamentType tournamentType = TournamentType.Individual;
      string value = tournamentType;
      Assert.AreEqual("individual", value);
    }

    [TestMethod]
    public void TestTournamentTypeImplicitSpecial() {
      string value = "series";
      TournamentType tournamentType = value;
      Assert.AreEqual(TournamentType.Series, tournamentType);
    }
  }
}