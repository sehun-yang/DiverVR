using UnityEngine;

public class DebugCube : MonoBehaviour
{
    private Vector3 initialPosition = Vector3.zero;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        transform.position = initialPosition + 3 * Mathf.Sin(Time.realtimeSinceStartup * 2) * Vector3.one;
    }
}