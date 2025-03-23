using UnityEngine;
using System;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Transform judgmentLine;

    [SerializeField] private int perfectThreshold = 120;
    [SerializeField] private int goodThreshold = 240;
    [SerializeField] private int missThreshold = 360;
    [SerializeField] private int earlyMissThreshold = 480;
    [SerializeField] private int earlyIgnoreThreshold = 600;

    public static event Action<string, Vector3> OnJudgment;

    void Start()
    {
        ValidateThresholds();
    }

void Update()
{
    if (GameSceneManager.IsPaused || GameSceneManager.IsResuming) return;
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
    if (GameSceneManager.IsPaused || GameSceneManager.IsResuming) return;
    if (!notesGenerator.isReady) return;

    double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
    double dspTime = GameSceneManager.GetGameDspTime();
    long currentTick = notesGenerator.GetCurrentTickWithTempo(dspTime);

    NoteController bestNote = null;
    long bestTickDifference = long.MaxValue;

    foreach (var note in notesGenerator.GetNoteControllers())
    {
        if (note.noteValue != noteValue) continue;

        long tickDifference = note.tick - currentTick;

        // 遠すぎるノーツは候補に入れない（ログも出さない）
        if (tickDifference >= earlyIgnoreThreshold)
        {
            continue;
        }

        long absDiff = Math.Abs(tickDifference);
        if (absDiff < Math.Abs(bestTickDifference))
        {
            bestNote = note;
            bestTickDifference = tickDifference;
        }
    }

    // 判定できるノートが見つからなかった場合のみログを出す
    if (bestNote == null)
    {
        Debug.Log("🟡 判定できるノートが見つかりませんでした（すべて遠すぎ or 判定範囲外）");
        return;
    }

    long tickDifferenceFinal = bestTickDifference;
    string judgmentResult;

    if (tickDifferenceFinal < -earlyMissThreshold)
    {
        judgmentResult = "Miss";
    }
    else if (tickDifferenceFinal < -perfectThreshold)
    {
        judgmentResult = "Good";
    }
    else if (tickDifferenceFinal <= perfectThreshold)
    {
        judgmentResult = "Perfect";
    }
    else if (tickDifferenceFinal <= goodThreshold)
    {
        judgmentResult = "Good";
    }
    else
    {
        judgmentResult = "Miss";
    }

    Debug.Log($"[RESULT] 判定={judgmentResult}, TickDiff={tickDifferenceFinal}");

    notesGenerator.GetNoteControllers().Remove(bestNote);
    Destroy(bestNote.gameObject);
    OnJudgment?.Invoke(judgmentResult, bestNote.transform.position);
}


    private void AutoMissCheck()
    {
        if (notesGenerator == null || !notesGenerator.isReady) return;

        double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
        double dspTime = GameSceneManager.GetGameDspTime() + offsetSec;
        long currentTick = notesGenerator.GetCurrentTickWithTempo(dspTime);

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