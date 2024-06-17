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

    private float savedAudioTimeForPause { get; set; }

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
        if (!GameManager.Instance.isPlaying) return;
        if (Length - progressTime <= 0.1f)
            Stop();
    }

    public void Play()
    {
        state = State.Playing;
        audioSource.Play();
    }

    public void Pause()
    {
        state = State.Paused;
        savedAudioTimeForPause = progressTime;
        audioSource.Pause();
    }

    public void UnPause()
    {
        state = State.Unpaused;

        if (audioSource.clip != null)
        {
            progressTime = savedAudioTimeForPause;
            audioSource.UnPause();
        }
        else
        {
            Debug.LogError("Audio clip is null. Cannot unpause.");
        }
    }

    public void Stop()
    {
        state = State.Stop;
        audioSource.Stop();
    }

    public void MovePosition(float time)
    {
        float currentTime = audioSource.time;

        currentTime += time;
        currentTime = Mathf.Clamp(currentTime, 0f, audioSource.clip.length - 0.0001f);

        audioSource.time = currentTime;
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
