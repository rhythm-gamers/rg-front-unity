using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class NoteGenerator : MonoBehaviour
{
    static NoteGenerator instance;
    public static NoteGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject parent;
    public GameObject notePrefab;
    public Material lineRendererMaterial;

    public float[] linePos = { -1.5f, -0.5f, 0.5f, 1.5f }; // 4키 기준
    readonly float defaultInterval = 0.005f; // 1배속 기준점 (1마디 전체가 화면에 그려지는 정도를 정의)
    public float Interval { get; private set; }


    IObjectPool<NoteShort> poolShort;
    public IObjectPool<NoteShort> PoolShort
    {
        get
        {
            if (poolShort == null)
            {
                poolShort = new ObjectPool<NoteShort>(CreatePooledShort, defaultCapacity: 256);
            }
            return poolShort;
        }
    }
    NoteShort CreatePooledShort()
    {
        GameObject note = Instantiate(notePrefab, parent.transform);
        return note.AddComponent<NoteShort>();
    }

    IObjectPool<NoteLong> poolLong;
    public IObjectPool<NoteLong> PoolLong
    {
        get
        {
            if (poolLong == null)
            {
                poolLong = new ObjectPool<NoteLong>(CreatePooledLong, defaultCapacity: 64);
            }
            return poolLong;
        }
    }
    NoteLong CreatePooledLong()
    {
        GameObject note = new GameObject("NoteLong");
        note.transform.parent = parent.transform;

        GameObject head = Instantiate(notePrefab);
        head.name = "head";
        head.transform.parent = note.transform;

        GameObject tail = Instantiate(notePrefab);
        tail.transform.parent = note.transform;
        tail.name = "tail";

        GameObject line = new GameObject("line");
        line.transform.parent = note.transform;

        line.AddComponent<LineRenderer>();
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.material = lineRendererMaterial;
        lineRenderer.sortingOrder = 3;
        lineRenderer.widthMultiplier = 0.8f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;

        return note.AddComponent<NoteLong>();
    }

    int currentBar = 3; // 최초 플레이 시 3마디 먼저 생성
    int next = 0;
    int prev = 0;
    public List<NoteObject> toReleaseList = new List<NoteObject>();

    Coroutine coGenTimer;
    Coroutine coReleaseTimer;
    Coroutine coInterpolate;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // 풀링 기반 생성 (처음 게임 플레이 및 재시작 시에 사용)
    public void StartGen()
    {
        // 노트 생성 중지
        StopGen();

        Interval = defaultInterval * GameManager.Instance.Speed;
        coGenTimer = StartCoroutine(IEGenTimer(GameManager.Instance.sheet.BarPerMilliSec * 0.001f)); // 음악의 1마디 시간마다 생성할 노트 오브젝트 탐색
        coReleaseTimer = StartCoroutine(IEReleaseTimer(GameManager.Instance.sheet.BarPerMilliSec * 0.001f * 0.5f)); // 1마디 시간의 절반 주기로 해제할 노트 오브젝트 탐색
        coInterpolate = StartCoroutine(IEInterpolate(GameManager.Instance.sheet.BeatPerSec, 4f));
    }

    // 노트 생성 중지 및 노트 Release
    public void StopGen()
    {
        if (coGenTimer != null)
        {
            StopCoroutine(coGenTimer);
            coGenTimer = null;
        }
        if (coReleaseTimer != null)
        {
            StopCoroutine(coReleaseTimer);
            coReleaseTimer = null;
        }
        if (coInterpolate != null)
        {
            StopCoroutine(coInterpolate);
            coInterpolate = null;
        }

        ReleaseCompleted();
    }

    void Gen()
    {
        List<Note> notes = GameManager.Instance.sheet.notes;
        List<Note> reconNotes = new List<Note>();

        for (; next < notes.Count; next++)
        {
            if (notes[next].time > currentBar * GameManager.Instance.sheet.BarPerMilliSec)
                break;
        }

        for (int j = prev; j < next; j++)
        {
            reconNotes.Add(notes[j]);
        }
        prev = next;

        float currentTime = AudioManager.Instance.GetMilliSec();
        float noteSpeed = Interval * 1000;
        foreach (Note note in reconNotes)
        {
            NoteObject noteObject = null;

            switch (note.type)
            {
                case (int)NoteType.Short:
                    noteObject = PoolShort.Get();
                    noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], (note.time - currentTime) * Interval, 0f) });
                    break;
                case (int)NoteType.Long:
                    noteObject = PoolLong.Get();
                    noteObject.SetPosition(new Vector3[] // 포지션은 노트 시간 - 현재 음악 시간
                    {
                        new Vector3(linePos[note.line - 1], (note.time - currentTime) * Interval, 0f),
                        new Vector3(linePos[note.line - 1], (note.tail - currentTime) * Interval, 0f)
                    });
                    break;
                default:
                    break;
            }
            noteObject.speed = noteSpeed;
            noteObject.note = note;
            noteObject.life = true;
            noteObject.gameObject.SetActive(true);
            noteObject.SetCollider();
            noteObject.Move();
            toReleaseList.Add(noteObject);
        }
    }



    public void DisposeNoteShort(Vector3 pos)
    {
        NoteObject noteObject = PoolShort.Get();
        noteObject.SetPosition(new Vector3[] { pos });
        noteObject.gameObject.SetActive(true);
        noteObject.SetCollider();
        toReleaseList.Add(noteObject);
    }

    NoteObject noteObjectTemp;
    public void DisposeNoteLong(int makingCount, Vector3[] pos)
    {
        if (makingCount == 0)
        {
            noteObjectTemp = PoolLong.Get();
            noteObjectTemp.SetPosition(new Vector3[] { pos[0], pos[1] });
            noteObjectTemp.gameObject.SetActive(true);
        }
        else if (makingCount == 1)
        {
            noteObjectTemp.SetPosition(new Vector3[] { pos[0], pos[1] });
            noteObjectTemp.SetCollider();
            toReleaseList.Add(noteObjectTemp);
        }
    }

    void ReleaseCompleted()
    {
        foreach (NoteObject note in toReleaseList)
        {
            note.gameObject.SetActive(false);

            if (note is NoteShort)
                PoolShort.Release(note as NoteShort);
            else
                PoolLong.Release(note as NoteLong);
        }

        // 현재 위치 초기화
        currentBar = 3;
        prev = 0;
        next = 0;
#if !UNITY_WEBGL
        Editor.Instance.objects.transform.position = Vector3.zero;
#endif
        // ReleaseList 초기화
        toReleaseList.Clear();
    }

    void Release()
    {
        List<NoteObject> reconNotes = new List<NoteObject>();
        foreach (NoteObject note in toReleaseList)
        {
            if (!note.life)
            {
                if (note is NoteShort)
                    PoolShort.Release(note as NoteShort);
                else
                    PoolLong.Release(note as NoteLong);

                note.gameObject.SetActive(false);
            }
            else
            {
                reconNotes.Add(note);
            }
        }
        toReleaseList.Clear();
        toReleaseList.AddRange(reconNotes);
    }

    public void Interpolate()
    {
        if (coInterpolate != null)
            StopCoroutine(coInterpolate);

        coInterpolate = StartCoroutine(IEInterpolate());
    }

    IEnumerator IEGenTimer(float interval)
    {
        while (true)
        {
            Gen();
            yield return new WaitForSeconds(interval);
            currentBar++;
        }
    }

    IEnumerator IEReleaseTimer(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            Release();
        }
    }

    IEnumerator IEInterpolate(float rate = 1f, float duration = 1f)
    {
        if (toReleaseList.Count == 0) yield break;

        float time = 0;
        Interval = defaultInterval * GameManager.Instance.Speed;

        float noteSpeed = Interval * 1000;
        while (time < duration)
        {
            float milli = AudioManager.Instance.GetMilliSec();

            foreach (NoteObject note in toReleaseList)
            {
                note.speed = noteSpeed;
                note.Interpolate(milli, Interval);
            }

            time += rate;
            yield return new WaitForSeconds(rate);
        }
    }

#if !UNITY_WEBGL
    // 한 번에 다 생성 (에디팅할때 사용)
    public void GenAll()
    {
        Gen2();
    }

    /// <summary>
    /// Editor Gen메소드, 노트의 이동은 노트 자신이 처리하지 않음.
    /// </summary>
    void Gen2()
    {
        Sheet sheet = GameManager.Instance.sheet;

        List<Note> notes = sheet.notes;

        float shortPrevPos = 0;
        int shortPrevTime = 0;

        float headLongPrevPos = 0;
        int headLongPrevTime = 0;

        float tailLongPrevPos = 0;
        int tailLongPrevTime = 0;

        foreach (Note note in notes)
        {
            NoteObject noteObject = null;
            switch (note.type)
            {
                case (int)NoteType.Short:
                    {
                        noteObject = PoolShort.Get();
                        float pos = Utils.Instance.MilliSecToBar(note.time - shortPrevTime);
                        shortPrevPos += pos;

                        noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], shortPrevPos, -1f) });
                        shortPrevTime = note.time;
                        break;
                    }

                case (int)NoteType.Long:
                    {
                        noteObject = PoolLong.Get();
                        if (headLongPrevTime == 0)
                        {
                            float pos = Utils.Instance.MilliSecToBar(note.time - headLongPrevTime);
                            headLongPrevPos += pos;

                            float pos2 = Utils.Instance.MilliSecToBar(note.tail - tailLongPrevTime);
                            tailLongPrevPos += pos2;

                            noteObject.SetPosition(new Vector3[]
                            {
                                new Vector3(linePos[note.line - 1], headLongPrevPos, -1f),
                                new Vector3(linePos[note.line - 1], tailLongPrevPos, -1f)
                            });
                        }
                        else
                        {
                            float pos = Utils.Instance.MilliSecToBar(note.time - headLongPrevTime);
                            headLongPrevPos += pos;

                            float pos2 = Utils.Instance.MilliSecToBar(note.tail - tailLongPrevTime);
                            tailLongPrevPos += pos2;

                            noteObject.SetPosition(new Vector3[]
                            {
                            new Vector3(linePos[note.line - 1], headLongPrevPos, -1f),
                            new Vector3(linePos[note.line - 1], tailLongPrevPos, -1f)
                            });
                        }
                        headLongPrevTime = note.time;
                        tailLongPrevTime = note.tail;

                        break;
                    }
                default:
                    break;
            }
            noteObject.note = note;
            noteObject.life = true;
            noteObject.gameObject.SetActive(true);
            noteObject.SetCollider();
            //noteObject.Move();
            toReleaseList.Add(noteObject); // 에디팅끝나면 Release호출해서 해제해주기
        }
    }
#endif
}
