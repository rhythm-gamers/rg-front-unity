using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Parser : MonoBehaviour
{
    static Parser instance;
    public static Parser Instance
    {
        get
        {
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

    private Sheet sheet;
    private AudioClip clip;
    private Sprite img;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public IEnumerator IEParseGameSheet(string path, string title)
    {
        yield return IEGetSheet(path, title);
        yield return IEGetClip(path, title);
        yield return IEGetImg(path, title);

        sheet.clip = clip;
        sheet.img = img;

        GameManager.Instance.sheet = sheet;
    }

    public Sheet ParseSheet(string sheetStr)
    {
        Sheet newSheet = new();

        string[] rows = sheetStr?.Split("\n");
        if (rows == null) return null;

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
                else if (row.StartsWith("Key"))
                    newSheet.keyNum = int.Parse(row.Split(':')[1].Trim());
            }
            else if (currentStep == Step.Audio)
            {
                if (row.StartsWith("BPM"))
                    newSheet.bpm = int.Parse(row.Split(':')[1].Trim());
                else if (row.StartsWith("Offset"))
                    newSheet.offset = int.Parse(row.Split(':')[1].Trim());
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

        newSheet.clip = clip;
        newSheet.img = img;

        return newSheet;
    }

    public string StringifyEditedSheet()
    {
        Sheet sheet = GameManager.Instance.sheet;

        List<Note> notes = new List<Note>();
        string noteStr = string.Empty;
        float baseTime = sheet.BarPerMilliSec / 16f;

        foreach (NoteObject note in NoteGenerator.Instance.toReleaseList)
        {
            if (!note.gameObject.activeSelf) // 비활성화되어있다면 삭제된 노트이므로 무시
                continue;

            float notePosX = note.transform.position.x;
            int findLine = 0;

            switch (sheet.keyNum)
            {
                case 4:
                    findLine = Find4KeyLine(notePosX);
                    break;
                case 5:
                    findLine = Find5KeyLine(notePosX);
                    break;
                case 6:
                    findLine = Find6KeyLine(notePosX);
                    break;
            }

            if (note is NoteShort)
            {
                NoteShort noteShort = note as NoteShort;
                int noteTime = Mathf.RoundToInt(noteShort.transform.localPosition.y * baseTime);

                notes.Add(new Note(noteTime, (int)NoteType.Short, findLine + 1, -1));
                //noteStr += $"{noteTime}, {(int)NoteType.Short}, {findLine + 1}\n";
            }
            else if (note is NoteLong)
            {
                NoteLong noteLong = note as NoteLong;
                int headTime = Mathf.RoundToInt(noteLong.transform.localPosition.y * baseTime);
                int tailTime = Mathf.RoundToInt((noteLong.transform.localPosition.y + noteLong.tail.transform.localPosition.y) * baseTime);

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
            $"Artist: {sheet.artist}\n" +
            $"Key: {sheet.keyNum}\n\n" +
            $"[Audio]\n" +
            $"BPM: {sheet.bpm}\n" +
            $"Offset: {sheet.offset}\n" +
            $"Signature: {sheet.signature[0]}/{sheet.signature[1]}\n\n" +
            $"[Note]\n" +
            $"{noteStr}";

        return writer.TrimEnd('\r', '\n');
    }

    public string StringifyNewSheet(Sheet sheet)
    {
        string writer = $"[Description]\n" +
            $"Title: {sheet.title}\n" +
            $"Artist: {sheet.artist}\n" +
            $"Key: {sheet.keyNum}\n\n" +
            $"[Audio]\n" +
            $"BPM: {sheet.bpm}\n" +
            $"Offset: {sheet.offset}\n" +
            $"Signature: {sheet.signature[0]}/{sheet.signature[1]}\n\n" +
            $"[Note]\n";

        return writer.TrimEnd('\r', '\n');
    }

    public IEnumerator IEGetSheet(string path, string title)
    {
        yield return StartCoroutine(NetworkManager.Instance.GetRequest($"{path}/{title}.sheet",
                data =>
                {
#if !UNITY_WEBGL
                    if (GameManager.Instance.state == GameManager.GameState.Edit)
                        SheetStorage.Instance.savedSheet = data;
#endif
                    sheet = ParseSheet(data);
                },
                error =>
                {
                    Debug.LogError($"Failed to download sheet: {error}");
                }
            )
        );
    }

    public IEnumerator IEGetClip(string path, string title)
    {
        yield return StartCoroutine(NetworkManager.Instance.GetAudioRequest($"{path}/{title}.mp3",
                data =>
                {
                    clip = data;
                    clip.name = title;
                },
                error =>
                {
                    Debug.LogError($"Failed to download clip: {error}");
                }
            )
        );
    }

    public IEnumerator IEGetImg(string path, string title)
    {
        yield return StartCoroutine(NetworkManager.Instance.GetImgRequest($"{path}/{title}.png",
               data =>
               {
                   img = Sprite.Create(data, new Rect(0, 0, data.width, data.height), new Vector2(0.5f, 0.5f));
                   img.name = title;
               },
               error =>
               {
                   Debug.LogError($"Failed to download img: {error}");
               }
           )
       );
    }

    public Sprite LoadImageFromLocal(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        return sprite;
    }

    private int Find4KeyLine(float notePosX)
    {
        if (notePosX < -1f && notePosX > -2f)
            return 0;
        else if (notePosX < 0f && notePosX > -1f)
            return 1;
        else if (notePosX < 1f && notePosX > 0f)
            return 2;
        else if (notePosX < 2f && notePosX > 1f)
            return 3;

        Debug.LogError("Unregistered Note Position");
        return 0;
    }
    private int Find5KeyLine(float notePosX)
    {
        if (notePosX < -1.5f && notePosX > -2.5f)
            return 0;
        else if (notePosX < -0.5f && notePosX > -1.5f)
            return 1;
        else if (notePosX < 0.5f && notePosX > -0.5f)
            return 2;
        else if (notePosX < 1.5f && notePosX > 0.5f)
            return 3;
        else if (notePosX < 2.5f && notePosX > 1.5f)
            return 4;

        Debug.LogError("Unregistered Note Position");
        return 0;
    }
    private int Find6KeyLine(float notePosX)
    {
        if (notePosX < -2f && notePosX > -3f)
            return 0;
        else if (notePosX < -1f && notePosX > -2f)
            return 1;
        else if (notePosX < 0f && notePosX > -1f)
            return 2;
        else if (notePosX < 1f && notePosX > 0f)
            return 3;
        else if (notePosX < 2f && notePosX > 1f)
            return 4;
        else if (notePosX < 3f && notePosX > 2f)
            return 5;

        Debug.LogError("Unregistered Note Position");
        return 0;
    }
}
