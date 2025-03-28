// ✅ NoteOnのDurationでロングノーツ判定を行い、帯PrefabをNoteControllerに設定するよう修正済み NotesGenerator.cs
using System;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using MPTKDemoCatchMusic;

public class NotesGenerator : MonoBehaviour
{
    public GameObject Notes;
    public GameObject LongNoteEnd;
    public GameObject LongNoteBodyPrefab; // 追加：帯プレハブ
    public Transform spawnPoint;
    public float noteSpeed = 1f;

    public List<NoteController> noteControllers = new List<NoteController>();
    public double startTime { get; private set; }
    public MidiFilePlayer midiFilePlayer;

    public int TPQN { get; private set; }
    public float BPM { get; private set; }

    public float chartDelay = 0.0f;
    public bool isReady { get; private set; } = false;
    public int chartDelayInTicks = 0;

    public Transform judgmentLine;

    public event Action OnChartPlaybackStart;
    public event Action<NoteController> OnNoteGenerated;

    private List<(long tick, double tempo)> cachedTempoEvents = new List<(long, double)>();
    public JudgmentManager judgmentManager;
    private bool isPaused = false;

    void Awake() => Debug.Log("✅ NotesGenerator の Awake が実行されました");

    void Start()
    {
        if (MusicManager.SelectedMusic != null)
        {
            Debug.Log($"🎯 NotesGenerator: 選択されたMIDIを受け取りました → {MusicManager.SelectedMusic.MidiFileName}");
        }
        else Debug.LogError("❌ GameScene に MIDI データが渡っていません！");
    }

    void Update()
    {
        double currentTime = GameSceneManager.GetGameDspTime() - startTime;
        if (GameSceneManager.IsPaused || GameSceneManager.IsResuming || !isReady || isPaused) return;

        noteControllers.RemoveAll(note => note == null);
        foreach (var note in noteControllers)
            note?.UpdatePosition((float)currentTime);
    }

    public void RemoveNote(NoteController note) => noteControllers.Remove(note);

    void CacheTempoEvents(MidiLoad midiLoad)
    {
        cachedTempoEvents.Clear();
        foreach (var ev in midiLoad.MPTK_MidiEvents)
            if (ev.Meta == MPTKMeta.SetTempo)
                cachedTempoEvents.Add((ev.Tick, ev.Value));
        cachedTempoEvents.Sort((a, b) => a.tick.CompareTo(b.tick));
    }

    IEnumerator LoadMidiFileAsync()
    {
        isReady = false;
        if (midiFilePlayer == null) yield break;
        midiFilePlayer.MPTK_KeepNoteOff = true;
        MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
        if (midiLoad == null) yield break;

        TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
        BPM = (float)midiFilePlayer.MPTK_Tempo;

        CacheTempoEvents(midiLoad);
        GenerateNotes(midiLoad);
        ScoreManager.Instance?.SetTotalNotes(noteControllers.Count);

        startTime = AudioSettings.dspTime + (Noteoffset.Instance?.GetChartDelay() ?? 0f);
        Debug.Log($"✅ NotesGenerator: isReady を true に設定しました（譜面再生準備完了）");
    }

    void GenerateNotes(MidiLoad midiLoad)
    {
        var events = midiLoad.MPTK_MidiEvents;
        int globalIndex = 0;

        Dictionary<int, Stack<MPTKEvent>> noteOnStacks = new();

        foreach (var ev in events)
        {
            if (ev.Command == MPTKCommand.NoteOn && ev.Velocity > 0)
            {
                if (!noteOnStacks.ContainsKey(ev.Value))
                    noteOnStacks[ev.Value] = new Stack<MPTKEvent>();

                noteOnStacks[ev.Value].Push(ev);
            }
            else if ((ev.Command == MPTKCommand.NoteOff || (ev.Command == MPTKCommand.NoteOn && ev.Velocity == 0)))
            {
                if (noteOnStacks.ContainsKey(ev.Value) && noteOnStacks[ev.Value].Count > 0)
                {
                    var noteOn = noteOnStacks[ev.Value].Pop();
                    long duration = ev.Tick - noteOn.Tick;

                    bool isLong = duration >= TPQN / 2;

                    double noteTime = GetTimeFromTick(noteOn.Tick);
                    double endTime = GetTimeFromTick(ev.Tick);

                    double travelTime = 5.0;
                    double timeUntilJudgment = noteTime - startTime;
                    double startZ = (timeUntilJudgment + travelTime) * noteSpeed;
                    float startX = GetFixedXPosition(noteOn.Value);

                    GameObject note = Instantiate(Notes);
                    note.transform.position = new Vector3(startX, spawnPoint.position.y, (float)startZ);
                    note.transform.rotation = Quaternion.identity;
                    note.transform.SetParent(null);
                    note.SetActive(true);

                    GameObject endNote = null;
                    if (isLong && LongNoteEnd != null)
                    {
                        double endTimeUntilJudgment = endTime - startTime;
                        double endZ = (endTimeUntilJudgment + travelTime) * noteSpeed;

                        endNote = Instantiate(LongNoteEnd);
                        endNote.transform.position = new Vector3(startX, spawnPoint.position.y, (float)endZ);
                        endNote.transform.rotation = Quaternion.identity;
                        endNote.transform.SetParent(null);
                        endNote.SetActive(true);

                        Debug.Log($"🔚 ロングノーツ終点生成: Z={endZ:F2}, duration={duration} tick");
                    }

                    NoteController controller = note.GetComponent<NoteController>();
                    string id = globalIndex.ToString();
                    globalIndex++;
                    controller.Initialize(noteTime, this, id);
                    controller.noteValue = noteOn.Value;
                    controller.tick = noteOn.Tick;
                    controller.isLongNote = isLong;
                    controller.endTick = ev.Tick;
                    controller.endTime = endTime;
                    controller.bodyPrefab = LongNoteBodyPrefab;
                    controller.SetEndNoteObject(endNote);

                    noteControllers.Add(controller);
                    OnNoteGenerated?.Invoke(controller);

                    // ✅ デバッグログ追加
                    if (controller.isLongNote)
                    {
                    Debug.Log($"🟦 ロングノーツ生成: ID={id}, ノート番号={noteOn.Value}, BodyPrefab={(controller.bodyPrefab != null)}, EndNote={(endNote != null)}");
                    }
                }
            }
        }
    }

    double GetTimeFromTick(long tick)
    {
        double currentTempo = 500000.0;
        long lastTick = 0;
        double currentTime = 0.0;

        foreach (var tempo in cachedTempoEvents)
        {
            double spt = currentTempo / 1000000.0 / TPQN;
            if (tempo.tick > tick) break;
            currentTime += (tempo.tick - lastTick) * spt;
            lastTick = tempo.tick;
            currentTempo = tempo.tempo;
        }
        double sptNow = currentTempo / 1000000.0 / TPQN;
        return currentTime + (tick - lastTick) * sptNow;
    }

    public long GetCurrentTickWithTempo(double time)
    {
        double currentTempo = 500000.0;
        long lastTick = 0;
        double currentTime = 0.0;

        foreach (var tempo in cachedTempoEvents)
        {
            double spt = currentTempo / 1000000.0 / TPQN;
            double deltaTime = tempo.tick - lastTick;
            double segmentTime = deltaTime * spt;

            if (currentTime + segmentTime > time)
                break;

            currentTime += segmentTime;
            lastTick = tempo.tick;
            currentTempo = tempo.tempo;
        }

        double sptNow = currentTempo / 1000000.0 / TPQN;
        long tickNow = lastTick + (long)((time - currentTime) / sptNow);
        return tickNow;
    }

    public float GetFixedXPosition(int noteValue)
    {
        switch (noteValue)
        {
            case 60: return -2.5f;
            case 61: return -1.5f;
            case 62: return -0.5f;
            case 63: return  0.5f;
            case 64: return  1.5f;
            case 65: return  2.5f;
            default: return 0f;
        }
    }

    public List<NoteController> GetNoteControllers() => noteControllers;
    public void StartPlayback() { isReady = true; OnChartPlaybackStart?.Invoke(); }
    public void SetStartTime(double time) { startTime = time; }
    public void LoadSelectedMidiAndGenerateNotes()
    {
        noteSpeed = GameSettings.NoteSpeed;
        if (MusicManager.SelectedMusic != null)
        {
            midiFilePlayer.MPTK_MidiName = MusicManager.SelectedMusic.MidiFileName;
            StartCoroutine(LoadMidiFileAsync());
        }
    }
    public void PausePlayback() => isPaused = true;
    public void ResumePlayback() => isPaused = false;
    public void ResetState()
    {
        isReady = false;
        foreach (var note in noteControllers)
            if (note != null) Destroy(note.gameObject);
        noteControllers.Clear();
    }
}