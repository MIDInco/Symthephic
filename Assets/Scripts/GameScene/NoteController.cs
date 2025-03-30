using UnityEngine;

public class NoteController : MonoBehaviour
{
    // ノートの値（例: 音符の種類）
    public int noteValue;

    // ノートのタイミングを表すTick値
    public long tick;

    // ロングノートかどうかを示すフラグ
    public bool isLongNote = false;

    // ロングノートの終了Tick値
    public long endTick;

    // ロングノートの終了時間（秒単位）
    public double endTime;

    // ロングノートの終点オブジェクト
    public GameObject endNoteObject;

    // ロングノートの帯（Body）のプレハブ
    public GameObject bodyPrefab;

    // ノートの見た目を表すHeadオブジェクト
    public GameObject headObject;

    // ロングノートの帯（Body）のインスタンス
    private GameObject bodyInstance;

    // ノートを生成したジェネレーターへの参照
    private NotesGenerator generator;

    // ノートの時間（秒単位）
    public double tickTimeSeconds;

    // ノートの一意なID
    public string uniqueID { get; private set; }

    // ノートの初期位置
    private Vector3 initialPosition;

    // ロングノートの終点の初期位置
    private Vector3 endInitialPosition;

    // ホールド開始時刻
    private float holdStartTime = -1f;

    // ノートが現在ホールドされているかどうか
    public bool isBeingHeld = false;

    // スムーズな長さ計算用のキャッシュ
    private float smoothedLength = -1f;

    // ノートを初期化するメソッド
    public void Initialize(double noteTime, NotesGenerator gen, string id)
    {
        tickTimeSeconds = noteTime;
        generator = gen;
        uniqueID = id;
        initialPosition = transform.position;
    }

    // ロングノートの帯（Body）のインスタンスを取得
    public GameObject GetBodyInstance()
    {
        return bodyInstance;
    }

    // ロングノートの終点オブジェクトを設定
    public void SetEndNoteObject(GameObject endObj)
    {
        endNoteObject = endObj;
        if (endObj != null)
        {
            endInitialPosition = endObj.transform.position;
            endNoteObject.name = $"EndNote_{uniqueID}";

            // 帯（Body）のインスタンスを生成
            if (bodyPrefab != null)
            {
                bodyInstance = GameObject.Instantiate(bodyPrefab);
                bodyInstance.SetActive(true);
                bodyInstance.transform.SetParent(transform); // 親オブジェクトを設定
                bodyInstance.name = $"LongBody_{uniqueID}";
                UpdateBody(); // 帯の見た目を更新
            }
        }
    }

    // ノートの位置を計算するヘルパーメソッド
    private Vector3 CalculatePosition(double currentTime, double targetTime, Vector3 basePosition)
    {
        double elapsedTime = currentTime - targetTime;
        double targetZ = -elapsedTime * generator.noteSpeed; // Z軸方向の移動を計算
        return new Vector3(basePosition.x, basePosition.y, (float)targetZ);
    }

    // ロングノートの帯（Body）の見た目を更新
    private void UpdateBody()
    {
        if (bodyInstance == null || endNoteObject == null) return;

        Vector3 start = transform.position; // 始点
        Vector3 end = endNoteObject.transform.position; // 終点

        // ホールド中の場合、帯を縮める
        if (isBeingHeld && holdStartTime > 0f)
        {
            float elapsed = Time.time - holdStartTime;
            float shrinkZ = elapsed * generator.noteSpeed;
            start.z += shrinkZ;

            if (start.z > end.z)
                start.z = end.z; // 始点が終点を超えないようにする
        }

        // 帯の中心位置を計算
        Vector3 center = (start + end) / 2f;

        // 帯の長さを計算
        float length = Vector3.Distance(start, end);

        // 帯の位置、向き、スケールを設定
        bodyInstance.transform.position = center;
        bodyInstance.transform.LookAt(end);
        bodyInstance.transform.localScale = new Vector3(0.2f, 0.01f, length);
    }

    // ノートの位置を更新
    public void UpdatePosition(float currentTime)
    {
        if (GameSceneManager.IsPaused || generator == null) return;

        // ノートの現在位置を計算
        Vector3 notePosition = CalculatePosition(currentTime, tickTimeSeconds, initialPosition);
        transform.position = notePosition;

        // ロングノートの場合、終点と帯を更新
        if (isLongNote && endNoteObject != null)
        {
            Vector3 endPosition = CalculatePosition(currentTime, endTime, endInitialPosition);
            endNoteObject.transform.position = endPosition;
            UpdateBody();

            // ノートが画面外に出た場合、オブジェクトを破棄
            if (endPosition.z < -10 && notePosition.z < -10)
            {
                GameObject.Destroy(endNoteObject);
                GameObject.Destroy(bodyInstance);
            }
        }
    }

    // ノートのホールドを開始
    public void StartHold()
    {
        isBeingHeld = true;
        holdStartTime = Time.time;
    }

    // ノートのホールドを終了
    public void EndHold()
    {
        isBeingHeld = false;
    }
}
