using UnityEngine;
using UnityEngine.UI;

public class ForestFireSimulation : MonoBehaviour
{
    [Header("Dimensiones de la Rejilla")]
    public int width = 100;
    public int height = 100;
    public float scale = 0.1f;

    [Header("Parámetros Interactivos (Modificables)")]
    [Range(0f, 1f)] public float windFactor = 0.5f;     // Viento (Este)
    [Range(0f, 1f)] public float humidity = 0.3f;      // Humedad
    [Range(0.01f, 0.2f)] public float updateInterval = 0.1f;

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
            // Crea un plano automático si no tiene
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

        // Reiniciar con la tecla R o botón
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
                // 80% árboles, 20% huecos vacíos iniciales
                if (Random.value < 0.8f)
                {
                    grid[x, y] = 1; // arbol
                }
                else
                {
                    grid[x, y] = 0; // Vacío
                }
            }
        }

        // Encender un foco inicial en el centro
        grid[width / 2, height / 2] = 2;
    }

    void StepSimulation()
    {
        int[,] newGrid = (int[,])grid.Clone();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == 2) // Si está ardiendo pasa a quemado
                {
                    newGrid[x, y] = 3;
                }
                else if (grid[x, y] == 1) // Si es árbol, evaluar fuego vecino
                {
                    bool catchFire = false;

                    // Revisar vecinos con peso de viento hacia la derecha (+X)
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
                                    // El viento favorece propagación si dx > 0
                                    if (dx > 0) probability += windFactor * 0.4f;
                                    // La humedad frena el fuego
                                    if (moistureGrid[x, y] < humidity)
                                    {
                                        catchFire = true;
                                    }
                                    else if (Random.value < probability)
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
                    case 0: pixels[index] = Color.black; break;       // Vacío
                    case 1: pixels[index] = new Color(0.1f, 0.6f, 0.1f); break; // Árbol (Verde)
                    case 2: pixels[index] = Color.red; break;       // Ardiendo (Rojo)
                    case 3: pixels[index] = new Color(0.3f, 0.3f, 0.3f); break; // Quemado (Gris)
                }
            }
        }

        displayTexture.SetPixels(pixels);
        displayTexture.Apply();
    }
}