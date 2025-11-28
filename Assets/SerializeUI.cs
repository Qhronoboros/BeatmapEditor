using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using AmazingTool;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class SerializeUI : MonoBehaviour
{
    public TestData data;
    public TestDataEditor dataEditor;
    public TextField fileNameTF;

    ISerializationStrategy<TestData> serializer;

    // Start is called before the first frame update
    void Start()
    {
        // serializer = new BinaryStrategy<TestData>();
        serializer = new XmlStrategy<TestData>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnGUI() {
        
	}

    public void Create() {
        data = new TestData();
        dataEditor.SetData(ref data);
    }

    public void Load() {
        string fileName = fileNameTF.text;
        string url = Path.Combine(Application.persistentDataPath, fileName);

        if ( File.Exists( url ) ) {

            FileStream fstream = null;
            try {
                fstream = new FileStream(url, FileMode.Open);
                data = (TestData)serializer.Deserialize(fstream);
                fstream.Close();

                dataEditor.SetData(ref data);
            }
            catch ( System.Exception e ) { 
                Debug.LogError("Serialization Error: "+e.Message);
            }
		}
    }

    public void Save() {
        string fileName = fileNameTF.text;
        string url = Path.Combine(Application.persistentDataPath, fileName);

        if (data != null ) {
            FileStream fstream = null;
            try {
                fstream = new FileStream(url, FileMode.CreateNew);
                serializer.Serialize(fstream, data);
                fstream.Close();
            }
            catch ( System.Exception e ) { 
                Debug.LogError("Serialization Error: "+e.Message);
            }
        }
    }
}

public interface ISerializationStrategy<T> {
    T Deserialize(Stream stream);
    void Serialize(Stream stream, T data);
}

public class XmlStrategy<T> : ISerializationStrategy<T>
{
    private XmlSerializer formatter;
    
    public XmlStrategy() {
        formatter = new XmlSerializer(typeof(T));
    }

    public T Deserialize(Stream stream) {
        T data = default(T);
        data = (T)formatter.Deserialize(stream);
        return data;
    }

    public void Serialize( Stream stream, T data) {
        formatter.Serialize(stream, data);
    }
}


public class JsonStrategy<T> : ISerializationStrategy<T>
{

    public JsonStrategy()
    {
    }

    public T Deserialize(Stream stream)
    {
        using (StreamReader reader = new StreamReader(stream))
        {
            string json = reader.ReadToEnd();
            return JsonUtility.FromJson<T>(json);
        }
    }

    public void Serialize(Stream stream, T data)
    {
        using (StreamWriter writer = new StreamWriter(stream))
        {
            string json = JsonUtility.ToJson(data);
            writer.Write(json);
            writer.Flush();
            writer.Close();
        }
    }
}

// Alternative: Pure Binary Strategy (for primitive/simple types)
// This approach manually serializes fields - useful for performance-critical scenarios
public class PureBinaryStrategy<T> : ISerializationStrategy<T> where T : IBinarySerializable, new()
{
    public T Deserialize(Stream stream)
    {
        using (BinaryReader reader = new BinaryReader(stream))
        {
            T data = new T();
            data.DeserializeFromBinary(reader);
            return data;
        }
    }

    public void Serialize(Stream stream, T data)
    {
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            data.SerializeToBinary(writer);
        }
    }
}

// Interface for types that can serialize themselves to binary
public interface IBinarySerializable
{
    void SerializeToBinary(BinaryWriter writer);
    void DeserializeFromBinary(BinaryReader reader);
}