using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    readonly string basePath = EnvManager.Instance.CloudfrontUrl;

    public AudioClip clip;
    public Sprite img;

    public IEnumerator IEParseSheet(string title)
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{basePath}/Sheet/{title}/{title}.sheet"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string contents = www.downloadHandler.text;
                SheetLoader.Instance.sheetContent = contents;

                GameManager.Instance.sheet = ParseSheet(contents);
            }
        }
        yield return IEGetClip(title);
        yield return IEGetImg(title);

        GameManager.Instance.sheet.clip = clip;
        GameManager.Instance.sheet.img = img;
    }

    public Sheet ParseSheet(string sheetStr)
    {
        Sheet newSheet = new();

        string[] rows = sheetStr.Split("\n");
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
                    newSheet.title = row.Split(':')[1].Trim();
                else if (row.StartsWith("Artist"))
                    newSheet.artist = row.Split(':')[1].Trim();
            }
            else if (currentStep == Step.Audio)
            {
                if (row.StartsWith("BPM"))
                    newSheet.bpm = int.Parse(row.Split(':')[1].Trim());
                else if (row.StartsWith("Signature"))
                {
                    string[] s = row.Split(':');
                    s = s[1].Split('/');
                    int[] sign = { int.Parse(s[0].Trim()), int.Parse(s[1].Trim()) };
                    newSheet.signature = sign;
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
                newSheet.notes.Add(new Note(time, type, line, tail));
            }
        }
        return newSheet;
    }

    public string StringifySheet(Sheet sheet)
    {
        List<Note> notes = new List<Note>();
        string noteStr = string.Empty;
        float baseTime = sheet.BarPerSec / 16;
        foreach (NoteObject note in NoteGenerator.Instance.toReleaseList)
        {
            if (!note.gameObject.activeSelf) // 비활성화되어있다면 삭제된 노트이므로 무시
                continue;

            float line = note.transform.position.x;
            int findLine = 0;
            if (line < -1f && line > -2f)
            {
                findLine = 0;
            }
            else if (line < 0f && line > -1f)
            {
                findLine = 1;
            }
            else if (line < 1f && line > 0f)
            {
                findLine = 2;
            }
            else if (line < 2f && line > 1f)
            {
                findLine = 3;
            }

            if (note is NoteShort)
            {
                NoteShort noteShort = note as NoteShort;
                int noteTime = (int)(noteShort.transform.localPosition.y * baseTime * 1000);

                notes.Add(new Note(noteTime, (int)NoteType.Short, findLine + 1, -1));
                //noteStr += $"{noteTime}, {(int)NoteType.Short}, {findLine + 1}\n";
            }
            else if (note is NoteLong)
            {
                NoteLong noteLong = note as NoteLong;
                int headTime = (int)(noteLong.transform.localPosition.y * baseTime * 1000);
                int tailTime = (int)((noteLong.transform.localPosition.y + noteLong.tail.transform.localPosition.y) * baseTime * 1000);

                notes.Add(new Note(headTime, (int)NoteType.Long, findLine + 1, tailTime));
                //noteStr += $"{headTime}, {(int)NoteType.Long}, {findLine + 1}, {tailTime}\n";
            }
        }

        notes = notes.OrderBy(a => a.time).ToList();

        foreach (Note n in notes)
        {
            switch (n.type)
            {
                case (int)NoteType.Short:
                    noteStr += $"{n.time}, {n.type}, {n.line}\n";
                    break;
                case (int)NoteType.Long:
                    noteStr += $"{n.time}, {n.type}, {n.line}, {n.tail}\n";
                    break;
            }
        }

        string writer = $"[Description]\n" +
            $"Title: {sheet.title}\n" +
            $"Artist: {sheet.artist}\n\n" +
            $"[Audio]\n" +
            $"BPM: {sheet.bpm}\n" +
            $"Signature: {sheet.signature[0]}/{sheet.signature[1]}\n\n" +
            $"[Note]\n" +
            $"{noteStr}";

        return writer.TrimEnd('\r', '\n');
    }


    public IEnumerator IEGetClip(string title)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"{basePath}/Sheet/{title}/{title}.mp3", AudioType.MPEG))
        {
            yield return request.SendWebRequest();
            clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = title;
        }
    }

    public IEnumerator IEGetImg(string title)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture($"{basePath}/Sheet/{title}/{title}.jpg"))
        {
            yield return request.SendWebRequest();
            Texture2D t = DownloadHandlerTexture.GetContent(request);
            img = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
            img.name = title;
        }
    }
}
