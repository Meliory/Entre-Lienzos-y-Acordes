using TMPro;
using UnityEngine;

[RequireComponent(typeof(ZonaPista))]
public class ZoneDecorator : MonoBehaviour
{
    [Header("Texto 3D")]
    [SerializeField] private float alturaTexto   = 2.5f;
    [SerializeField] private float tamañoTexto   = 4f;
    [SerializeField] private TMP_FontAsset       fuente;

    [Header("Borde")]
    [SerializeField] private float anchuraLinea  = 0.06f;
    [SerializeField] [Range(1f, 4f)] private float brillo = 1.8f;
    [SerializeField] private Material            bordeMaterial;

    private Transform _labelTransform;

    // ================================================================== //

    private void Start()
    {
        var zona = GetComponent<ZonaPista>();
        var col  = GetComponent<BoxCollider>();

        Color color  = zona.TintColor;
        string label = zona.DisplayName;

        if (col != null) CrearBorde(col, color);
        CrearEtiqueta(label, color, col);
    }

    private void LateUpdate()
    {
        if (_labelTransform == null || Camera.main == null) return;

        Vector3 haciaCamera = Camera.main.transform.position - _labelTransform.position;
        if (haciaCamera != Vector3.zero)
            _labelTransform.rotation = Quaternion.LookRotation(-haciaCamera, Vector3.up);
    }

    // ================================================================== //
    //  Borde LineRenderer en el suelo
    // ================================================================== //

    private void CrearBorde(BoxCollider col, Color color)
    {
        var go = new GameObject("ZoneBorder");
        go.transform.SetParent(transform);

        var lr = go.AddComponent<LineRenderer>();
        lr.loop           = true;
        lr.useWorldSpace  = true;
        lr.widthMultiplier = anchuraLinea;
        lr.positionCount  = 4;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // Esquinas del suelo de la zona
        Vector3 wc = transform.TransformPoint(col.center);
        Vector3 we = Vector3.Scale(col.size * 0.5f, transform.lossyScale);
        float   y  = wc.y - Mathf.Abs(we.y) + 0.04f;

        lr.SetPosition(0, new Vector3(wc.x - Mathf.Abs(we.x), y, wc.z - Mathf.Abs(we.z)));
        lr.SetPosition(1, new Vector3(wc.x + Mathf.Abs(we.x), y, wc.z - Mathf.Abs(we.z)));
        lr.SetPosition(2, new Vector3(wc.x + Mathf.Abs(we.x), y, wc.z + Mathf.Abs(we.z)));
        lr.SetPosition(3, new Vector3(wc.x - Mathf.Abs(we.x), y, wc.z + Mathf.Abs(we.z)));

        // Color HDR para que el bloom lo ilumine
        Color hdr = new Color(color.r * brillo, color.g * brillo, color.b * brillo, 1f);

        Material mat;
        if (bordeMaterial != null)
        {
            mat = new Material(bordeMaterial);
        }
        else
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Sprites/Default");
            mat = new Material(shader);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", hdr);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     hdr);

        lr.material    = mat;
        lr.startColor  = hdr;
        lr.endColor    = hdr;
    }

    // ================================================================== //
    //  Etiqueta TextMeshPro 3D billboard
    // ================================================================== //

    private void CrearEtiqueta(string nombre, Color color, BoxCollider col)
    {
        var go = new GameObject("ZoneLabel");
        go.transform.SetParent(transform);

        // Posición: centro de la zona, a altura configurable desde el suelo
        Vector3 centro  = col != null ? transform.TransformPoint(col.center) : transform.position;
        float   sueloY  = col != null ? centro.y - Mathf.Abs(Vector3.Scale(col.size * 0.5f, transform.lossyScale).y) : centro.y;
        go.transform.position = new Vector3(centro.x, sueloY + alturaTexto, centro.z);

        var tmp              = go.AddComponent<TextMeshPro>();
        tmp.text             = nombre;
        tmp.fontSize         = tamañoTexto;
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.color            = color;
        tmp.fontStyle        = FontStyles.Bold;
        tmp.outlineWidth     = 0.15f;
        tmp.outlineColor     = new Color32(0, 0, 0, 200);

        if (fuente != null) tmp.font = fuente;

        _labelTransform = go.transform;
    }
}
