using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class DeviceKeyManager
{
    private const string k_DeviceKey = "UniqueDeviceKey";

    public static string GetOrCreateDeviceKey()
    {
        if (PlayerPrefs.HasKey(k_DeviceKey))
        {
            return PlayerPrefs.GetString(k_DeviceKey);
        }

        string newKey = Guid.NewGuid().ToString();
        PlayerPrefs.SetString(k_DeviceKey, newKey);
        PlayerPrefs.Save();
        return newKey;
    }
}
