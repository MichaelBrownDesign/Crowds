using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleCameraController : MonoBehaviour
{
    [SerializeField] private new Camera     camera;
    [SerializeField] private Transform      pivot;
    [SerializeField] private float          zoomSpeed       = 1;
    [SerializeField] private float          rotationSpeed   = 1;
    [SerializeField] private float          minDist         = 350;

    private void Update()
    {
        var scrollDelta = Input.mouseScrollDelta;
        Vector3 pivotDir = (pivot.position - camera.transform.position).normalized;
        camera.transform.position += pivotDir * scrollDelta.y * zoomSpeed * Time.deltaTime;

        if(Vector3.Distance(camera.transform.position, pivot.position) < minDist)
        {
            camera.transform.position = pivot.position - pivotDir * minDist;
        }

        float horizontal    = Input.GetAxis("Horizontal")   * rotationSpeed * Time.deltaTime;
        float vertical      = Input.GetAxis("Vertical")     * rotationSpeed * Time.deltaTime;

        pivot.rotation *= Quaternion.AngleAxis(horizontal, Vector3.up);
        pivot.rotation *= Quaternion.AngleAxis(vertical, Vector3.right);

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pivot.rotation = Quaternion.Euler(pivot.rotation.eulerAngles + new Vector3(mouseY, mouseX, 0));


            //pivot.rotation *= Quaternion.AngleAxis(mouseY, Vector3.up);//Quaternion.Euler(new Vector3(mouseY, mouseX, 0));
            //pivot.rotation *= Quaternion.AngleAxis(mouseX, Vector3.right);       
        }
    }
    
}
