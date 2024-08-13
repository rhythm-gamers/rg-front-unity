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

    private float SavedAudioTimeForWebGL { get; set; }

    void Awake()
    {
        if (instance == null)
            instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        CheckIsFinished();
    }

    private void CheckIsFinished()
    {
        if (audioSource.clip == null) return;
        if (!GameManager.Instance.isPlaying) return;

        if (Length - progressTime <= 0.1f)
        {
            if (GameManager.Instance.state == GameManager.GameState.Game)
            {
                Stop();
            }

#if !UNITY_WEBGL
            else if (GameManager.Instance.state == GameManager.GameState.Edit)
            {
                Editor.Instance.Stop();
            }
#endif
        }
    }

    public void InitForEdit()
    {
        // 한번 재생해야 곡 시간을 자유롭게 변경 가능
        Play();
        Pause();
        progressTime = 0f;
    }

    public void Play()
    {
        if (audioSource.clip == null) return;

        state = State.Playing;
        audioSource.Play();
    }

    public void Pause()
    {
        if (audioSource.clip == null) return;
#if UNITY_WEBGL
        SavedAudioTimeForWebGL = progressTime;
#endif

        state = State.Paused;
        audioSource.Pause();
    }

    public void UnPause()
    {
        if (audioSource.clip == null) return;
#if UNITY_WEBGL
        progressTime = SavedAudioTimeForWebGL;
#endif

        state = State.Unpaused;
        audioSource.UnPause();
    }

    public void Stop()
    {
        if (audioSource.clip == null) return;

        state = State.Stop;
        audioSource.Stop();
    }

    public void Insert(AudioClip clip)
    {
        audioSource.clip = clip;
        Stop();
    }

    public float GetMilliSec()
    {
        return (audioSource.time + Judgement.Instance.WebGLAudioLatency) * 1000f;
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }


#if !UNITY_WEBGL
    public IEnumerator MovePosition(float time)
    {
        double currentTime = audioSource.time;

        if (currentTime + time < 0)
            progressTime = 0f;
        else
            progressTime = (float)(currentTime + time);

        yield return null;
        Editor.Instance.CalibratePosition();
    }
#endif
}
