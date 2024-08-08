#if !UNITY_WEBGL

using System.IO;
using UnityEngine;

public class SheetStorage : MonoBehaviour
{
    static SheetStorage instance;
    public static SheetStorage Instance
    {
        get
        {
            return instance;
        }
    }

    public string savedSheet;

    public readonly string[] keyNums = { "4", "5", "6" };


    private string localSaveFilePath;

    void Awake()
    {
        if (instance == null)
            instance = this;

        localSaveFilePath = $"{Application.persistentDataPath}/Sheet";
    }

    public void Init()
    {
        string title = GameManager.Instance.sheet.title;

        savedSheet = LoadSavedSheet($"{localSaveFilePath}/{title}/{title}.sheet");

        if (savedSheet != null)
            GameManager.Instance.sheet = Parser.Instance.ParseSheet(savedSheet);
    }

    /*
     * 저장
        1) 노트 오브젝트 읽어서 y좌표 기반으로 시간 계산
        BarPerSec / 16 * 노트y좌표 = 저장될 시간

        롱노트의 경우
        Head y좌표 = NoteLong의 y좌표
        Tail y좌표 = NoteLong.y + tail.y가 최종좌표
     */

    public string LoadSavedSheet(string filePath)
    {
        if (File.Exists(filePath))
            return File.ReadAllText(filePath);
        else
            return null;
    }

    public void SaveNewSheet(Sheet sheet, string fullFilePath)
    {
        string sheetContent = Parser.Instance.StringifyNewSheet(sheet);

        File.WriteAllText(fullFilePath, sheetContent);
        Debug.Log($"New Sheet saved successfully at {fullFilePath}");
    }
    public void SaveEditedSheet()
    {
        string title = GameManager.Instance.sheet.title;
        savedSheet = Parser.Instance.StringifyEditedSheet();

        string fullFilePath = Path.Combine(localSaveFilePath, title, $"{title}.sheet");
        File.WriteAllText(fullFilePath, savedSheet);

        Editor.Instance.ShowProgressLog($"Sheet saved successfully at {fullFilePath}");
        Debug.Log($"Sheet saved successfully at {fullFilePath}");
    }
    private void SaveThumbnail(Sprite sprite, string fullFilePath)
    {
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        texture.SetPixels(sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height));
        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(fullFilePath, pngData);
            Debug.Log($"Thumbnail saved successfully at {fullFilePath}");
        }
    }
    private void SaveMp3(string audioPath, string fullFilePath)
    {
        File.Copy(audioPath, fullFilePath, true);

        Debug.Log($"Mp3 saved successfully at {fullFilePath}");
    }

    public bool CompareEditedSheet()
    {
        string editedSheet = Parser.Instance.StringifyEditedSheet();
        bool isChangedEditedSheet = editedSheet != savedSheet;
        return isChangedEditedSheet;
    }

    public void AddNewSheet(Sheet sheet, Sprite sprite, string audioPath)
    {
        string title = sheet.title;

        foreach (string keyNum in keyNums)
        {
            if (!Directory.Exists($"{localSaveFilePath}/{keyNum}/{title}"))
            {
                Directory.CreateDirectory($"{localSaveFilePath}/{keyNum}/{title}");
            }

            SaveNewSheet(sheet, $"{localSaveFilePath}/{keyNum}/{title}/{title}.sheet");
            SaveThumbnail(sprite, $"{localSaveFilePath}/{keyNum}/{title}/{title}.png");
            SaveMp3(audioPath, $"{localSaveFilePath}/{keyNum}/{title}/{title}.mp3");
        }

        Debug.Log($"Sheet files saved successfully");
    }

    public void Upload()
    {
        SaveEditedSheet();

        string title = GameManager.Instance.sheet.title;
        S3Uploader.Instance.UploadSheet(localSaveFilePath, title);
    }

    public void Download()
    {
        SaveEditedSheet();

        string title = GameManager.Instance.sheet.title;
        string path = Path.Combine(Application.dataPath, "Sheet", title);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        try
        {
            string saveFilePath = Path.Combine(path, $"{title}.sheet");
            File.WriteAllText(saveFilePath, savedSheet);

            Editor.Instance.ShowProgressLog("Sheet downloaded successfully at " + saveFilePath);
            Debug.Log("Sheet downloaded successfully at " + saveFilePath);
        }
        catch (IOException e)
        {
            Editor.Instance.ShowProgressLog("Error while downloading the sheet: " + e.Message);
            Debug.LogError("Error while downloading the sheet: " + e.Message);
        }
    }
}

#endif
