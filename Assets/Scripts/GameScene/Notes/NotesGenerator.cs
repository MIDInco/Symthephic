using System;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using MPTKDemoCatchMusic;

public class NotesGenerator : MonoBehaviour
{
    public GameObject LongNotePrefab;
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

    void Update()
    {
        if (GameSceneManager.IsPaused || GameSceneManager.IsResuming || !isReady) return;

        double currentTime = GameSceneManager.GetGameDspTime() - startTime;
        noteControllers.RemoveAll(note => note == null);
        foreach (var note in noteControllers) note?.UpdatePosition((float)currentTime);
        if (isPaused) return;
    }

    public void RemoveNote(NoteController note)
    {
        noteControllers.Remove(note);
    }

    void CacheTempoEvents(MidiLoad midiLoad)
    {
        cachedTempoEvents.Clear();
        foreach (var ev in midiLoad.MPTK_MidiEvents)
            if (ev.Meta == MPTKMeta.SetTempo)
                cachedTempoEvents.Add((ev.Tick, ev.Value));
        cachedTempoEvents.Sort((a, b) => a.tick.CompareTo(b.tick));
    }

    public long GetCurrentTickWithTempo(double dspTime)
    {
        double timeSinceStart = dspTime - startTime;
        double currentTempo = 500000.0;
        long lastTick = 0;
        double currentTickTime = 0.0;

        foreach (var tempo in cachedTempoEvents)
        {
            double secondsPerTick = currentTempo / 1000000.0 / TPQN;
            double nextTickTime = currentTickTime + (tempo.tick - lastTick) * secondsPerTick;
            if (nextTickTime > timeSinceStart) break;
            currentTickTime = nextTickTime;
            lastTick = tempo.tick;
            currentTempo = tempo.tempo;
        }

        double remainingTime = timeSinceStart - currentTickTime;
        double secondsPerTickNow = currentTempo / 1000000.0 / TPQN;
        return lastTick + (long)(remainingTime / secondsPerTickNow);
    }

    IEnumerator LoadMidiFileAsync()
    {
        isReady = false;
        if (midiFilePlayer == null) yield break;
        MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
        if (midiLoad == null) yield break;

        TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
        BPM = (float)midiFilePlayer.MPTK_Tempo;
        CacheTempoEvents(midiLoad);
        GenerateNotes(midiLoad);

        ScoreManager.Instance?.SetTotalNotes(noteControllers.Count);
        double audioStartTime = AudioSettings.dspTime;
        float chartDelayOffset = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
        startTime = audioStartTime + chartDelayOffset;
        isReady = false;
    }

    void GenerateNotes(MidiLoad midiLoad)
    {
        Debug.Log("📜 ノート生成開始（ロングノート対応）");
        double eighthNoteThreshold = 60.0 / BPM / 2.0; // 8分音符より長ければロングノート

        double TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
        int globalIndex = 0;
        Dictionary<int, Queue<MPTKEvent>> ongoingNotes = new();

        foreach (var ev in midiLoad.MPTK_MidiEvents)
        {
            if (ev.Command == MPTKCommand.NoteOn && ev.Value > 0)
            {
                if (!ongoingNotes.ContainsKey(ev.Value))
                    ongoingNotes[ev.Value] = new Queue<MPTKEvent>();
                ongoingNotes[ev.Value].Enqueue(ev);
            }
            else if (ev.Command == MPTKCommand.NoteOff || (ev.Command == MPTKCommand.NoteOn && ev.Value == 0))
            {
                if (!ongoingNotes.ContainsKey(ev.Value) || ongoingNotes[ev.Value].Count == 0) continue;
                MPTKEvent startEvent = ongoingNotes[ev.Value].Dequeue();

                long startTick = startEvent.Tick;
                long endTick = ev.Tick;
                double startTime = TickToSeconds(startTick, TPQN);
                double endTime = TickToSeconds(endTick, TPQN);
                double duration = endTime - startTime;

                double travelTime = 2.0;
                double spawnTime = startTime - travelTime;
                double startZ = -noteSpeed * travelTime;
                float startX = GetFixedXPosition(ev.Value);

                GameObject note = Instantiate(LongNotePrefab, new Vector3(startX, spawnPoint.position.y, (float)startZ), Quaternion.identity);
                note.SetActive(true);

                var controller = note.GetComponent<NoteController>();
                if (controller != null)
                {
                    string uniqueID = globalIndex.ToString();
                    globalIndex++;

                    controller.tick = startTick;
                    controller.endTick = endTick;
                    controller.noteValue = ev.Value;
                    controller.Initialize(startTime, endTime, this, uniqueID, duration >= eighthNoteThreshold);

                    noteControllers.Add(controller);
                    OnNoteGenerated?.Invoke(controller);
                }
            }
        }

        Debug.Log("✅ ノート生成完了！");
        Debug.Log($"🧮 生成されたノーツ数: {noteControllers.Count}");
    }

    public float GetFixedXPosition(int noteValue)
    {
        return noteValue switch
        {
            60 => -2.5f,
            61 => -1.5f,
            62 => -0.5f,
            63 => 0.5f,
            64 => 1.5f,
            65 => 2.5f,
            _ => 0f
        };
    }

    public List<NoteController> GetNoteControllers() => noteControllers;

    public void StartPlayback()
    {
        if (isReady) return;
        isReady = true;
        OnChartPlaybackStart?.Invoke();
    }

    public void SetStartTime(double time)
    {
        startTime = time;
    }

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

    double TickToSeconds(long tick, double tpqn)
    {
        double time = 0.0;
        double currentTempo = 500000;
        long lastTick = 0;

        foreach (var tempo in cachedTempoEvents)
        {
            if (tempo.tick >= tick) break;
            double delta = (tempo.tick - lastTick) * (currentTempo / 1000000.0 / tpqn);
            time += delta;
            lastTick = tempo.tick;
            currentTempo = tempo.tempo;
        }

        time += (tick - lastTick) * (currentTempo / 1000000.0 / tpqn);
        return time;
    }
}
