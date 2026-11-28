using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VeterinerKlinigi.Infrastructure;

public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var rawValue = valueProviderResult.FirstValue?.Trim();

        if (string.IsNullOrEmpty(rawValue))
        {
            if (bindingContext.ModelType == typeof(decimal?))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }

        var normalized = NormalizeDecimalText(rawValue);
        if (decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var parsedValue))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedValue);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Geçerli bir sayısal değer giriniz.");
        }

        return Task.CompletedTask;
    }

    private static string NormalizeDecimalText(string input)
    {
        var compact = input.Replace(" ", string.Empty);
        var lastComma = compact.LastIndexOf(',');
        var lastDot = compact.LastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0)
        {
            return lastComma > lastDot
                ? compact.Replace(".", string.Empty).Replace(',', '.')
                : compact.Replace(",", string.Empty);
        }

        if (lastComma >= 0)
        {
            return compact.Replace(',', '.');
        }

        return compact;
    }
}

public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(decimal) ||
            context.Metadata.ModelType == typeof(decimal?))
        {
            return new FlexibleDecimalModelBinder();
        }

        return null;
    }
}