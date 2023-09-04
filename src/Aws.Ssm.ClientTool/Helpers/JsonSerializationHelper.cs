namespace Aws.Ssm.ClientTool.Helpers;

public static class JsonSerializationHelper
{
    private static readonly Utf8Json.IJsonFormatterResolver Resolver = Utf8Json.Resolvers.CompositeResolver.Create(
        Utf8Json.Resolvers.EnumResolver.UnderlyingValue, // Importance sequence of resolvers - first enum
        Utf8Json.Resolvers.StandardResolver.ExcludeNull,
        Utf8Json.Resolvers.StandardResolver.Default);

    public static TObject Deserialize<TObject>(string source)
    {
        if (source == null) return default(TObject);

        return Utf8Json.JsonSerializer.Deserialize<TObject>(source);
    }

    public static string Serialize<TObject>(TObject source)
    {
        return Utf8Json.JsonSerializer.ToJsonString(source, Resolver);
    }
}