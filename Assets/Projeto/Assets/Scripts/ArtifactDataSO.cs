using UnityEngine;

/// <summary>
/// ScriptableObject que armazena todos os dados de um artefato.
/// É o "backend" central de textos do ChronoMundi.
///
/// Vantagens:
///   - Editar textos sem tocar em cena ou código
///   - Reutilizar o mesmo artefato em múltiplos contextos
///   - Substituir AudioClip sem rebuild
///   - Localization futura: troca o SO por idioma
///
/// Onde ficam salvo: Assets/Projeto/Assets/Data/Artifacts/
///
/// Como usar:
///   1. Crie via menu: Assets → Create → ChronoMundi → Artifact Data
///   2. Preencha os campos no Inspector
///   3. O RoomBuilder aplica automaticamente ao InteracleObject da cena
///   4. Em runtime o InteracleObject lê via ArtifactDataSO.ApplyTo(this)
/// </summary>
[CreateAssetMenu(
    fileName = "ArtifactData_NomeDoArtefato",
    menuName  = "ChronoMundi/Artifact Data",
    order     = 0)]
public class ArtifactDataSO : ScriptableObject
{
    // ── Identificação ────────────────────────────────────────────────────
    [Header("Identificação")]
    [Tooltip("ID único usado pelo RoomBuilder para encontrar este SO. Ex: 'Machado_PreHistoria'")]
    public string artifactID;

    [Tooltip("Era a que pertence. Usado para filtrar no RoomBuilder.")]
    public EraType era;

    // ── Textos ────────────────────────────────────────────────────────────
    [Header("Textos — exibidos no NarratorSystem (legenda curta)")]
    [Tooltip("Legenda exibida durante a narração. 1-2 linhas, máx 120 caracteres.")]
    [TextArea(1, 3)]
    public string subtitleText;

    [Header("Textos — exibidos no ArtifactInfoPanel (painel de info)")]
    [Tooltip("Nome do artefato exibido no título do painel.")]
    public string artifactName;

    [Tooltip("Descrição longa exibida no corpo do painel. Pode ter 2-4 parágrafos.")]
    [TextArea(4, 10)]
    public string artifactDescription;

    // ── Áudio ─────────────────────────────────────────────────────────────
    [Header("Áudio (opcional — arraste o AudioClip quando tiver)")]
    [Tooltip("Clip de narração. Se null, usa apenas a legenda em texto.")]
    public AudioClip narrationClip;

    // ── Metadados ─────────────────────────────────────────────────────────
    [Header("Metadados (não afetam o jogo)")]
    [Tooltip("Nome do autor/responsável pelo texto.")]
    public string textAuthor;
    [Tooltip("Data da última revisão dos textos.")]
    public string lastRevision;
    [TextArea(1, 3)]
    public string devNotes;

    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica os dados deste SO em um InteracleObject da cena.
    /// Chamado pelo RoomBuilder (Editor) e opcionalmente em runtime.
    /// </summary>
    public void ApplyTo(InteracleObject target)
    {
        if (target == null) return;

        target.artifactName        = artifactName;
        target.artifactDescription = artifactDescription;
        target.subtitleText        = subtitleText;
        target.narrationClip       = narrationClip;
    }

    /// <summary>Retorna true se todos os campos obrigatórios estão preenchidos.</summary>
    public bool IsComplete()
    {
        return !string.IsNullOrWhiteSpace(artifactID)
            && !string.IsNullOrWhiteSpace(artifactName)
            && !string.IsNullOrWhiteSpace(subtitleText)
            && !string.IsNullOrWhiteSpace(artifactDescription);
    }
}

/// <summary>Era histórica do artefato.</summary>
public enum EraType
{
    PreHistoria,
    IdadeMedia,
    FuturoTech
}
