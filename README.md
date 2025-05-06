# project-laboratory
Eigenfluid simulation written in C# using compute and unlit shaders in Unity. BME Project laboratory. 

Simulation is implemented based on this article: **Fluid Simulation Using Laplacian Eigenfunctions**
<sub>by *TYLER DE WITT, CHRISTIAN LESSIG, and EUGENE FIUME*</sub>
https://dl.acm.org/doi/pdf/10.1145/2077341.2077351

## Files:
* 2D_Eigenfluid/Assets/Scripts/EigenfluidRenderer.cs - main simulation logic
* 2D_Eigenfluid/Assets/Scripts/MouseMotion.cs - mouse motion and detection handler
* 2D_Eigenfluid/Assets/Shaders/Eigenfluid.compute - shader logic
* 2D_Eigenfluid/Assets/Scripts/ParticleHandler.cs - particle generation and lifetime management
* 2D_Eigenfluid/Assets/Shaders/Particle.shader - unlit shader for rendering particles as simple dots
