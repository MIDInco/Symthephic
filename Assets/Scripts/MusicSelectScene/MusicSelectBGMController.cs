using UnityEngine;
using UnityEngine.Audio;

public class MusicSelectBGMController : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioMixer audioMixer;

    private const string VolumeParameter = "MasterVolume";

    void Start()
    {
        ApplyVolume();
        if (!bgmSource.isPlaying) bgmSource.Play();
    }

    public void ApplyVolume()
    {
        float value = Mathf.Clamp(GameSettings.MasterVolume, 0.0001f, 1f);
        float db = Mathf.Lerp(-40f, 0f, Mathf.Pow(value, 0.4f));
        audioMixer.SetFloat(VolumeParameter, db);
    }
}
