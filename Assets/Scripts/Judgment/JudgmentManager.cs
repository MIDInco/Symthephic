using UnityEngine;
using System;
using MidiPlayerTK;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Transform judgmentLine;

    [SerializeField] private int perfectThreshold = 50; // ✅ 開発者が設定可能なPerfect閾値
    [SerializeField] private int goodThreshold = 120;   // ✅ 開発者が設定可能なGood閾値（Perfectより広くする必要あり）
    private int missThreshold;                          // 🚀 Miss閾値は自動計算

    public static event Action<string, Vector3> OnJudgment;

    void Start()
    {
        UpdateJudgmentThresholds();
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
            goodThreshold = perfectThreshold + 1; // 🚀 強制的に適正値に修正
        }

        // 🚀 Missの範囲を自動調整
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

            long tickDifference = note.tick - currentTick; // 🎯 負なら遅い、正なら早い

            // 🎯 Good範囲の負のノートを優先的に判定
            if (tickDifference < 0 && Math.Abs(tickDifference) <= goodThreshold)
            {
                if (Math.Abs(tickDifference) < Math.Abs(delayedGoodTickDifference))
                {
                    delayedGoodNote = note;
                    delayedGoodTickDifference = tickDifference;
                }
            }

            // 🎯 通常の最近ノート判定（現在のロジック）
            if (Math.Abs(tickDifference) < Math.Abs(bestTickDifference))
            {
                bestNote = note;
                bestTickDifference = tickDifference;
            }
        }

        // 🎯 Goodの範囲内で負のノートがあれば、それを優先
        if (delayedGoodNote != null)
        {
            bestNote = delayedGoodNote;
            bestTickDifference = delayedGoodTickDifference;
        }

        // 🎯 判定処理
        if (bestNote != null)
        {
            if (Math.Abs(bestTickDifference) <= perfectThreshold)
            {
                judgmentResult = "Perfect";
            }
            else if (Math.Abs(bestTickDifference) <= goodThreshold)
            {
                judgmentResult = "Good";
            }
            else
            {
                judgmentResult = "Miss";
            }
        }

        // 🎯 Miss 判定の処理（デバッグログを追加）
        if (bestNote == null || judgmentResult == "Miss")
        {
            string missReason;
            if (bestNote == null)
            {
                missReason = "適切なノートが見つからなかった";
            }
            else if (bestTickDifference > goodThreshold)
            {
                missReason = "早すぎた入力（Goodの範囲を超えた）";
            }
            else
            {
                missReason = "遅すぎた入力（Goodの範囲を超えた）";
            }

            Debug.Log($"🚫 Miss (NoteValue={noteValue}) - {missReason} | TickDifference={bestTickDifference}");
            return;
        }

        Debug.Log($"🎯 判定: {judgmentResult} (Note={bestNote.noteValue}, Tick={bestNote.tick}, TickDifference={bestTickDifference})");

        // 🎯 Miss 以外の場合にノート削除
        notesGenerator.GetNoteControllers().Remove(bestNote);
        Destroy(bestNote.gameObject);

        // 🎯 エフェクトを通知
        OnJudgment?.Invoke(judgmentResult, bestNote.transform.position);
    }
}
