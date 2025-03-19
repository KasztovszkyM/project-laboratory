using Mono.Cecil.Cil;
using UnityEngine;

public class MouseMotion : MonoBehaviour
{
    private Vector3 point;
    private float duration;
    private GameObject Object;
    private float startTime = 0.0f;

    private  BoxCollider Collider;
    private float ScaleX, ScaleY, ScaleZ;

  void Start()
    {
        //Fetch the Collider from the GameObject
        Collider = GetComponent<BoxCollider>();
        if(Screen.width <= Screen.height){
                this.ScaleX = Screen.width;
                this.ScaleY = Screen.width;
            }
        else{  
            this.ScaleX = Screen.height;
            this.ScaleY = Screen.height;
                }
        ScaleZ = 1.0f;

        Collider.size = new Vector3(ScaleX,ScaleY,ScaleZ);
        }

    void Update(){
    if (Input.GetMouseButtonDown(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit)){
                Debug.Log(hit.point.z + " & " + hit.point.y + "\n"); 
                startTime = Time.time;
                point = hit.point;
                Object = hit.transform.gameObject;
        }
    } 
    if (Input.GetMouseButtonUp(0)){
        duration = Time.time - startTime;
    }
}
}
