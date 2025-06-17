# How to Use Dependency Injection in C# Extension Methods

Have you ever wanted to use services from your dependency injection (DI) container inside a C# extension method? While extension methods are static and can't have constructor injection, you can still wire them up to your DI container in a clean, testable way. Here’s a practical pattern you can use—no generics, no Rx, just simple and powerful DI for your extension methods!

## The Problem

Extension methods in C# are static, so you can’t inject services directly into them, unless you inject them as parameters. Sometimes, though, you want your extension methods to use services managed by your DI container—maybe for logging, transformation, or business logic.

## The Pattern

1. **Create your service and interface as usual.**
2. **Add a static `Initialize(IServiceProvider)` method to your extension class.**
3. **Use the provider inside the `Initialize(IServiceProvider)` method to resolve your services and store them in private static fields.**
4. **Use the services as needed.**

> **Note:** This pattern is best for singleton or stateless services. Avoid using it for scoped or transient services, or services that depend on per-request/user state.

## Example: Enhancing a Widget

Suppose you have a `Widget` class and want an extension method that “enhances” it using a service from DI.

```csharp
public class Widget
{
    public string Name { get; set; }
    public int Value { get; set; }
    public override string ToString() => $"Widget(Name={Name}, Value={Value})";
}

public interface IWidgetService
{
    Widget Enhance(Widget widget);
}

public class WidgetService : IWidgetService
{
    public Widget Enhance(Widget widget)
    {
        // Example: Capitalize name and double the value
        return new Widget
        {
            Name = widget.Name.ToUpperInvariant(),
            Value = widget.Value * 2
        };
    }
}
```

### The Extension Method

```csharp
public static class WidgetExtensions
{
    private static IWidgetService? _widgetService;

    // Call this once at app startup!
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _widgetService = serviceProvider.GetService<IWidgetService>();
        if (_widgetService == null)
            throw new InvalidOperationException("No IWidgetService registered.");
    }

    public static Widget EnhanceWithDI(this Widget widget)
    {
        if (_widgetService == null)
            throw new InvalidOperationException("WidgetExtensions not initialized with IWidgetService.");
        return _widgetService.Enhance(widget);
    }
}
```

### Setting Up in `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

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
        Console.WriteLine($"Original: {widget}");
        Console.WriteLine($"Enhanced: {enhanced}");
    }
}
```

### Testing the Extension Method with xUnit

```csharp
using Xunit;
using ExtensionMethodDependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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
```


### Output

```
Original: Widget(Name=test, Value=42)
Enhanced: Widget(Name=TEST, Value=84)
```

## Why This Works

- You only need to call `WidgetExtensions.Initialize()` once at app startup.
- The extension method can now access any service from the DI container.
- This pattern is simple, testable, and works for any extension method that needs DI.

## Limitations

- This approach uses a static field to store the service provider, so it’s best for application-level extension methods, not for libraries meant to be used in multiple apps at once.
- Be careful with multi-threading and service lifetimes (avoid using this for scoped services in ASP.NET Core request pipelines).

## Conclusion

You don’t have to give up on DI just because you’re writing extension methods! With this pattern, you can keep your code clean and take full advantage of the DI container everywhere—even in your static extension helpers.