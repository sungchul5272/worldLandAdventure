using UnityEngine;

public class RotateEarth : MonoBehaviour
{
    public float rotationSpeed = 5f; 

    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}