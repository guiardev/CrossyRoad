using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour {

	public Transform player;
	public float moveSpeed;

	public Vector3 offSet;
	public float rotX, rotY;
	public float size;
	public bool follow;

	private Vector3 pas;
	private Quaternion rot;
	private float s;
	
	// Use this for initialization
	void Start (){
		transform.position = offSet;
		transform.rotation = Quaternion.Euler(rotX, rotY, 0);
	}
	
	// Update is called once per frame
	void Update () {

		if(follow){
			pas = Vector3.Lerp(transform.position, player.position + offSet, moveSpeed * Time.deltaTime);
			rot = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotX, rotY, 0), moveSpeed * Time.deltaTime);
			s = Mathf.Lerp(Camera.main.orthographicSize, size, moveSpeed * Time.deltaTime);

			transform.position = pas;
			transform.rotation = rot;
			Camera.main.orthographicSize = s;
		}
	}
}
