using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fluid{
    public class EigenfluidRenderer : MonoBehaviour
    {
        public ComputeShader computeShader;
        private ComputeBuffer coefBuffer;
        private ComputeBuffer eigenFunctionBuffer;
        private RenderTexture renderTexture;
        private SpriteRenderer spriteRenderer;
        private int width;
        private int height; 

        private readonly int N = 16;
        private int sqrtN; 
        private readonly Dictionary<Vector2, int> reverseWaveNumberLookup = new();
        private readonly Dictionary<int, Vector2> waveNumberLookup = new();

        private readonly bool randomInit = false;
        private Vector2[,,] eigenFunctions; //--
        private float[] eigenValues; //--
        private float[] coefs;
        private float[,,] sturctCoeffMatrices;
        private float density = 0.1f;
        private float timeStep = 0.1f;
        
        void Start()
        {
            this.InitializeTexture();

            this.Initialize();

            // Dispatch Compute Shader
            this.UpdateShader();
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



        private void Initialize(){
            this.sqrtN = (int)Math.Sqrt((double)this.N);
            this.FillLookupTables();

            eigenValues = new float[this.N];
            this.FillEigenValues();

            eigenFunctions = new Vector2[this.N,this.width,this.height];
            this.PrecomputeEigenFunctions();        

            coefs = new float[this.N];
            this.FillCoeffVector();

            sturctCoeffMatrices = new float[this.N,this.N,this.N];
            this.PrecomputeStructCoeffMatrix();

        }

        private void PrecomputeStructCoeffMatrix()
        {
            for(int i = 0; i< this.N; i++){
                for(int j = 0; j< this.N; j++){
                    for(int k = 0; k<this.N; k++){
                        sturctCoeffMatrices[k,i,j] = 0.0f;
                    }
                }
            }


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
            if(index >= 0 && index < this.N){
                sturctCoeffMatrices[index-1,i-1,j-1] = factor * coeffValue * iEigenValueInv;
                sturctCoeffMatrices[index-1,j-1,i-1] = -factor * coeffValue * jEigenValueInv;
            }
        }

        private int MatrixIndex(Vector2 lookupIndex){
            if(reverseWaveNumberLookup.ContainsKey(lookupIndex)){
                        //Debug.Log(reverseWaveNumberLookup[lookupIndex]);
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
                        
                        this.eigenFunctions[k-1,i,j] = vel;
                    }
                }
            }
        }

        private void FillLookupTables(){
            int k = 1;
            for(int i = 1; i<=this.sqrtN; i++){
                for(int j = 1; j<=this.sqrtN; j++){
                    Vector2 k1k2 = new Vector2(i, j);
                    this.reverseWaveNumberLookup.Add(k1k2, k);
                    this.waveNumberLookup.Add(k, k1k2);
                    k++;
                }
            }
        }

        private void FillEigenValues(){
            for(int k = 1; k <= this.N; k++){
                Vector2 k1k2 = this.waveNumberLookup[k];
                this.eigenValues[k-1] = -1.0f* (k1k2.x*k1k2.x + k1k2.y*k1k2.y);
            }
        }

        public void FillCoeffVector(){
            if(!this.randomInit){
                for(int i = 0; i<this.N; i++){
                    this.coefs[i] = 0.0f;
                }
                //coefs[15] = 1.0f;
            }

            else{
                System.Random random = new();
                for(int i = 0; i<this.N; i++){
                    this.coefs[i] = (float)random.NextDouble()/this.N;
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

            coefBuffer = new ComputeBuffer(this.N, sizeof(float));
            eigenFunctionBuffer = new ComputeBuffer(this.N * this.width * this.height, sizeof(float)*2);
        }

        void Update()
        {
            //TODO implement update function
            float initialEnergy = this.CalculateEnergy(); //intial energy
            
            float[] derivedCoefs = new float[this.N];
             for(int k=0; k < this.N; k++){
                derivedCoefs[k] = this.ComputeDerivedCoeff(k); //derivation
            }

            for(int k = 0; k<this.N; k++){
               this.coefs[k] += derivedCoefs[k] * this.timeStep; //explicit euler integration
            }

            float changedEnergy = this.CalculateEnergy(); //enery after time step
            float normFactor = MathF.Sqrt(initialEnergy/changedEnergy);
            for(int k = 0; k<this.N; k++){
                coefs[k] *= normFactor; //renormalize energy
            }

            //TODO implement dissapation and force input

            this.UpdateShader();

            //for sure this can be optimized
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

        public float CalculateEnergy(){
            float result = 0.0f;
            for(int i = 0; i<this.N; i++){
                result += this.coefs[i] * this.coefs[i];
            }
            return result;
        }
        public float ComputeDerivedCoeff(int k){
            float[,] mat = new float[this.N, this.N];
            for (int i = 0; i < this.N; i++) {
                for (int j = 0; j < this.N; j++) {
                    mat[i, j] = this.sturctCoeffMatrices[k, i, j];
                }
            }
            float result = 0.0f;

            if (mat.GetLength(0) != this.N || mat.GetLength(1) != this.N) {
                throw new ArgumentException("matrix invalid size");
            }
            // Compute w^T * C * w
            for (int i = 0; i < this.N; i++) {
                for (int j = 0; j < this.N; j++) {
                    result += this.coefs[i] * mat[i, j] * this.coefs[j];
                }
            }
            return result; 
        }

        private void UpdateShader(){
            computeShader.SetInt("dimN", this.N);
            computeShader.SetInt("width", this.width);
            computeShader.SetInt("height", this.height);

            Vector2[] flattenedFuncitons = this.FlattenArray(eigenFunctions);
            eigenFunctionBuffer.SetData(flattenedFuncitons);
            computeShader.SetBuffer(0, "eigenfunctions", eigenFunctionBuffer);

            coefBuffer.SetData(coefs);
            computeShader.SetBuffer(0, "coefs", coefBuffer);
            computeShader.SetTexture(0, "result", this.renderTexture);
        }
        private Vector2[] FlattenArray(Vector2[,,] array){
            Vector2[] result = new Vector2[this.N * renderTexture.width * renderTexture.height];
            for (int x = 0; x < this.width; x++) {
                for (int y = 0; y < this.height; y++) {
                    for (int n = 0; n < this.N; n++) {
                        int index = n + (x * this.N) + (y * width * this.N);
                        result[index] = eigenFunctions[n, x, y];
                    }
                }
            }
            return result;
        }
        
        void OnDestroy()
        {
             if (renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = null;
            }

            if (coefBuffer != null)
            {
                coefBuffer.Release();
                coefBuffer = null;
            }

            if (eigenFunctionBuffer != null)
            {
                eigenFunctionBuffer.Release();
                eigenFunctionBuffer = null;
            }        
        }

    }
}