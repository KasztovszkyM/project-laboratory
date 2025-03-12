using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fluid{
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

        private readonly Boolean randomInit = false;
        private Vector2[,,] eigenFunctions; //--
        private float[] eigenValues; //--
        private float[] coeffs;
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

            eigenFunctions = new Vector2[this.N,this.width,this.height];
            this.PrecomputeEigenFunctions();        

            coeffs = new float[this.N];
            this.FillCoeffVector();

            sturctCoeffMatrixes = new float[this.N,this.N,this.N];
            this.PrecomputeStructCoeffMatrix();

        }

        private void PrecomputeStructCoeffMatrix()
        {
            for(int i = 1; i<= this.N; i++){
                for(int j = 1; j<= this.N; j++){
                    Vector2 i1i2 = waveNumberLookup[i];
                    Vector2 j1j2 = waveNumberLookup[j];
                    float iEigenValueInv = 1.0f/eigenValues[i-1];
                    float jEigenValueInv = 1.0f/eigenValues[j-1];

                    ProcessLookup(i1i2, j1j2, i, j, iEigenValueInv, jEigenValueInv);
                }
            }
        }

        private void ProcessLookup(Vector2 i1i2, Vector2 j1j2, int i, int j, float iEigenValueInv, float jEigenValueInv)
        {
            Vector2 lookupIndex;
            
            lookupIndex = new Vector2(i1i2.x + j1j2.x, i1i2.y + j1j2.y); 
            int index1 = MatrixIndex(lookupIndex);

            lookupIndex = new Vector2(i1i2.x + j1j2.x, i1i2.y - j1j2.y);
            int index2 = MatrixIndex(lookupIndex);

            lookupIndex = new Vector2(i1i2.x - j1j2.x, i1i2.y + j1j2.y);
            int index3 = MatrixIndex(lookupIndex);

            lookupIndex = new Vector2(i1i2.x - j1j2.x, i1i2.y - j1j2.y);
            int index4 = MatrixIndex(lookupIndex);

            float coeffValue1 = i1i2.x*j1j2.y - i1i2.y*j1j2.x;
            float coeffValue2 = i1i2.x*j1j2.y + i1i2.y*j1j2.x;

            UpdateStructCoeffMatrix(index1, i, j, coeffValue1, iEigenValueInv, jEigenValueInv, 0.25f);
            UpdateStructCoeffMatrix(index2, i, j, coeffValue2, iEigenValueInv, jEigenValueInv, -0.25f);
            UpdateStructCoeffMatrix(index3, i, j, coeffValue2, iEigenValueInv, jEigenValueInv, 0.25f);
            UpdateStructCoeffMatrix(index4, i, j, coeffValue1, iEigenValueInv, jEigenValueInv, -0.25f);
        }

        private void UpdateStructCoeffMatrix(int index, int i, int j, float coeffValue, float iEigenValueInv, float jEigenValueInv, float factor)
        {
            if(index != -1 && index <= this.N){
                sturctCoeffMatrixes[index,i-1,j-1] = factor * coeffValue * iEigenValueInv;
                sturctCoeffMatrixes[index,j-1,i-1] = -factor * coeffValue * jEigenValueInv;
            }
        }

        private int MatrixIndex(Vector2 lookupIndex){
            if(reverseWaveNumberLookup.ContainsKey(lookupIndex)){
                        return reverseWaveNumberLookup[lookupIndex];
                    }
            return -1;
        }

        private void PrecomputeEigenFunctions()
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
                        
                        eigenFunctions[k-1,i,j] = vel;
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

        public void FillCoeffVector(){
            if(!randomInit){
                for(int i = 0; i<this.N; i++){
                    coeffs[i] = 0.0f;
                }
            }

            else{
                System.Random random = new();
                for(int i = 0; i< this.N; i++){
                    coeffs[i] = (float)random.NextDouble()/this.N;
                }
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
}