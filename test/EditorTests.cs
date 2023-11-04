using Xunit;

namespace EditorTest
{
    public class EditorTests
    {
        [Fact]
        public void GetModifierID()
        {
            Assert.Equal("Bullet Data", Editor.ModifierNamer.Get(Editor.ModifierID.BUNDLE_REF_BULLET));
        }


        
    }
}