using UnityEngine;

public class NoteController : MonoBehaviour
{
    public double tickTimeSeconds; // 🎯 **このノートの Tick が譜面内で何秒に相当するか**
    private NotesGenerator generator;

    public int noteValue { get; set; }
    public long tick { get; set; } 
    public string uniqueID { get; private set; }

    public void Initialize(double tickTime, NotesGenerator gen, string id)
    {
        tickTimeSeconds = tickTime; // 🎯 譜面内の時間 (秒)
        generator = gen;
        uniqueID = id;
    }

    public void UpdatePosition(float currentTime)
    {
        if (generator == null) return;

        double elapsedTime = currentTime - tickTimeSeconds;
        double targetZ = -elapsedTime * generator.noteSpeed;

        transform.position = new Vector3(transform.position.x, transform.position.y, (float)targetZ);

        if (targetZ < -10f)
        {
            generator.RemoveNote(this);
            Destroy(gameObject);
        }
    }
}
