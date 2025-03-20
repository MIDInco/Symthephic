using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    public AudioSource audioSource;
    public event Action OnAudioPlaybackStarted; // 🎯 オーディオ開始イベント

    private bool hasAudioStarted = false; // 🎯 再生確認フラグ

    public void PlayAudioNow()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("❌ AudioSource または AudioClip が設定されていません！");
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play(); // 🎯 すぐに再生
            Debug.Log($"✅ オーディオを即時再生！ (時間: {AudioSettings.dspTime:F3})");

            // 🎯 オーディオが開始されたことを通知
            OnAudioPlaybackStarted?.Invoke();
        }
        else
        {
            Debug.Log("⚠ 既にオーディオが再生中です。");
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 🎯 シーンを跨いでもオーディオを保持
        }
        else
        {
            Destroy(gameObject); // 🎯 二重生成を防ぐ
        }
    }

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("❌ AudioSource がアタッチされていません！");
        }
        else if (audioSource.clip == null)
        {
            Debug.LogError("❌ AudioClip が設定されていません！");
        }
    }

    void Update()
    {
        // 🎯 オーディオが再生開始されたらイベントを発火（1回のみ）
        if (!hasAudioStarted && audioSource != null && audioSource.isPlaying)
        {
            hasAudioStarted = true;
            OnAudioPlaybackStarted?.Invoke();
            Debug.Log($"🎵 オーディオが再生開始されました！ {AudioSettings.dspTime:F3} sec");
        }
    }

    public void PlayAudioWithDelay(float delaySeconds)
    {
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("❌ AudioSource または AudioClip が設定されていません！");
            return;
        }

        if (!audioSource.isPlaying)
        {
            double playTime = AudioSettings.dspTime + delaySeconds; // 🎯 2 秒後に再生予約
            audioSource.PlayScheduled(playTime);
            Debug.Log($"✅ {playTime:F3} sec にオーディオを再生予定 (現在時刻 {AudioSettings.dspTime:F3})");
        }
        else
        {
            Debug.Log("⚠ 既にオーディオが再生中です。");
        }
    }
}
