using TMPro;
using UnityEngine;

public class CinematicaDialogo : MonoBehaviour
{
    [System.Serializable]
    public class LineaDialogo
    {
        public string personaje;
        [TextArea(2, 5)]
        public string texto;
    }

    [System.Serializable]
    public class PanelPersonaje
    {
        public string         personaje;   // debe coincidir exactamente con LineaDialogo.personaje
        public GameObject     panel;
        public TextMeshProUGUI speakerText;
        public TextMeshProUGUI dialogueText;
    }

    [Header("UI — Paneles por personaje")]
    public PanelPersonaje[] paneles;

    [Header("Diálogos (en orden)")]
    public LineaDialogo[] lineas;

    [Header("Props")]
    public Fire fireCandle;
    public GameObject candle;

    private int _lineaActual = -1;

    // Llamado por Signal "MostrarDialogo"
    public void MostrarSiguiente()
    {
        _lineaActual++;
        if (_lineaActual >= lineas.Length) return;

        var linea = lineas[_lineaActual];

        // Ocultar todos los paneles y mostrar el del personaje actual
        foreach (var p in paneles)
        {
            bool esSuyo = p.personaje == linea.personaje;
            p.panel?.SetActive(esSuyo);
            if (esSuyo)
            {
                if (p.speakerText)  p.speakerText.text  = linea.personaje;
                if (p.dialogueText) p.dialogueText.text = linea.texto;
            }
        }
    }

    // Llamado por Signal "OcultarDialogo"
    public void Ocultar()
    {
        foreach (var p in paneles)
            p.panel?.SetActive(false);
    }

    public void ApagarVela()
    {
        fireCandle.Extinguish();
    }

    public void EsconderVela()
    {
        candle.SetActive(false);
    }
}
