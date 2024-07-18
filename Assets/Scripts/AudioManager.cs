using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            return instance;
        }
    }

    public AudioSource audioSource;

    public float Length
    {
        get
        {
            float len = 0f;
            if (audioSource.clip != null)
                len = audioSource.clip.length;
            return len;
        }
    }
    public float progressTime
    {
        get
        {
            float time = 0f;
            if (audioSource.clip != null)
                time = audioSource.time;
            return time;
        }
        set
        {
            if (audioSource.clip != null)
                audioSource.time = value;
        }
    }

    public enum State
    {
        Playing,
        Paused,
        Unpaused,
        Stop,
    }
    public State state = State.Stop;

    public float savedAudioTimeForWebGL { get; set; }

    void Awake()
    {
        if (instance == null)
            instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.Game)
            CheckIsFinished();
    }

    private void CheckIsFinished()
    {
        if (!GameManager.Instance.isPlaying) return;
        if (Length - progressTime <= 0.1f)
            Stop();
    }

    public IEnumerator InitForEdit()
    {
        // 한번 재생해야 곡 시간을 자유롭게 변경 가능
        Play();
        yield return new WaitUntil(() => IsPlaying());
        progressTime = 0f;
        Pause();
    }

    public void Play()
    {
        state = State.Playing;
        audioSource.Play();
    }

    public void Pause()
    {
        state = State.Paused;
        savedAudioTimeForWebGL = progressTime;
        audioSource.Pause();
    }

    public void UnPause()
    {
        if (audioSource.clip == null)
        {
            Debug.LogError("Audio clip is null. Cannot unpause.");
            return;
        }

        state = State.Unpaused;

        progressTime = savedAudioTimeForWebGL;
        audioSource.UnPause();
    }

    public void Stop()
    {
        state = State.Stop;
        audioSource.Stop();
    }

    public void MovePositionByAudioState(float time)
    {
        float currentTime = audioSource.time;
        void MovePosition(float time)
        {
            if (currentTime + time < 0)
                progressTime = 0f;
            else
            {
                progressTime += time;
                savedAudioTimeForWebGL += time;
            }
        }

        if (IsPlaying())
        {
            MovePosition(time);
            Pause();
            UnPause();
        }
        else
        {
            MovePosition(time);
            UnPause();
            Pause();
        }
    }

    public void Insert(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    public float GetMilliSec()
    {
        return audioSource.time * 1000;
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}
