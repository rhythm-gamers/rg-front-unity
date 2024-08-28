using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JudgeType
{
    Rhythm,
    Great,
    Good,
    Miss
}

public class Judgement : MonoBehaviour
{
    static Judgement instance;
    public static Judgement Instance
    {
        get
        {
            return instance;
        }
    }

    public float StandardDeviation
    {
        get
        {
            return Utils.Instance.CalculateStandardDeviationFromZero(judgeTimes);
        }
    }

    private List<int> judgeTimes = new();
    private List<int> inputTimes = new();
    public int GetJudgedNoteLength()
    {
        return judgeTimes.Count;
    }
    public int GetJudgeTimeAt(int idx)
    {
        return judgeTimes[idx];
    }
    public int GetInputTimeAt(int idx)
    {
        return inputTimes[idx];
    }


    readonly int miss = 300;
    readonly int good = 200;
    readonly int great = 150;
    readonly int rhythm = 50; // perfect

    List<Queue<Note>> notes = new List<Queue<Note>>();
    Queue<Note> note1 = new Queue<Note>();
    Queue<Note> note2 = new Queue<Note>();
    Queue<Note> note3 = new Queue<Note>();
    Queue<Note> note4 = new Queue<Note>();
    Queue<Note> note5 = new Queue<Note>();
    Queue<Note> note6 = new Queue<Note>();

    int[] longNoteCheck = new int[6] { 0, 0, 0, 0, 0, 0 };

    private int currentTime = 0;
    private Coroutine coCheckMiss;

    private readonly object[] dequeuingLock = new object[] { new(), new(), new(), new(), new(), new() };

    /// <summary>
    /// WebGL에서 오디오가 재생될 때 발생하는 레이턴시를 임의로 보완
    /// 현재, WebGL에서 생성한 AudioContext에 접근할 수 있는 인터페이스가 없기 때문에 이게 최선으로 보임
    /// 이후에 더 나은 접근 방법이 있다면 수정
    /// </summary>
    public float WebGLAudioLatency = 0f;

    bool IsMiss(float time) => time <= miss && time >= -miss;
    bool IsOverGood(float time) => time <= good && time >= -good;

    void Awake()
    {
        if (instance == null)
            instance = this;

#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLAudioLatency = 0.1f;
#endif
    }

    public void Init()
    {
        foreach (var note in notes)
        {
            note.Clear();
        }
        notes.Clear();

        foreach (var note in GameManager.Instance.sheet.notes)
        {
            if (note.line == 1)
                note1.Enqueue(note);
            else if (note.line == 2)
                note2.Enqueue(note);
            else if (note.line == 3)
                note3.Enqueue(note);
            else if (note.line == 4)
                note4.Enqueue(note);
            else if (note.line == 5)
                note5.Enqueue(note);
            else
                note6.Enqueue(note);
        }
        notes.Add(note1);
        notes.Add(note2);
        notes.Add(note3);
        notes.Add(note4);
        notes.Add(note5);
        notes.Add(note6);

        judgeTimes = new();
        inputTimes = new();
    }

    public void StartMissCheck()
    {
        coCheckMiss = StartCoroutine(IECheckMiss());
    }

    public void StopMissCheck()
    {
        if (coCheckMiss != null)
        {
            StopCoroutine(coCheckMiss);
        }
    }

    public IEnumerator JudgeNote(int line)
    {
        if (GameManager.Instance.state == GameManager.GameState.Edit) yield break;
        if (!GameManager.Instance.isPlaying || notes[line].Count == 0) yield break;

        int savedCurrentTime = (int)AudioManager.Instance.GetMilliSec();
        int judgeOffsetFromUser = Sync.Instance.judgeOffsetFromUser;

        lock (dequeuingLock[line])
        {
            Note note = notes[line].Peek();
            int judgeTime = savedCurrentTime - note.time + judgeOffsetFromUser;

            if (IsMiss(judgeTime))
            {
                if (IsOverGood(judgeTime))
                {
                    HandleOverGoodOnShort(judgeTime, note.type);
                }
                else
                {
                    HandleFastOrSlowMiss(judgeTime, note.type);
                }
                judgeTimes.Add(judgeTime);
                inputTimes.Add(note.time);
                Score.Instance.UpdateScore();
                JudgeEffect.Instance.OnEffect(line);
                HandleByNoteType(note.type, line);
            }
        }
    }

    public IEnumerator CheckLongNote(int line)
    {
        if (GameManager.Instance.state == GameManager.GameState.Edit) yield break;
        if (longNoteCheck[line] == 0) yield break;
        if (notes[line].Count == 0) yield break;

        int judgeOffsetFromUser = Sync.Instance.judgeOffsetFromUser;
        int savedCurrentTime = (int)AudioManager.Instance.GetMilliSec();

        lock (dequeuingLock[line])
        {
            Note note = notes[line].Peek();
            int judgeTime = savedCurrentTime - note.tail + judgeOffsetFromUser;

            bool IsOnLongNote = (savedCurrentTime >= note.time - miss + judgeOffsetFromUser) && (savedCurrentTime <= note.tail + miss + judgeOffsetFromUser);
            if (IsOnLongNote)
            {
                if (IsOverGood(judgeTime))
                {
                    HandleOverGoodOnLong(judgeTime);
                }
                else
                {
                    HandleFastOrSlowMiss(judgeTime, note.type);
                }
                judgeTimes.Add(judgeTime);
                inputTimes.Add(note.time);
                Score.Instance.UpdateScore();
                longNoteCheck[line] = 0;
                notes[line].Dequeue();
            }
        }
    }

    private void HandleOverGoodOnShort(float time, int noteType)
    {
        if (time <= rhythm && time >= -rhythm)
        {
            if (noteType == (int)NoteType.Short)
                Score.Instance.data.rhythm.Short++;
            else
                Score.Instance.data.rhythm.Long++;

            Score.Instance.data.judge = JudgeType.Rhythm;
        }
        else if (time <= great && time >= -great)
        {
            if (noteType == (int)NoteType.Short)
                Score.Instance.data.great.Short++;
            else
                Score.Instance.data.great.Long++;

            if (time < 0)
                Score.Instance.data.great.Fast++;
            else
                Score.Instance.data.great.Slow++;

            Score.Instance.data.judge = JudgeType.Great;
        }
        else if (time <= good && time >= -good)
        {
            if (noteType == (int)NoteType.Short)
                Score.Instance.data.good.Short++;
            else
                Score.Instance.data.good.Long++;

            if (time < 0)
                Score.Instance.data.good.Fast++;
            else
                Score.Instance.data.good.Slow++;

            Score.Instance.data.judge = JudgeType.Good;
        }
        Score.Instance.data.combo++;
    }

    private void HandleOverGoodOnLong(float time)
    {
        if (time <= great && time >= -great)
        {
            Score.Instance.data.rhythm.Long++;
            Score.Instance.data.judge = JudgeType.Rhythm;
        }
        else if (time <= good && time >= -good)
        {
            if (time < 0)
                Score.Instance.data.great.Fast++;
            else
                Score.Instance.data.great.Slow++;

            Score.Instance.data.great.Long++;
            Score.Instance.data.judge = JudgeType.Great;
        }
        Score.Instance.data.combo++;
    }

    private void HandleFastOrSlowMiss(float time, int noteType)
    {
        if (time < 0)
            Score.Instance.data.miss.Fast++;
        else
            Score.Instance.data.miss.Slow++;

        if (noteType == (int)NoteType.Short)
            Score.Instance.data.miss.Short++;
        else
            Score.Instance.data.miss.Long++;

        Score.Instance.data.judge = JudgeType.Miss;
        Score.Instance.data.combo = 0;
    }

    private void HandleByNoteType(int noteType, int line)
    {
        if (noteType == (int)NoteType.Short)
        {
            notes[line].Dequeue();
        }
        else if (noteType == (int)NoteType.Long)
        {
            longNoteCheck[line] = 1;
        }
    }

    IEnumerator IECheckMiss()
    {
        while (true)
        {
            currentTime = (int)AudioManager.Instance.GetMilliSec();
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].Count <= 0)
                    continue;

                int savedCurrentTime = currentTime;
                lock (dequeuingLock[i])
                {
                    Note note = notes[i].Peek();
                    float judgeTime = note.time - savedCurrentTime;
                    float lastJudgeTime = note.tail - savedCurrentTime;

                    if (note.type == (int)NoteType.Long)
                    {
                        if (longNoteCheck[i] == 0) // Head 판정처리가 안된 경우
                        {
                            if (judgeTime < -miss)
                            {
                                Score.Instance.data.miss.Slow += 2;
                                Score.Instance.data.miss.Long += 2;
                                Score.Instance.data.judge = JudgeType.Miss;
                                Score.Instance.data.combo = 0;
                                judgeTimes.Add(-miss);
                                judgeTimes.Add(-miss);
                                inputTimes.Add(note.time);
                                inputTimes.Add(note.time);

                                Score.Instance.UpdateScore();
                                notes[i].Dequeue();
                            }
                        }
                        else // Head 판정처리가 된 경우 (롱노트 계속 누르고 있었던 경우)
                        {
                            if (lastJudgeTime < -miss)
                            {
                                judgeTimes.Add(-miss);
                                inputTimes.Add(note.time);
                                HandleFastOrSlowMiss(-miss, note.type);

                                Score.Instance.UpdateScore();
                                notes[i].Dequeue();
                            }
                        }
                    }

                    else if (note.type == (int)NoteType.Short)
                    {
                        if (judgeTime < -miss)
                        {
                            judgeTimes.Add(-miss);
                            inputTimes.Add(note.time);
                            HandleFastOrSlowMiss(-miss, note.type);

                            Score.Instance.UpdateScore();
                            notes[i].Dequeue();
                        }
                    }
                }

                yield return null;
            }

            yield return null;
        }
    }
}
