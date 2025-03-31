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

    // ホールド開始時刻 (Time.time) - 互換性のため残すか、不要なら削除
    private float holdStartTime = -1f;
    // ホールド開始時のゲーム内時間 (DSPタイムベース)
    private double holdStartGameTime = -1.0;

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
        // generator の null チェックも追加
        if (bodyInstance == null || endNoteObject == null || generator == null) return;

        Vector3 baseStartPosition = transform.position; // UpdatePosition で更新された現在の Head の位置
        Vector3 start = baseStartPosition;
        Vector3 end = endNoteObject.transform.position; // 終点

        // ホールド中の場合、帯を縮める (ゲーム内時間ベースで計算)
        if (isBeingHeld && holdStartGameTime >= 0 && generator.startTime > 0)
        {
            // 現在のゲーム内時間を取得 (DSPタイムベース)
            double currentGameTime = GameSceneManager.GetGameDspTime() - generator.startTime;
            // ホールド開始からの経過ゲーム内時間を計算
            double elapsedGameTime = currentGameTime - holdStartGameTime;

            // 経過時間が負の場合は 0 にする (誤差対策)
            if (elapsedGameTime < 0) elapsedGameTime = 0;

            // 縮小距離を計算
            float shrinkDistance = (float)elapsedGameTime * generator.noteSpeed;

            // 縮小後の目標 Z 座標を計算 (現在の Head の Z 座標 + 縮小距離)
            float targetZ = baseStartPosition.z + shrinkDistance;

            // 始点が終点の Z 座標を超えないようにクランプ
            start.z = Mathf.Min(targetZ, end.z);

            // // オプション: 完全に縮んだらホールド状態を解除するなどの処理も検討可能
            // if (start.z >= end.z)
            // {
            //     EndHold(); // 自動でホールド解除する場合
            // }
        }
        // else の場合、start は baseStartPosition のまま (縮小しない)

        // 帯の中心位置を計算
        Vector3 center = (start + end) / 2f;

        // 帯の長さを計算
        float length = Vector3.Distance(start, end);

        // 長さが非常に小さい場合のゼロ除算や LookAt の問題を避ける
        if (length > 0.001f)
        {
            // 帯の位置、向き、スケールを設定
            bodyInstance.transform.position = center;
            bodyInstance.transform.LookAt(end); // start が end に近づくと向きが不安定になる可能性に注意
            bodyInstance.transform.localScale = new Vector3(0.2f, 0.01f, length);
        }
        else
        {
            // 長さがほぼゼロになったら非表示にするか、スケールをゼロにする
            bodyInstance.transform.localScale = Vector3.zero; // スケールをゼロに
            // または bodyInstance.SetActive(false); // 非表示に
        }
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
        holdStartTime = Time.time; // 従来の holdStartTime も一応記録

        // ゲーム内時間を記録 (generator と startTime が有効な場合のみ)
        if (generator != null && generator.startTime > 0)
        {
            holdStartGameTime = GameSceneManager.GetGameDspTime() - generator.startTime;
        }
        else
        {
            holdStartGameTime = -1.0; // 無効な値としてマーク
            Debug.LogWarning("StartHold called but generator or startTime is not ready.");
        }
    }

    // ノートのホールドを終了
    public void EndHold()
    {
        isBeingHeld = false;
        holdStartGameTime = -1.0; // ゲーム内時間の記録もリセット
    }
}
