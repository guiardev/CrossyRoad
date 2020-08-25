using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private manager manager;

	public Animator playerAnimator;

	public float moveDistance;	 // Distancia a ser movida
	public float moveSpeed;      // Vecolidade de Movimento

	public bool isIdle;      // indica se o personagem esta parado  
	public bool isDead; 	 // indica se o personagem esta Morto
	public bool isCanMove;   // indica se pode se mover
	public bool isMoving;	 // indica se o personagem esta se movendo
	public bool jumpStart;	 // indica o inicio do pulo
	public bool isJumping;	 // indica se o personagem esta pulando

	public Vector3 target;   // Armazena o destino do movimento

	[Header("Audios")] 
	public AudioClip audioIdle1;
	public AudioClip audioIdle2;
	public AudioClip audioHop;
	public AudioClip audioHit;
	public AudioClip audioSplash;
	public AudioClip audioCoin;
	private AudioSource tocarSom;

	// Use this for initialization
	void Start (){
		manager = FindObjectOfType(typeof(manager)) as manager;
		tocarSom = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update (){

		// if(isDead){
		// 	return;
		// }

		if(manager.currentState != GameState.GAMEPLAY){
			return;
		}

		AnimatorController();
		conIdle();
		CanMove();
		Moving();
	}

	void conIdle(){  // metodo que aguarda as teclas de movimento enquanto o personagem esta parado

		if(isDead){
			return;
		}

		if(isIdle){
			if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)){

				checkIfCanMove();
				tocarSom.volume = 1;
				tocarSom.PlayOneShot(audioIdle1);
			}
		}
	}

	void checkIfCanMove(){   // raycast para verificar possiveis obstaculos

		RaycastHit hit;
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 5, transform.position.z), transform.forward, out hit, moveDistance);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 5, transform.position.z), transform.forward * moveDistance, Color.red, 2);

		if(hit.collider == null){
			setMove();
		}else if(hit.collider.tag != "collider"){
			setMove();
		}else{
			Debug.Log("tem obstaculo");
		}
	}

	void setMove(){
		isIdle = false;
		isCanMove = true;
		jumpStart = true;
	}

	void CanMove(){

		if(isCanMove){

			if(Input.GetKeyUp(KeyCode.UpArrow)){

				target = new Vector3(transform.position.x, transform.position.y, transform.position.z + moveDistance);

			}else if(Input.GetKeyUp(KeyCode.DownArrow)){

				target = new Vector3(transform.position.x, transform.position.y, transform.position.z - moveDistance);

			}else if(Input.GetKeyUp(KeyCode.LeftArrow)){

				target = new Vector3(transform.position.x - moveDistance, transform.position.y, transform.position.z);

			}else if(Input.GetKeyUp(KeyCode.RightArrow)){

				target = new Vector3(transform.position.x + moveDistance, transform.position.y, transform.position.z);
			}

			if(Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)){

				jumpStart = false;
				isJumping = true;
				isMoving = true;
				isCanMove = false;
				tocarSom.volume = 1;
				tocarSom.PlayOneShot(audioIdle2);
			}
		}
	}

	void Moving(){   // Metodo responsavel pelo movimento

		if(isMoving){

				transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed);

			if(transform.position == target){
				MoveComplete();
			}
		}
	}

	void MoveComplete(){
		isIdle = true;
		isMoving = false;
		isJumping = false;
	}

	public void gotHit(){
       isDead = true;
       tocarSom.PlayOneShot(audioHit);
       playerAnimator.SetBool("Dead", isDead);
       manager.GameOver();
	}
	
	public void gotHitWater(){
		isDead = true;
		tocarSom.PlayOneShot(audioSplash);
		GetComponentInChildren<Renderer>().enabled = false;
		manager.GameOver();
	}

	void AnimatorController(){

		if(!isDead){

			if(Input.GetKeyDown(KeyCode.UpArrow)){
				transform.rotation = Quaternion.Euler(0,0,0);
			}else if(Input.GetKeyDown(KeyCode.RightArrow)){
				transform.rotation = Quaternion.Euler(0,90,0);
			}else if(Input.GetKeyDown(KeyCode.DownArrow)){
				transform.rotation = Quaternion.Euler(0,180,0);
			}else if(Input.GetKeyDown(KeyCode.LeftArrow)){
				transform.rotation = Quaternion.Euler(0,-90,0);
			}
		}

		playerAnimator.SetBool("PreJump", jumpStart);
		playerAnimator.SetBool("Jump", isJumping);
	}

	void OnTriggerEnter(Collider col){

		switch (col.tag){
			
			case "moeda":
				Debug.Log("peguei uma moeda");
				manager.atualizarMoedas(1);
				tocarSom.volume = 0.5f;
				tocarSom.PlayOneShot(audioCoin);
				Destroy(col.gameObject);
			break;

			case "tomate":
				Debug.Log("peguei um tomate");
				manager.atualizarTempo(5);
				tocarSom.volume = 0.5f;
				tocarSom.PlayOneShot(audioCoin);
				Destroy(col.gameObject);
			break;

			case "hit":
				gotHit();
			break;
		}

	}

	private void OnTriggerStay(Collider col){
		
		switch(col.tag){
			
			case "tronco":
				transform.parent = col.transform;
				transform.position = new Vector3(transform.position.x, 19, transform.position.z);
			break;
			
			case "agua":
				if(isIdle && transform.parent == null){
					gotHitWater();
				}
			break;
			
			case "chao":
				if(isIdle){
					transform.position = new Vector3(col.transform.position.x, col.transform.position.y + 20, transform.position.z);
				}
				
			break;

			case "chegada":

				if(isIdle && manager.currentState == GameState.GAMEPLAY){
					manager.faseConcluida();	
				}
				
			break;
		}
	}

	private void OnTriggerExit(Collider col){
		
		if(col.tag == "tronco"){
			transform.parent = null;
		}
	}
}