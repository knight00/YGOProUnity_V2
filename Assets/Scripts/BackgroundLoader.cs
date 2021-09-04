using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class BackgroundLoader : MonoBehaviour
{
    private void Awake()
    {
        var player = GetComponent<VideoPlayer>();
        player.url = Path.Combine(Directory.GetCurrentDirectory(), player.url);
        player.Play();
    }
}