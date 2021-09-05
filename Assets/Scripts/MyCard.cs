using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public class MyCard
    {
        public static void LoadAvatar(string username, Action<Texture2D> callback)
        {
            var request =
                UnityWebRequestTexture.GetTexture($"https://sapi.moecube.com:444/accounts/users/{username}.png");
            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (request.error != null)
                {
                    Debug.LogWarning($"{request.url}: {request.error}");
                    return;
                }

                callback(((DownloadHandlerTexture) request.downloadHandler).texture);
            };
        }
    }
}