using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;

public class Parser
{
    static Parser instance;
    public static Parser Instance
    {
        get
        {
            if (instance == null)
                instance = new Parser();
            return instance;
        }
    }

    enum Step
    {
        Description,
        Audio,
        Note,
    }
    Step currentStep = Step.Description;

    public Sheet sheet;
    string basePath = "http://127.0.0.1:3000/Sheet";

    public AudioClip clip;
    public Sprite img;

    public IEnumerator IEParse(string title)
    {
        sheet = new Sheet();
        string contents = string.Empty;

        using (UnityWebRequest www = UnityWebRequest.Get($"{basePath}/{title}/{title}.sheet"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                contents = www.downloadHandler.text;
                string[] rows = contents.Split("\n");

                foreach (string row in rows)
                {
                    if (row.StartsWith("[Description]"))
                    {
                        currentStep = Step.Description;
                        continue;
                    }
                    else if (row.StartsWith("[Audio]"))
                    {
                        currentStep = Step.Audio;
                        continue;
                    }
                    else if (row.StartsWith("[Note]"))
                    {
                        currentStep = Step.Note;
                        continue;
                    }

                    if (currentStep == Step.Description)
                    {
                        if (row.StartsWith("Title"))
                            sheet.title = row.Split(':')[1].Trim();
                        else if (row.StartsWith("Artist"))
                            sheet.artist = row.Split(':')[1].Trim();
                    }
                    else if (currentStep == Step.Audio)
                    {
                        if (row.StartsWith("BPM"))
                            sheet.bpm = int.Parse(row.Split(':')[1].Trim());
                        else if (row.StartsWith("Offset"))
                            sheet.offset = int.Parse(row.Split(':')[1].Trim());
                        else if (row.StartsWith("Signature"))
                        {
                            string[] s = row.Split(':');
                            s = s[1].Split('/');
                            int[] sign = { int.Parse(s[0].Trim()), int.Parse(s[1].Trim()) };
                            sheet.signature = sign;
                        }
                    }
                    else if (currentStep == Step.Note)
                    {
                        if (string.IsNullOrEmpty(row))
                            break;

                        string[] s = row.Split(',');
                        int time = int.Parse(s[0].Trim());
                        int type = int.Parse(s[1].Trim());
                        int line = int.Parse(s[2].Trim());
                        int tail = -1;
                        if (s.Length > 3)
                            tail = int.Parse(row.Split(',')[3].Trim());
                        sheet.notes.Add(new Note(time, type, line, tail));
                    }
                }
            }
        }
        yield return IEGetClip(title);
        yield return IEGetImg(title);

        sheet.clip = clip;
        sheet.img = img;

    }




    public IEnumerator IEGetClip(string title)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"{basePath}/{title}/{title}.mp3", AudioType.MPEG))
        {
            yield return request.SendWebRequest();
            clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = title;
        }
    }

    public IEnumerator IEGetImg(string title)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture($"{basePath}/{title}/{title}.jpg"))
        {
            yield return request.SendWebRequest();
            Texture2D t = DownloadHandlerTexture.GetContent(request);
            img = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
            img.name = title;
        }
    }
}
