using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIController : MonoBehaviour
{
    public Button saveButton, loadButton, incrementButton, decrementButton;
    public IntegerField integerValue;
    public ProjectData data;
    
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        loadButton = root.Q<Button>("Load");
        saveButton = root.Q<Button>("Save");
        incrementButton = root.Q<Button>("Increment");
        decrementButton = root.Q<Button>("Decrement");
        
        integerValue = root.Q<IntegerField>("IntValue");
        
        loadButton.clicked += OnLoadClicked;
        saveButton.clicked += OnSaveClicked;
        
        incrementButton.clicked += OnIncrementValue;
        decrementButton.clicked += OnDecrementValue;
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
