using System;
using System.Collections.Generic;
using UnityEngine;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public float goodThreshold = 120f;
    public float perfectThreshold = 60f;
    public float missThreshold = 180f;

    public event Action<string, Vector3> OnJudgment;

    private Dictionary<int, NoteController> heldLongNotes = new Dictionary<int, NoteController>();

    void Update()
    {
        if (GameSceneManager.IsPaused || GameSceneManager.IsResuming || !notesGenerator.isReady) return;
        AutoMissCheck();
        CheckHeldNotesReleasedEarly();
    }

    public void ProcessKeyPress(int noteValue)
    {
        if (!notesGenerator.isReady) return;

        double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
        double dspTime = GameSceneManager.GetGameDspTime();
        double currentTime = dspTime - notesGenerator.startTime + offsetSec;

        long currentTick = notesGenerator.GetCurrentTickWithTempo(currentTime);

        var notes = notesGenerator.GetNoteControllers();
        for (int i = 0; i < notes.Count; i++)
        {
            var note = notes[i];
            if (note.noteValue != noteValue) continue;

            long delta = Math.Abs(note.tick - currentTick);

            if (delta <= perfectThreshold)
            {
                if (note.isLongNote)
                {
                    heldLongNotes[noteValue] = note;
                    Debug.Log($"⏳ ホールド開始: Note={noteValue}");

                    Vector3 effectPosition = note.transform.position;
                    OnJudgment?.Invoke("Perfect", effectPosition);
                    ScoreManager.Instance?.RegisterPerfect();
                    PhraseManager.Instance?.IncreasePhrase();
                    return;
                }
                HandleJudgment("Perfect", note, false);
                return;
            }
            else if (delta <= goodThreshold)
            {
                if (note.isLongNote)
                {
                    heldLongNotes[noteValue] = note;
                    Debug.Log($"⏳ ホールド開始 (Good): Note={noteValue}");

                    Vector3 effectPosition = note.transform.position;
                    OnJudgment?.Invoke("Good", effectPosition);
                    ScoreManager.Instance?.RegisterGood();
                    PhraseManager.Instance?.IncreasePhrase();
                    return;
                }
                HandleJudgment("Good", note, false);
                return;
            }
        }
    }

    public void ProcessKeyRelease(int noteValue)
    {
        if (!heldLongNotes.ContainsKey(noteValue)) return;

        var note = heldLongNotes[noteValue];
        heldLongNotes.Remove(noteValue);

        double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
        double dspTime = GameSceneManager.GetGameDspTime();
        double currentTime = dspTime - notesGenerator.startTime + offsetSec;
        long currentTick = notesGenerator.GetCurrentTickWithTempo(currentTime);

        long delta = Math.Abs(note.endTick - currentTick);

        if (delta <= perfectThreshold)
        {
            HandleJudgment("Perfect", note, true);
        }
        else if (delta <= goodThreshold)
        {
            HandleJudgment("Good", note, true);
        }
        else
        {
            HandleJudgment("Miss", note, true);
        }
    }

    private void CheckHeldNotesReleasedEarly()
    {
        List<int> releasedKeys = new List<int>();
        foreach (var pair in heldLongNotes)
        {
            int noteValue = pair.Key;
            KeyCode key = GetKeyCodeForNoteValue(noteValue);
            if (!Input.GetKey(key))
            {
                Debug.LogWarning($"⚠️ 早期離し: Note={noteValue}");
                ProcessKeyRelease(noteValue);
                releasedKeys.Add(noteValue);
            }
        }

        foreach (var key in releasedKeys)
        {
            heldLongNotes.Remove(key);
        }
    }

    private KeyCode GetKeyCodeForNoteValue(int noteValue)
    {
        return noteValue switch
        {
            60 => KeyCode.S,
            61 => KeyCode.D,
            62 => KeyCode.F,
            63 => KeyCode.J,
            64 => KeyCode.K,
            65 => KeyCode.L,
            _ => KeyCode.None,
        };
    }

    private void AutoMissCheck()
    {
        if (!notesGenerator.isReady) return;

        double offsetSec = Noteoffset.Instance != null ? Noteoffset.Instance.GetOffset() : 0.0;
        double dspTime = GameSceneManager.GetGameDspTime();
        double currentTime = dspTime - notesGenerator.startTime + offsetSec;

        long currentTick = notesGenerator.GetCurrentTickWithTempo(currentTime);

        var notes = notesGenerator.GetNoteControllers();
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            var note = notes[i];

            if (note.isLongNote && heldLongNotes.ContainsValue(note))
                continue;

            if (note.tick < currentTick - missThreshold)
            {
                Debug.Log($"❌ AutoMiss - ノートを逃しました (Note={note.noteValue}, Tick={note.tick}, 遅れ={note.tick - currentTick})");

                notes.RemoveAt(i);
                if (note.isLongNote)
                {
                    if (note.endNoteObject != null) Destroy(note.endNoteObject);
                    Transform parent = note.transform.parent;
                    if (parent != null)
                    {
                        var body = parent.Find($"LongBody_{note.uniqueID}");
                        if (body != null) Destroy(body.gameObject);
                    }
                }
                Destroy(note.gameObject);

                OnJudgment?.Invoke("Miss", note.transform.position);
                ScoreManager.Instance?.RegisterMiss();
                PhraseManager.Instance?.ResetPhrase();
            }
        }
    }

    private void HandleJudgment(string type, NoteController note, bool isEnd)
    {
        if (heldLongNotes.ContainsValue(note))
        {
            int keyToRemove = -1;
            foreach (var pair in heldLongNotes)
            {
                if (pair.Value == note)
                {
                    keyToRemove = pair.Key;
                    break;
                }
            }
            if (keyToRemove != -1)
                heldLongNotes.Remove(keyToRemove);
        }

        notesGenerator.RemoveNote(note);

        if (note.isLongNote)
        {
            if (note.endNoteObject != null) Destroy(note.endNoteObject);
            Transform parent = note.transform.parent;
            if (parent != null)
            {
                var body = parent.Find($"LongBody_{note.uniqueID}");
                if (body != null) Destroy(body.gameObject);
            }
        }

        Destroy(note.gameObject);

        Vector3 effectPosition = isEnd && note.isLongNote && note.endNoteObject != null
            ? note.endNoteObject.transform.position
            : note.transform.position;

        OnJudgment?.Invoke(type, effectPosition);

        switch (type)
        {
            case "Perfect":
                ScoreManager.Instance?.RegisterPerfect();
                PhraseManager.Instance?.IncreasePhrase();
                break;
            case "Good":
                ScoreManager.Instance?.RegisterGood();
                PhraseManager.Instance?.IncreasePhrase();
                break;
            case "Miss":
                ScoreManager.Instance?.RegisterMiss();
                PhraseManager.Instance?.ResetPhrase();
                break;
        }
    }
}
