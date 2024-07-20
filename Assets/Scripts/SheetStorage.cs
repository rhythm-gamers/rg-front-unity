using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SheetStorage : MonoBehaviour
{
    private string savedSheet;
    private string saveFilePath;

    public void Init()
    {

        saveFilePath = Path.Combine(Application.persistentDataPath, "lastSavedSheet.sheet");

        savedSheet = LoadSavedSheet();
        GameManager.Instance.editorSheet = Parser.Instance.ParseSheet(savedSheet);
    }

    /*
     * 저장
        1) 노트 오브젝트 읽어서 y좌표 기반으로 시간 계산
        BarPerSec / 16 * 노트y좌표 = 저장될 시간

        롱노트의 경우
        Head y좌표 = NoteLong의 y좌표
        Tail y좌표 = NoteLong.y + tail.y가 최종좌표
     */
    public void SaveSheet()
    {
        Sheet sheet = GameManager.Instance.editorSheet;
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
                int noteTime = (int)(noteShort.transform.localPosition.y * baseTime * 1000) + sheet.offset;

                notes.Add(new Note(noteTime, (int)NoteType.Short, findLine + 1, -1));
                //noteStr += $"{noteTime}, {(int)NoteType.Short}, {findLine + 1}\n";
            }
            else if (note is NoteLong)
            {
                NoteLong noteLong = note as NoteLong;
                int headTime = (int)(noteLong.transform.localPosition.y * baseTime * 1000) + sheet.offset;
                int tailTime = (int)((noteLong.transform.localPosition.y + noteLong.tail.transform.localPosition.y) * baseTime * 1000) + sheet.offset;

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
            $"Offset: {sheet.offset}\n" +
            $"Signature: {sheet.signature[0]}/{sheet.signature[1]}\n\n" +
            $"[Note]\n" +
            $"{noteStr}";

        writer.TrimEnd('\r', '\n');
        savedSheet = writer;
        File.WriteAllText(saveFilePath, writer);

        Editor.Instance.ShowProgressLog($"Sheet saved successfully at {saveFilePath}");
        Debug.Log($"Sheet saved successfully at {saveFilePath}");
    }

    private string LoadSavedSheet()
    {
        if (File.Exists(saveFilePath))
            return File.ReadAllText(saveFilePath);
        else
            return SheetLoader.Instance.sheetContent; ;
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
            path += $"{GameManager.Instance.editorSheet.title}.sheet";
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


}
