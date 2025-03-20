using UnityEngine;
using System.Collections.Generic;

public class JudgmentManager : MonoBehaviour
{
    public NotesGenerator notesGenerator; // NotesGenerator への参照
    public Transform judgmentLine; // 判定ラインの Transform
    public float judgmentThreshold = 1f; // 判定範囲（変更可能）

public void ProcessKeyPress(int noteValue)
{
    Debug.Log($"[JudgmentManager] Key Pressed: {noteValue}");

    float judgmentThreshold = 1.0f; // 判定範囲
    float minZThreshold = 1f; // 🎯 判定ラインより手前（Z>0.1f）のノーツは無視

    float judgmentZ = judgmentLine.position.z; // 判定ラインのZ座標
    List<NoteController> notesToRemove = new List<NoteController>();

    foreach (var note in notesGenerator.GetNoteControllers())
    {
        float noteZ = note.transform.position.z;

        // 🎯 追加: 判定ラインより手前（Z>0.1f）のノーツは無視
        if (noteZ > judgmentZ + minZThreshold)
        {
            Debug.Log($"⏳ まだ判定できないノーツ (Note={note.noteValue}, Z={noteZ})");
            continue; // このノーツは無視
        }

        // 🎯 追加: 押されたキーに対応するノートのみ判定する
        if (note.noteValue != noteValue) 
        {
            Debug.Log($"🚫 キーが対応していないノート (Note={note.noteValue}, Expected={noteValue})");
            continue; // このノーツは無視
        }

        // 🎯 修正: 判定範囲を適用し、対応するノートのみ削除
        if (noteZ > judgmentZ - judgmentThreshold)
        {
            Debug.Log($"🎯 判定成功！Note={note.noteValue} at Tick={note.tick}");
            notesToRemove.Add(note);
        }
    }

    // 🎯 判定成功したノートだけ削除
    foreach (var note in notesToRemove)
    {
        notesGenerator.GetNoteControllers().Remove(note);
        Destroy(note.gameObject);
    }
}

}
