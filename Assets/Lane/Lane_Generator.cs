using UnityEngine;

public class Lane_Generator : MonoBehaviour
{
    public NotesGenerator notesGenerator; // NotesGenerator への参照
    public Transform topPoint;
    public Transform judgmentLine;
    public GameObject laneLinePrefab; // 🎯 プレハブをInspectorで設定

    public int[] noteValues = { 60, 61, 62, 63, 64, 65 }; // C4~F4

    private LineRenderer[] separatorLines; // 🎯 ノートの間の区切り線のみ

    void Start()
    {
        if (notesGenerator == null)
        {
            notesGenerator = FindFirstObjectByType<NotesGenerator>();
            if (notesGenerator == null)
            {
                Debug.LogError("❌ NotesGenerator がシーンに見つかりません！");
                return;
            }
        }

        if (laneLinePrefab == null)
        {
            Debug.LogError("❌ LineRenderer プレハブが設定されていません！");
            return;
        }

        separatorLines = new LineRenderer[noteValues.Length - 1]; // 🎯 5本の区切り線のみ

        for (int i = 0; i < noteValues.Length - 1; i++)
        {
            GameObject lineObj = Instantiate(laneLinePrefab, transform); // 🎯 プレハブを複製
            lineObj.name = $"Separator_{noteValues[i]}_{noteValues[i + 1]}";
            separatorLines[i] = lineObj.GetComponent<LineRenderer>();
        }
    }

    void Update()
    {
        if (notesGenerator == null) return;

        for (int i = 0; i < noteValues.Length - 1; i++)
        {
            float x1 = notesGenerator.GetFixedXPosition(noteValues[i]);
            float x2 = notesGenerator.GetFixedXPosition(noteValues[i + 1]);

            float midX = (x1 + x2) / 2.0f; // 🎯 ノートの間の中央
            Vector3 topPos = new Vector3(midX, topPoint.position.y, topPoint.position.z);
            Vector3 bottomPos = new Vector3(midX, judgmentLine.position.y, judgmentLine.position.z);

            // 🎯 bottomPos.z が負なら、0 で止める
            if (bottomPos.z < 0)
            {
                bottomPos.z = 0;
            }

            separatorLines[i].SetPosition(0, topPos);
            separatorLines[i].SetPosition(1, bottomPos);
        }
    }
}
