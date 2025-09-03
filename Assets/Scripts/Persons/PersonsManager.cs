using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PersonDataManager : Singleton<PersonDataManager>
{
    private bool isReady = false;
    private TaskCompletionSource<bool> _readySource = new TaskCompletionSource<bool>();
    public List<PersonData> AllCharacters { get; private set; } = new();

    private async void Start()
    {
        await LoadAllCharacters();

        isReady = true;
        _readySource.TrySetResult(true);
    }

    public async Task LoadAllCharacters()
    {
        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/Persons?select=*&apikey={ServerData.k_ApiKey}";
        UnityWebRequest request = SupabaseUtility.CreateGetRequest(url);
        var op = request.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load characters: {request.error}");
            return;
        }

        AllCharacters.Clear();
        foreach (JObject obj in JArray.Parse(request.downloadHandler.text))
        {
            PersonData person = new PersonData
            {
                id = obj["person_id"]?.ToString(),
                first_name = obj["first_name"]?.ToString(),
                last_name = obj["last_name"]?.ToString(),
                photo_url = obj["photo_url"]?.ToString(),
                prefab_id = obj["prefab_id"]?.ToString()
            };

            // Load prefab and portrait
            person.characterPrefab = Resources.Load<GameObject>($"Characters/{person.first_name}");
            if (!string.IsNullOrEmpty(person.photo_url))
            {
                if (this != null && gameObject != null)
                    StartCoroutine(LoadImageCoroutine(person.photo_url, tex => person.portrait = tex));
            }
            
            AllCharacters.Add(person);
        }
    }

private IEnumerator LoadImageCoroutine(string url, Action<Texture2D> callback)
{
    UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
    yield return req.SendWebRequest();

    if (req.result == UnityWebRequest.Result.Success)
    {
        callback(DownloadHandlerTexture.GetContent(req));
    }
    else
    {
        Debug.LogWarning($"Failed to load image: {req.error}");
    }
}

    public PersonData GetByName(string name) => AllCharacters.FirstOrDefault(c => c.name == name);
    public PersonData GetById(string id) => AllCharacters.FirstOrDefault(c => c.id == id);

    internal async Task WaitUntilReady()
    {
        if (isReady)
            return;
        await _readySource.Task;
    }
}
