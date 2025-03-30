using System;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using MPTKDemoCatchMusic; // 必要に応じて削除または調整

public class NotesGenerator : MonoBehaviour
{
    public GameObject Notes; // 通常ノーツのプレハブ
    public GameObject LongNoteEnd; // ロングノーツ終点プレハブ
    public GameObject LongNoteBodyPrefab; // ロングノーツ帯プレハブ
    public Transform spawnPoint; // ノーツ生成基準点 (Y座標用)
    public float noteSpeed = 1f; // ノーツの速度

    public List<NoteController> noteControllers = new List<NoteController>();
    public double startTime { get; private set; } // ゲーム開始のDSPタイム
    public MidiFilePlayer midiFilePlayer;

    public int TPQN { get; private set; } // MIDIの分解能
    public float BPM { get; private set; } // 初期BPM (参考値)

    // public float chartDelay = 0.0f; // Noteoffset.Instanceから取得するため不要に
    public bool isReady { get; private set; } = false; // ノーツ生成準備完了フラグ
    // public int chartDelayInTicks = 0; // Tick単位の遅延は現在未使用

    public Transform judgmentLine; // 判定ラインのTransform

    public event Action OnChartPlaybackStart; // 譜面再生開始イベント
    public event Action<NoteController> OnNoteGenerated; // ノーツ生成イベント

    private List<(long tick, double tempo)> cachedTempoEvents = new List<(long, double)>(); // テンポ変更イベントのキャッシュ
    public JudgmentManager judgmentManager; // 判定マネージャー
    private bool isPaused = false; // ポーズ状態フラグ

    // --- 新しく追加するメンバー変数 ---
    private List<MPTKEvent> midiEvents; // ソート済みの全MIDIイベント
    private int nextEventIndex = 0; // 次に処理するMIDIイベントのインデックス
    private Dictionary<int, MPTKEvent> pendingNoteOns = new Dictionary<int, MPTKEvent>(); // NoteOffを待っているNoteOnイベント (キー: NoteValue)
    private double noteGenerationLeadTime = 5.0; // ノーツを事前に生成しておく時間 (秒)。ノーツが画面外から現れるまでの時間などに基づいて調整。
    private int totalScoreNotes = 0; // スコア計算対象の総ノーツ数
    // --- ここまで ---

    void Awake()
    {
        Debug.Log("✅ NotesGenerator の Awake が実行されました");
        // 必要に応じて noteSpeed を GameSettings から読み込む
        noteSpeed = GameSettings.NoteSpeed;
    }

    void Start()
    {
        if (MusicManager.SelectedMusic != null)
        {
            Debug.Log($"🎯 NotesGenerator: 選択されたMIDIを受け取りました → {MusicManager.SelectedMusic.MidiFileName}");
            // StartCoroutine(LoadMidiFileAsync()); // LoadSelectedMidiAndGenerateNotes から呼ばれるように変更
        }
        else
        {
            Debug.LogError("❌ GameScene に MIDI データが渡っていません！");
        }
    }

    void Update()
    {
        // isReady前、ポーズ中、startTime未設定の場合は何もしない
        if (!isReady || isPaused || startTime <= 0) return;

        double currentGameDspTime = GameSceneManager.GetGameDspTime(); // 現在のDSPタイムを取得
        double currentTime = currentGameDspTime - startTime; // ゲーム内経過時間

        // ノーツ生成処理
        GenerateNotesIfNeeded(currentTime);

        // 既存のノーツ位置更新処理
        noteControllers.RemoveAll(note => note == null); // 破棄されたノーツをリストから削除
        foreach (var note in noteControllers)
        {
            note?.UpdatePosition((float)currentTime); // 各ノーツの位置を更新
        }
    }

    // MIDIファイルを非同期で読み込み、イベントをキャッシュする
    IEnumerator LoadMidiFileAsync()
    {
        isReady = false;
        nextEventIndex = 0; // イベントインデックスをリセット
        pendingNoteOns.Clear(); // 待機中NoteOnをクリア
        noteControllers.Clear(); // 既存ノーツリストをクリア (念のため)
        totalScoreNotes = 0; // 総ノーツ数をリセット

        if (midiFilePlayer == null)
        {
            Debug.LogError("❌ MidiFilePlayerが設定されていません。");
            yield break;
        }

        // NoteOffイベントを保持するように設定
        midiFilePlayer.MPTK_KeepNoteOff = true;
        // midiFilePlayer.MPTK_KeepEndTrack = true; // 必要であればEndTrackも保持

        Debug.Log($"⏳ MIDIファイルの読み込みを開始: {midiFilePlayer.MPTK_MidiName}");
        MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
        if (midiLoad == null || midiLoad.MPTK_MidiEvents == null)
        {
            Debug.LogError($"❌ MIDIファイルの読み込みに失敗しました: {midiFilePlayer.MPTK_MidiName}");
            yield break;
        }
        Debug.Log($"✅ MIDIファイルの読み込み完了: {midiFilePlayer.MPTK_MidiName}, Events: {midiLoad.MPTK_MidiEvents.Count}");

        TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
        BPM = (float)midiFilePlayer.MPTK_Tempo; // 初期BPMを取得

        CacheTempoEvents(midiLoad); // テンポイベントをキャッシュ

        // 全MIDIイベントをTick順でソートして保持
        midiEvents = midiLoad.MPTK_MidiEvents.OrderBy(ev => ev.Tick).ToList();

        // --- 全ノーツ数を事前に計算 ---
        CalculateTotalNotes();
        ScoreManager.Instance?.SetTotalNotes(totalScoreNotes);
        Debug.Log($"📊 総ノーツ数 (スコア対象): {totalScoreNotes}");
        // --- ここまで ---

        // startTime と isReady の設定は ChartPlaybackManager が担当するため、ここでは行わない
        isReady = true; // MIDIデータの読み込みと解析が完了したことを示すフラグ
        Debug.Log("✅ NotesGenerator: MIDIデータの読み込みと解析が完了しました。再生準備OK。");

        // 譜面再生開始イベントの発火 (必要であれば)
        // OnChartPlaybackStart?.Invoke(); // StartPlaybackメソッドで行うのでここでは不要かも
    }

    // テンポ変更イベントをキャッシュする
    void CacheTempoEvents(MidiLoad midiLoad)
    {
        cachedTempoEvents.Clear();
        if (midiLoad.MPTK_MidiEvents == null) return;

        foreach (var ev in midiLoad.MPTK_MidiEvents)
        {
            if (ev.Meta == MPTKMeta.SetTempo)
            {
                cachedTempoEvents.Add((ev.Tick, ev.Value)); // Valueがテンポ値 (microsec/quarter note)
            }
        }
        // Tick順にソート
        cachedTempoEvents.Sort((a, b) => a.tick.CompareTo(b.tick));
        Debug.Log($"🎶 テンポイベントをキャッシュしました: {cachedTempoEvents.Count} 件");
    }

    // 事前に全ノーツ数を計算する
    void CalculateTotalNotes()
    {
        totalScoreNotes = 0;
        if (midiEvents == null) return;

        Dictionary<int, long> noteOnTicks = new Dictionary<int, long>();

        foreach (var ev in midiEvents)
        {
            if (ev.Command == MPTKCommand.NoteOn && ev.Velocity > 0)
            {
                // 同じノートナンバーのNoteOnが既にあれば上書き（前のNoteOffが欠落している場合など）
                noteOnTicks[ev.Value] = ev.Tick;
            }
            else if (ev.Command == MPTKCommand.NoteOff || (ev.Command == MPTKCommand.NoteOn && ev.Velocity == 0))
            {
                if (noteOnTicks.ContainsKey(ev.Value))
                {
                    long noteOnTick = noteOnTicks[ev.Value];
                    long duration = ev.Tick - noteOnTick;
                    // 8分音符以上のTick数 (TPQN / 2) を基準にロングノーツを判定
                    bool isLong = duration >= TPQN / 2;

                    totalScoreNotes += isLong ? 2 : 1; // ロングノーツは始点と終点で2カウント

                    noteOnTicks.Remove(ev.Value); // 処理済みとして削除
                }
            }
        }
    }


    // 必要に応じてノーツを生成する
    void GenerateNotesIfNeeded(double currentGameTime)
    {
        if (midiEvents == null) return;

        while (nextEventIndex < midiEvents.Count)
        {
            MPTKEvent currentEvent = midiEvents[nextEventIndex];
            double eventTime = GetTimeFromTick(currentEvent.Tick);

            // イベント発生時間が、現在のゲーム時間 + 先行生成時間 より未来の場合は、まだ生成しない
            if (eventTime > currentGameTime + noteGenerationLeadTime)
            {
                break; // これ以降のイベントはまだ先なのでループを抜ける
            }

            // --- イベント処理 ---
            if (currentEvent.Command == MPTKCommand.NoteOn && currentEvent.Velocity > 0)
            {
                // NoteOnイベントを待機リストに追加（同じノート番号が既にあれば上書き）
                pendingNoteOns[currentEvent.Value] = currentEvent;
            }
            else if (currentEvent.Command == MPTKCommand.NoteOff || (currentEvent.Command == MPTKCommand.NoteOn && currentEvent.Velocity == 0))
            {
                // NoteOff (または Velocity 0 の NoteOn) イベント
                if (pendingNoteOns.TryGetValue(currentEvent.Value, out MPTKEvent noteOnEvent))
                {
                    // 対応するNoteOnが見つかったらノーツを生成
                    InstantiateNote(noteOnEvent, currentEvent, currentGameTime); // generationTime を渡す
                    pendingNoteOns.Remove(currentEvent.Value); // 待機リストから削除
                }
                // 対応するNoteOnが見つからない場合は無視（NoteOffが先行しているなど）
            }
            // --- イベント処理ここまで ---

            nextEventIndex++; // 次のイベントへ
        }
    }

    // ノーツオブジェクトを生成・初期化する
void InstantiateNote(MPTKEvent noteOn, MPTKEvent noteOff, double generationTime)
{
    long durationTicks = noteOff.Tick - noteOn.Tick;
    bool isLong = durationTicks >= TPQN / 2;

    double noteOnTime = GetTimeFromTick(noteOn.Tick);
    double noteOffTime = GetTimeFromTick(noteOff.Tick);
    float startX = GetFixedXPosition(noteOn.Value);
    float judgmentLineZ = judgmentLine != null ? judgmentLine.position.z : 0f;
    float timeToJudgeAtGeneration = (float)(noteOnTime - generationTime);
    float initialZ = judgmentLineZ + timeToJudgeAtGeneration * noteSpeed;

    // 🔷 制御用のNoteRootオブジェクト（NoteControllerアタッチ用）
    GameObject noteRoot = new GameObject($"Note_{noteOn.Value}_{noteOn.Tick}");
    noteRoot.transform.position = new Vector3(startX, spawnPoint.position.y, initialZ);
    noteRoot.transform.rotation = Quaternion.identity;

    // 🔷 Headオブジェクト（見た目専用）
    GameObject head = Instantiate(Notes);
    head.transform.SetParent(noteRoot.transform);
    head.transform.localPosition = Vector3.zero;
    head.SetActive(true);

    // 🔷 EndNoteオブジェクト（ロングノーツのみ）
    GameObject endNoteObject = null;
    if (isLong && LongNoteEnd != null)
    {
        float timeToEndJudgeAtGeneration = (float)(noteOffTime - generationTime);
        float endInitialZ = judgmentLineZ + timeToEndJudgeAtGeneration * noteSpeed;

        endNoteObject = Instantiate(LongNoteEnd);
        endNoteObject.transform.position = new Vector3(startX, spawnPoint.position.y, endInitialZ);
        endNoteObject.transform.rotation = Quaternion.identity;
        endNoteObject.SetActive(true);
    }

    // 🔷 NoteController追加と初期化
    NoteController controller = noteRoot.AddComponent<NoteController>();
    string id = $"{noteOn.Value}_{noteOn.Tick}";
    controller.Initialize(noteOnTime, this, id);
    controller.noteValue = noteOn.Value;
    controller.tick = noteOn.Tick;
    controller.isLongNote = isLong;
    controller.endTick = noteOff.Tick;
    controller.endTime = noteOffTime;
    controller.bodyPrefab = LongNoteBodyPrefab;
    controller.SetEndNoteObject(endNoteObject);
    controller.headObject = head; // 🆕 Head登録

    noteControllers.Add(controller);
    OnNoteGenerated?.Invoke(controller);
}


    // Tick値からゲーム内時間（startTimeからの経過秒数）を取得
    double GetTimeFromTick(long tick)
    {
        double currentTempo = 500000.0; // デフォルトテンポ (120 BPM)
        long lastTick = 0;
        double currentTime = 0.0;

        // キャッシュされたテンポイベントを順に見ていく
        foreach (var tempoEvent in cachedTempoEvents)
        {
            // 現在のテンポでの1Tickあたりの秒数 (Seconds Per Tick)
            double secondsPerTick = currentTempo / 1000000.0 / TPQN;

            // 目的のTickが現在のテンポ区間に入る前に見つかった場合
            if (tempoEvent.tick > tick)
            {
                break; // ループを抜ける
            }

            // 前回のテンポ変更からの経過Tick数 * 1Tickあたりの秒数 を加算
            currentTime += (tempoEvent.tick - lastTick) * secondsPerTick;
            lastTick = tempoEvent.tick; // Tickを更新
            currentTempo = tempoEvent.tempo; // テンポを更新
        }

        // 最後のテンポ区間での時間を計算して加算
        double lastSecondsPerTick = currentTempo / 1000000.0 / TPQN;
        currentTime += (tick - lastTick) * lastSecondsPerTick;

        return currentTime;
    }

    // ゲーム内時間からTick値を取得 (必要であれば使う)
    public long GetCurrentTickWithTempo(double time)
    {
        double currentTempo = 500000.0;
        long lastTick = 0;
        double currentTime = 0.0;
        double targetTime = time; // startTimeからの経過時間

        foreach (var tempo in cachedTempoEvents)
        {
            double spt = currentTempo / 1000000.0 / TPQN;
            double segmentDuration = (tempo.tick - lastTick) * spt;

            // 目的の時間が現在のテンポ区間内にある場合
            if (currentTime + segmentDuration >= targetTime)
            {
                double timeIntoSegment = targetTime - currentTime;
                // spt が 0 または非常に小さい場合のゼロ除算を避ける
                if (spt <= double.Epsilon) return lastTick;
                return lastTick + (long)(timeIntoSegment / spt);
            }

            currentTime += segmentDuration;
            lastTick = tempo.tick;
            currentTempo = tempo.tempo;
        }

        // 最後のテンポ区間でのTickを計算
        double lastSpt = currentTempo / 1000000.0 / TPQN;
        double timeIntoLastSegment = targetTime - currentTime;
        // timeIntoLastSegment が負になる場合は startTime より前の時間なので 0 Tick とする (またはエラー処理)
        if (timeIntoLastSegment < 0) return 0;

        // spt が 0 または非常に小さい場合のゼロ除算を避ける
        if (lastSpt <= double.Epsilon) return lastTick;

        return lastTick + (long)(timeIntoLastSegment / lastSpt);
    }


    // ノート番号からX座標を取得
    public float GetFixedXPosition(int noteValue)
    {
        // キーボードのC4(60)からB4(71)に対応させる例
        // 必要に応じてマッピングを変更してください
        switch (noteValue)
        {
            // --- レーンのマッピング例 ---
            case 60: return -2.5f; // レーン1
            case 61: return -1.5f; // レーン2
            case 62: return -0.5f; // レーン3
            case 63: return 0.5f;  // レーン4
            case 64: return 1.5f;  // レーン5
            case 65: return 2.5f;  // レーン6
            // --- 必要に応じて他のノート番号も追加 ---
            default:
                // マッピング外のノート番号の場合、ログを出して中央(0)に配置するなど
                // Debug.LogWarning($"未定義のノート番号: {noteValue}。中央レーンに配置します。");
                return 0f;
        }
    }

    // 現在管理しているNoteControllerのリストを返す
    public List<NoteController> GetNoteControllers() => noteControllers;

    // 譜面再生を開始する (isReadyをtrueにする)
    public void StartPlayback()
    {
        if (isReady) // 既に再生中の場合は何もしないか、ログを出す
        {
            Debug.LogWarning("譜面は既に再生中です。");
            return;
        }
        if (startTime <= 0)
        {
            Debug.LogError("❌ startTimeが設定されていないため、再生を開始できません。");
            return;
        }
        // isReady = true; // LoadMidiFileAsync の最後で true にするので、ここでは不要かも
        OnChartPlaybackStart?.Invoke(); // 再生開始イベントを発火
        Debug.Log("▶️ 譜面再生を開始します。");
    }

    // startTime を外部から設定する (通常は LoadMidiFileAsync 内で設定)
    public void SetStartTime(double time)
    {
        startTime = time;
        Debug.Log($"⏱️ startTime が外部から設定されました: {startTime}");
    }

    // 選択されたMIDIを読み込んでノーツ生成準備を開始するメソッド
    public void LoadSelectedMidiAndGenerateNotes()
    {
        // noteSpeed を最新の設定値に更新
        noteSpeed = GameSettings.NoteSpeed;
        Debug.Log($"⚙️ Note Speed を適用: {noteSpeed}");

        if (MusicManager.SelectedMusic != null && midiFilePlayer != null)
        {
            midiFilePlayer.MPTK_MidiName = MusicManager.SelectedMusic.MidiFileName;
            StartCoroutine(LoadMidiFileAsync());
        }
        else
        {
            if (MusicManager.SelectedMusic == null)
                Debug.LogError("❌ MusicManager に選択された楽曲がありません。");
            if (midiFilePlayer == null)
                Debug.LogError("❌ MidiFilePlayer がアタッチされていません。");
        }
    }

    // 再生を一時停止
    public void PausePlayback()
    {
        isPaused = true;
        Debug.Log("⏸️ 譜面再生を一時停止しました。");
    }

    // 再生を再開
    public void ResumePlayback()
    {
        isPaused = false;
        Debug.Log("▶️ 譜面再生を再開しました。");
    }

    // 状態をリセット (リトライ時などに使用)
    public void ResetState()
    {
        isReady = false;
        isPaused = false;
        startTime = 0;
        nextEventIndex = 0;
        pendingNoteOns.Clear();
        midiEvents?.Clear(); // nullチェック
        cachedTempoEvents.Clear();
        totalScoreNotes = 0;

        // 生成済みのノーツを全て破棄
        foreach (var note in noteControllers)
        {
            if (note != null)
            {
                // ロングノーツの終点も破棄する必要があるか確認
                if (note.endNoteObject != null)
                {
                    Destroy(note.endNoteObject);
                }
                Destroy(note.gameObject);
            }
        }
        noteControllers.Clear(); // リストをクリア

        Debug.Log("🔄 NotesGenerator の状態をリセットしました。");
    }

    // ノーツが判定ラインを通過するなどして不要になった時に呼ばれる
    public void RemoveNote(NoteController note)
    {
        if (note != null)
        {
            noteControllers.Remove(note);
            // Destroy(note.gameObject); // 破棄は NoteController 自身や JudgmentManager が行う想定
        }
    }
}
