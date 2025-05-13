using System;
using Fluid;
using UnityEngine;
using System.Collections.Generic;

public class ParticleHandler : MonoBehaviour
{
    private EigenfluidRenderer Renderer;
    private List<Particle> particles;
    private Vector3[] positions;
    public int nParticles;
    public Material particleMaterial;
    private int width;
    private int height;
    public float particleRadius;
    public Mesh sphereMesh;

    void Start()
    {
        positions = new Vector3[nParticles];
        Renderer = GetComponent<EigenfluidRenderer>();
        this.width = Renderer.width;
        this.height = Renderer.height;
        this.InitParticles();
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
        
        List<Matrix4x4> matrices = new List<Matrix4x4>(nParticles);
        for (int i = 0; i < nParticles; i++)
        {
            Matrix4x4 m = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one * particleRadius);
            matrices.Add(m);
        }
        const int batchSize = 1023;
        for (int i = 0; i < nParticles; i += batchSize)
        {
            int len = Mathf.Min(batchSize, nParticles - i);
            Graphics.DrawMeshInstanced(sphereMesh, 0, particleMaterial, matrices.GetRange(i, len));
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for(int i = 0; i < nParticles; i++){
            if(Application.isPlaying){
                Gizmos.DrawSphere(positions[i], particleRadius);
            }       
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

public class Particle{
    private Vector3 position;
    private Vector3 velocity;

    public Particle(Vector3 pos){
        this.position = pos;
    }
    public void RecalculatePos(Vector3 vel, float dt){
        this.velocity = vel;
        this.position += velocity * 10.0f; //change intuitive number to logical one
    }

    public Vector3 GetPosition(){
        return this.position;
    }
}
}