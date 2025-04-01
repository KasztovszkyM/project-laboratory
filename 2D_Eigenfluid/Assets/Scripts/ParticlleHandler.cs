using System;
using Fluid;
using UnityEngine;

public class Particlles : MonoBehaviour
{
    private EigenfluidRenderer renderer;
    private Particle[] particles;
    public int nParticles;
    public Material particleMaterial;
    private ComputeBuffer particleBuffer;
    private int width;
    private int height;


    void Start()
    {
        renderer = GetComponent<EigenfluidRenderer>();
        this.width = renderer.width;
        this.height = renderer.height;

        this.InitParticles();
        particleBuffer = new ComputeBuffer(nParticles, sizeof(float) * 2);
    }

    void Update()
    {
        
    }

    void OnRenderObject()
    {
        particleMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, nParticles);
    }
void OnDestroy()
    {
        if(particleBuffer != null){
            particleBuffer.Release();
        }
    }
    private void InitParticles(){
        //TODO random paritcles
        particles = new Particle[nParticles];
        System.Random random = new();
        for(int i = 0; i<nParticles; i++){
           particles[i] = new Particle(new Vector2((float)random.NextDouble(), (float)random.NextDouble()));
        }
    }
}

public class Particle{
    Vector2 position;
    Vector2 velocity;

    public Particle(Vector2 pos){
        this.position = pos;
    }
    public void RecalculatePos(){
        this.position += velocity;
    }
}