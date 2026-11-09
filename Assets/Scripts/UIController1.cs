using System.Collections;
using System.IO;
using System.Linq;
using SFB;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIControllerTest : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioWaveDisplay audioWaveDisplay;

    public Button saveProjectButton, loadProjectButton, importJSONButton, exportJSONButton, loadMusicButton;
    public Label songTitleLabel;

    public Button saveButton, loadButton, incrementButton, decrementButton;
    public IntegerField integerValue;
    public ProjectData data;

    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement topUI = root.Q<VisualElement>("TopUI");

        saveProjectButton = topUI.Q<Button>("SaveProject");
        loadProjectButton = topUI.Q<Button>("LoadProject");
        importJSONButton = topUI.Q<Button>("ImportJSON");
        exportJSONButton = topUI.Q<Button>("ExportJSON");
        loadMusicButton = topUI.Q<Button>("LoadMusic");
        songTitleLabel = topUI.Q<Label>("SongTitle");

        loadButton = root.Q<Button>("Load");
        saveButton = root.Q<Button>("Save");
        incrementButton = root.Q<Button>("Increment");
        decrementButton = root.Q<Button>("Decrement");
        
        integerValue = root.Q<IntegerField>("IntValue");
        
        loadMusicButton.clicked += OnLoadMusicClicked;
    }

    public void OnLoadMusicClicked()
    {
        Debug.Log("Loading Music");
        string[] url = StandaloneFileBrowser.OpenFilePanel("Select File", Application.persistentDataPath, "*", false);

        string uri = "file://" + url[0];

        StartCoroutine(LoadAndPlay(uri));
    }

IEnumerator LoadAndPlay(string uri)
{
    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
    {
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();

            Debug.Log("Music Loaded");
            songTitleLabel.text = uri.Split('\\').Last();

            audioWaveDisplay.SetDisplay();
        }
    }
}

    public void OnLoadClicked()
    {
        Debug.Log("Loading file");
        string[] url = StandaloneFileBrowser.OpenFilePanel("Select File", Application.persistentDataPath, "*", false);

        FileStream fStream = null;
        try
        {
            fStream = File.OpenRead(url[0]);

            using (StreamReader sReader = new StreamReader(fStream))
            {
                string jsonText = sReader.ReadToEnd();
                data = JsonUtility.FromJson<ProjectData>(jsonText);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        
        if (fStream != null)
            fStream.Close();
            
        InputLoadedData();
    }

    public void OnSaveClicked()
    {
        Debug.Log("Saving file");
        SetData();
        
        string url = StandaloneFileBrowser.SaveFilePanel("Select File", Application.persistentDataPath, "newFile.txt", "");

        FileStream fStream = null;
        try
        {
            fStream = File.Create(url);

            using(StreamWriter sWriter = new StreamWriter(fStream))
            {
                string jsonText = JsonUtility.ToJson(data);
                sWriter.Write(jsonText);
            }

            fStream.Flush();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        
        if (fStream != null)
            fStream.Close();
    }
    
    public void SetData()
    {
        data.value = integerValue.value;
    }
    
    public void InputLoadedData()
    {
        integerValue.value = data.value;
    }
    
    public void OnIncrementValue() => integerValue.value += 1;
    public void OnDecrementValue() => integerValue.value -= 1;
}
