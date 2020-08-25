using System.Security.Cryptography;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnController : MonoBehaviour {

	public List<GameObject> veiculos;
	public bool esquerda;

	public int nCarros, nCarrosMax;
	public float delayEntreCarros;
	public float delayEntreSpawn;

	public float minSpeed, maxSpeed, moveSpeed;

	// Use this for initialization
	void Start (){
		
		nCarros = Random.Range(1, nCarrosMax + 1);
		moveSpeed = Random.Range(minSpeed, maxSpeed) * -1;

		StartCoroutine("spawn");
	}

	IEnumerator spawn(){

		for(int i = 0; i < nCarros; i++){

			int id = Random.Range(0, veiculos.Count);  // faz um sorteio de qual veiculo sera lancado

			float posX = transform.position.x;
			float posY = veiculos[id].transform.position.y;
			float posZ = transform.position.z;
			
			GameObject tempVeiculo = Instantiate(veiculos[id], new Vector3(posX, posY, posZ), transform.rotation);
			tempVeiculo.transform.parent = transform;   // esse codigo faz com que o spawn dos veiculos fica entre os spawn pai deles
			tempVeiculo.GetComponent<Mover>().moveSpeed = moveSpeed;

			if(esquerda){
				tempVeiculo.transform.rotation = Quaternion.Euler(0, 180, 0);
			}

			yield return new WaitForSeconds(delayEntreCarros);
		}

		yield return new WaitForSeconds(delayEntreSpawn);
		
		nCarros = Random.Range(1, nCarrosMax + 1);
		StartCoroutine("spawn");
	}

}