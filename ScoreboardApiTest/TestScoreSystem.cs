using ScoreboardLiveApi;
using System.Reflection;

namespace ScoreboardApiTest {
  [TestClass]
  public class TestScoreSystem {
    [TestMethod]
    public void TestTestScoreSystemFromString() {
      Assert.AreEqual(ScoreSystem.Elitserien, ScoreSystem.FromString("elitserien"));
      Assert.AreEqual(ScoreSystem.FiveSet11, ScoreSystem.FromString("5set11"));
      Assert.AreEqual(ScoreSystem.FiveSet11Max15, ScoreSystem.FromString("5set11max15"));
      Assert.AreEqual(ScoreSystem.Standard, ScoreSystem.FromString("standard"));
      Assert.ThrowsException<ArgumentException>(() => Special.FromString("invalid"));
    }

    [TestMethod]
    public void TestTestScoreSystemToString() {
      Assert.AreEqual("elitserien", ScoreSystem.Elitserien.ToString());
      Assert.AreEqual("5set11", ScoreSystem.FiveSet11.ToString());
      Assert.AreEqual("5set11max15", ScoreSystem.FiveSet11Max15.ToString());
      Assert.AreEqual("standard", ScoreSystem.Standard.ToString());
    }

    [TestMethod]
    public void TestTestScoreSystemEquals() {
      Assert.AreEqual(ScoreSystem.Elitserien, ScoreSystem.Elitserien);
      Assert.AreEqual(ScoreSystem.FiveSet11, ScoreSystem.FiveSet11);
      Assert.AreEqual(ScoreSystem.FiveSet11Max15, ScoreSystem.FiveSet11Max15);
      Assert.AreEqual(ScoreSystem.Standard, ScoreSystem.Standard);
    }

    [TestMethod]
    public void TestTestScoreSystemNotEquals() {
      Assert.AreNotEqual(ScoreSystem.Elitserien, ScoreSystem.FiveSet11);
      Assert.AreNotEqual(ScoreSystem.FiveSet11Max15, ScoreSystem.FiveSet11);
      Assert.AreNotEqual(ScoreSystem.Standard, ScoreSystem.FiveSet11Max15);
      Assert.AreNotEqual(ScoreSystem.Standard, ScoreSystem.Elitserien);
    }

    [TestMethod]
    public void TestTestScoreSystemImplicitString() {
      ScoreSystem standard = ScoreSystem.Standard;
      string value = standard;
      Assert.AreEqual("standard", value);
    }

    [TestMethod]
    public void TestTestScoreSystemImplicitSpecial() {
      string value = "5set11";
      ScoreSystem fiveset11 = value;
      Assert.AreEqual(ScoreSystem.FiveSet11, fiveset11);
    }
  }
}