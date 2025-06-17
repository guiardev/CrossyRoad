using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState {
	TITULO, GAMEPLAY, GAMEOVER, FASECONCLUIDA
}

public class manager : MonoBehaviour{

	private PlayerController playerController;

	public GameState currentState;

	[SerializeField] private GameObject hudInfoGamePlay, hudTitulo, hudGameOver, hudLoading, hudLoja, hudCompleto;
	[SerializeField] private Text moedasTxt, tempoTxt;
	public Text moedasMapaTxt, tomatesMapaTxt, FaseTxt, precoPersonagemTxt;
	public int moedas, tempo, fase, moedasMapa, tomatesMapa, moedasColetadas, tomatesColetados, precoPersonagem;

	[Header("Selecao personagem")]
	public Mesh[] skimPersonagem;
	private MeshFilter meshPersonagem;
	
	private int idPersonagem;
	public int[] precoCharacter;
	
	// Use this for initialization
	void Start (){
		
//	    PlayerPrefs.SetInt("faseCompleta", 0);
//		PlayerPrefs.DeleteAll();
		
		PlayerPrefs.SetInt("Personagem0", 1);
		
		moedas = PlayerPrefs.GetInt("moedas");
		moedasTxt.text = moedas.ToString();

		playerController = Object.FindFirstObjectByType(typeof(PlayerController)) as PlayerController;
		
		fase += PlayerPrefs.GetInt("faseCompleta");
		FaseTxt.text = "Fase atual: " + fase.ToString();
		
		meshPersonagem = playerController.GetComponentInChildren<MeshFilter>();
		idPersonagem = PlayerPrefs.GetInt("idPersonagemAtual");
		meshPersonagem.mesh = skimPersonagem[idPersonagem];
		precoPersonagemTxt.text = precoCharacter[idPersonagem].ToString();
	
		switch (currentState){
			
			case GameState.TITULO:
				hudInfoGamePlay.SetActive(false);
				hudTitulo.SetActive(true);
				hudLoading.SetActive(false);
				hudLoja.SetActive(false);
				hudCompleto.SetActive(false);

			break;
			
			case GameState.GAMEPLAY:
				StartCoroutine("contagemRegressiva");
				
				hudInfoGamePlay.SetActive(true);
				hudTitulo.SetActive(false);
				hudLoading.SetActive(false);
				hudLoja.SetActive(false);
				hudCompleto.SetActive(false);

				break;
		}
	}
	
	// Update is called once per frame
	void Update(){

		if(currentState == GameState.TITULO){
			
			if(Input.GetKeyDown(KeyCode.Return)){
					
				if(PlayerPrefs.GetInt("Personagem" + idPersonagem.ToString()) == 1){
					hudLoading.SetActive(true);
					SceneManager.LoadScene("GamePlay");
				}else{
					
					if(moedas >= precoCharacter[idPersonagem]){
						
						PlayerPrefs.SetInt("Personagem" + idPersonagem.ToString(), 1);
						moedas -= precoCharacter[idPersonagem];
						
						PlayerPrefs.SetInt("moedas", moedas);
						PlayerPrefs.SetInt("idPersonagemAtual", idPersonagem);
						
						hudLoading.SetActive(true);
						SceneManager.LoadScene("GamePlay");
					}
				}
			}

			if(Input.GetKeyDown(KeyCode.RightArrow)){
				selecionarPersonagem(1);
			}else if(Input.GetKeyDown(KeyCode.LeftArrow)){
				selecionarPersonagem(-1);
			}
			
		}else if(currentState == GameState.GAMEPLAY){
			moedasMapaTxt.text = "MOEDAS NO MAPA: " + moedasMapa.ToString();
			tomatesMapaTxt.text = "TOMATES NO MAPA: " + tomatesMapa.ToString();
		}
		
	}

	IEnumerator contagemRegressiva(){
		
		tempoTxt.text = tempo.ToString();
		yield return new WaitForSeconds(1);
		tempo -= 1;

		if (tempo == 0){
			playerController.gotHit();
		}

		if(currentState == GameState.GAMEPLAY){
			StartCoroutine("contagemRegressiva");
		}
	}

	public void atualizarMoedas(int valor){
		moedas += valor;
		moedasMapa -= 1;
		moedasTxt.text = moedas.ToString();
		PlayerPrefs.SetInt("moedas", moedas);
		moedasColetadas += 1;
	}
	
	public void atualizarTempo(int valor){
		tempo += valor;
		tomatesMapa -= 1;
		tempoTxt.text = tempo.ToString();
		tomatesColetados += 1;
	}

	public void GameOver(){
		currentState = GameState.GAMEOVER;
		hudInfoGamePlay.SetActive(false);
		hudGameOver.SetActive(true);
	}

	public void jogarNovamente() {
		hudLoading.SetActive(true);
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
	}

	public void faseConcluida(){

		if(currentState == GameState.GAMEPLAY){
			PlayerPrefs.SetInt("faseCompleta", PlayerPrefs.GetInt("faseCompleta") + 1);
			Debug.Log("faseCompleta");
			hudInfoGamePlay.SetActive(false);
			hudCompleto.SetActive(true);
			currentState = GameState.FASECONCLUIDA;
		}
	}
	
	public void voltarTitulo(){
		hudLoading.SetActive(true);
		SceneManager.LoadScene("Title");
	}

	public void exit(){
		Application.Quit();
	}

	void selecionarPersonagem(int i){
		
		idPersonagem += i;

		if(idPersonagem >= skimPersonagem.Length){
			idPersonagem = 0;
		}else if(idPersonagem < 0){
			idPersonagem = skimPersonagem.Length - 1;
		}

		meshPersonagem.mesh = skimPersonagem[idPersonagem];

		precoPersonagemTxt.text = precoCharacter[idPersonagem].ToString();

		// Verificar se temos o personagem para exibir ou nao o hud loja
		if(PlayerPrefs.GetInt("Personagem" + idPersonagem.ToString()) == 0){
			hudLoja.SetActive(true);
		}else{
			hudLoja.SetActive(false);
			PlayerPrefs.SetInt("idPersonagemAtual", idPersonagem);
		}
	}
	
}