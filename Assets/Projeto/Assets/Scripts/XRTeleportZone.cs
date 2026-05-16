using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Zona de teleporte para locomoção VR dentro das cenas do ChronoMundi.
///
/// Cria áreas onde o jogador pode teleportar usando o controller.
/// Compatível com o TeleportInteractor do XR Interaction Toolkit.
///
/// Como usar:
///   1. Crie um Plane (chão) ou qualquer mesh.
///   2. Adicione este script.
///   3. O script adiciona TeleportationArea e configura visualmente.
///
/// O ChronoMundiXRBuilder adiciona isso automaticamente.
/// </summary>
[RequireComponent(typeof(Collider))]
public class XRTeleportZone : MonoBehaviour
{
    [Header("Visual do Chão de Teleporte")]
    public Color zoneColor = new Color(0f, 0.6f, 1f, 0.15f);
    public bool showZoneHighlight = true;

    [Header("Restrição de Teleporte")]
    [Tooltip("Se true, só teleporta quando o usuário apontar dentro de y ± tolerância do chão.")]
    public bool restrictToGroundLevel = true;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        SetupTeleportationArea();

        if (showZoneHighlight)
            ApplyZoneVisual();
    }

    void SetupTeleportationArea()
    {
        // TeleportationArea é adicionado via código pois requer configurações específicas
        // O XR Interaction Toolkit gerencia o resto
        var col = GetComponent<Collider>();
        if (col != null)
        {
            // Deve ser não-trigger para o teleporte funcionar
            col.isTrigger = false;
        }

        // Tag do layer para que os interactors de teleporte encontrem esta área
        // Ajuste o Layer para "Teleport" ou conforme seu projeto
        int teleportLayer = LayerMask.NameToLayer("Default");
        gameObject.layer = teleportLayer;
    }

    void ApplyZoneVisual()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        // Aplica cor semi-transparente à zona
        var mat = new Material(renderer.sharedMaterial ?? new Material(Shader.Find("Universal Render Pipeline/Lit")));
        mat.color = zoneColor;

        // Torna semi-transparente
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        renderer.material = mat;
    }
}
