using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int noteValue;
    public long tick;
    public bool isLongNote = false;
    public long endTick;
    public double endTime;

    public GameObject endNoteObject;

    private NotesGenerator generator;
    public double tickTimeSeconds;
    public string uniqueID { get; private set; }

    private Vector3 initialPosition;
    private Vector3 endInitialPosition;

    public void Initialize(double noteTime, NotesGenerator gen, string id)
    {
        tickTimeSeconds = noteTime;
        generator = gen;
        uniqueID = id;
        initialPosition = transform.position;
    }

    public void SetEndNoteObject(GameObject endObj)
    {
        endNoteObject = endObj;
        if (endObj != null)
        {
            endInitialPosition = endObj.transform.position;
            endNoteObject.name = $"EndNote_{uniqueID}";
        }
    }

    private Vector3 CalculatePosition(double currentTime, double targetTime, Vector3 basePosition)
    {
        double elapsedTime = currentTime - targetTime;
        double targetZ = -elapsedTime * generator.noteSpeed;
        return new Vector3(basePosition.x, basePosition.y, (float)targetZ);
    }

    public void UpdatePosition(float currentTime)
    {
        if (GameSceneManager.IsPaused || generator == null) return;

        Vector3 notePosition = CalculatePosition(currentTime, tickTimeSeconds, initialPosition);
        transform.position = notePosition;

        if (isLongNote && endNoteObject != null)
        {
            Vector3 endPosition = CalculatePosition(currentTime, endTime, endInitialPosition);
            endNoteObject.transform.position = endPosition;

            if (endPosition.z < -10 && notePosition.z < -10)
            {
                GameObject.Destroy(endNoteObject);
            }
        }

        if (notePosition.z < -10)
        {
            if (!isLongNote || endNoteObject == null)
            {
                generator.RemoveNote(this);
                Destroy(gameObject);
            }
        }
    }
}