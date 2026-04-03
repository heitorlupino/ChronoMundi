using UnityEngine;

/// <summary>
/// Garante que os singletons essenciais (GameManager, NarratorSystem, LoadingScreen)
/// existam em QUALQUER cena, sem precisar colocá-los manualmente em cada uma.
///
/// Funciona via [RuntimeInitializeOnLoadMethod] — executado automaticamente pelo Unity
/// antes de qualquer Awake(), inclusive no Play Mode e em builds.
///
/// Como usar:
///   Apenas mantenha este script no projeto. Nenhuma configuração adicional é necessária.
///   Os prefabs são criados via código; para customizar UI, crie prefabs nomeados:
///     "GameManager_Prefab", "NarratorSystem_Prefab", "LoadingScreen_Prefab"
///   e coloque-os em Resources/. Se não existirem, o bootstrapper cria versões mínimas.
/// </summary>
public static class SceneBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureGameManager();
        EnsureNarratorSystem();
        EnsureLoadingScreen();

        Debug.Log("[SceneBootstrapper] Singletons verificados e prontos.");
    }

    // ── GameManager ────────────────────────────────────────────────────────
    static void EnsureGameManager()
    {
        if (GameManager.Instance != null) return;

        // Tenta carregar prefab customizado
        var prefab = Resources.Load<GameObject>("GameManager_Prefab");
        if (prefab != null)
        {
            Object.Instantiate(prefab);
            return;
        }

        // Cria versão mínima
        var go = new GameObject("[GameManager]");
        go.AddComponent<GameManager>();
        Object.DontDestroyOnLoad(go);
    }

    // ── NarratorSystem ─────────────────────────────────────────────────────
    static void EnsureNarratorSystem()
    {
        if (NarratorSystem.Instance != null) return;

        var prefab = Resources.Load<GameObject>("NarratorSystem_Prefab");
        if (prefab != null)
        {
            Object.Instantiate(prefab);
            return;
        }

        var go = new GameObject("[NarratorSystem]");
        go.AddComponent<AudioSource>();
        go.AddComponent<NarratorSystem>();
        Object.DontDestroyOnLoad(go);

        // Avisa que o painel de legenda precisa ser configurado manualmente
        Debug.LogWarning("[SceneBootstrapper] NarratorSystem criado sem UI de legenda. " +
                         "Crie um prefab 'NarratorSystem_Prefab' em Resources/ com o Canvas configurado.");
    }

    // ── LoadingScreen ──────────────────────────────────────────────────────
    static void EnsureLoadingScreen()
    {
        if (LoadingScreen.Instance != null) return;

        var prefab = Resources.Load<GameObject>("LoadingScreen_Prefab");
        if (prefab != null)
        {
            Object.Instantiate(prefab);
            return;
        }

        var go = new GameObject("[LoadingScreen]");
        go.AddComponent<LoadingScreen>();
        Object.DontDestroyOnLoad(go);

        Debug.LogWarning("[SceneBootstrapper] LoadingScreen criado sem UI. " +
                         "Crie um prefab 'LoadingScreen_Prefab' em Resources/ com Canvas e Slider configurados.");
    }
}
