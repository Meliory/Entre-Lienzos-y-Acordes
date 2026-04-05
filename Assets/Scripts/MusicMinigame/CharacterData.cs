using UnityEngine;
using UnityEngine.UI;

public class CharacterData : MonoBehaviour
{
    private bool minigameActive = false;

    [Header("Identificación")]
    public string characterName;

    [Header("FMOD")]
    [FMODUnity.EventRef]
    public string fmodParameter; //"stability_aura", etc

    [Header("Estabilidad")]
    [Range(0, 100)]
    public float stability = 100f;
    public float passiveDrain = 0f;
    public float eventDrain = 12f;
    public bool eventActive = false;

    [Header("UI")]
    public Slider stabilitySlider;


    static readonly Color ColOk = new Color(0.18f, 0.55f, 0.34f);
    static readonly Color ColTense = new Color(0.72f, 0.55f, 0.10f);
    static readonly Color ColCritical = new Color(0.72f, 0.18f, 0.18f);

    private void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (minigameActive)
        {
            ApplyDrain();
            UpdateUI();
            PushToFMOD();
        }
    }

    void ApplyDrain()
    {
        if (stability <= 0) return;

        float drain = passiveDrain + (eventActive ? eventDrain : 0f);
        stability = Mathf.Clamp(stability - drain * Time.deltaTime, 0f, 100f);

        if(stability <= 0) MinigameManager.Instance.OnCharacterFailed(characterName);
    }

    void UpdateUI()
    {
        stabilitySlider.value = stability;
    }

    void PushToFMOD()
    {
        if (string.IsNullOrEmpty(fmodParameter)) return;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodParameter, stability);
    }

    public void ActivateEvent()
    {
        eventActive = true;
    }

    public void ActiveMinigame()
    {
        minigameActive = true;
    }

    public void DeactivateMinigame()
    {
        minigameActive = false;
    }
}
