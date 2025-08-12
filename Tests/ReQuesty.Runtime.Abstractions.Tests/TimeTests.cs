using Xunit;

namespace ReQuesty.Runtime.Abstractions.Tests
{
    public class TimeTests
    {
        [Fact]
        public void TestTimeEquality()
        {
            Time time1 = new(10, 30, 0);
            Time time2 = new(new DateTime(2024, 7, 17, 10, 30, 0));
            Time time3 = new(12, 0, 0);

            Assert.Equal(time1, time2);
            Assert.NotEqual(time1, time3);
        }

        [Fact]
        public void TestTimeToString()
        {
            Time time = new(15, 45, 30);
            string expectedString = "15:45:30";

            Assert.Equal(expectedString, time.ToString());
        }
    }
}
