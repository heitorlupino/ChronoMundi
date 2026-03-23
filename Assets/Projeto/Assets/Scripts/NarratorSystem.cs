using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Usando TMP, consistente com o restante do projeto

public class NarratorSystem : MonoBehaviour
{
    public static NarratorSystem Instance;

    [Header("Áudio")]
    public AudioSource audioSource;

    [Header("Legenda")]
    public TextMeshProUGUI subtitleText; // Trocado de Text para TMP
    public GameObject subtitlePanel;

    void Awake()
    {
        // Singleton: garante que só existe uma instância
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayNarration(AudioClip clip, string subtitle)
    {
        // Verificações de null para evitar crashes
        if (clip == null)
        {
            Debug.LogWarning("NarratorSystem: AudioClip não atribuído!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("NarratorSystem: AudioSource não atribuído!");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        if (subtitlePanel != null) subtitlePanel.SetActive(true);
        if (subtitleText != null)  subtitleText.text = subtitle;

        CancelInvoke();
        Invoke("HideSubtitle", clip.length);
    }

    void HideSubtitle()
    {
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null)  subtitleText.text = "";
    }
}