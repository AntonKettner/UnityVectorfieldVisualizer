using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class Data
{
    public List<float> x;
    public List<float> y;
    public List<float> z;
    public List<float> x_Rot;
    public List<float> y_Rot;
    public List<float> scalefactor;
}

public class Skyr_data_loader : MonoBehaviour
{
    // The data


    public Data data;

    void Start()
    {
        
    }

    public void LoadData(int fileNumber)
    {
        // string relativePath = $"energy_comp/energy/json_data/spinfield_{fileNumber}.json";
        string relativePath = $"atomistic_FINAL_creation_ROMMING/json_data/spinfield_{fileNumber}.json";
        string dataPath = Path.Combine(Application.streamingAssetsPath, relativePath);

        // Check if the file exists before trying to read it
        if (!File.Exists(dataPath))
        {
            // Debug.Log($"File not found: {dataPath}");
            return;
        }

        // Read the file
        string json = File.ReadAllText(dataPath);

        // Deserialize the JSON
        if (data != null)
        {
            data.x.Clear();
            data.y.Clear();
            data.z.Clear();
            data.x_Rot.Clear();
            data.y_Rot.Clear();
            data.scalefactor.Clear();
        }
        data = JsonUtility.FromJson<Data>(json);

        ReplaceNaNsWithZeros(data.x_Rot);

    }


    private void ReplaceNaNsWithZeros(List<float> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (float.IsNaN(list[i]))
            {
                list[i] = 0f;
            }
        }
    }
}
