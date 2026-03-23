using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Permite que o ray do controller VR interaja com Canvas em World Space.
/// Compatível com XR Interaction Toolkit.
/// Coloque este script no GameObject do seu XR Ray Interactor (controller).
/// O Canvas deve estar em modo "World Space" com um GraphicRaycaster.
/// </summary>
public class VRUIInteraction : MonoBehaviour
{
    [Header("Configurações do Ray")]
    public float rayDistance    = 10f;
    public LayerMask uiLayer;              // Defina a layer "UI" aqui
    public LineRenderer lineRenderer;      // Visual do raio (opcional)

    [Header("Cor do Ray")]
    public Color colorNormal  = new Color(0f, 0.8f, 1f, 0.8f);
    public Color colorHover   = new Color(0f, 1f, 0.4f, 0.8f);

    [Header("Input (XR Controller)")]
    public bool useXRInput = true; // false = usa mouse (para testar no editor)

    // ── Estado interno ─────────────────────────────────────────────────────
    private GameObject  _hoveredObject;
    private PointerEventData _pointerData;
    private EventSystem _eventSystem;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        _eventSystem = EventSystem.current;
        _pointerData = new PointerEventData(_eventSystem);

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            SetRayColor(colorNormal);
        }
    }

    void Update()
    {
        CastUIRay();

        if (ShouldClick())
            ClickCurrentHover();
    }

    // ── Raycast na UI ─────────────────────────────────────────────────────
    void CastUIRay()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        // Atualiza visual do LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * rayDistance);
        }

        // Raycast físico para encontrar Canvas em World Space
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, uiLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj != _hoveredObject)
            {
                // Saiu do hover anterior
                if (_hoveredObject != null)
                    SendPointerEvent(_hoveredObject, ExecuteEvents.pointerExitHandler);

                // Entrou no novo objeto
                _hoveredObject = hitObj;
                SendPointerEvent(_hoveredObject, ExecuteEvents.pointerEnterHandler);
                SetRayColor(colorHover);

                if (lineRenderer != null)
                    lineRenderer.SetPosition(1, hit.point);
            }
        }
        else
        {
            if (_hoveredObject != null)
            {
                SendPointerEvent(_hoveredObject, ExecuteEvents.pointerExitHandler);
                _hoveredObject = null;
                SetRayColor(colorNormal);
            }
        }
    }

    void ClickCurrentHover()
    {
        if (_hoveredObject == null) return;

        SendPointerEvent(_hoveredObject, ExecuteEvents.pointerDownHandler);
        SendPointerEvent(_hoveredObject, ExecuteEvents.pointerUpHandler);
        SendPointerEvent(_hoveredObject, ExecuteEvents.pointerClickHandler);
        Debug.Log($"[VRUIInteraction] Clicou em: {_hoveredObject.name}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    void SendPointerEvent<T>(GameObject target, ExecuteEvents.EventFunction<T> handler)
        where T : IEventSystemHandler
    {
        _pointerData.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ExecuteEvents.Execute(target, _pointerData, handler);
    }

    bool ShouldClick()
    {
#if UNITY_EDITOR
        // No editor, usa clique do mouse para testar
        if (!useXRInput) return Input.GetMouseButtonDown(0);
#endif

        // XR — detecta trigger do controller via Input System
        // Adapte o nome do botão de acordo com seu binding no XR
        return Input.GetButtonDown("Fire1");

        // Se usar o novo Input System com XR Interaction Toolkit:
        // return triggerAction?.action?.WasPressedThisFrame() ?? false;
    }

    void SetRayColor(Color color)
    {
        if (lineRenderer == null) return;
        lineRenderer.startColor = color;
        lineRenderer.endColor   = new Color(color.r, color.g, color.b, 0f);
    }
}