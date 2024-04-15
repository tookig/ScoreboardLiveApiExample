using ScoreboardLiveApi;
using System.Reflection;

namespace ScoreboardApiTest {
  [TestClass]
  public class TestMatch {
    private void CheckAllSets((int, int)[] sets, MatchExtended match) {
      Assert.AreEqual(sets[0].Item1, match.Team1Set1);
      Assert.AreEqual(sets[0].Item1, match.Sets[1][1]);
      Assert.AreEqual(sets[0].Item1, match.Sets[1].Team1Score);

      Assert.AreEqual(sets[0].Item2, match.Team2Set1);
      Assert.AreEqual(sets[0].Item2, match.Sets[1][2]);
      Assert.AreEqual(sets[0].Item2, match.Sets[1].Team2Score);

      Assert.AreEqual(sets[1].Item1, match.Team1Set2);
      Assert.AreEqual(sets[1].Item1, match.Sets[2][1]);
      Assert.AreEqual(sets[1].Item1, match.Sets[2].Team1Score);

      Assert.AreEqual(sets[1].Item2, match.Team2Set2);
      Assert.AreEqual(sets[1].Item2, match.Sets[2][2]);
      Assert.AreEqual(sets[1].Item2, match.Sets[2].Team2Score);

      Assert.AreEqual(sets[2].Item1, match.Team1Set3);
      Assert.AreEqual(sets[2].Item1, match.Sets[3][1]);
      Assert.AreEqual(sets[2].Item1, match.Sets[3].Team1Score);

      Assert.AreEqual(sets[2].Item2, match.Team2Set3);
      Assert.AreEqual(sets[2].Item2, match.Sets[3][2]);
      Assert.AreEqual(sets[2].Item2, match.Sets[3].Team2Score);

      Assert.AreEqual(sets[3].Item1, match.Team1Set4);
      Assert.AreEqual(sets[3].Item1, match.Sets[4][1]);
      Assert.AreEqual(sets[3].Item1, match.Sets[4].Team1Score);

      Assert.AreEqual(sets[3].Item2, match.Team2Set4);
      Assert.AreEqual(sets[3].Item2, match.Sets[4][2]);
      Assert.AreEqual(sets[3].Item2, match.Sets[4].Team2Score);

      Assert.AreEqual(sets[4].Item1, match.Team1Set5);
      Assert.AreEqual(sets[4].Item1, match.Sets[5][1]);
      Assert.AreEqual(sets[4].Item1, match.Sets[5].Team1Score);

      Assert.AreEqual(sets[4].Item2, match.Team2Set5);
      Assert.AreEqual(sets[4].Item2, match.Sets[5][2]);
      Assert.AreEqual(sets[4].Item2, match.Sets[5].Team2Score);
    }

    [TestMethod]
    public void TestScore() {
      var match = new MatchExtended();
      (int, int)[] sets = [(1, 2), (3, 4), (5, 6), (7, 8), (9, 10)];

      match.Team1Set1 = sets[0].Item1;
      match.Team1Set2 = sets[1].Item1;
      match.Team1Set3 = sets[2].Item1;
      match.Team1Set4 = sets[3].Item1;
      match.Team1Set5 = sets[4].Item1;

      match.Team2Set1 = sets[0].Item2;
      match.Team2Set2 = sets[1].Item2;
      match.Team2Set3 = sets[2].Item2;
      match.Team2Set4 = sets[3].Item2;
      match.Team2Set5 = sets[4].Item2;

      CheckAllSets(sets, match);
    }


    [TestMethod]
    public void TestScoreReversed() {
      var match = new MatchExtended();
      (int, int)[] sets = [(1, 2), (3, 4), (5, 6), (7, 8), (9, 10)];

      match.Sets[1][1] = sets[0].Item1;
      match.Sets[2][1] = sets[1].Item1;
      match.Sets[3][1] = sets[2].Item1;
      match.Sets[4][1] = sets[3].Item1;
      match.Sets[5][1] = sets[4].Item1;

      match.Sets[1][2] = sets[0].Item2;
      match.Sets[2][2] = sets[1].Item2;
      match.Sets[3][2] = sets[2].Item2;
      match.Sets[4][2] = sets[3].Item2;
      match.Sets[5][2] = sets[4].Item2;

      CheckAllSets(sets, match);
    }
  }
}