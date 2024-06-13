using System.Collections;
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

    string basePath = "https://drt2kw8kpttus.cloudfront.net";

    public AudioClip clip;
    public Sprite img;

    public IEnumerator IEParse(string title)
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{basePath}/Sheet/{title}/{title}.sheet"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string contents = www.downloadHandler.text;
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
                            SheetLoader.Instance.originSheet.title = row.Split(':')[1].Trim();
                        else if (row.StartsWith("Artist"))
                            SheetLoader.Instance.originSheet.artist = row.Split(':')[1].Trim();
                    }
                    else if (currentStep == Step.Audio)
                    {
                        if (row.StartsWith("BPM"))
                            SheetLoader.Instance.originSheet.bpm = int.Parse(row.Split(':')[1].Trim());
                        else if (row.StartsWith("Offset"))
                            SheetLoader.Instance.originSheet.offset = int.Parse(row.Split(':')[1].Trim());
                        else if (row.StartsWith("Signature"))
                        {
                            string[] s = row.Split(':');
                            s = s[1].Split('/');
                            int[] sign = { int.Parse(s[0].Trim()), int.Parse(s[1].Trim()) };
                            SheetLoader.Instance.originSheet.signature = sign;
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
                        SheetLoader.Instance.originSheet.notes.Add(new Note(time, type, line, tail));
                    }
                }
            }
        }
        yield return IEGetClip(title);
        yield return IEGetImg(title);

        SheetLoader.Instance.originSheet.clip = clip;
        SheetLoader.Instance.originSheet.img = img;
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
