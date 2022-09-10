using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSword : MonoBehaviour
{
    Rigidbody rb;
    public float initForce;
    public Vector3 rota;
    public float maxForce;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.AddForce(Vector3.up * initForce, ForceMode.Impulse);
        rb.AddTorque(rota * maxForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        rb.AddTorque(rota * maxForce / 10, ForceMode.Force);
        
    }

    private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Ground")
		{
            rb.constraints = RigidbodyConstraints.FreezeAll;
		}

	}

}
