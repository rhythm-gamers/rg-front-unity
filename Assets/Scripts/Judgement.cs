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

    readonly int miss = 300;
    readonly int good = 200;
    readonly int great = 150;
    readonly int rhythm = 50; // perfect

    List<Queue<Note>> notes = new List<Queue<Note>>();
    Queue<Note> note1 = new Queue<Note>();
    Queue<Note> note2 = new Queue<Note>();
    Queue<Note> note3 = new Queue<Note>();
    Queue<Note> note4 = new Queue<Note>();

    int[] longNoteCheck = new int[4] { 0, 0, 0, 0 };

    /// <summary>
    /// User에 의해 조정된 판정 타이밍
    /// </summary>

    private int currentTime = 0;
    private Coroutine coCheckMiss;

    private readonly object[] dequeuingLock = new object[] { new(), new(), new(), new() };

    bool IsMiss(float time) => time <= miss && time >= -miss;
    bool IsOverGood(float time) => time <= good && time >= -good;

    void Awake()
    {
        if (instance == null)
            instance = this;
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
            else
                note4.Enqueue(note);
        }
        notes.Add(note1);
        notes.Add(note2);
        notes.Add(note3);
        notes.Add(note4);
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
        if (notes[line].Count <= 0 || !AudioManager.Instance.IsPlaying()) yield break;

        int savedCurrentTime = (int)AudioManager.Instance.GetMilliSec();

        lock (dequeuingLock[line])
        {
            Note note = notes[line].Peek();
            float judgeTime = savedCurrentTime - note.time;

            if (IsMiss(judgeTime))
            {
                if (IsOverGood(judgeTime))
                {
                    HandleOverGoodOnShort(judgeTime);
                }
                else
                {
                    HandleFastOrSlowMiss(judgeTime);
                }
                Score.Instance.UpdateScore();
                JudgeEffect.Instance.OnEffect(line);
                HandleByNoteType(note.type, line);
            }
        }
    }

    public IEnumerator CheckLongNote(int line)
    {
        if (GameManager.Instance.state == GameManager.GameState.Edit) yield break;
        if (notes[line].Count <= 0) yield break;
        if (longNoteCheck[line] == 0) yield break;

        int savedCurrentTime = (int)AudioManager.Instance.GetMilliSec();
        lock (dequeuingLock[line])
        {
            Note note = notes[line].Peek();
            float judgeTime = savedCurrentTime - note.tail;

            bool IsOnLongNote = (savedCurrentTime >= note.time - miss) && (savedCurrentTime <= note.tail + miss);
            if (IsOnLongNote)
            {
                if (IsOverGood(judgeTime))
                {
                    HandleOverGoodOnLong(judgeTime);
                }
                else
                {
                    HandleFastOrSlowMiss(judgeTime);
                }
                Score.Instance.UpdateScore();
                longNoteCheck[line] = 0;
                notes[line].Dequeue();
            }
        }
    }

    private void HandleOverGoodOnShort(float time)
    {
        if (time <= rhythm && time >= -rhythm)
        {
            Score.Instance.data.rhythm++;
            Score.Instance.data.judge = JudgeType.Rhythm;
        }
        else if (time <= great && time >= -great)
        {
            Score.Instance.data.great++;
            Score.Instance.data.judge = JudgeType.Great;
        }
        else if (time <= good && time >= -good)
        {
            Score.Instance.data.good++;
            Score.Instance.data.judge = JudgeType.Good;
        }
        Score.Instance.data.combo++;
    }

    private void HandleOverGoodOnLong(float time)
    {
        if (time <= great && time >= -great)
        {
            Score.Instance.data.rhythm++;
            Score.Instance.data.judge = JudgeType.Rhythm;
        }
        else if (time <= good && time >= -good)
        {
            Score.Instance.data.great++;
            Score.Instance.data.judge = JudgeType.Great;
        }
        Score.Instance.data.combo++;
    }

    private void HandleFastOrSlowMiss(float time)
    {
        if (time > 0)
            Score.Instance.data.slowMiss++;
        else
            Score.Instance.data.fastMiss++;

        HandleMiss();
    }

    private void HandleMiss()
    {
        Score.Instance.data.miss++;
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
                                Score.Instance.data.slowMiss += 2;
                                Score.Instance.data.miss += 2;
                                Score.Instance.data.judge = JudgeType.Miss;
                                Score.Instance.data.combo = 0;

                                Score.Instance.UpdateScore();
                                notes[i].Dequeue();
                            }
                        }
                        else // Head 판정처리가 된 경우 (롱노트 계속 누르고 있었던 경우)
                        {
                            if (lastJudgeTime < -miss)
                            {
                                Score.Instance.data.slowMiss++;
                                HandleMiss();

                                Score.Instance.UpdateScore();
                                notes[i].Dequeue();
                            }
                        }
                    }

                    else if (note.type == (int)NoteType.Short)
                    {
                        if (judgeTime < -miss)
                        {
                            Score.Instance.data.slowMiss++;
                            HandleMiss();

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
