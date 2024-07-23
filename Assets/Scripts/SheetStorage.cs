using System.IO;
using UnityEngine;

public class SheetStorage : MonoBehaviour
{
    private string savedSheet;
    private string saveFilePath;

    public void Init()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "lastSavedSheet.sheet");

        savedSheet = LoadSavedSheet();

        if (savedSheet != null)
            GameManager.Instance.sheet = Parser.Instance.ParseSheet(savedSheet);
        else
            savedSheet = SheetLoader.Instance.sheetContent;
    }

    /*
     * 저장
        1) 노트 오브젝트 읽어서 y좌표 기반으로 시간 계산
        BarPerSec / 16 * 노트y좌표 = 저장될 시간

        롱노트의 경우
        Head y좌표 = NoteLong의 y좌표
        Tail y좌표 = NoteLong.y + tail.y가 최종좌표
     */

    public string LoadSavedSheet()
    {
        if (File.Exists(saveFilePath))
            return File.ReadAllText(saveFilePath);
        else
            return null;
    }


#if !UNITY_WEBGL
    public void SaveSheet()
    {
        savedSheet = Parser.Instance.StringifyEditedSheet();
        File.WriteAllText(saveFilePath, savedSheet);

        Editor.Instance.ShowProgressLog($"Sheet saved successfully at {saveFilePath}");
        Debug.Log($"Sheet saved successfully at {saveFilePath}");
    }

    public bool CompareEditedSheet()
    {
        string editedSheet = Parser.Instance.StringifyEditedSheet();
        bool isChangedEditedSheet = editedSheet != savedSheet;
        return isChangedEditedSheet;
    }

    public void Upload()
    {
        SaveSheet();
        S3Uploader.Instance.UploadFile(savedSheet, "binary/octet-stream");
    }

    public void Download()
    {
        SaveSheet();

        string path = Application.dataPath + "/Sheet/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        try
        {
            path += $"{GameManager.Instance.sheet.title}.sheet";
            File.WriteAllText(path, savedSheet);

            Editor.Instance.ShowProgressLog("Sheet downloaded successfully at " + path);
            Debug.Log("Sheet downloaded successfully at " + path);
        }
        catch (IOException e)
        {
            Editor.Instance.ShowProgressLog("Error while downloading the sheet: " + e.Message);
            Debug.LogError("Error while downloading the sheet: " + e.Message);
        }
    }
#endif

}
