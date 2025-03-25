using ScoreboardApiLib.Helpers;
using ScoreboardLiveApi;
using System.Reflection;

namespace ScoreboardApiTest {
  [TestClass]
  public class TestSeriesSetup {
    [TestMethod]
    public void TestConstructor() {
      SeriesSetup ss11111 = new SeriesSetup();
      Assert.AreEqual(ss11111.ToString(), "11111");
      SeriesSetup ss00000 = new SeriesSetup(0,0,0,0,0);
      Assert.AreEqual(ss00000.ToString(), "00000");
      SeriesSetup ss12345 = new SeriesSetup(1,2,3,4,5);
      Assert.AreEqual(ss12345.ToString(), "12345");
      SeriesSetup ss99999 = new SeriesSetup(9,9,9,9,9);
      Assert.AreEqual(ss99999.ToString(), "99999");
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SeriesSetup(10, 1, 1, 1, 1));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SeriesSetup(1, -1, 1, 1, 1));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SeriesSetup(1, 1, 18, 1, 1));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SeriesSetup(1, 1, 1, int.MinValue, 1));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SeriesSetup(1, 1, 1, 1, int.MaxValue));
    }

    [TestMethod]
    public void TestSet() {
      SeriesSetup ss = new(0, 0, 0, 0, 0);
      Assert.AreEqual(ss.ToString(), "00000");
      ss.Set(Category.MensSingles, 1);
      Assert.AreEqual(ss.ToString(), "10000");
      ss.Set(Category.WomensSingles, 2);
      Assert.AreEqual(ss.ToString(), "12000");
      ss.Set(Category.MensDoubles, 3);
      Assert.AreEqual(ss.ToString(), "12300");
      ss.Set(Category.WomensDoubles, 4);
      Assert.AreEqual(ss.ToString(), "12340");
      ss.Set(Category.MixedDoubles, 5);
      Assert.AreEqual(ss.ToString(), "12345");
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => ss.Set(Category.MensSingles, -1));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => ss.Set(Category.WomensSingles, 10));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => ss.Set(Category.MensDoubles, int.MinValue));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => ss.Set(Category.WomensDoubles, int.MaxValue));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => ss.Set(Category.MixedDoubles, 10));
    }

  }
}