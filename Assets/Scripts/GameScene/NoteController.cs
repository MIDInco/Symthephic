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
        if (generator == null) return;

        double elapsedTime = currentTime - tickTimeSeconds;
        double targetZ = -elapsedTime * generator.noteSpeed;

        transform.position = new Vector3(transform.position.x, transform.position.y, (float)targetZ);

        // 一定距離を超えたら削除
        if (targetZ < -10)
        {
            generator.RemoveNote(this);
            Destroy(gameObject);
        }
    }
}
