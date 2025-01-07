using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Raiqub.LlmTools.FlowPair.Common;

public static class HttpBlockingExtensions
{
    public static HttpResponseMessage Get(
        this HttpClient httpClient,
        [StringSyntax(StringSyntaxAttribute.Uri)] string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
    }

    public static TValue? GetFromJson<TValue>(
        this HttpClient httpClient,
        [StringSyntax(StringSyntaxAttribute.Uri)] string requestUri,
        JsonTypeInfo<TValue> jsonTypeInfo)
    {
        var responseMessage = httpClient.Get(requestUri);
        responseMessage.EnsureSuccessStatusCode();
        return responseMessage.Content.ReadFromJson(jsonTypeInfo);
    }

    public static HttpResponseMessage Post(
        this HttpClient httpClient,
        [StringSyntax(StringSyntaxAttribute.Uri)] string requestUri,
        HttpContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
        return httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
    }

    public static HttpResponseMessage PostAsJson<TValue>(
        this HttpClient httpClient,
        [StringSyntax(StringSyntaxAttribute.Uri)] string requestUri,
        TValue value,
        JsonTypeInfo<TValue> jsonTypeInfo)
    {
        var content = JsonContent.Create(value, jsonTypeInfo);
        return httpClient.Post(requestUri, content);
    }

    public static string ReadAsString(this HttpContent content)
    {
        using var stream = content.ReadAsStream();

        var encoding = GetEncoding(content);
        using var reader = encoding == null
            ? new StreamReader(stream, detectEncodingFromByteOrderMarks: true)
            : new StreamReader(stream, encoding);

        return reader.ReadToEnd();
    }

    public static T? ReadFromJson<T>(this HttpContent content, JsonTypeInfo<T> jsonTypeInfo)
    {
        using var stream = GetContentStream(content);
        return JsonSerializer.Deserialize(stream, jsonTypeInfo);
    }

    private static Encoding? GetEncoding(HttpContent content)
    {
        Encoding? encoding = null;

        var charset = content.Headers.ContentType?.CharSet;
        if (charset is null)
        {
            return encoding;
        }

        // Remove at most a single set of quotes.
        if (charset.Length > 2 && charset[0] == '\"' && charset[^1] == '\"')
        {
            encoding = Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
        }
        else
        {
            encoding = Encoding.GetEncoding(charset);
        }

        return encoding;
    }

    private static Stream GetContentStream(HttpContent content)
    {
        var originalStream = content.ReadAsStream();

        return GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
            ? Encoding.CreateTranscodingStream(originalStream, sourceEncoding, Encoding.UTF8)
            : originalStream;
    }
}
