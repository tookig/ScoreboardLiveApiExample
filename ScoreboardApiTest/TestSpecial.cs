using ScoreboardLiveApi;
using System.Reflection;

namespace ScoreboardApiTest {
  [TestClass]
  public class TestSpecial {
    [TestMethod]
    public void TestSpecialFromString() {
      Assert.AreEqual(Special.None, Special.FromString("none"));
      Assert.AreEqual(Special.WalkOver, Special.FromString("walkover"));
      Assert.AreEqual(Special.Disqualified, Special.FromString("disqualified"));
      Assert.AreEqual(Special.Retired, Special.FromString("retired"));
      Assert.ThrowsException<ArgumentException>(() => Special.FromString("invalid"));
    }

    [TestMethod]
    public void TestSpecialToString() {
      Assert.AreEqual("none", Special.None.ToString());
      Assert.AreEqual("walkover", Special.WalkOver.ToString());
      Assert.AreEqual("disqualified", Special.Disqualified.ToString());
      Assert.AreEqual("retired", Special.Retired.ToString());
    }

    [TestMethod]
    public void TestSpecialEquals() {
      Assert.AreEqual(Special.None, Special.None);
      Assert.AreEqual(Special.WalkOver, Special.WalkOver);
      Assert.AreEqual(Special.Disqualified, Special.Disqualified);
      Assert.AreEqual(Special.Retired, Special.Retired);
    }

    [TestMethod]
    public void TestSpecialNotEquals() {
      Assert.AreNotEqual(Special.None, Special.WalkOver);
      Assert.AreNotEqual(Special.None, Special.Disqualified);
      Assert.AreNotEqual(Special.None, Special.Retired);
      Assert.AreNotEqual(Special.WalkOver, Special.Disqualified);
      Assert.AreNotEqual(Special.WalkOver, Special.Retired);
      Assert.AreNotEqual(Special.Disqualified, Special.Retired);
    }

    [TestMethod]
    public void TestSpecialImplicitString() {
      Special special = Special.None;
      string value = special;
      Assert.AreEqual("none", value);
    }

    [TestMethod]
    public void TestSpecialImplicitSpecial() {
      string value = "none";
      Special special = value;
      Assert.AreEqual(Special.None, special);
    }
  }
}