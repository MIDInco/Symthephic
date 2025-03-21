using UnityEngine;
using System.Collections.Generic;

public class NotesTimingDebugger : MonoBehaviour
{
    public NotesGenerator notesGenerator;

    private float tickZeroTime = -1f;  // 🎯 譜面の再生開始時の時間
    private float firstNoteTime = -1f; // 🎯 最初に生成されたノートの相対時間
    private string firstNoteID = "";   // 🎯 最初のノートのID

    void Awake()
    {
        if (notesGenerator == null)
        {
            Debug.LogError("[NotesTimingDebugger] ❌ NotesGenerator が設定されていません。");
            return;
        }

        Debug.Log("✅ NotesTimingDebugger の Awake が実行されました");
        notesGenerator.OnChartPlaybackStart += OnChartPlaybackStart;
    }

    private void OnChartPlaybackStart()
    {
        tickZeroTime = (float)AudioSettings.dspTime; // 🎯 譜面の開始時点を記録
        Debug.Log($"🚀 [Tick=0] 譜面の再生開始基準時間 (tickZeroTime): {tickZeroTime:F3} sec");

        notesGenerator.OnNoteGenerated += OnNoteGenerated;
        Debug.Log("✅ OnNoteGenerated にリスナーを登録しました！");
    }

    private void OnNoteGenerated(NoteController note)
    {
        float noteGeneratedTime = (float)AudioSettings.dspTime - tickZeroTime; // 🎯 相対時間で計算

        if (firstNoteTime < 0)
        {
            firstNoteTime = noteGeneratedTime;
            firstNoteID = note.uniqueID;

            float timeDiff = firstNoteTime; // 🎯 Tick=0 からの時間差を直接計算
            Debug.Log($"🎯 [最初のノート] ID={firstNoteID}, 生成時間={firstNoteTime:F3} sec");
            Debug.Log($"⏱ [デバッグ] Tick=0 から最初のノートまでの時間: {timeDiff:F3} sec");
            Debug.Log($"📌 [OnNoteGenerated] ID={note.uniqueID}, Tick={note.tick}, 譜面時間={note.tickTimeSeconds:F3} sec");
        }
    }




}
