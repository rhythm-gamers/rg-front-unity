using System;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType
{
    Short = 0,
    Long = 1,
}

public struct Note
{
    public int time;
    public int type;
    public int line;
    public int tail;

    public Note(int time, int type, int line, int tail)
    {
        this.time = time;
        this.type = type;
        this.line = line;
        this.tail = tail;
    }
}

public class Sheet
{
    // [Description]
    public string title;
    public string artist;

    // [Audio]
    public int bpm;
    public int[] signature;
    public int offset;

    // [Note]
    public List<Note> notes = new List<Note>();


    public AudioClip clip;
    public Sprite img;

    public float BarPerSec { get; private set; } // 1Bar에 몇 초 걸리는지
    public float BeatPerSec { get; private set; }

    public int BarPerMilliSec { get; private set; }
    public int BeatPerMilliSec { get; private set; }

    public void Init()
    {
        BarPerMilliSec = Mathf.RoundToInt(signature[0] / (bpm / 60f) * 1000);
        BeatPerMilliSec = Mathf.RoundToInt(BarPerMilliSec / 192f);

        BarPerSec = BarPerMilliSec * 0.001f;
        BeatPerSec = BarPerSec / 192f;
    }
}