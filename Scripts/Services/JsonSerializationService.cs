using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriptor.Models;

namespace Scriptor.Services
{
    public class JsonSerializationService
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[]
            {
                new GuidConverter(),
                new IndexColumnsConverter()
            }
        };

        public static string SerializeToJson(Project project)
        {
            return JsonConvert.SerializeObject(project, _settings);
        }

        public static void SerializeToFile(Project project, string filePath)
        {
            var json = SerializeToJson(project);
            File.WriteAllText(filePath, json);
        }

        public static Project DeserializeFromJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                throw new ArgumentException("JSON inválido", nameof(jsonString));

            return JsonConvert.DeserializeObject<Project>(jsonString, _settings);
        }

        public static Project DeserializeFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

            var json = File.ReadAllText(filePath);
            return DeserializeFromJson(json);
        }
    }

    /// <summary>
    /// Converter customizado para Guid
    /// Serializa como string, desserializa de string para Guid
    /// </summary>
    public class GuidConverter : JsonConverter<Guid>
    {
        public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return Guid.Empty;

            if (Guid.TryParse(reader.Value.ToString(), out var guid))
                return guid;

            throw new JsonSerializationException($"Valor inválido para Guid: {reader.Value}");
        }

        public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    /// <summary>
    /// Converter customizado para List<Column> em Index
    /// Serializa apenas IDs das colunas (evita referências circulares)
    /// Desserialização requer reconstitução de referências (fazer em ProjectService)
    /// </summary>
    public class IndexColumnsConverter : JsonConverter<List<Column>>
    {
        public override List<Column> ReadJson(JsonReader reader, Type objectType, List<Column> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new List<Column>();

            var jArray = JArray.Load(reader);
            var columns = new List<Column>();

            foreach (var item in jArray)
            {
                // Na desserialização, receber apenas IDs (será reconstituído depois)
                if (item.Type == JTokenType.String)
                {
                    // Se for string (ID), criar Column com ID apenas
                    if (Guid.TryParse(item.Value<string>(), out var id))
                    {
                        columns.Add(new Column { Id = id });
                    }
                }
                else if (item.Type == JTokenType.Object)
                {
                    // Se for objeto, desserializar como Column normal
                    var column = item.ToObject<Column>();
                    columns.Add(column);
                }
            }

            return columns;
        }

        public override void WriteJson(JsonWriter writer, List<Column> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            if (value != null)
            {
                foreach (var column in value)
                {
                    writer.WriteValue(column.Id.ToString());
                }
            }

            writer.WriteEndArray();
        }
    }
}
