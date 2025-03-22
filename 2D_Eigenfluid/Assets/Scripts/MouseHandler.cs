using System;
using Fluid;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    private Vector3 point;
    private float duration;
    private float startTime = 0.0f;

    private EigenfluidRenderer Renderer;
    private float ScaleX, ScaleY, ScaleZ;

  void Start()
    {
        Renderer = GetComponent<EigenfluidRenderer>();        
        }

    void Update(){
        if (Input.GetMouseButtonDown(0)) 
        {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane spritePlane = new Plane(Vector3.right, transform.position); //vector3.right bc it is along the x axis

                if (spritePlane.Raycast(ray, out float enter))
                {
                    Vector3 worldPoint = ray.GetPoint(enter);
                    Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                    localPoint.x *= -1.0f;

                    localPoint.x = (localPoint.x + 2.0f) *0.25f *(float)Math.PI;
                    localPoint.y = (localPoint.y + 2.0f) *0.25f *(float)Math.PI;

                    this.point = localPoint;
                    Debug.Log(localPoint);
                }
                         
        } 
        if (Input.GetMouseButtonUp(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane spritePlane = new Plane(Vector3.right, transform.position); //vector3.right bc it is along the x axis

                if (spritePlane.Raycast(ray, out float enter))
                {
                    Vector3 worldPoint = ray.GetPoint(enter);
                    Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                    localPoint.x *= -1.0f;

                    localPoint.x = (localPoint.x + 2.0f) *0.25f *(float)Math.PI;
                    localPoint.y = (localPoint.y + 2.0f) *0.25f *(float)Math.PI;

                    Vector3 direction = this.point - localPoint;
                }
            duration = Time.time - startTime;
        }
    }
}
