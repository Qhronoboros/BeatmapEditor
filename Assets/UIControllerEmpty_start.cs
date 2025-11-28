using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AmazingTool;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using UnityEngine.UIElements;
using SFB;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace AmazingTool
{
    [System.Serializable]
    public class TestData_start
    {
        public string name = "default";

        [OptionalField(VersionAdded = 2)]
        public string lastName = "lastName";

        public float someFloat = 10f;
        public int someInt = 10;
        public List<int> someListOfData = new List<int>();

        public TestData_start()
        {
            Debug.Log("Constructor: "+name);
        }

        [OnSerializing]
        void Serializing(StreamingContext context)
        {
            Debug.Log("OnSerializing");
            Debug.Log("NAME: " + name);
        }

        [OnSerialized]
		void Serialized(StreamingContext context)
		{
            Debug.Log("OnSerialized");
            Debug.Log("NAME: " + name);
        }

		[OnDeserializing]
		void Deserializing(StreamingContext context)
		{
            Debug.Log("OnDeserializing");
            Debug.Log("NAME: " + name);
        }

		[OnDeserialized]
		void Deserialized(StreamingContext context)
		{
            Debug.Log("OnDeserialized");
            Debug.Log("NAME: " + name);
        }
	}

    public class UIControllerEmpty_start : MonoBehaviour
    {
        public TextField fileNameInput;
        public Button createButton, saveButton, loadButton;
        public TestData_start myData;

        // TODO: Move this to a class of some sort, maybe with a generic / interface for applying/updating?
        public TextField nameField, intField, floatField;
        public VisualElement dataEditor;

        BinaryFormatter binaryFormatter;
        XmlSerializer xmlSerializer;
        BinaryWriter binaryWriter;

        // Start is called before the first frame update
        void Start()
        {
            #region UI Init
            var root = GetComponent<UIDocument>().rootVisualElement;

            // file input field
            fileNameInput = root.Q<TextField>("filename");

            // get top level buttons
            createButton = root.Q<Button>("create");
            saveButton = root.Q<Button>("save");
            loadButton = root.Q<Button>("load");

            // get data editor & child name field
            dataEditor = root.Q<IMGUIContainer>("data-editor");
            nameField = dataEditor.Q<TextField>("name");
            intField = dataEditor.Q<TextField>("int");
            floatField = dataEditor.Q<TextField>("float");

            // implement button reactions
            createButton.clicked += CreateButton_clicked;
            saveButton.clicked += SaveButton_clicked;
            loadButton.clicked += LoadButton_clicked;

            StartCoroutine(ChangeChecker());
            #endregion

            binaryFormatter = new BinaryFormatter();
            xmlSerializer = new XmlSerializer(typeof(TestData_start));
        }

        IEnumerator ChangeChecker()
        {
            // TODO: Focus Events...
            while (Application.isPlaying)
            {
                ApplyChanges();
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("EXIT");
        }

        private void CreateButton_clicked()
        {
            string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);

            FileStream fileStream = null;
            try
            {
                fileStream = File.Create(url);

                // write something
                binaryFormatter.Serialize(fileStream, myData);

                // flush
                fileStream.Flush();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message, gameObject);
            }
            finally
            {
                // close
                if (fileStream != null) fileStream.Close();
            }
        }

        public void LoadButton_clicked()
        {
            string[] options = StandaloneFileBrowser.OpenFilePanel("Open File", Application.persistentDataPath, "txt", false);
            if (options.Length == 0) return;

            string url = options[0];

            // string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);

            FileStream fileStream = null;
            try
            {
                // Early return
                // if ( !File.Exists(url) )

                fileStream = File.OpenRead(url);

                // read something
                myData = (TestData_start)binaryFormatter.Deserialize(fileStream);

                UpdateEditorDisplay();
            }
            catch (System.Exception e)
            {
                // Woops achteraf
                Debug.LogError(e.GetType() + " " + e.Message, gameObject);
            }
            finally
            {
                // close
                if (fileStream != null) fileStream.Close();
            }

            Debug.Log(myData.name);
        }

        public void SaveButton_clicked()
        {
            string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);

            FileStream fileStream = null;
            try
            {
                if (File.Exists(url))
                {
                    // TODO: Warn the user
                }

                fileStream = File.Create(url);

                // write something
                binaryFormatter.Serialize(fileStream, myData);

                // flush
                fileStream.Flush();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message, gameObject);
            }
            finally
            {
                // close
                if (fileStream != null) fileStream.Close();
            }
        }

        #region UI stuff

        private void ApplyChanges()
        {
            if (myData == null) myData = new TestData_start();

            myData.name = nameField.text;
            myData.someInt = SanitizeInt(intField);
            myData.someFloat = SanitizeFloat(floatField);
        }

        private int SanitizeInt(TextField field)
        {
            string sanitized;
            int retVal = 0;
            try
            {
                sanitized = Regex.Replace(field.text, @"[^-+0-9]", "");//"[^0-9]"
                retVal = int.Parse(sanitized);
                sanitized = retVal.ToString();
                field.SetValueWithoutNotify(sanitized);
            }
            catch (System.OverflowException e)
            {
                retVal = int.MaxValue;
                sanitized = retVal.ToString();
                field.SetValueWithoutNotify(sanitized);
            }
            catch (System.FormatException e)
            {
                if (field.panel.focusController.focusedElement == field)
                    sanitized = field.text;
                else
                {
                    Debug.LogWarning("Format exception: " + e.Message);
                    sanitized = "0";
                    field.SetValueWithoutNotify(sanitized);
                }
            }
            return retVal;
        }

        private float SanitizeFloat(TextField field)
        {
            string sanitized;
            float retVal = 0;
            try
            {
                sanitized = Regex.Replace(field.text, @"[^-+0-9\.eE]", ""); //"[^-0-9.]"
                retVal = float.Parse(sanitized);
                sanitized = retVal.ToString();
                field.SetValueWithoutNotify(sanitized);
            }
            catch (System.FormatException e)
            {
                if (field.panel.focusController.focusedElement == field)
                    sanitized = field.text;
                else
                {
                    Debug.LogWarning("Format exception: " + e.Message);
                    sanitized = "0";
                    field.SetValueWithoutNotify(sanitized);
                }
            }
            return retVal;
        }

        private void UpdateEditorDisplay()
        {
            nameField.SetValueWithoutNotify(myData.name);
            intField.SetValueWithoutNotify(myData.someInt.ToString());
            floatField.SetValueWithoutNotify(myData.someFloat.ToString());
        }
        #endregion
    }
}