using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class proceduralMap : MonoBehaviour {

	private manager manager;

	[Header("Config Blocos Cenario")] 
	public int tamanhoBloco;
	public GameObject[] blocoPrefab;
	public int[] ocupaBloco;
	public bool[] temDecoracao;
	public bool[] temColetavel;
	public int idBlocoAgua;
	public bool spawnAguaEsquerda;
	public GameObject blocoChegada;
	public GameObject blocoLimitador;
	public GameObject spawnPrefab;
	public int linhasCena;

	[Header("Config Mapa")]
	public int blocosLinha;  // sempre use numero par
	public int qtdLinhas;
	public int qtdLinhasIncioFim;
	public int qtdLimitadores;

	[Header("Progressao da fase")] 
	public int addBlocoLinha;
	public int blocoMaxLinha;
	public int addLinha;
	public int linhasMax;

	[Header("Blocos Decoracao")]
	public GameObject[] decorecaoPrefab;
	public GameObject[] coletavaPrefab;
	public string[] nomeColetavel;

	public int chanceDecoracao;
	public int prioridadeColetavel;
	public int chanceColetavel;

	[Header("Objetos Spawn")]
	public GameObject[] bloco1;
	public GameObject[] bloco2;
	public GameObject[] bloco3;
	public GameObject[] bloco4;

	[Header("Hierarquia Mapa")] 
	public Transform blocosJogaveis;
	public Transform blocosLimitadores;
	public Transform blocosDecoracao;
	public Transform blocosColetaveis;
	public Transform blocoSpawn;

	// Use this for initialization
	void Start(){
		
		manager = FindObjectOfType(typeof(manager)) as manager;

		blocosLinha += (addBlocoLinha * PlayerPrefs.GetInt("faseCompleta"));   // nao deixar mapa maior que o esperado
		if(blocosLinha > blocoMaxLinha){
			blocosLinha = blocoMaxLinha;
			Debug.Log("blocosLinha : " + blocosLinha);
		}

		qtdLinhas += (addLinha * PlayerPrefs.GetInt("faseCompleta"));
		if(qtdLinhas > linhasMax){
			qtdLinhas = linhasMax;
			Debug.Log("qtdLinhas : " + qtdLinhas);
		}
		
		montarMapa();
	}

	public void montarMapa(){
		
		int idBloco = 0;  // valor inicial zero
		int meio = blocosLinha / 2;
		float posXinicial = (meio * tamanhoBloco) * -1;
		float posZinicial = (qtdLimitadores * tamanhoBloco) * -1;
		
		// Gera linha nao jogavel no inicio da fase 
		for (int linha = 0; linha < qtdLimitadores; linha++){
			gerarLinhaInicialFinal(blocoPrefab[idBloco], meio, linha, posXinicial - (tamanhoBloco * qtdLimitadores), posZinicial);
		}

		//gera as linhas inicias do mapa
		for(int linha = 0; linha < qtdLinhasIncioFim; linha++){

			bool dec = false;
			
			if(linha == 0){
				dec = false;
			}else{
				dec = true;
			}
			
			gerarLinha(blocoPrefab[idBloco], meio, posXinicial, ocupaBloco[idBloco], dec, dec, !temDecoracao[idBloco], idBloco);
		}
		
		//gera linhas jogaveis no mapa en si
		for(int linha = 0; linha < qtdLinhas; linha++){
			idBloco = Random.Range(0, blocoPrefab.Length);
			gerarLinha(blocoPrefab[idBloco], meio, posXinicial, ocupaBloco[idBloco], temDecoracao[idBloco],
							temColetavel[idBloco], !temDecoracao[idBloco], idBloco);
		}

		//gera linhas finais chegada do mapa
		for(int linha = 0; linha < qtdLinhasIncioFim; linha++){
			gerarLinha(blocoChegada, meio, posXinicial, 1, false, false, false, idBloco);
		}
		
		// Gera linha nao jogavel no final da fase
		idBloco = 0;
		for (int linha = 0; linha < qtdLimitadores; linha++){
			gerarLinhaInicialFinal(blocoPrefab[idBloco], meio, linha, posXinicial - (tamanhoBloco * qtdLimitadores), linhasCena * tamanhoBloco);
		}

	}

	public void gerarLinha(GameObject blocoPrefab, int meio, float posXinicial, int ocupaBloco, bool decoravel, bool coletavel, bool spawn, int idBloco){
		
		Vector3 posicaoBloco = Vector3.zero;

		for(int blocoAtual = 0; blocoAtual < qtdLimitadores; blocoAtual++){ // gerar blocos limite lado esquerdo

			float psX = (posXinicial - (qtdLimitadores * tamanhoBloco)) + tamanhoBloco * blocoAtual;
			float psY = blocoPrefab.transform.position.y;
			float psZ = blocoPrefab.transform.position.z + (tamanhoBloco * linhasCena);

			posicaoBloco = new Vector3(psX, psY, psZ);

			Instantiate(blocoPrefab, posicaoBloco, blocoPrefab.transform.rotation, blocosLimitadores);

			for(int i = 0; i < ocupaBloco; i++)
			{
				Instantiate(blocoLimitador, new Vector3(psX, blocoLimitador.transform.position.y, (tamanhoBloco * linhasCena) + tamanhoBloco * i), 
															blocoPrefab.transform.rotation, blocosLimitadores);
			}
			
		}
		
		for(int blocoAtual = 0; blocoAtual <= blocosLinha; blocoAtual++){ // blocos jogaveis
			
			posicaoBloco = new Vector3(posXinicial + (tamanhoBloco * blocoAtual), blocoPrefab.transform.position.y, 
										blocoPrefab.transform.position.z + (tamanhoBloco * linhasCena));
			
			Instantiate(blocoPrefab, posicaoBloco, blocoPrefab.transform.rotation, blocosJogaveis);

			if(decoravel && coletavel){
				
				int rand = Random.Range(0,100);

				if(rand < prioridadeColetavel){
					inserirColetavel(posicaoBloco, ocupaBloco);
				}else{
					inserirDecoracao(posicaoBloco);
				}
				
			}else if(!decoravel && coletavel){
				inserirColetavel(posicaoBloco, ocupaBloco);
			}
			
		}

		if(spawn){
			SpawnLinha(meio, idBloco, ocupaBloco);
		}
		
		
		for(int blocoAtual = 0; blocoAtual < qtdLimitadores; blocoAtual++){ // gerar blocos limite lado direito

			float psX = (posXinicial + (blocosLinha + 1) * tamanhoBloco) + tamanhoBloco * blocoAtual;
			float psY = blocoPrefab.transform.position.y;
			float psZ = blocoPrefab.transform.position.z + (tamanhoBloco * linhasCena);

			posicaoBloco = new Vector3(psX, psY, psZ);

			Instantiate(blocoPrefab, posicaoBloco, blocoPrefab.transform.rotation, blocosLimitadores);

			for(int i = 0; i < ocupaBloco; i++){
				Instantiate(blocoLimitador, new Vector3(psX, blocoLimitador.transform.position.y, (tamanhoBloco * linhasCena) + tamanhoBloco * i), 
					blocoPrefab.transform.rotation, blocosLimitadores);
			}
			
		}

		linhasCena += ocupaBloco;
	}

	void gerarLinhaInicialFinal(GameObject blocoPrefab, int maio, int linhaAtual, float posXinical, float posZinical){
		
		Vector3 posicaoBloco = Vector3.zero;

		for (int blocoAtual = 0; blocoAtual <= (blocosLinha + (qtdLimitadores * 2)); blocoAtual++){
			
			posicaoBloco = new Vector3(posXinical + (tamanhoBloco * blocoAtual), blocoPrefab.transform.position.y, posZinical + (tamanhoBloco * linhaAtual));
			
			Instantiate(blocoPrefab, posicaoBloco, blocoPrefab.transform.rotation, blocosLimitadores);
			Instantiate(blocoLimitador, new Vector3(posicaoBloco.x, blocoLimitador.transform.position.y, posicaoBloco.z), 
														blocoLimitador.transform.rotation, blocosLimitadores);
		}
		
	}

	public void inserirDecoracao(Vector3 posicaoBloco){
		
		int rand = Random.Range(0, 100);

		if(rand <= chanceDecoracao){
			int idDec = Random.Range(0, decorecaoPrefab.Length);
			Instantiate(decorecaoPrefab[idDec], new Vector3(posicaoBloco.x, posicaoBloco.y + tamanhoBloco, posicaoBloco.z), 
				decorecaoPrefab[idDec].transform.rotation, blocosDecoracao);
		}
	}

	void inserirColetavel(Vector3 posicaoBloco, int ocupaBloco){

		for(int i = 0; i < ocupaBloco; i++){
			
			int rand = Random.Range(0, 100);

			if (rand <= chanceColetavel){

				int idCol = Random.Range(0, coletavaPrefab.Length);

				Instantiate(coletavaPrefab[idCol], new Vector3(posicaoBloco.x, posicaoBloco.y + tamanhoBloco, posicaoBloco.z + 
			                               (tamanhoBloco * 1)), coletavaPrefab[idCol].transform.rotation, blocosColetaveis);

				switch(coletavaPrefab[idCol].tag){
					
					case "moeda":
						manager.moedasMapa += 1;
					break;
					
					case "tomate":
						manager.tomatesMapa += 1;
					break;
				}
				
			}
		}
		
	}

	void SpawnLinha(int meio, int idBloco, int ocupaBlocos){
		
		Vector3 posicaoBloco = Vector3.zero;
		
		float psX = 0;
		float psY = 0;
		float psZ = 0;
		
		bool esquerda;
		int rand = Random.Range(0, 100);

		switch(ocupaBlocos){
			
			case 1:  // estrada simples e agua
				
				psX = (meio + qtdLimitadores) * tamanhoBloco;
				psY = tamanhoBloco;
				psZ = tamanhoBloco * linhasCena;

				if(idBloco != idBlocoAgua){
					
					if(rand < 50){
						esquerda = true;
						psX *= -1;
					}else{
						esquerda = false;
					}
					
				}else{   // se for o bloco de agua
					if(spawnAguaEsquerda){
						esquerda = true;
						psX *= -1;
					}else{
						esquerda = false;
					}
				}
				
				Debug.Log(spawnAguaEsquerda);
				spawnAguaEsquerda = !spawnAguaEsquerda;
				posicaoBloco = new Vector3(psX, psY, psZ);
				inserirSpawn(posicaoBloco, esquerda, idBloco);
				
			break;
			
			case 2: // estrada dupla
				
				psX = ((meio + qtdLimitadores) * tamanhoBloco) * -1;
				psY = tamanhoBloco;
				psZ = tamanhoBloco * linhasCena;
				posicaoBloco = new Vector3(psX, psY, psZ);
				
				inserirSpawn(posicaoBloco, true, idBloco);
				
				// =============================================== // 

				psX = ((meio + qtdLimitadores) * tamanhoBloco);
				psY = tamanhoBloco;
				psZ = (tamanhoBloco * linhasCena) + tamanhoBloco;
				posicaoBloco = new Vector3(psX, psY, psZ);
				
				inserirSpawn(posicaoBloco, false, idBloco);
				
			break;
			
			case 3:
				
				psX = (meio + qtdLimitadores) * tamanhoBloco;
				psY = tamanhoBloco;
				psZ = (tamanhoBloco * linhasCena) + tamanhoBloco;
		
				if(rand < 50){
					esquerda = true;
					psX *= -1;
					posicaoBloco = new Vector3(psX, psY, psZ);
				}else{
					posicaoBloco = new Vector3(psX, psY, psZ);
					esquerda = false;
				}
		
				inserirSpawn(posicaoBloco, esquerda, idBloco);
				
			break;
			
		}
	}

	void inserirSpawn(Vector3 posicaoBloco, bool esquerda, int idBloco){
		
		// Inserir informacoes no objeto spawn

		SpawnController spawnController;
		GameObject spw = Instantiate(spawnPrefab, posicaoBloco, spawnPrefab.transform.rotation, blocoSpawn);
		spawnController = spw.GetComponent<SpawnController>();
		spawnController.esquerda = esquerda;

		int nCarrosMax = 0;
		int minSpeed = 0;
		int maxSpeed = 0;
		float delayEntreCarros = 0;
		float delayEntreSpawn = 0;
		
		switch(idBloco) {

			case 1:
				foreach (GameObject obj in bloco1){
					
					spawnController.veiculos.Add(obj);

					if(PlayerPrefs.GetInt("faseCompleta") >= 75){
						nCarrosMax = 4;
						minSpeed = 45;
						maxSpeed = 70;
						delayEntreCarros = 2f;
						delayEntreSpawn = 5;
					}else if (PlayerPrefs.GetInt("faseCompleta") >= 50){
						nCarrosMax = 3;
						minSpeed = 35;
						maxSpeed = 50;
						delayEntreCarros = 2f;
						delayEntreSpawn = 6;
					}else if (PlayerPrefs.GetInt("faseCompleta") >= 25){
						nCarrosMax = 2;
						minSpeed = 25;
						maxSpeed = 40;
						delayEntreCarros = 2f;
						delayEntreSpawn = 7;
					}else {
						nCarrosMax = 1;
						minSpeed = 20;
						maxSpeed = 35; 
						delayEntreCarros = 0;
						delayEntreSpawn = 7;
					}
					
					spawnController.nCarrosMax = nCarrosMax;
					spawnController.minSpeed = minSpeed;
					spawnController.maxSpeed = maxSpeed;
					spawnController.delayEntreCarros = delayEntreCarros;
					spawnController.delayEntreSpawn = delayEntreSpawn;
				}
			break;
			
			case 2:
				foreach (GameObject obj in bloco2){
					
					spawnController.veiculos.Add(obj);
					
					if(PlayerPrefs.GetInt("faseCompleta") >= 75){
						nCarrosMax = 5;
						minSpeed = 60;
						maxSpeed = 80;
						delayEntreCarros = 1f;
						delayEntreSpawn = 3;
					}else if(PlayerPrefs.GetInt("faseCompleta") >= 50){
						nCarrosMax = 4;
						minSpeed = 45;
						maxSpeed = 60;
						delayEntreCarros = 2f;
						delayEntreSpawn = 6;
					}else if(PlayerPrefs.GetInt("faseCompleta") >= 25){
						nCarrosMax = 3;
						minSpeed = 45;
						maxSpeed = 50;
						delayEntreCarros = 3.5f;
						delayEntreSpawn = 7;
					}else{
						nCarrosMax = 2;
						minSpeed = 35;
						maxSpeed = 50; 
						delayEntreCarros = 2f;
						delayEntreSpawn = 7;
					}
					
					spawnController.nCarrosMax = nCarrosMax;
					spawnController.minSpeed = minSpeed;
					spawnController.maxSpeed = maxSpeed;
					spawnController.delayEntreCarros = delayEntreCarros;
					spawnController.delayEntreSpawn = delayEntreSpawn;
				}
			break;
			
			case 3:
				foreach (GameObject obj in bloco3){
					spawnController.veiculos.Add(obj);
					spawnController.nCarrosMax = 1;
					spawnController.minSpeed = 100;
					spawnController.maxSpeed = 200;
					spawnController.delayEntreCarros = 1;
					spawnController.delayEntreSpawn = 7;
				}
			break;
			
			case 4:
				foreach (GameObject obj in bloco4){
					spawnController.veiculos.Add(obj);
					spawnController.nCarrosMax = 3;
					spawnController.minSpeed = 25;
					spawnController.maxSpeed = 40;
					spawnController.delayEntreCarros = 2;
					spawnController.delayEntreSpawn = 5;
				}
			break;	
			
		}
		
	}

}