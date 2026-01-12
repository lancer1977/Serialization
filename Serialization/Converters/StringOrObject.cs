namespace PolyhydraGames.Core.Serialization.Converters
{
    /// <summary>
    /// Wrapper that represents either a string or an object value.
    /// </summary>
    public readonly record struct StringOrObject<T>(string? StringValue, T? ObjectValue)
    {
        public StringOrObject(string? s) : this(s, default) { }
        public StringOrObject(T? obj) : this(default, obj) { }

        public bool IsString => StringValue is not null;
        public bool IsObject => ObjectValue is not null;
    }
}