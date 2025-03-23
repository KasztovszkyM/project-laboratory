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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane spritePlane = new Plane(Vector3.right, transform.position); //vector3.right bc it is along the x axis

            if (spritePlane.Raycast(ray, out float enter))
            {
                Vector3 worldPoint = ray.GetPoint(enter);
                Vector3 mousePos = transform.InverseTransformPoint(worldPoint);
                currPoint = new Vector2(mousePos.x, mousePos.y);
                currPoint.x *= -1.0f;

                currPoint.x = (currPoint.x + 2.0f) *0.25f *(float)Math.PI;
                currPoint.y = (currPoint.y + 2.0f) *0.25f *(float)Math.PI;

                if(lastPoint != Vector2.zero && IsInBounds(lastPoint)){
                    Vector2 force = currPoint - lastPoint;
                    Renderer.ProjectForce(lastPoint, force);
                }
                lastPoint = currPoint;
                //Debug.Log(currPoint);
            }                 
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
