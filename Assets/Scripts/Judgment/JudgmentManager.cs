using UnityEngine;
using System;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Transform judgmentLine;

    [SerializeField] private int perfectThreshold = 50;
    [SerializeField] private int goodThreshold = 120;
    private int missThreshold;

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
    }

    public void ProcessKeyPress(int noteValue)
    {
        if (!notesGenerator.isReady) return;

        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - notesGenerator.startTime;

        double tickDuration = (60.0 / notesGenerator.midiFilePlayer.MPTK_Tempo) / notesGenerator.TPQN;
        long currentTick = (long)(elapsedTime / tickDuration);

        NoteController bestNote = null;
        long bestTickDifference = long.MaxValue;
        string judgmentResult = "Miss";

        foreach (var note in notesGenerator.GetNoteControllers())
        {
            if (note.noteValue != noteValue) continue;

            long tickDifference = note.tick - currentTick;

            if (Mathf.Abs(tickDifference) < Mathf.Abs(bestTickDifference))
            {
                bestNote = note;
                bestTickDifference = tickDifference;
            }
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
                return;
            }

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
