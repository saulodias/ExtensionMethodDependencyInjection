//using System;
using Microsoft.Extensions.DependencyInjection;

namespace ExtensionMethodDependencyInjection
{
    // An arbitrary type to play with
    public class Widget
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public override string ToString() => $"Widget(Name={Name}, Value={Value})";
    }

    // A service that operates on Widget
    public interface IWidgetService
    {
        Widget Enhance(Widget widget);
    }

    public class WidgetService : IWidgetService
    {
        public Widget Enhance(Widget widget)
        {
            // Arbitrary logic: capitalize name and double value
            return new Widget
            {
                Name = widget.Name.ToUpperInvariant(),
                Value = widget.Value * 2
            };
        }
    }

    // Extension methods for Widget that use DI
    public static class WidgetExtensions
    {
        private static IServiceProvider? _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Extension method that uses a DI service
        public static Widget EnhanceWithDI(this Widget widget)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Extension methods not initialized with IServiceProvider.");

            var service = _serviceProvider.GetService<IWidgetService>();
            if (service == null)
                throw new InvalidOperationException("No IWidgetService registered.");

            return service.Enhance(widget);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Set up DI
            var services = new ServiceCollection();
            services.AddSingleton<IWidgetService, WidgetService>();
            var provider = services.BuildServiceProvider();

            // Initialize extension methods with service provider
            WidgetExtensions.Initialize(provider);

            // Use the extension method
            var widget = new Widget { Name = "test", Value = 42 };
            var enhanced = widget.EnhanceWithDI();
            Console.WriteLine($"Original: {widget}\nEnhanced: {enhanced}");
        }
    }
}
