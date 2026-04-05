using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MusicUI : MonoBehaviour
{
    public static MusicUI Instance { get; private set; }

    [Tooltip("Panel contenedor")]
    [SerializeField]
    private GameObject panel;

    [Tooltip("Prefab de fila")]
    [SerializeField]
    private GameObject filaPrefab;

    //Pistas activas
    private ZonaPista[] _pistas;
    private readonly List<Slider> _sliders = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        panel.SetActive(false);
    }

    public void Mostrar(ZonaPista[] pistas)
    {
        //Limpiar filas anteriores
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }
        _sliders.Clear();

        _pistas = pistas;

        //Crear filas para cada pista
        foreach (var pista in pistas)
        {
            GameObject fila = Instantiate(filaPrefab, panel.transform);

            //nombre
            var texto = fila.GetComponentInChildren<TextMeshProUGUI>();
            if (texto != null)
            {
                texto.text = pista.DisplayName;
            }

            //Guardar referencia
            var slider = fila.GetComponentInChildren<Slider>();
            if (slider != null)
            {
                slider.interactable = false;
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
            }
            _sliders.Add(slider);
        }

        panel.SetActive(true);
    }

    public void Ocultar()
    {
        panel.SetActive(false);
        _pistas = null;
        _sliders.Clear();
    }

    private void Update()
    {
        if(_pistas == null) return;

        for(int i = 0; i < _pistas.Length; i++)
        {
            if(_sliders[i] != null)
            {
                _sliders[i].value = MusicManager.Instance.GetParamValue(_pistas[i]);
            }
        }
    }
}
