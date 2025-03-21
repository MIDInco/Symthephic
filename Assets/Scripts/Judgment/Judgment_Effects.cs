using UnityEngine;

public class Judgment_Effects : MonoBehaviour
{
    public ParticleSystem perfectEffect;
    public ParticleSystem goodEffect;
    public ParticleSystem missEffect;

    void Start()
    {
        // 🎯 JudgmentManager からイベントを受け取る
        JudgmentManager.OnJudgment += PlayEffect;
    }

    void OnDestroy()
    {
        // 🎯 イベントの購読を解除
        JudgmentManager.OnJudgment -= PlayEffect;
    }

    // 🎯 判定に応じたエフェクトを再生
    void PlayEffect(string judgment, Vector3 position)
    {
        switch (judgment)
        {
            case "Perfect":
                if (perfectEffect != null)
                    Instantiate(perfectEffect, position, Quaternion.identity).Play();
                break;
            case "Good":
                if (goodEffect != null)
                    Instantiate(goodEffect, position, Quaternion.identity).Play();
                break;
            case "Miss":
                if (missEffect != null)
                    Instantiate(missEffect, position, Quaternion.identity).Play();
                break;
        }
    }
}
