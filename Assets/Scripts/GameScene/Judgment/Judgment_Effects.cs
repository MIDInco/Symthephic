using UnityEngine;

public class Judgment_Effects : MonoBehaviour
{
    public GameObject perfectEffectPrefab;
    public GameObject goodEffectPrefab;
    public GameObject missEffectPrefab;

    public ParticleSystem perfectEffect;
    public ParticleSystem goodEffect;
    public ParticleSystem missEffect;

    private JudgmentManager manager;

    public bool useParticleEffects = false; // 切り替え用
    [SerializeField] private float effectYPosition = -10f; // インスペクタから設定可能

void Start()
{
    manager = FindFirstObjectByType<JudgmentManager>();
    if (manager != null)
        manager.OnJudgment += PlayEffect;
}


void OnDestroy()
{
    if (manager != null)
        manager.OnJudgment -= PlayEffect;
}

    void PlayEffect(string judgment, Vector3 position)
    {
        if (useParticleEffects)
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
        else
        {
            GameObject prefab = null;

            switch (judgment)
            {
                case "Perfect": prefab = perfectEffectPrefab; break;
                case "Good": prefab = goodEffectPrefab; break;
                case "Miss": prefab = missEffectPrefab; break;
            }

            if (prefab != null)
            {
                Vector3 spawnPosition = position; // 判定された位置を取得
                spawnPosition.y = effectYPosition; // インスペクタで設定したY座標に変更

                GameObject effect = Instantiate(prefab, spawnPosition, Quaternion.identity);
                Destroy(effect, 0.3f); // 0.3秒後に削除
            }
        }
    }
}
