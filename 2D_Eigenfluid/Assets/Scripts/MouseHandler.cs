using System;
using Fluid;
using Unity.Mathematics;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    private Vector2 currPoint;
    private Vector2 lastPoint = Vector2.zero;
    private EigenfluidRenderer Renderer;

  void Start()
    {
        Renderer = GetComponent<EigenfluidRenderer>();        
    }

    void Update(){
        if (Input.GetMouseButton(0)) 
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Renderer.spriteRenderer.transform.position.z - Camera.main.transform.position.z));
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            currPoint.x = (localPoint.x - Renderer.spriteRenderer.bounds.min.x) / (Renderer.spriteRenderer.sprite.rect.width /  Renderer.spriteRenderer.sprite.pixelsPerUnit) * MathF.PI;
            currPoint.y = (localPoint.y - Renderer.spriteRenderer.bounds.min.y) / (Renderer.spriteRenderer.sprite.rect.height / Renderer.spriteRenderer.sprite.pixelsPerUnit) * MathF.PI;
           
            if(lastPoint != Vector2.zero && IsInBounds(lastPoint)){
                Debug.Log("current point: " + currPoint);
                Vector2 force = currPoint - lastPoint;
                Renderer.ProjectForce(lastPoint, force);
            }
            lastPoint = currPoint;
        } 
        if (Input.GetMouseButtonUp(0)){
            lastPoint = Vector2.zero;
        }
    }
    private static bool IsInBounds(Vector2 pos){
        if(pos.x <= 0.0f || 
           pos.x >= (float)Math.PI || 
           pos.y <= 0.0f || 
           pos.y >= (float)Math.PI)
           { 
            return false; 
            }
        else{
            return true; 
            }
    }

}
