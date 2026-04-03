using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerenciador de áudio ambiente do ChronoMundi.
/// Toca trilha sonora diferente em cada cena com crossfade suave.
/// Persiste entre cenas via DontDestroyOnLoad.
///
/// Como usar:
///   1. Adicione ao prefab do GameManager ou crie um prefab "AudioManager_Prefab" em Resources/.
///   2. Configure as entradas em sceneTracks: nome da cena → AudioClip.
///   3. Nenhum outro setup necessário; o AudioManager detecta a cena automaticamente.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SceneTrack
    {
        public string sceneName;
        public AudioClip track;
        [Range(0f, 1f)]
        public float volume = 0.4f;
    }

    [Header("Trilhas por Cena")]
    public List<SceneTrack> sceneTracks = new List<SceneTrack>();

    [Header("Crossfade")]
    public float crossfadeDuration = 1.5f;

    [Header("Volume Geral")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    // ── Dois AudioSources para crossfade ──────────────────────────────────
    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private bool _usingA = true;

    private AudioSource ActiveSource => _usingA ? _sourceA : _sourceB;
    private AudioSource InactiveSource => _usingA ? _sourceB : _sourceA;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _sourceA = gameObject.AddComponent<AudioSource>();
        _sourceB = gameObject.AddComponent<AudioSource>();

        foreach (var src in new[] { _sourceA, _sourceB })
        {
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var entry = sceneTracks.Find(t => t.sceneName == scene.name);

        if (entry != null && entry.track != null)
            CrossfadeTo(entry.track, entry.volume * masterVolume);
        else
            FadeOut();
    }

    // ── API pública ────────────────────────────────────────────────────────

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        // Atualiza o source ativo imediatamente
        if (ActiveSource.isPlaying)
        {
            var entry = sceneTracks.Find(t => t.sceneName == SceneManager.GetActiveScene().name);
            if (entry != null)
                ActiveSource.volume = entry.volume * masterVolume;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    void CrossfadeTo(AudioClip clip, float targetVolume)
    {
        if (ActiveSource.clip == clip) return; // mesma trilha, não interrompe

        InactiveSource.clip = clip;
        InactiveSource.volume = 0f;
        InactiveSource.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(targetVolume));
    }

    void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeSourceOut(ActiveSource));
    }

    IEnumerator Crossfade(float targetVolume)
    {
        float elapsed = 0f;
        float startVolA = ActiveSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;

            ActiveSource.volume = Mathf.Lerp(startVolA, 0f, t);
            InactiveSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        ActiveSource.Stop();
        ActiveSource.volume = 0f;

        _usingA = !_usingA; // troca qual é o "ativo"
    }

    IEnumerator FadeSourceOut(AudioSource src)
    {
        float start = src.volume;
        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            src.volume = Mathf.Lerp(start, 0f, elapsed / crossfadeDuration);
            yield return null;
        }

        src.Stop();
        src.volume = 0f;
    }
}
