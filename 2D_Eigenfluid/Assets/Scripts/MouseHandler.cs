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
            Vector3 localPoint = new Vector3(worldPoint.x / (Renderer.displayWidth/2.0f), worldPoint.y / (Renderer.displayHeight/2.0f));
            //Debug.Log(localPoint);
            currPoint.x = (localPoint.x +1.0f) *0.5f * Mathf.PI;
            currPoint.y = (localPoint.y +1.0f) *0.5f *Mathf.PI;
            
            if(lastPoint != Vector2.zero && IsInBounds(lastPoint)){
                //Debug.Log("current point: " + currPoint);
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
        if(pos.x < 0.0f || 
           pos.x > (float)Math.PI || 
           pos.y < 0.0f || 
           pos.y > (float)Math.PI)
           { 
            return false; 
            }
        else{
            return true; 
            }
    }

}
