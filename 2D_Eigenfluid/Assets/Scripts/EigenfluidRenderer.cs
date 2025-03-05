using System;
using System.Collections.Generic;
using UnityEngine;

public class EigenfluidRenderer : MonoBehaviour
{
    public ComputeShader computeShader;
    private RenderTexture renderTexture;
    private SpriteRenderer spriteRenderer;
    private int width;
    private int height; 

    private readonly int N = 16;
    private int sqrtN; 
    private readonly Dictionary<Vector2, int> reverseWaveNumberLookup = new();
    private readonly Dictionary<int, Vector2> waveNumberLookup = new();

    private Vector2[,,] velocityCoeff; //--
    private float[] eigenValues; //--
    private float[,,] sturctCoeffMatrixes;
    

    void Start()
    {
        
        this.InitializeTexture();

        this.Initialize();

        // Dispatch Compute Shader
        computeShader.SetTexture(0, "Result", this.renderTexture);
        int threadGroupsX = Mathf.CeilToInt(this.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(this.height / 8.0f);
        computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Convert RenderTexture to Sprite
        Texture2D texture2D = new Texture2D(this.width, this.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, this.width, this.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;

        // Assign the texture to a sprite
        this.spriteRenderer.sprite = Sprite.Create(texture2D, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    void OnDestroy()
    {
        this.renderTexture.Release();
    }

    private void Initialize(){
        this.sqrtN = (int)Math.Sqrt((double)this.N);
        this.FillLookupTables();

        eigenValues = new float[this.N];
        this.FillEigenValues();

        velocityCoeff = new Vector2[this.N,this.width,this.height];
        this.PrecomputeVelocityCoeffs();        


        sturctCoeffMatrixes = new float[this.N,this.N,this.N];
        //TODO precompute sturctCoeffMatrix

    }

    private void PrecomputeVelocityCoeffs()
    {
        Vector2 vel = new();
        for(int i = 0; i< this.width; i++){
            for(int j = 0; j< this.height;j++){
                for(int k = 1; k <= this.N; k++){
                    Vector2 k1k2 = waveNumberLookup[k];
                    
                    float x = i*((float)Math.PI/this.width);
                    float y = j*((float)Math.PI/this.height);

                    float denominator = 1.0f/(k1k2.x*k1k2.x + k1k2.y*k1k2.y);
                    vel.x = denominator *  (k1k2.y * MathF.Sin(k1k2.x * x) * MathF.Cos(k1k2.y * y));
                    vel.y = denominator *  -1.0f * (k1k2.x * MathF.Cos(k1k2.x * x) * MathF.Sin(k1k2.y * y));
                    
                    velocityCoeff[k-1,i,j] = vel;
                }
            }
        }
    }

    private void FillLookupTables(){
        int k = 1;
        for(int i = 1; i<=this.sqrtN; i++){
            for(int j = 1; j<=this.sqrtN; j++){
                Vector2 k1k2 = new Vector2(i, j);
                reverseWaveNumberLookup.Add(k1k2, k);
                waveNumberLookup.Add(k, k1k2);
                k++;
            }
        }
    }

    private void FillEigenValues(){
        for(int k = 1; k <= this.N; ){
            Vector2 k1k2 = waveNumberLookup[k];
            eigenValues[k-1] = -1.0f* (k1k2.x*k1k2.x + k1k2.y*k1k2.y);
        }
    }

    private void InitializeTexture(){
        this.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        this.width = Screen.width;
        this.height = Screen.height;

        // Create a RenderTexture for compute shader
        renderTexture = new RenderTexture(this.width, this.height, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }
}
