using UnityEngine;
using System;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Transform judgmentLine;

[SerializeField] private int perfectThreshold = 120; // Perfect の範囲
[SerializeField] private int goodThreshold = 240;    // Good の最大範囲

[SerializeField] private int missThreshold = 360; // AutoMiss発動までの猶予

[SerializeField] private int earlyMissThreshold = 480; // 早すぎる Miss の閾値
[SerializeField] private int earlyIgnoreThreshold = 600;



    public static event Action<string, Vector3> OnJudgment;

    void Start()
    {
        ValidateThresholds();
    }

    void Update()
    {
        AutoMissCheck();
    }

    private void ValidateThresholds()
    {
        if (goodThreshold <= perfectThreshold)
        {
            Debug.LogWarning("⚠ Good閾値はPerfect閾値より大きくする必要があります。自動修正されました。");
            goodThreshold = perfectThreshold + 1;
        }

        if (missThreshold <= goodThreshold)
        {
            Debug.LogWarning("⚠ Miss閾値はGood閾値より大きくする必要があります。自動修正されました。");
            missThreshold = goodThreshold + 60;
        }
    }

public void ProcessKeyPress(int noteValue)
{
    if (!notesGenerator.isReady) return;

    double currentTime = AudioSettings.dspTime;
    double elapsedTime = currentTime - notesGenerator.startTime;
    double tickDuration = (60.0 / notesGenerator.midiFilePlayer.MPTK_Tempo) / notesGenerator.TPQN;
    double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
    long currentTick = (long)((elapsedTime + offsetSec) / tickDuration);

    NoteController bestNote = null;
    long bestTickDifference = long.MaxValue;

    foreach (var note in notesGenerator.GetNoteControllers())
    {
        if (note.noteValue != noteValue) continue;

        long tickDifference = note.tick - currentTick;

        // 🎯 超早押しの無視（←ここが重要！）
    if (tickDifference >= earlyIgnoreThreshold)
    {
        Debug.Log($"[IGNORE] ノートはまだ先すぎる: TickDiff={tickDifference}");
        continue;
    }


        long absDiff = Math.Abs(tickDifference);

        if (absDiff < Math.Abs(bestTickDifference))
        {
            bestNote = note;
            bestTickDifference = tickDifference;
        }
    }

    if (bestNote != null)
    {
        long tickDifference = bestTickDifference;
        string judgmentResult;

        // 🎯 早すぎる入力に対して Miss 判定
        if (tickDifference < -earlyMissThreshold)
        {
            judgmentResult = "Miss"; // 早すぎるMiss
        }
        else if (tickDifference < -perfectThreshold)
        {
            judgmentResult = "Good"; // 早めのGood
        }
        else if (tickDifference <= perfectThreshold)
        {
            judgmentResult = "Perfect";
        }
        else if (tickDifference <= goodThreshold)
        {
            judgmentResult = "Good"; // 遅めのGood
        }
        else
        {
            judgmentResult = "Miss"; // 遅すぎるMiss
        }

        Debug.Log($"[RESULT] 判定={judgmentResult}, TickDiff={tickDifference}");

        notesGenerator.GetNoteControllers().Remove(bestNote);
        Destroy(bestNote.gameObject);
        OnJudgment?.Invoke(judgmentResult, bestNote.transform.position);
    }
}



    private void AutoMissCheck()
    {
        if (notesGenerator == null || !notesGenerator.isReady) return;

        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - notesGenerator.startTime;
        double tickDuration = (60.0 / notesGenerator.midiFilePlayer.MPTK_Tempo) / notesGenerator.TPQN;
        double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
        long currentTick = (long)((elapsedTime + offsetSec) / tickDuration);

        var notes = notesGenerator.GetNoteControllers();
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            var note = notes[i];

            if (note.tick < currentTick - missThreshold)
            {
                Debug.Log($"❌ AutoMiss - ノートを逃しました (Note={note.noteValue}, Tick={note.tick}, 遅れ={note.tick - currentTick})");

                notes.RemoveAt(i);
                Destroy(note.gameObject);
                OnJudgment?.Invoke("Miss", note.transform.position);
            }
        }
    }
}
