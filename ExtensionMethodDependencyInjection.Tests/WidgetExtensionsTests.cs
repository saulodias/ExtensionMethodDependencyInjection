using Xunit;
using ExtensionMethodDependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ExtensionMethodDependencyInjection.Tests
{
    public class WidgetExtensionsTests
    {
        [Fact]
        public void EnhanceWithDI_EnhancesWidgetCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IWidgetService, WidgetService>();
            var provider = services.BuildServiceProvider();
            WidgetExtensions.Initialize(provider);

            var widget = new Widget { Name = "unit", Value = 10 };

            // Act
            var enhanced = widget.EnhanceWithDI();

            // Assert
            Assert.Equal("UNIT", enhanced.Name);
            Assert.Equal(20, enhanced.Value);
        }
    }
}
