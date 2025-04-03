using System;
using Fluid;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    private EigenfluidRenderer Renderer;
    private Particle[] particles;
    public int nParticles;
    public Material particleMaterial;
    private ComputeBuffer particleBuffer;
    private int width;
    private int height;


    void Start()
    {
        Renderer = GetComponent<EigenfluidRenderer>();
        this.width = Renderer.width;
        this.height = Renderer.height;

        this.InitParticles();
        particleBuffer = new ComputeBuffer(nParticles, sizeof(float) * 2);
    }

    void Update()
    {
        Vector2[] positions = new Vector2[nParticles];
        for(int i = 0; i < nParticles; i++){
            //Debug.Log(positions[i]);
            particles[i].RecalculatePos(CalculateVelocity(particles[i].GetPosition()), Renderer.timeStep);
            positions[i] = particles[i].GetPosition();
        }
        particleBuffer.SetData(positions);
        particleMaterial.SetBuffer("_ParticlePositions",particleBuffer);
        particleMaterial.SetInt("_nParticles", nParticles);

        particleMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, nParticles);        
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
           particles[i] = new Particle(new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f)));
           Debug.Log(particles[i]);
        }
    }

    private Vector2 CalculateVelocity(Vector2 pos){
        Vector2 velocity = new(0.0f, 0.0f);
        Vector2 gridPosition = new ((pos.x+1.0f)*width/2.0f , (pos.y+1.0f)*height/2.0f);
        if(gridPosition.x<= Renderer.width && gridPosition.y <= Renderer.height && gridPosition.x >= 0 && gridPosition.y >= 0){
            for(int k = 0; k < Renderer.N; k++){
                velocity += Renderer.GetCoefs()[k] * Renderer.GetEigenFunctions()[k, (int)gridPosition.x , (int)gridPosition.y];
            }
        }
        Debug.Log(velocity);
        return velocity;
        
    }
}

public class Particle{
    private Vector2 position;
    private Vector2 velocity;

    public Particle(Vector2 pos){
        this.position = pos;
    }
    public void RecalculatePos(Vector2 vel, float dt){
        this.velocity = vel;
        this.position += velocity * dt;
    }

    public Vector2 GetPosition(){
        return this.position;
    }
}