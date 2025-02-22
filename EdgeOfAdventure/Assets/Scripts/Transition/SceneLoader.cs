using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour, ISaveable
{
    [Header("Event listeners")]
    [SerializeField]private SceneLoadEventSO loadEventSO; 
    [SerializeField]private VoidEventSO newGameEvent;
    [SerializeField]private VoidEventSO backToMenuEvent;

    [Header("Broadcast")]
    [SerializeField]private VoidEventSO aftSceneLoadedEvent;
    [SerializeField]private FadeEventSO fadeEvent;
    [SerializeField]private SceneLoadEventSO unloadedSceneEvent;
    
    [Header("Game Scenes")]
    [SerializeField]private GameSceneSO firstLoadscene;
    [SerializeField]private GameSceneSO menuScene;
    private GameSceneSO currentLoadedscene;
    private GameSceneSO sceneToload;

    [Header("Arguments")]
    private string path;
    private Vector3 destPos;
    private bool fadeScreen;
    private bool isLoading;
    [SerializeField]private float fadeDuration;
    [SerializeField]public Transform playerTrans;
    [SerializeField]public Vector3 firstPosition;

    [Header("Component")]
    [SerializeField]private Button NewGameBtn;

    private void Awake() {
        //Addressables.LoadSceneAsync(firstLoadscene.GetRef(), LoadSceneMode.Additive);
        //currentLoadedscene = firstLoadscene;
        //currentLoadedscene.GetRef().LoadSceneAsync(LoadSceneMode.Additive);
        path = Application.persistentDataPath + "/SAVE DATA/data.sav";
        NewGameBtn.onClick.AddListener(NewGame);
    }

    private void Start()
    {
        //NewGame();
        loadEventSO.RaiseLoadRequestEvent(menuScene, firstPosition, true);
    }

    private void OnEnable() {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent; 
        newGameEvent.OnEventRaised += NewGame;
        backToMenuEvent.OnEventRaised += OnBackToMenuEvent;

        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    private void OnDisable() {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent; 
        newGameEvent.OnEventRaised -= NewGame;
        backToMenuEvent.OnEventRaised -= OnBackToMenuEvent;

        ISaveable saveable = this;
        saveable.UnRegisterSaveData();
    }

    private void OnBackToMenuEvent()
    {
        sceneToload = menuScene;
        loadEventSO.RaiseLoadRequestEvent(sceneToload, firstPosition, true);
    }

    private void NewGame()
    {
        sceneToload = firstLoadscene;
        //OnLoadRequestEvent(sceneToload, firstPosition, true);
        loadEventSO.RaiseLoadRequestEvent(sceneToload, firstPosition, true);
        try {
            // Check if file exists with its full path
            if (File.Exists(path)) {
                // If file found, delete it
                File.Delete(path);
                Debug.Log("File deleted.");
            } else Console.WriteLine("File not found");

        } catch (IOException ioExp) {
            Console.WriteLine(ioExp.Message);
        }
    }

    private void OnLoadRequestEvent(GameSceneSO sceneToload, Vector3 destPos, bool fadeScreen)
    {
        if (isLoading)
            return;

        isLoading = true;
        this.sceneToload = sceneToload;
        this.destPos = destPos;
        this.fadeScreen = fadeScreen;
        if (currentLoadedscene != null) {
            StartCoroutine(UnLoadPreviousScene());
        }
        else
        {
            LoadNewScene();
        }
        
        //Debug.Log(sceneToload.GetRef().SubObjectName);
    }

    private IEnumerator UnLoadPreviousScene() 
    {
        if (fadeScreen) 
        {
            fadeEvent.FadeIn(fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);

        unloadedSceneEvent.RaiseLoadRequestEvent(sceneToload, firstPosition, true);
        
        yield return currentLoadedscene.GetRef().UnLoadScene();

        playerTrans.gameObject.SetActive(false);
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        var loadingOption = sceneToload.GetRef().LoadSceneAsync(LoadSceneMode.Additive, true);
        loadingOption.Completed += OnLoadCompleted;
    }

    public void BackToMain() {
        sceneToload = menuScene;
        Debug.Log("Back to Main");
        playerTrans.GetComponent<PlayerController>().SetInputControlToDisable();
        OnLoadRequestEvent(sceneToload, firstPosition, true);
    }

    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
    {
        currentLoadedscene = sceneToload;

        playerTrans.position = destPos;

        playerTrans.gameObject.SetActive(true);
        if (fadeScreen)
        {
            fadeEvent.FadeOut(fadeDuration);
        }

        isLoading = false;
        if (currentLoadedscene.GetSceneType() == SceneType.Location) aftSceneLoadedEvent.RaiseEvent();
    }

    public DataDefination GetDataID()
    {
        return GetComponent<DataDefination>();
    }

    public void GetSaveData(Data data)
    {
        data.SaveGameScene(currentLoadedscene);
    }

    public void LoadData(Data data)
    {
        var playerID = playerTrans.GetComponent<DataDefination>().ID;
        if (data.characterPosDict.ContainsKey(playerID))
        {
            destPos = data.characterPosDict[playerID].ToVector3();
            sceneToload = data.GetSavedScene();

            OnLoadRequestEvent(sceneToload, destPos, true);
        }
    }
}
