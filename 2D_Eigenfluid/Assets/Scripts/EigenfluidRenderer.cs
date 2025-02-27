using UnityEngine;

public class EigenfluidRenderer : MonoBehaviour
{
    public ComputeShader computeShader;
    private RenderTexture renderTexture;
    private SpriteRenderer spriteRenderer;
    private int width;
    private int height; 

    private readonly int dimension = 5;
    private Vector3[] velocityCoeff;
    private Vector3[] voricityCoeff;
    private float[] eigenValues;
    private float[,,] sturctCoeffMatrixes;

    private Vector3 xAxis = new(1.0f, 0.0f, 0.0f);
    private Vector3 yAxis = new(0.0f, 1.0f, 0.0f);
    private Vector3 zAxis = new(0.0f, 0.0f, 1.0f);

    void Start()
    {
        this.Initialize();

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        width = Screen.width;
        height = Screen.height;

        // Create a RenderTexture for compute shader
        renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // Dispatch Compute Shader
        computeShader.SetTexture(0, "Result", renderTexture);
        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
        computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Convert RenderTexture to Sprite
        Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;

        // Assign the texture to a sprite
        spriteRenderer.sprite = Sprite.Create(texture2D, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    void OnDestroy()
    {
        renderTexture.Release();
    }

    private void Initialize(){
        velocityCoeff = new Vector3[dimension];
        voricityCoeff = new Vector3[dimension];
        eigenValues = new float[dimension];
        //TODO compute coefficients
        
        sturctCoeffMatrixes = new float[dimension,dimension,dimension];
        //TODO precompute sturctCoeffMatrix

    }
}
