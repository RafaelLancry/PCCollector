using UnityEngine;

public class ScenePersist : MonoBehaviour
{
    void Awake()
    {
        // Conta todas as instâncias de GameSession (ativas e inativas)
        var numScenePersists = FindObjectsByType<ScenePersist>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (numScenePersists.Length > 1)
        {
            // Já existe outra; destrói esta e sai para não chamar DontDestroyOnLoad
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void ResetScenePersist()
    {
        Destroy(gameObject);
    }
}
