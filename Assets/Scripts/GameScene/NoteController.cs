using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int noteValue; // ノートの音程
    public long tick; // ノートのタイミング（MIDI Tick）
    
    private NotesGenerator generator; // ノートを管理するジェネレーター
    public double tickTimeSeconds; // ノートの秒単位のタイミング
    public string uniqueID { get; private set; } // ノートの一意識別ID

    public void Initialize(double noteTime, NotesGenerator gen, string id)
    {
        tickTimeSeconds = noteTime; // MIDI Tick を秒単位に変換した値
        generator = gen;
        uniqueID = id;
    }

    public void UpdatePosition(float currentTime)
    {
        if (GameSceneManager.IsPaused) return; // ← 追加
        if (generator == null) return;

        double elapsedTime = currentTime - tickTimeSeconds;
        double targetZ = -elapsedTime * generator.noteSpeed;

         // ✅ 👇 ここに追記
    if (uniqueID == "1")
    {
        Debug.Log($"🛰 ノートID=1 | Z={transform.position.z:F3} | elapsed={elapsedTime:F3} | currentTime={currentTime:F3} | tickTime={tickTimeSeconds:F3}");
    }

        transform.position = new Vector3(transform.position.x, transform.position.y, (float)targetZ);

        // 一定距離を超えたら削除
        if (targetZ < -10)
        {
            generator.RemoveNote(this);
            Destroy(gameObject);
        }
    }
}
