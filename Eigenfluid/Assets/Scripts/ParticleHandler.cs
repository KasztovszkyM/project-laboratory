using System;
using Fluid;
using UnityEngine;
using System.Collections.Generic;

public class ParticleHandler : MonoBehaviour
{
    public EigenfluidRenderer RendererXY;
    public EigenfluidRenderer RendererXZ;
    public EigenfluidRenderer RendererYZ;
    private List<Particle> particles;
    private Vector3[] positions;
    public int nParticles;
    public Material particleMaterial;
    public float particleRadius;
    public Mesh sphereMesh;
    private List<Matrix4x4> matrices;
    void Start()
    {
        positions = new Vector3[nParticles];
        matrices = new List<Matrix4x4>();
        this.InitParticles();
    }

    void Update()
    {
        ReAddParticles();
        for (int i = 0; i < nParticles; i++)
        {
            particles[i].RecalculatePos(CalculateVelocity3D(particles[i].GetPosition()), RendererXY.timeStep);
            if (IsInbounds(particles[i].GetPosition()))
            {
                positions[i].x = particles[i].GetPosition().x / RendererXY.width * 2.0f - 1.0f;
                positions[i].y = particles[i].GetPosition().y / RendererXY.height * 2.0f - 1.0f;
                positions[i].z = particles[i].GetPosition().z / RendererXY.height * 2.0f - 1.0f;
            }
        }

        matrices.Clear();
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
    private void InitParticles()
    {
        particles = new List<Particle>(nParticles);
        for (int i = 0; i < nParticles; i++)
        {
            particles.Add(new Particle(new Vector3(UnityEngine.Random.Range(0.0f, RendererXY.width), UnityEngine.Random.Range(0.0f, RendererXY.height), UnityEngine.Random.Range(0.0f, RendererXY.height))));
        }
    }

    private void ReAddParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            if (!IsInbounds(particles[i].GetPosition()))
            {
                particles.RemoveAt(i);
            }
        }

        for (int i = particles.Count; i < nParticles; i++)
        {
            particles.Add(new Particle(new Vector3(UnityEngine.Random.Range(0.0f, RendererXY.width), UnityEngine.Random.Range(0.0f, RendererXY.height), UnityEngine.Random.Range(0.0f, RendererXY.height))));
        }
    }
    private Vector3 CalculateVelocity3D(Vector3 pos)
    {
        Vector2 vXY = CalculateVelocity(new Vector2(pos.x, pos.y),0);
        Vector2 vXZ = CalculateVelocity(new Vector2(pos.x, pos.z),1);
        Vector2 vYZ = CalculateVelocity(new Vector2(pos.y, pos.z),2);

        Vector3 velocity3D = Vector3.zero;
        velocity3D += new Vector3(vXY.x, vXY.y, 0f);
        velocity3D += new Vector3(vXZ.x, 0f, vXZ.y);
        velocity3D += new Vector3(0f, vYZ.x, vYZ.y);

        return velocity3D;
    }
    private Vector2 CalculateVelocity(Vector2 pos, int axis)
    {
        Vector2 velocity = Vector2.zero;
        int indX = Mathf.Clamp((int)Math.Floor(pos.x), 0, RendererXY.width);
        int indY = Mathf.Clamp((int)Math.Floor(pos.y), 0, RendererXY.height);
        if (!IsInbounds(new Vector3(pos.x, pos.y, 2))) // 2 is arbitrary, just to not be out of bounds
        {
            return velocity;
        }
        if (axis == 0)
        {
            for (int k = 0; k < RendererXY.N; k++)
            {
                velocity += RendererXY.GetCoefs()[k] * RendererXY.GetEigenFunctions()[k, indX, indY];
            }
        }
        else if (axis == 1)
        {
            for (int k = 0; k < RendererXY.N; k++)
            {
                velocity += RendererXZ.GetCoefs()[k] * RendererXZ.GetEigenFunctions()[k, indX, indY];
            }
        }
        else if (axis == 2)
        {
           for (int k = 0; k < RendererXY.N; k++)
            {
                velocity += RendererYZ.GetCoefs()[k] * RendererYZ.GetEigenFunctions()[k, indX, indY];
            }
        }
        return velocity;
    }
    private bool IsInbounds(Vector3 gridPosition)
    {
        if (
        gridPosition.x < RendererXY.width
        && gridPosition.y < RendererXY.height
        && gridPosition.z < RendererXY.height
        && gridPosition.x > 1
        && gridPosition.y > 1
        && gridPosition.z > 1)
        {
            return true;
        }
        return false;
    }

    public class Particle
    {
        private Vector3 position;
        private Vector3 velocity;

        public Particle(Vector3 pos)
        {
            this.position = pos;
        }
        public void RecalculatePos(Vector3 vel, float dt)
        {
            this.velocity = vel;
            this.position += velocity * 10.0f; //change intuitive number to logical one
        }

        public Vector3 GetPosition()
        {
            return this.position;
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
}