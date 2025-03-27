using UnityEngine;

public class LongNoteController : NoteController // ← ここを修正！
{
    private Transform head;
    private Transform body;

    public override void Initialize(double startTime, double endTime, NotesGenerator gen, string id = "", bool isLong = true)
    {
        base.Initialize(startTime, endTime, gen, id, isLong);

        head = transform.Find("Head");
        body = transform.Find("Body");

        if (head == null || body == null)
        {
            Debug.LogError("❌ LongNoteController: Head または Body が見つかりません！");
        }
    }

    public override void UpdatePosition(float currentTime)
    {
        if (GameSceneManager.IsPaused || generator == null) return;

        double elapsedTime = currentTime - tickTimeSeconds;
        double travelZ = -elapsedTime * generator.noteSpeed;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, (float)travelZ);

        // ロングノートの長さを調整
        float durationSec = (float)(endTimeSeconds - tickTimeSeconds);
        float lengthZ = durationSec * generator.noteSpeed;
        if (body != null)
        {
            body.localScale = new Vector3(1f, 1f, lengthZ);
            body.localPosition = new Vector3(0f, 0f, lengthZ / 2f); // Headより奥にずれる
        }
    }
}
