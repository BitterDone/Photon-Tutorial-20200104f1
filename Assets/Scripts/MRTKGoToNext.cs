using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SceneSystem;
using Microsoft.MixedReality.Toolkit.UI;

/*----------------------------------
     * Code to handle moving from set up scene to set up scene
     * 
     * MUST BE SET UP MANUALLY IN THE INSPECTOR
     * ---------------------------------*/

public class MRTKGoToNext : MonoBehaviour
{

    IMixedRealitySceneSystem sceneSystem;

    public List<SceneLoadInformation> LoaderInformation = new List<SceneLoadInformation>();//holds data for all ways to load scenes

    public void Start()
    {
        Debug.Log("Starting scene Loader set up in " + SceneManager.GetActiveScene().name);

        sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();

        for (int i = 0; i < LoaderInformation.Count; i++)
        {
            SceneLoadInformation lI = LoaderInformation[i];

            if (lI.doAutomaticallly)
            {
                GoToScene(lI.sceneName);
                Destroy(gameObject);
            }
            else
            {
                EnumSwitchSetUp(LoaderInformation[i]);
            }
        }
    }

    private void EnumSwitchSetUp(SceneLoadInformation LI)
    {
        switch (LI.loadMethod)
        {
            case SceneLoadInformation.LoadMethod.Next:
                AddGoToNext(LI);
                break;
            case SceneLoadInformation.LoadMethod.Previous:
                AddGoToPrevious(LI);
                break;
            case SceneLoadInformation.LoadMethod.Direct:
                AddGoToScene(LI);
                break;
            default:
                Debug.LogWarning("No Valid Load Method");
                break;
        }
    }

    public void AddGoToNext(SceneLoadInformation li)
    {
        li.Interactable.OnClick.AddListener(() => GoToNextScene(li.loadSceneMode));
    }

    public void AddGoToPrevious(SceneLoadInformation li)
    {
        li.Interactable.OnClick.AddListener(() => GoToPreviousScene(li.loadSceneMode));
    }

    public void AddGoToScene(SceneLoadInformation li)
    {
        li.Interactable.OnClick.AddListener(() => GoToScene(li.sceneName, li.loadSceneMode));
    }

    async void GoToNextScene(LoadSceneMode loadType = LoadSceneMode.Single)
    {
        if (sceneSystem.NextContentExists)
        {
            await sceneSystem.LoadNextContent(false, loadType);
        }
        else
        {
            Debug.LogWarning("No Next Scene Available");
        }
    }

    async void GoToPreviousScene(LoadSceneMode loadType = LoadSceneMode.Single)
    {
        if (sceneSystem.PrevContentExists)
        {
            await sceneSystem.LoadPrevContent(false, loadType);
        }
        else
        {
            Debug.LogWarning("No Previous Scene Available");
        }
    }

    async void GoToScene(string name, LoadSceneMode loadType = LoadSceneMode.Single)
    {
        await sceneSystem.LoadContent(name, loadType);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
    }
}

[System.Serializable]
public struct SceneLoadInformation
{
    public bool doAutomaticallly;//will run when the scene first loads so there won't be an interactable element
    public Interactable Interactable;
    public enum LoadMethod { Next, Previous, Direct };//ways to move about scenes
    public LoadMethod loadMethod;//select what way to move about scenes
    public LoadSceneMode loadSceneMode;//single load scene methods
    public string sceneName;//only used for direct loading
}
