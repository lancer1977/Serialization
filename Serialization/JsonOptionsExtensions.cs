using System.Text.Json;
using PolyhydraGames.Core.Serialization.Converters;

namespace PolyhydraGames.Core.Serialization;

/// <summary>
/// Convenience methods to register a "sane defaults" bundle of converters for messy REST APIs.
/// </summary>
public static class JsonOptionsExtensions
{
    /// <summary>
    /// Adds a bundle of converters that help with common REST API gotchas:
    /// numbers/bools as strings, flexible dates, empty-string-as-null, and more.
    /// </summary>
    public static JsonSerializerOptions AddPolyhydraRestApiDefaults(this JsonSerializerOptions options)
    {
        // Value flexibility
        options.Converters.Add(new EmptyStringToNullConverter());
        options.Converters.Add(new FlexibleBooleanConverter());

        options.Converters.Add(new FlexibleInt32Converter());
        options.Converters.Add(new FlexibleInt64Converter());
        options.Converters.Add(new FlexibleDoubleConverter());
        options.Converters.Add(new FlexibleDecimalConverter());

        // Date/time
        options.Converters.Add(new FlexibleDateTimeOffsetConverter());
        options.Converters.Add(new FlexibleDateOnlyConverter());

        // NOTE:
        // Generic shape converters (SingleOrArrayConverter<T>, StringOrObjectConverter<T>)
        // are typically applied per-property because they require a specific T.
        return options;
    }
}
