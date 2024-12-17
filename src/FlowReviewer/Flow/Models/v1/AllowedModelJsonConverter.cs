using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Raiqub.Generators.EnumUtilities", "1.9.0.0")]
    public sealed class AllowedModelJsonConverter : JsonConverter<AllowedModel>
    {
        private const int MaxCharStack = 256;

        public override AllowedModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return ReadFromString(ref reader);

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, AllowedModel value, JsonSerializerOptions options)
        {
            var jsonString = value.ToJsonString();
            if (jsonString is not null)
            {
                writer.WriteStringValue(jsonString);
            }
            else
            {
                throw new JsonException();
            }
        }

        private AllowedModel ReadFromString(ref Utf8JsonReader reader)
        {
            var length = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;

            char[]? rented = null;
            var name = length <= MaxCharStack ? stackalloc char[MaxCharStack] : (rented = ArrayPool<char>.Shared.Rent(length));
            try
            {
                var charsWritten = reader.CopyString(name);
                name = name.Slice(0, charsWritten);

                var isParsed = AllowedModelFactory.TryParseJsonString(name, ignoreCase: false, out var result);
                if (!isParsed)
                {
                    throw new JsonException();
                }

                return result;
            }
            finally
            {
                if (rented != null)
                {
                    ArrayPool<char>.Shared.Return(rented);
                }
            }
        }
    }
