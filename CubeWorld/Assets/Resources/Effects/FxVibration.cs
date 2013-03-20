using UnityEngine;
using System.Collections;

public class FxVibration : MonoBehaviour 
{
    private Vector3 startPosition;
    public float vibrationRadius = 0.1f;

	void Start () 
    {
        startPosition = gameObject.transform.position;	
	}
	
	void Update () 
    {
        gameObject.transform.position = startPosition + Random.insideUnitSphere * vibrationRadius;
	}
}
