using UnityEngine;
using UnityEngine.UI;

public class ForestFireSimulation : MonoBehaviour
{
    [Header("Dimensiones de la Rejilla")]
    public int width = 100;
    public int height = 100;
    public float scale = 0.1f;

    [Header("Parámetros Interactivos (Modificables)")]
    [Range(-1f, 1f)] public float windFactor = 0.5f;      // Viento (-1 a 1)
    [Range(0f, 1f)] public float humidity = 0.3f;       // Humedad global
    [Range(0.02f, 0.5f)] public float updateInterval = 0.1f; // Intervalo seguro

    private int[,] grid; // 0: Vacío, 1: Árbol, 2: Ardiendo, 3: Quemado
    private float[,] moistureGrid;
    private Texture2D displayTexture;
    private Renderer quadRenderer;
    private float timer;

    void Start()
    {
        quadRenderer = GetComponent<Renderer>();
        if (quadRenderer == null)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.transform.SetParent(transform);
            plane.transform.localPosition = Vector3.zero;
            plane.transform.localScale = new Vector3(10, 10, 1);
            quadRenderer = plane.GetComponent<Renderer>();
        }

        displayTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        displayTexture.filterMode = FilterMode.Point;
        quadRenderer.material.mainTexture = displayTexture;

        InitializeGrid();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            StepSimulation();
            DrawGrid();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            InitializeGrid();
        }
    }

    public void InitializeGrid()
    {
        grid = new int[width, height];
        moistureGrid = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                moistureGrid[x, y] = Random.Range(0f, 1f);
                if (Random.value < 0.8f)
                {
                    grid[x, y] = 1; // Árbol
                }
                else
                {
                    grid[x, y] = 0; // Vacío
                }
            }
        }

        grid[width / 2, height / 2] = 2; // Foco inicial en el centro
    }

    void StepSimulation()
    {
        int[,] newGrid = (int[,])grid.Clone();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == 2)
                {
                    newGrid[x, y] = 3; // Pasa a quemado
                }
                else if (grid[x, y] == 1)
                {
                    bool catchFire = false;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (grid[nx, ny] == 2)
                                {
                                    float probability = 0.4f;

                                    // Viento: favorece u obstaculiza según la dirección
                                    if (dx > 0) probability += windFactor * 0.3f;
                                    if (dx < 0) probability -= windFactor * 0.3f;

                                    // Humedad: frena la propagación correctamente
                                    float resistance = moistureGrid[x, y] * humidity;
                                    probability -= resistance * 0.5f;

                                    probability = Mathf.Clamp01(probability);

                                    if (Random.value < probability)
                                    {
                                        catchFire = true;
                                    }
                                }
                            }
                        }
                    }

                    if (catchFire)
                    {
                        newGrid[x, y] = 2;
                    }
                }
            }
        }
        grid = newGrid;
    }

    void DrawGrid()
    {
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                switch (grid[x, y])
                {
                    case 0: pixels[index] = Color.black; break;
                    case 1: pixels[index] = new Color(0.1f, 0.6f, 0.1f); break; // Árbol
                    case 2: pixels[index] = Color.red; break;       // Ardiendo
                    case 3: pixels[index] = new Color(0.3f, 0.3f, 0.3f); break; // Quemado
                }
            }
        }

        displayTexture.SetPixels(pixels);
        displayTexture.Apply();
    }

    //  para la UI con protección para que no se buggee la velocidad
    public void SetWindFactor(float value)
    {
        windFactor = value;
    }

    public void SetHumidity(float value)
    {
        humidity = value;
    }

    public void SetUpdateInterval(float value)
    {
        updateInterval = Mathf.Max(0.02f, value);
    }
}