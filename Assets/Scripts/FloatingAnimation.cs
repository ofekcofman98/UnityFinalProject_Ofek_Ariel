using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [SerializeField] private float amplitude = 15f;  // How high it floats
    [SerializeField] private float frequency = 2f;     // How fast it floats

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition; // Use localPosition if marker is child of object
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, offset, 0f);
    }
}
