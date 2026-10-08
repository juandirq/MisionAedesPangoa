using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class RiverWaterAnimator : MonoBehaviour
{
    [SerializeField] private float fase;
    [SerializeField, Range(0, 3)] private int cantidadCascadas;
    [SerializeField] private Vector4 caida1;
    [SerializeField] private Vector4 caida2;
    [SerializeField] private Vector4 caida3;
    [SerializeField] private Vector4 base1;
    [SerializeField] private Vector4 base2;
    [SerializeField] private Vector4 base3;

    private static readonly int WaterTimeId = Shader.PropertyToID("_WaterTime");
    private static readonly int WaterPhaseId = Shader.PropertyToID("_WaterPhase");
    private static readonly int FallCountId = Shader.PropertyToID("_FallCount");
    private static readonly int FallRect1Id = Shader.PropertyToID("_FallRect1");
    private static readonly int FallRect2Id = Shader.PropertyToID("_FallRect2");
    private static readonly int FallRect3Id = Shader.PropertyToID("_FallRect3");
    private static readonly int BaseRect1Id = Shader.PropertyToID("_BaseRect1");
    private static readonly int BaseRect2Id = Shader.PropertyToID("_BaseRect2");
    private static readonly int BaseRect3Id = Shader.PropertyToID("_BaseRect3");

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock properties;
    private float elapsed;

    public float TiempoAnimacion => elapsed;
    public float Fase => fase;
    public int CantidadCascadas => cantidadCascadas;
    public Vector4 Caida1 => caida1;
    public Vector4 Caida2 => caida2;
    public Vector4 Caida3 => caida3;
    public Vector4 Base1 => base1;
    public Vector4 Base2 => base2;
    public Vector4 Base3 => base3;

    public void ConfigurarFase(float value)
    {
        fase = value;
        ApplyProperties();
    }

    public void ConfigurarCascadas(int count, Vector4 fall1, Vector4 splash1,
        Vector4 fall2, Vector4 splash2, Vector4 fall3, Vector4 splash3)
    {
        cantidadCascadas = Mathf.Clamp(count, 0, 3);
        caida1 = fall1;
        base1 = splash1;
        caida2 = fall2;
        base2 = splash2;
        caida3 = fall3;
        base3 = splash3;
        ApplyProperties();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        properties = new MaterialPropertyBlock();
        ApplyProperties();
    }

    private void OnEnable()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (properties == null) properties = new MaterialPropertyBlock();
        ApplyProperties();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        ApplyProperties();
    }

    private void ApplyProperties()
    {
        if (spriteRenderer == null) return;
        if (properties == null) properties = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(properties);
        properties.SetFloat(WaterTimeId, elapsed);
        properties.SetFloat(WaterPhaseId, fase);
        properties.SetFloat(FallCountId, cantidadCascadas);
        properties.SetVector(FallRect1Id, caida1);
        properties.SetVector(FallRect2Id, caida2);
        properties.SetVector(FallRect3Id, caida3);
        properties.SetVector(BaseRect1Id, base1);
        properties.SetVector(BaseRect2Id, base2);
        properties.SetVector(BaseRect3Id, base3);
        spriteRenderer.SetPropertyBlock(properties);
    }
}
