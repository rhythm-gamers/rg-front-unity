#if !UNITY_WEBGL

using UnityEngine;

public class GridObject : MonoBehaviour
{
    public int index;

    int[] snap;

    GameObject[] lines;
    bool isActive;

    void Start()
    {
        snap = EditorController.Instance.snap;
        lines = new GameObject[GridGenerator.Instance.lineCount];

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = transform.GetChild(i).gameObject;
            lines[i].name = $"Line_{i}";
            lines[i].SetActive(false);

            if (i % snap[0] == 0) // 4비트
                lines[i].GetComponent<SpriteRenderer>().color = Color.white;
            else if (i % snap[1] == 0) // 8비트
                lines[i].GetComponent<SpriteRenderer>().color = new Color(197 / 255f, 21 / 255f, 21 / 255f);
            else if (i % snap[2] == 0) // 12비트
                lines[i].GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(197 / 255f, 21 / 255f, 21 / 255f), new Color(166 / 255f, 155 / 255f, 31 / 255f), 0.5f);
            else if (i % snap[3] == 0) // 16비트
                lines[i].GetComponent<SpriteRenderer>().color = new Color(166 / 255f, 155 / 255f, 31 / 255f);
            else if (i % snap[4] == 0) // 24비트
                lines[i].GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(166 / 255f, 155 / 255f, 31 / 255f), new Color(27 / 255f, 28 / 255f, 188 / 255f), 0.5f);
            else if (i % snap[5] == 0) // 32비트
                lines[i].GetComponent<SpriteRenderer>().color = new Color(27 / 255f, 28 / 255f, 188 / 255f);
            else if (i % snap[6] == 0) // 48비트
                lines[i].GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(27 / 255f, 28 / 255f, 188 / 255f), new Color(123 / 255f, 123 / 255f, 123 / 255f), 0.5f);
            else if (i % snap[7] == 0) // 64비트
                lines[i].GetComponent<SpriteRenderer>().color = new Color(123 / 255f, 123 / 255f, 123 / 255f);
        }

        ActiveGridsBySnap(snap[0]);

        EditorController.Instance.GridSnapListener = ChangeSnap;
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.Game) return;

        // 자기 차례와 멀면 비활성화 상태
        int currentBar = Editor.Instance.currentBar;
        if (index >= currentBar - 3 && index <= currentBar + 3)
        {
            isActive = true;
            ChangeSnap(snap[EditorController.Instance.SnapIdx]);
        }
        else
        {
            isActive = false;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].SetActive(false);
            }
        }
    }

    void ActiveGridsBySnap(int snap)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (i % snap == 0)
                lines[i].SetActive(true);
            else
                lines[i].SetActive(false);
        }
    }

    void ChangeSnap(int snap)
    {
        if (isActive)
        {
            ActiveGridsBySnap(snap);

            UIText uiSnap = UIController.Instance.FindUI("UI_E_Snap").uiObject as UIText;
            uiSnap.SetText($"{lines.Length / snap}비트");
        }
    }
}

#endif