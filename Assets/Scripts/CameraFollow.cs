using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform objetivo;
    public float suavidad = 5f;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - objetivo.position;
    }

    void LateUpdate()
    {
        Vector3 posicionDeseada = objetivo.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            posicionDeseada,
            suavidad * Time.deltaTime
        );
    }
}