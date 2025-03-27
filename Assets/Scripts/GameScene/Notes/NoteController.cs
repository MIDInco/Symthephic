using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int noteValue;
    public long tick;
    public long endTick;
    public bool isLongNote = false;

    protected NotesGenerator generator;
    public double tickTimeSeconds;
    public double endTimeSeconds;
    public string uniqueID { get; private set; }

    public virtual void Initialize(double startTime, double endTime, NotesGenerator gen, string id, bool isLong)
    {
        tickTimeSeconds = startTime;
        endTimeSeconds = endTime;
        generator = gen;
        uniqueID = id;
        isLongNote = isLong;
    }

    public virtual void UpdatePosition(float currentTime)
    {
        if (GameSceneManager.IsPaused || generator == null) return;

        double elapsedTime = currentTime - tickTimeSeconds;
        double targetZ = -elapsedTime * generator.noteSpeed;

        transform.position = new Vector3(transform.position.x, transform.position.y, (float)targetZ);

        if (targetZ < -10)
        {
            generator.RemoveNote(this);
            Destroy(gameObject);
        }
    }
}
