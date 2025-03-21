using UnityEngine;
using System;
using MidiPlayerTK;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Transform judgmentLine;

    [SerializeField] private int perfectThreshold = 50; // 開発者が設定可能
    [SerializeField] private int goodThreshold = 120;   // 開発者が設定可能
    private int missThreshold; // 自動設定される

    public static event Action<string, Vector3> OnJudgment;

    void Start()
    {
        UpdateJudgmentThresholds();
    }

    void Update()
    {
        AutoMissCheck();
    }

    public void UpdateJudgmentThresholds()
    {
        if (notesGenerator.midiFilePlayer == null)
        {
            Debug.LogError("⚠ MidiFilePlayerが設定されていません！");
            return;
        }

        if (goodThreshold <= perfectThreshold)
        {
            Debug.LogError("⚠ Good閾値はPerfect閾値より大きくする必要があります！");
            goodThreshold = perfectThreshold + 1;
        }

        missThreshold = goodThreshold + 1;

        double BPM = notesGenerator.midiFilePlayer.MPTK_Tempo;
        int TPQN = notesGenerator.TPQN;

        if (BPM <= 0 || TPQN <= 0)
        {
            Debug.LogError("⚠ BPMまたはTPQNが無効です！");
            return;
        }

        Debug.Log($"🎯 判定閾値更新: Perfect=±{perfectThreshold} Tick, Good=±{perfectThreshold + 1}~{goodThreshold} Tick, Miss=±{missThreshold}~∞");
    }

    public void ProcessKeyPress(int noteValue)
    {
        if (!notesGenerator.isReady) return;

        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - notesGenerator.startTime;

        double tickDuration = (60.0 / notesGenerator.midiFilePlayer.MPTK_Tempo) / notesGenerator.TPQN;
        long currentTick = (long)(elapsedTime / tickDuration);

        NoteController bestNote = null;
        NoteController delayedGoodNote = null;
        long bestTickDifference = long.MaxValue;
        long delayedGoodTickDifference = long.MaxValue;
        string judgmentResult = "Miss";

        foreach (var note in notesGenerator.GetNoteControllers())
        {
            if (note.noteValue != noteValue) continue;

            long tickDifference = note.tick - currentTick;

            if (tickDifference < 0 && Mathf.Abs(tickDifference) <= goodThreshold)
            {
                if (Mathf.Abs(tickDifference) < Mathf.Abs(delayedGoodTickDifference))
                {
                    delayedGoodNote = note;
                    delayedGoodTickDifference = tickDifference;
                }
            }

            if (Mathf.Abs(tickDifference) < Mathf.Abs(bestTickDifference))
            {
                bestNote = note;
                bestTickDifference = tickDifference;
            }
        }

        if (delayedGoodNote != null)
        {
            bestNote = delayedGoodNote;
            bestTickDifference = delayedGoodTickDifference;
        }

        if (bestNote != null)
        {
            if (Mathf.Abs(bestTickDifference) <= perfectThreshold)
            {
                judgmentResult = "Perfect";
            }
            else if (Mathf.Abs(bestTickDifference) <= goodThreshold)
            {
                judgmentResult = "Good";
            }
            else
            {
                judgmentResult = "Miss";
                Debug.Log($"🚫 Miss - 早すぎた入力（Goodの範囲を超えた） | TickDifference={bestTickDifference}");
                return;
            }
        }

        if (bestNote == null)
        {
            Debug.Log($"🚫 Miss - 該当ノートが見つかりません (NoteValue={noteValue})");
            return;
        }

        Debug.Log($"🎯 判定: {judgmentResult} (Note={bestNote.noteValue}, Tick={bestNote.tick}, TickDifference={bestTickDifference})");

        notesGenerator.GetNoteControllers().Remove(bestNote);
        Destroy(bestNote.gameObject);
        OnJudgment?.Invoke(judgmentResult, bestNote.transform.position);
    }

    private void AutoMissCheck()
    {
        if (notesGenerator == null || !notesGenerator.isReady) return;

        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - notesGenerator.startTime;
        double tickDuration = (60.0 / notesGenerator.midiFilePlayer.MPTK_Tempo) / notesGenerator.TPQN;
        long currentTick = (long)(elapsedTime / tickDuration);

        var notes = notesGenerator.GetNoteControllers();
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            var note = notes[i];

            if (note.tick < currentTick - goodThreshold)
            {
                Debug.Log($"❌ AutoMiss - ノートを逃しました (Note={note.noteValue}, Tick={note.tick}, 遅れ={note.tick - currentTick})");
                notes.RemoveAt(i);
                Destroy(note.gameObject);
                OnJudgment?.Invoke("Miss", note.transform.position);
            }
        }
    }
}
