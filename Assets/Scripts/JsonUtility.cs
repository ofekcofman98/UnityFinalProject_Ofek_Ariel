using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonUtility
{
    public static JsonSerializerSettings Settings => new JsonSerializerSettings
    {
        Converters = new List<JsonConverter>
        {
            new OperatorConverter()
        }
    };
}
