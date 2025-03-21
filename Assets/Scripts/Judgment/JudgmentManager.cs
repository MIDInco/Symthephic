using UnityEngine;
using System;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Transform judgmentLine;

    [SerializeField] private int perfectThreshold = 120; // Tick 単位
    [SerializeField] private int goodThreshold = 240;   // Tick 単位
    [SerializeField] private int missThreshold = 240;   // AutoMissまでの猶予

    [SerializeField] private int earlyIgnoreThreshold = 960; // Tick単位（ノートより240Tick以上早いと無視）


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

        // 🎯 超早押しは無視（判定対象にすらしない）
        if (tickDifference <= -earlyIgnoreThreshold) continue;

        long absDiff = Math.Abs(tickDifference);

        if (absDiff < Math.Abs(bestTickDifference))
        {
            bestNote = note;
            bestTickDifference = tickDifference;
        }
}


        if (bestNote != null)
        {
            long absDiff = Math.Abs(bestTickDifference);
            string judgmentResult;

            if (absDiff <= perfectThreshold)
                judgmentResult = "Perfect";
            else if (absDiff <= goodThreshold)
                judgmentResult = "Good";
            else
                judgmentResult = "Miss";

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
