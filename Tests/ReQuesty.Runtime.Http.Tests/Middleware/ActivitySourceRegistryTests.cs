using ReQuesty.Runtime.Http.Middleware;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware.Registries
{
    public class ActivitySourceRegistryTests
    {
        [Fact]
        public void Defensive()
        {
            Assert.Throws<ArgumentNullException>(() => ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(""));
            Assert.Throws<ArgumentNullException>(() => ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(null!));
        }

        [Fact]
        public void CreatesNewInstanceOnFirstCallAndReturnsSameInstance()
        {
            // Act
            System.Diagnostics.ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source");
            Assert.NotNull(activitySource);

            System.Diagnostics.ActivitySource activitySource2 = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source");
            Assert.NotNull(activitySource);

            // They are the same instance
            Assert.Equal(activitySource, activitySource2);
            Assert.Equal("sample source", activitySource.Name);
            Assert.Equal("sample source", activitySource2.Name);
        }

        [Fact]
        public void CreatesDifferentInstances()
        {
            // Act
            System.Diagnostics.ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source");
            Assert.NotNull(activitySource);
            Assert.Equal("sample source", activitySource.Name);

            System.Diagnostics.ActivitySource activitySource2 = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source 2");
            Assert.NotNull(activitySource);
            Assert.Equal("sample source 2", activitySource2.Name);

            // They are not the same instance
            Assert.NotEqual(activitySource, activitySource2);
        }
    }
}
