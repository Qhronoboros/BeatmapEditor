using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazingTool_2025
{
    [System.Serializable]
    public class TestData_start : IBinarySerializable
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

        public void SerializeToBinary(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(lastName);
            writer.Write(someFloat);
            writer.Write(someInt);
            writer.Write(someListOfData.Count);
            foreach (var item in someListOfData)
            {
                writer.Write(item);
            }
        }

        public void DeserializeFromBinary(BinaryReader reader)
        {
            name = reader.ReadString();
            lastName = reader.ReadString();
            someFloat = reader.ReadSingle();
            someInt = reader.ReadInt32();
            someListOfData = new List<int>(reader.ReadInt32());
            for (int i = 0; i < someListOfData.Count; ++i )
            {
                someListOfData[i] = reader.ReadInt32();
            }
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

        XmlSerializer xmlSerializer;
        BinaryWriter bWriter;
        BinaryReader bReader;

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

            xmlSerializer = new XmlSerializer(typeof(TestData_start));
        }

        IEnumerator ChangeChecker() {
            // TODO: Focus Events... apply changes on select/deselect/return/..., etc.
            while( Application.isPlaying ) {
                ApplyChanges();
                yield return new WaitForSeconds(1f);
		    }

            Debug.Log("EXIT");
	    }

	    private void CreateButton_clicked() 
        {
            Debug.Log("CREATE");
            string url = StandaloneFileBrowser.SaveFilePanel("Select File", Application.persistentDataPath, "newFile.txt", "");

            FileStream fStream = null;
            try
            {
                fStream = File.Create(url);

                //BinaryWriter writer = new BinaryWriter(fStream);
                //(myData as IBinarySerializable).SerializeToBinary(writer);

                using(StreamWriter sWriter = new StreamWriter(fStream))
                {
                    string jsonText = JsonUtility.ToJson(myData);
                    sWriter.Write(jsonText);
                }

                // xmlSerializer.Serialize(fStream, myData);
                fStream.Flush();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            //finally
            //{
            if (fStream != null)
                fStream.Close();
            //}
        }

        public void LoadButton_clicked()
        {
            Debug.Log("LOAD");
            string[] url = StandaloneFileBrowser.OpenFilePanel("Select File", Application.persistentDataPath, "*", false);

            FileStream fStream = null;
            try
            {
                fStream = File.OpenRead(url[0]);

                using (StreamReader sReader = new StreamReader(fStream))
                {
                    string jsonText = sReader.ReadToEnd();
                    myData = JsonUtility.FromJson<TestData_start>(jsonText);
                }

                //myData = (TestData_start)xmlSerializer.Deserialize(fStream);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            //finally
            //{
            if (fStream != null)
                fStream.Close();

            UpdateEditorDisplay();
        }

        public void SaveButton_clicked() 
        {
            Debug.Log("SAVE");
            string url = StandaloneFileBrowser.SaveFilePanel("Select File", Application.persistentDataPath, "newFile.txt", "");
        }

	    #region UI stuff

	    private void ApplyChanges() {
            if (myData == null) myData = new TestData_start();

            myData.name = nameField.text;
            myData.someInt = SanitizeInt(intField);
            myData.someFloat = SanitizeFloat(floatField);
        }

        private int SanitizeInt( TextField field ) {
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

        private float SanitizeFloat( TextField field ) {
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

        private void UpdateEditorDisplay() {
            nameField.SetValueWithoutNotify(myData.name);
            intField.SetValueWithoutNotify(myData.someInt.ToString()); 
            floatField.SetValueWithoutNotify(myData.someFloat.ToString());
        }
        #endregion
    }
}