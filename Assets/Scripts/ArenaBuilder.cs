using UnityEngine;

/// <summary>
/// Builds a bounded floor + 4 walls with a bouncy Physics Material.
/// </summary>
public class ArenaBuilder : MonoBehaviour
{
    [Header("Arena size")]
    [SerializeField] float arenaHalfSize = 20f;
    [SerializeField] float wallHeight = 4f;
    [SerializeField] float wallThickness = 1f;
    [SerializeField] float floorThickness = 0.5f;

    [Header("Bounce (Spec 6)")]
    [SerializeField] float bounciness = 0.9f;
    [SerializeField] float friction = 0.1f;

    PhysicsMaterial bounceMaterial;

    void Awake()
    {
        if (transform.Find("Floor") != null)
            return;

        BuildArena();
    }

    public void BuildArena()
    {
        bounceMaterial = CreateBounceMaterial();

        CreateBox(
            "Floor",
            new Vector3(0f, -floorThickness * 0.5f, 0f),
            new Vector3(arenaHalfSize * 2f, floorThickness, arenaHalfSize * 2f),
            new Color(0.15f, 0.18f, 0.25f),
            false);

        float wallY = wallHeight * 0.5f;
        float span = arenaHalfSize * 2f;

        CreateBox("Wall_North", new Vector3(0f, wallY, arenaHalfSize), new Vector3(span, wallHeight, wallThickness), new Color(0.35f, 0.4f, 0.5f), true);
        CreateBox("Wall_South", new Vector3(0f, wallY, -arenaHalfSize), new Vector3(span, wallHeight, wallThickness), new Color(0.35f, 0.4f, 0.5f), true);
        CreateBox("Wall_East", new Vector3(arenaHalfSize, wallY, 0f), new Vector3(wallThickness, wallHeight, span), new Color(0.35f, 0.4f, 0.5f), true);
        CreateBox("Wall_West", new Vector3(-arenaHalfSize, wallY, 0f), new Vector3(wallThickness, wallHeight, span), new Color(0.35f, 0.4f, 0.5f), true);
    }

    PhysicsMaterial CreateBounceMaterial()
    {
        // Spec 6 — Unity Physics Material (bounciness). See Unity Manual class-PhysicsMaterial.
        var mat = new PhysicsMaterial("ArenaBounce")
        {
            bounciness = bounciness,
            dynamicFriction = friction,
            staticFriction = friction,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
        return mat;
    }

    void CreateBox(string name, Vector3 localPos, Vector3 scale, Color color, bool applyBounce)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;

        var renderer = go.GetComponent<Renderer>();
        var shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        renderer.sharedMaterial = new Material(shader);
        renderer.sharedMaterial.color = color;

        var col = go.GetComponent<Collider>();
        if (applyBounce && bounceMaterial != null)
            col.material = bounceMaterial;

        // Static walls/floor — no rigidbody (primitives don't add one; keep safe)
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (Application.isPlaying)
                Object.Destroy(rb);
            else
                Object.DestroyImmediate(rb);
        }
    }
}
