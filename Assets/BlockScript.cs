using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    float speed = 5f;

    Rigidbody rb;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = -transform.up * speed;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (transform.parent.GetComponent<BoardScript>().GetCurrentBlock() == this.gameObject)
        {
            Controls();
        }
        else
        {
            Vector3 newPos = transform.localPosition;
            newPos.x = Mathf.Round(newPos.x);
            newPos.y = Mathf.Round(newPos.y);
            transform.localPosition = newPos;
        }
	}

    void Controls()
    {
        if (Input.GetButtonDown("Left"))
        {
            transform.position -= transform.right;
        }

        if (Input.GetButtonDown("Right"))
        {
            transform.position += transform.right;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == gameObject.tag)
        {
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
