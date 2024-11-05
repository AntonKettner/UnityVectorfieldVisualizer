// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;

// [System.Serializable]
// public class Data
// {
//     public List<float> x;
//     public List<float> y;
//     public List<float> z;
//     public List<float> x_Rot;
//     public List<float> y_Rot;
//     public List<float> scalefactor;
// }

// public class Skyr_data_loader : MonoBehaviour
// {
//     // The data
//     public Data data;
//     void Start()
//     {
        
//     }

//     public void LoadData(int fileNumber)
//     {
//         // string relativePath = $"energy_comp/energy/json_data/spinfield_{fileNumber}.json";
//         string relativePath = $"atomistic_FINAL_creation_ROMMING/json_data/spinfield_{fileNumber}.json";
//         string dataPath = Path.Combine(Application.streamingAssetsPath, relativePath);

//         // Check if the file exists before trying to read it
//         if (!File.Exists(dataPath))
//         {
//             // Debug.Log($"File not found: {dataPath}");
//             return;
//         }

//         // Read the file
//         string json = File.ReadAllText(dataPath);

//         // Deserialize the JSON
//         if (data != null)
//         {
//             data.x.Clear();
//             data.y.Clear();
//             data.z.Clear();
//             data.x_Rot.Clear();
//             data.y_Rot.Clear();
//             data.scalefactor.Clear();
//         }
//         data = JsonUtility.FromJson<Data>(json);

//         ReplaceNaNsWithZeros(data.x_Rot);

//     }


//     private void ReplaceNaNsWithZeros(List<float> list)
//     {
//         for (int i = 0; i < list.Count; i++)
//         {
//             if (float.IsNaN(list[i]))
//             {
//                 list[i] = 0f;
//             }
//         }
//     }
// }





using UnityEngine;
using Mono.Data.SqliteClient;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Newtonsoft.Json;

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
    public Data data;

    private string DecompressData(byte[] compressedData)
    {
        byte[] gzipData = new byte[compressedData.Length + 2];
        gzipData[0] = 0x1f;
        gzipData[1] = 0x8b;
        System.Array.Copy(compressedData, 0, gzipData, 2, compressedData.Length);

        using (var compressedStream = new MemoryStream(gzipData))
        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            gzipStream.CopyTo(resultStream);
            return System.Text.Encoding.UTF8.GetString(resultStream.ToArray());
        }
    }

    public void LoadData(int fileNumber)
    {
        string dbPath = Path.Combine(Application.streamingAssetsPath, "x_current", $"spinfield_{fileNumber}.db");

        if (!File.Exists(dbPath))
        {
            Debug.LogError($"Database file not found: {dbPath}");
            return;
        }

        string connection = "URI=file:" + dbPath;
        
        try
        {
            using (IDbConnection dbcon = new SqliteConnection(connection))
            {
                dbcon.Open();
                using (IDbCommand command = dbcon.CreateCommand())
                {
                    // Just get the first (and only) row from this database file
                    command.CommandText = "SELECT * FROM spinfields LIMIT 1";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (data == null)
                            {
                                data = new Data
                                {
                                    x = new List<float>(),
                                    y = new List<float>(),
                                    z = new List<float>(),
                                    x_Rot = new List<float>(),
                                    y_Rot = new List<float>(),
                                    scalefactor = new List<float>()
                                };
                            }
                            else
                            {
                                data.x.Clear();
                                data.y.Clear();
                                data.z.Clear();
                                data.x_Rot.Clear();
                                data.y_Rot.Clear();
                                data.scalefactor.Clear();
                            }

                            data.x = JsonConvert.DeserializeObject<List<float>>(
                                DecompressData((byte[])reader["x_coords"]));
                            data.y = JsonConvert.DeserializeObject<List<float>>(
                                DecompressData((byte[])reader["y_coords"]));
                            data.z = JsonConvert.DeserializeObject<List<float>>(
                                DecompressData((byte[])reader["z_coords"]));
                            data.x_Rot = JsonConvert.DeserializeObject<List<float>>(
                                DecompressData((byte[])reader["x_rot"]));
                            data.y_Rot = JsonConvert.DeserializeObject<List<float>>(
                                DecompressData((byte[])reader["y_rot"]));
                            data.scalefactor = JsonConvert.DeserializeObject<List<float>>(
                                DecompressData((byte[])reader["scalefactor"]));

                            ReplaceNaNsWithZeros(data.x_Rot);
                        }
                        else
                        {
                            Debug.LogError($"No data found in database file {fileNumber}");
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading database {fileNumber}: {e.Message}\n{e.StackTrace}");
        }
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