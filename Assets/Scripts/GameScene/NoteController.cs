using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int noteValue;
    public long tick;
    public bool isLongNote = false;
    public long endTick;
    public double endTime;

    public GameObject endNoteObject;
    public GameObject bodyPrefab; // Inspectorで設定
    private GameObject bodyInstance;

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

    public GameObject GetBodyInstance()
    {
        return bodyInstance;
    }

    public void SetEndNoteObject(GameObject endObj)
    {
        endNoteObject = endObj;
        if (endObj != null)
        {
            endInitialPosition = endObj.transform.position;
            endNoteObject.name = $"EndNote_{uniqueID}";

            // 帯の生成（Prefabが設定されていれば）
            if (bodyPrefab != null)
            {
                bodyInstance = GameObject.Instantiate(bodyPrefab);
                bodyInstance.SetActive(true); // ← 明示的に可視化
                bodyInstance.transform.SetParent(transform.parent); // 同じ階層に配置
                bodyInstance.name = $"LongBody_{uniqueID}";

                UpdateBody(); // ✅ ここを追加！
            }
        }
    }

    private Vector3 CalculatePosition(double currentTime, double targetTime, Vector3 basePosition)
    {
        double elapsedTime = currentTime - targetTime;
        double targetZ = -elapsedTime * generator.noteSpeed;
        return new Vector3(basePosition.x, basePosition.y, (float)targetZ);
    }

    private void UpdateBody()
    {
        if (bodyInstance == null || endNoteObject == null) return;

        bodyInstance.SetActive(true); // ✅ 念のため明示的に有効化

        Vector3 start = transform.position;
        Vector3 end = endNoteObject.transform.position;
        Vector3 center = (start + end) / 2f;
        float length = Vector3.Distance(start, end);

        bodyInstance.transform.position = center;
        bodyInstance.transform.LookAt(end);
        bodyInstance.transform.localScale = new Vector3(0.2f, 0.01f, length);
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

            // 帯の追従
            UpdateBody();

            if (endPosition.z < -10 && notePosition.z < -10)
            {
                GameObject.Destroy(endNoteObject);
                GameObject.Destroy(bodyInstance);
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
