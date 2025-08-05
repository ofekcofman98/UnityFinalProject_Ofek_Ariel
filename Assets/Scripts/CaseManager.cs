using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CaseManager : Singleton<CaseManager>
{
    public string CaseId { get; private set; }
    public string VictimId { get; private set; }

    public async Task LoadCaseData(string caseId)
    {
        CaseId = caseId;

        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/CrimeEvidence?case_id=eq.{caseId}&select=victim_id&apikey={ServerData.k_ApiKey}";
        UnityWebRequest request = SupabaseUtility.CreateGetRequest(url);
        var op = request.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Failed to fetch case data: {request.error}");
            return;
        }

        JArray array = JArray.Parse(request.downloadHandler.text);
        if (array.Count > 0)
        {
            VictimId = array[0]["victim_id"]?.ToString();
            Debug.Log($"🧍‍♂️ Victim ID loaded: {VictimId}");
        }
    }
}