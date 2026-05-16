using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Personagem guia do ChronoMundi.
///
/// Cada cena histórica tem um guia visual diferente (cor + nome).
/// O guia:
///   1. Aparece na entrada da cena com uma fala de boas-vindas
///   2. Segue o jogador mantendo distância lateral
///   3. Reage a cada artefato interagido (fala um comentário)
///   4. Fala uma linha final quando todos os artefatos são explorados
///   5. Usa o NarratorSystem existente para exibir legendas
///
/// Compatível com Desktop e VR (detecta via VRModeManager).
/// </summary>
public class CompanionCharacter : MonoBehaviour
{
    // ── Identidade ────────────────────────────────────────────────────────
    [Header("Identidade do Guia")]
    public string companionName = "Cronos";
    public Color  companionColor = new Color(0.4f, 0.8f, 1f);

    // ── Diálogos ──────────────────────────────────────────────────────────
    [Header("Diálogos")]
    [TextArea] public string introLine =
        "Olá! Sou seu guia nesta era. Explore os artefatos ao redor!";

    [TextArea] public string allDoneLines =
        "Incrível! Você conheceu todos os artefatos desta era. Pronto para voltar?";

    [Tooltip("Um comentário por artefato, em ordem de interação. " +
             "Se tiver menos que os artefatos, reutiliza o último.")]
    [TextArea(2,3)]
    public List<string> exhibitComments = new List<string>
    {
        "Que peça fascinante! Olhe com atenção cada detalhe.",
        "Impressionante, não é? A história está viva aqui.",
        "Este artefato conta uma história que atravessa séculos."
    };

    // ── Áudio ─────────────────────────────────────────────────────────────
    [Header("Áudio (opcional)")]
    [Tooltip("Clips de voz para cada comentário. Índice igual ao exhibitComments.")]
    public List<AudioClip> exhibitAudioClips  = new List<AudioClip>();
    public AudioClip        introAudioClip;
    public AudioClip        allDoneAudioClip;

    // ── Movimento ─────────────────────────────────────────────────────────
    [Header("Movimento")]
    public float followDistance = 1.8f;   // distância lateral do jogador
    public float followHeight   = 0f;     // offset Y em relação ao jogador
    public float moveSpeed      = 4f;
    public float rotateSpeed    = 6f;
    [Tooltip("Lado do jogador: 1 = direita, -1 = esquerda")]
    public float side = 1f;

    // ── Visual ────────────────────────────────────────────────────────────
    [Header("Visual")]
    public float bobAmplitude = 0.06f;
    public float bobSpeed     = 2.2f;
    [Tooltip("Referência ao Renderer do corpo do guia para aplicar a cor.")]
    public Renderer bodyRenderer;

    // ── Name Tag ──────────────────────────────────────────────────────────
    [Header("Name Tag (WorldSpace)")]
    public TextMeshProUGUI nameTagTMP;
    [Tooltip("Offset do name tag acima do guia.")]
    public Vector3 nameTagOffset = new Vector3(0, 1.4f, 0);

    // ═════════════════════════════════════════════════════════════════════
    // Internos
    // ═════════════════════════════════════════════════════════════════════

    Transform  _player;
    Vector3    _bobOrigin;
    int        _exhibitCount     = 0;
    bool       _allDoneSaid      = false;
    bool       _introPlayed      = false;
    float      _delayBeforeIntro = 2.2f; // espera o fade-in da cena

    static readonly int s_EmissionColor = Shader.PropertyToID("_EmissionColor");

    // ═════════════════════════════════════════════════════════════════════

    void Start()
    {
        _bobOrigin = transform.position;
        ApplyColor();
        SetupNameTag();
        FindPlayer();
        HookExhibits();
        StartCoroutine(PlayIntroDelayed());
    }

    void Update()
    {
        if (_player == null) FindPlayer();
        if (_player == null) return;

        FollowPlayer();
        BobAnimation();
        FacePlayer();
    }

    // ── Intro ─────────────────────────────────────────────────────────────

    IEnumerator PlayIntroDelayed()
    {
        yield return new WaitForSeconds(_delayBeforeIntro);
        if (!_introPlayed)
        {
            _introPlayed = true;
            SayLine(introAudioClip, $"<b>{companionName}:</b> {introLine}");
        }
    }

    // ── Gancho nos artefatos ──────────────────────────────────────────────

    void HookExhibits()
    {
        var exhibits = FindObjectsByType<InteracleObject>(FindObjectsSortMode.None);
        foreach (var ex in exhibits)
            ex.OnInteracted += OnExhibitInteracted;

        Debug.Log($"[Companion:{companionName}] {exhibits.Length} artefato(s) registrados.");
    }

    void OnExhibitInteracted(InteracleObject exhibit)
    {
        // Aguarda a narração do artefato terminar antes de comentar
        StartCoroutine(CommentAfterDelay(exhibit));
    }

    IEnumerator CommentAfterDelay(InteracleObject exhibit)
    {
        // Espera o NarratorSystem terminar a narração do artefato
        float wait = exhibit.narrationClip != null
            ? exhibit.narrationClip.length + 0.5f
            : 3.2f;
        yield return new WaitForSeconds(wait);

        // Comentário indexado
        if (exhibitComments.Count == 0) yield break;
        int idx = Mathf.Min(_exhibitCount, exhibitComments.Count - 1);
        string comment = exhibitComments[idx];

        AudioClip clip = (exhibitAudioClips != null && idx < exhibitAudioClips.Count)
            ? exhibitAudioClips[idx]
            : null;

        SayLine(clip, $"<b>{companionName}:</b> {comment}");
        _exhibitCount++;

        // Checa se todos os artefatos foram vistos
        CheckAllDone();
    }

    void CheckAllDone()
    {
        if (_allDoneSaid) return;

        var all = FindObjectsByType<InteracleObject>(FindObjectsSortMode.None);
        bool allInteracted = true;
        foreach (var ex in all)
            if (!ex.HasBeenInteracted) { allInteracted = false; break; }

        if (!allInteracted) return;

        _allDoneSaid = true;
        StartCoroutine(SayAllDoneDelayed());
    }

    IEnumerator SayAllDoneDelayed()
    {
        yield return new WaitForSeconds(1.5f);
        SayLine(allDoneAudioClip, $"<b>{companionName}:</b> {allDoneLines}");
    }

    // ── NarratorSystem ────────────────────────────────────────────────────

    void SayLine(AudioClip clip, string text)
    {
        if (NarratorSystem.Instance != null)
            NarratorSystem.Instance.PlayNarration(clip, text);
        else
            Debug.Log($"[Companion] {text}");
    }

    // ── Movimento e visual ────────────────────────────────────────────────

    void FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) _player = go.transform;
    }

    void FollowPlayer()
    {
        Vector3 targetOffset = _player.right * (followDistance * side)
                             + Vector3.up    * followHeight;
        Vector3 target = _player.position + targetOffset;
        transform.position = Vector3.MoveTowards(transform.position, target,
            moveSpeed * Time.deltaTime);
    }

    void FacePlayer()
    {
        Vector3 dir = (_player.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot,
            rotateSpeed * Time.deltaTime);
    }

    void BobAnimation()
    {
        float y = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            _bobOrigin.y + y,
            transform.localPosition.z);
    }

    void ApplyColor()
    {
        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer == null) return;

        var mat = bodyRenderer.material;
        mat.color = companionColor;
        if (mat.HasProperty(s_EmissionColor))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor(s_EmissionColor, companionColor * 0.6f);
        }
    }

    void SetupNameTag()
    {
        if (nameTagTMP == null) return;
        nameTagTMP.text  = companionName;
        nameTagTMP.color = companionColor;
        nameTagTMP.transform.localPosition = nameTagOffset;
    }

    void OnDestroy()
    {
        // Remove listeners para evitar vazamento
        var exhibits = FindObjectsByType<InteracleObject>(FindObjectsSortMode.None);
        foreach (var ex in exhibits)
            ex.OnInteracted -= OnExhibitInteracted;
    }
}
