using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Xunit.Sdk;

namespace Ciandt.FlowTools.FlowPair.Tests.Testing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class JsonResourceDataAttribute : DataAttribute
{
    private static readonly ConcurrentDictionary<Assembly, ResourceReader> s_resourceReaders = new();
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = JsonSerializerOptions.Web;

    private static readonly JsonDocumentOptions s_jsonDocumentOptions = new()
    {
        AllowTrailingCommas = s_jsonSerializerOptions.AllowTrailingCommas,
        CommentHandling = s_jsonSerializerOptions.ReadCommentHandling,
        MaxDepth = s_jsonSerializerOptions.MaxDepth
    };

    private readonly string _resourceName;
    private readonly string? _propertyName;

    /// <summary>Load data from a JSON file as the data source for a theory.</summary>
    /// <param name="resourceName">The resource name that contains the JSON content to load.</param>
    /// <param name="propertyName">The name of the property on the JSON file that contains the data for the test.</param>
    public JsonResourceDataAttribute(string resourceName, string? propertyName = null)
    {
        _resourceName = resourceName;
        _propertyName = propertyName;
    }

    public override IEnumerable<object?[]> GetData(MethodInfo testMethod)
    {
        if (testMethod == null || testMethod.DeclaringType == null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        var parameters = testMethod.GetParameters().Select(p => p.ParameterType).ToArray();
        var resourceReader = s_resourceReaders.GetOrAdd(testMethod.DeclaringType.Assembly, x => new ResourceReader(x));
        var content = resourceReader.GetString(_resourceName);

        using var jsonDocument = JsonDocument.Parse(content, s_jsonDocumentOptions);

        return Deserialize(
            string.IsNullOrEmpty(_propertyName)
                ? jsonDocument.RootElement
                : jsonDocument.RootElement.GetProperty(_propertyName),
            parameters);
    }

    private static IEnumerable<object?[]> Deserialize(JsonElement jsonElement, Type[] parameters)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            return new[] { DeserializeData(jsonElement, parameters) };
        }

        var list = new List<object?[]>();
        foreach (var element in jsonElement.EnumerateArray())
        {
            list.Add(DeserializeData(element, parameters));
        }

        return list;
    }

    private static object?[] DeserializeData(JsonElement jsonElement, Type[] parameters)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (parameters.Length > 1)
                throw new InvalidOperationException("JSON content must be an array to represent each parameter");

            return new[] { DeserializeParameter(jsonElement, parameters[0]) };
        }

        var result = new object?[parameters.Length];
        var index = 0;
        foreach (var element in jsonElement.EnumerateArray())
        {
            result[index] = DeserializeParameter(element, parameters[index]);
            index++;
        }

        return result;
    }

    private static object? DeserializeParameter(JsonElement element, Type parameterType)
    {
        return parameterType == typeof(string) && element.ValueKind != JsonValueKind.String
            ? element.GetRawText()
            : element.Deserialize(parameterType, s_jsonSerializerOptions);
    }
}
