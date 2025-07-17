using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class OperatorConverter : JsonConverter<IOperatorStrategy>
{
    public override void WriteJson(JsonWriter writer, IOperatorStrategy value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            ["Type"] = value.GetType().Name,
            ["Data"] = JObject.FromObject(value, JsonSerializer.CreateDefault())
        };
        obj.WriteTo(writer);
    }

    public override IOperatorStrategy ReadJson(JsonReader reader, Type objectType, IOperatorStrategy existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        string typeName = obj["Type"]?.ToString();


var dataToken = obj["Data"];
if (dataToken == null || obj["Type"] == null)
    throw new JsonSerializationException("Invalid IOperatorStrategy format: missing Type or Data");

JObject data = dataToken.Type == JTokenType.Object ? (JObject)dataToken : new JObject();


        if (typeName == null || data == null)
            throw new JsonSerializationException("Invalid IOperatorStrategy format: missing Type or Data");



        Type baseType = typeof(EqualOperator); // or IOperatorStrategy
string fullTypeName = baseType.Namespace + "." + typeName + ", " + baseType.Assembly.FullName;
Type type = Type.GetType(fullTypeName);

        if (type == null)
            throw new JsonSerializationException($"Unknown operator type: {typeName}");



if (type == null)
{
    Debug.LogError($"❌ Failed to find type '{fullTypeName}'");
    throw new JsonSerializationException($"Unknown operator type: {typeName}");
}

        return (IOperatorStrategy)data.ToObject(type, serializer);
    }
}
