using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isInvisible : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnBecameInvisible(){  // metado detectar quando objeto sai do campo de visao da camera
		Destroy(transform.parent.gameObject);
	}
}