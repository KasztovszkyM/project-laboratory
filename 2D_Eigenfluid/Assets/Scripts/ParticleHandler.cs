using System;
using Fluid;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ParticleHandler : MonoBehaviour
{
    private EigenfluidRenderer Renderer;
    private List<Particle> particles;
    private Vector2[] positions;
    public int nParticles;
    public Material particleMaterial;
    private ComputeBuffer particleBuffer;
    private int width;
    private int height;


    void Start()
    {
        positions = new Vector2[nParticles];
        Renderer = GetComponent<EigenfluidRenderer>();
        this.width = Renderer.width;
        this.height = Renderer.height;

        this.InitParticles();
        particleBuffer = new ComputeBuffer(nParticles, sizeof(float) * 2);
    }

    void Update()
    {
        
        ReAddParticles();   

        for(int i = 0; i < nParticles; i++){
            particles[i].RecalculatePos(CalculateVelocity(particles[i].GetPosition()), Renderer.timeStep);
            if(IsInbounds(particles[i].GetPosition())){
                positions[i].x = particles[i].GetPosition().x/Renderer.width*2.0f - 1.0f;
                positions[i].y = particles[i].GetPosition().y/Renderer.height*2.0f - 1.0f;
            }
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
        particles = new List<Particle>(nParticles);
        for(int i = 0; i<nParticles; i++){
           particles.Add(new Particle(new Vector2(UnityEngine.Random.Range(0.0f, Renderer.width), UnityEngine.Random.Range(0.0f, Renderer.height))));
        }
    }

    private void ReAddParticles(){
        for(int i = particles.Count-1; i >=0; i--){
            if(!IsInbounds(particles[i].GetPosition())){
                particles.RemoveAt(i);
            }
        }

        for(int i = particles.Count; i <nParticles; i++){
            particles.Add(new Particle(new Vector2(UnityEngine.Random.Range(0.0f, Renderer.width), UnityEngine.Random.Range(0.0f, Renderer.height))));
        }
    }

    private Vector2 CalculateVelocity(Vector2 pos){
        Vector2 velocity = new(0.0f, 0.0f);
        //Vector2 gridPosition = new ((pos.x+1.0f)*width/2.0f , (pos.y+1.0f)*height/2.0f);
        int indX =Mathf.Clamp((int)Math.Floor(pos.x),0,Renderer.width);
        int indY = Mathf.Clamp((int)Math.Floor(pos.y),0,Renderer.height);
        if(IsInbounds(pos)){
            for(int k = 0; k < Renderer.N; k++){
                velocity += Renderer.GetCoefs()[k] * Renderer.GetEigenFunctions()[k, indX, indY];
            }
        }
        return velocity;
        
    }

    private bool IsInbounds(Vector2 gridPosition){
        if(gridPosition.x < Renderer.width && gridPosition.y < Renderer.height && gridPosition.x > 1 && gridPosition.y > 1){
            return true;
        }
        return false;
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
        this.position += velocity * 10.0f; //change intuitive number to logical one
    }

    public Vector2 GetPosition(){
        return this.position;
    }
}