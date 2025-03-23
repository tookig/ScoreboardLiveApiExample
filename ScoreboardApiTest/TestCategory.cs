using ScoreboardLiveApi;
using System.Reflection;

namespace ScoreboardApiTest {
  [TestClass]
  public class TestCategory {
    [TestMethod]
    public void TestCategoryFromString() {
      Assert.AreEqual(Category.MensSingles, Category.FromString("ms"));
      Assert.AreEqual(Category.WomensSingles, Category.FromString("ws"));
      Assert.AreEqual(Category.MensDoubles, Category.FromString("md"));
      Assert.AreEqual(Category.WomensDoubles, Category.FromString("wd"));
      Assert.AreEqual(Category.MixedDoubles, Category.FromString("xd"));
      Assert.ThrowsException<ArgumentException>(() => Category.FromString("invalid"));
    }

    [TestMethod]
    public void TestCategoryToString() {
      Assert.AreEqual("ms", Category.MensSingles.ToString());
      Assert.AreEqual("ws", Category.WomensSingles.ToString());
      Assert.AreEqual("md", Category.MensDoubles.ToString());
      Assert.AreEqual("wd", Category.WomensDoubles.ToString());
      Assert.AreEqual("xd", Category.MixedDoubles.ToString());
    }

    [TestMethod]
    public void TestCategoryEquals() {
      Assert.AreEqual(Category.MensSingles, Category.MensSingles);
      Assert.AreEqual(Category.WomensSingles, Category.WomensSingles);
      Assert.AreEqual(Category.MensDoubles, Category.MensDoubles);
      Assert.AreEqual(Category.WomensDoubles, Category.WomensDoubles);
      Assert.AreEqual(Category.MixedDoubles, Category.MixedDoubles);
    }

    [TestMethod]
    public void TestCagetoryNotEquals() {
      Assert.AreNotEqual(Category.MensSingles, Category.WomensSingles);
      Assert.AreNotEqual(Category.WomensSingles, Category.WomensDoubles);
      Assert.AreNotEqual(Category.MensDoubles, Category.MixedDoubles);
      Assert.AreNotEqual(Category.WomensDoubles, Category.MensSingles);
      Assert.AreNotEqual(Category.MixedDoubles, Category.WomensSingles);
    }

    [TestMethod]
    public void TestCagetoryImplicitString() {
      Category category = Category.MensSingles;
      string value = category;
      Assert.AreEqual("ms", value);
    }

    [TestMethod]
    public void TestCagetoryImplicitSpecial() {
      string value = "xd";
      Category category = value;
      Assert.AreEqual(Category.MixedDoubles, category);
    }
  }
}