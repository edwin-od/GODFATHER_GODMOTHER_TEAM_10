using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSword : MonoBehaviour
{
    Rigidbody rb;
    public float initForce;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.AddForce(new Vector3(0, initForce), ForceMode.Impulse);
    }

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Ground")
		{
            rb.constraints = RigidbodyConstraints.FreezeAll;
		}

	}

}
