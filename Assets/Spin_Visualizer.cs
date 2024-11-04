using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using Math = System.Math;

public class Spin_Visualizer : MonoBehaviour
{
    // The data loader
    private Skyr_data_loader dataLoader;

    // The prefab for the data points
    public GameObject dataPointPrefab;

    // The current file number
    private int currentFileNumber;

    // The pool of data points
    private List<GameObject> dataPointPool = new List<GameObject>();

    // Pool size - this should be large enough to hold all data points you may need at once.
    private const int poolSize = 20000;

    // The index of the next available data point in the pool
    private int nextDataPointIndex = 0;

    // A flag to check if play is enabled
    private bool play = false;

    // A flag to check if playback is enabled
    private bool playback = false;

    // The file count
    private int fileCount;

    // The animation time
    private float animtime = 1.0f;

    // if system is animating
    private int animationCounter = 0;

    private RecorderController recorderController;

    private string simulation_mode = "atomistic"; //atomistic or continuum

    private bool first = true;


    void Start()
    {
        // Get the data loader
        dataLoader = GetComponent<Skyr_data_loader>();

        // Check if the data loader was found
        if (dataLoader == null)
        {
            // UnityEngine.Debug.LogError("Skyr_data_loader component not found in the Start func.");
            return;
        }

        // set the current file number to the start value (0)
        // currentFileNumber = 0;
        currentFileNumber = 300;

        // string relativePath = $"energy_comp/energy/json_data/";
        string relativePath = $"atomistic_FINAL_creation_ROMMING/json_data/";
        string folderPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        fileCount = Directory.GetFiles(folderPath, "*.json").Length;

        // log the file count
        UnityEngine.Debug.Log("fileCount in start: " + fileCount);

        // Load the default data
        dataLoader.LoadData(currentFileNumber);

        // Visualize the data
        StartCoroutine(VisualizeData());

        // set the Cam to the default position
        transformCam();

        // log the play flag
        UnityEngine.Debug.Log("play: " + play);
    }

    void Update()
    {

        // Right arrow is pressed
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(currentFileNumber < fileCount-2)
            {
                // Increment the file number
                currentFileNumber++;

                // Load the data
                StartCoroutine(loadandvisualize());
            }
            // else
            // {
            //     // log that end has been reached
            //     UnityEngine.Debug.Log("end has been reached");
            // }

            // log the current file number
            UnityEngine.Debug.Log("spinfield: " + (currentFileNumber + 1) + " / " + fileCount);
        }

        // left arrow is pressed
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(currentFileNumber > 0)
            {
                // Increment the file number
                currentFileNumber--;

                // Load the data
                StartCoroutine(loadandvisualize());
            }
            // else
            // {
            //     // log that start has been reached
            //     UnityEngine.Debug.Log("start has been reached");
            // }
            // log the current file number
            UnityEngine.Debug.Log("spinfield: " + (currentFileNumber + 1) + " / " + fileCount);
        }

        // P is pressed --> loop to show next spinfield until end is reached
        if (Input.GetKeyDown(KeyCode.P))
        {
            // change the play flag
            play = !play;
            
            // log the current file number
            UnityEngine.Debug.Log("play after Toggle: " + play);

            // if(!play)
            // {
            //     // execute powershell script
            //     Compress_mp4s();

            //     // // delete old mp4s
            //     // DeleteOldMP4sScript();
            // }
        }

        // Toggle playbackwards mode when key B is pressed
        if (Input.GetKeyDown(KeyCode.B))
        {
            // change the playback flag
            playback = !playback;
            
            // log the current file number
            UnityEngine.Debug.Log("playback after Toggle: " + playback);

        }


        // get to the next data set if loop mode is enabled
        if(play && animationCounter == 0)
        {
            // if the end is not reached
            if(currentFileNumber < fileCount-2)
            {
                // Increment the file number
                currentFileNumber++;

                if (animationCounter == 0 && recorderController != null && recorderController.IsRecording())
                {
                    UnityEngine.Debug.Log("animations finished");
                    
                    recorderController.StopRecording();

                }

                // setup the recorder
                SetupRecorder();

                // Visualize the data
                UnityEngine.Debug.Log("starting visualization");
                
                if (recorderController != null && !recorderController.IsRecording())
                {
                    recorderController.StartRecording();
                }

                // Load and show the data
                // UnityEngine.Debug.Log("stopped visualization");
                StartCoroutine(loadandvisualize());
            }
            else
            {
                // set the play flag to false
                play = false;

                // log that end has been reached
                UnityEngine.Debug.Log("end has been reached, stopping recording and starting conversion to one mp4");

                if (animationCounter == 0 && recorderController != null && recorderController.IsRecording())
                {
                    UnityEngine.Debug.Log("animations finished");
                    
                    recorderController.StopRecording();

                }

                // // delete old mp4s
                // DeleteOldMP4sScript();
            }

            // // log the current file number
            // UnityEngine.Debug.Log("spinfield: " + (currentFileNumber + 1) + " / " + fileCount);
        }

        // get to the next data set if loop mode is enabled
        if(playback && animationCounter == 0)
        {
            if(currentFileNumber > 0)
            {
                // Increment the file number
                currentFileNumber--;

                // Load the data
                StartCoroutine(loadandvisualize());
            }
            else
            {
                // set the playback flag to false
                playback = false;

                // log that start has been reached
                UnityEngine.Debug.Log("back to start");
            }
            
            // // log the current file number
            // UnityEngine.Debug.Log("spinfield: " + (currentFileNumber + 1) + " / " + fileCount);
        }
    }


    // public void DeleteOldMP4sScript()
    // {
    //     UnityEngine.Debug.Log("DeleteOldMP4sScript method called.");

    //     string pathToScript = Application.streamingAssetsPath + "/OUTPUT/DeleteOldMP4sScript.ps1";
    //     UnityEngine.Debug.Log("Path to PowerShell script: " + pathToScript);

    //     ProcessStartInfo startInfo = new ProcessStartInfo()
    //     {
    //         FileName = "powershell.exe",
    //         Arguments = $"-NoProfile -ExecutionPolicy Unrestricted -File \"{pathToScript}\"",
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         UseShellExecute = false,
    //         CreateNoWindow = true
    //     };

    //     Process process = new Process()
    //     {
    //         StartInfo = startInfo
    //     };

    //     try
    //     {
    //         process.Start();
    //         string output = process.StandardOutput.ReadToEnd();
    //         string errors = process.StandardError.ReadToEnd();
    //         process.WaitForExit();

    //         if (!string.IsNullOrEmpty(output))
    //         {
    //             UnityEngine.Debug.Log("PowerShell Output: " + output);
    //         }
    //         if (!string.IsNullOrEmpty(errors))
    //         {
    //             UnityEngine.Debug.LogError("PowerShell Errors: " + errors);
    //         }
    //     }
    //     catch (System.Exception e)
    //     {
    //         UnityEngine.Debug.LogError("Error executing PowerShell script: " + e.Message);
    //     }
    // }

    // public void Compress_mp4s()
    // {
    //     UnityEngine.Debug.Log("Compress_mp4s method called.");
    //     string original_WD = Directory.GetCurrentDirectory();
    //     string custom_WD = Application.streamingAssetsPath + "/OUTPUT";
    //     Directory.SetCurrentDirectory(custom_WD);

    //     string pathToScript = Application.streamingAssetsPath + "/OUTPUT/compress.ps1";
    //     UnityEngine.Debug.Log("Path to PowerShell script: " + pathToScript);

    //     ProcessStartInfo startInfo = new ProcessStartInfo()
    //     {
    //         FileName = "powershell.exe",
    //         Arguments = $"-NoProfile -ExecutionPolicy Unrestricted -File \"{pathToScript}\"",
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         UseShellExecute = false,
    //         CreateNoWindow = true
    //     };

    //     Process process = new Process()
    //     {
    //         StartInfo = startInfo
    //     };


    //     process.Start();
    //     Directory.SetCurrentDirectory(original_WD);
    // }

    IEnumerator loadandvisualize()
    {
        // Load the data
        dataLoader.LoadData(currentFileNumber);
        
        UpdateData();

        yield return null;
    }





    IEnumerator LoopThroughData()
    {
        // Increment the file number
        currentFileNumber++;

        // The number of files in the folder
        string relativePath = $"energy_comp/energy/json_data/";
        string folderPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        int fileCount = Directory.GetFiles(folderPath).Length;
        // Check if we have reached the end of the files

        for (int i = currentFileNumber; i < fileCount; i++)
        {
            // Load the data
            dataLoader.LoadData(i);

            
            // Visualize the data
            UpdateData();

            // Wait until animationcounter == 0
            yield return new WaitUntil(() => animationCounter == 0);
            // yield return new WaitForSeconds(0.1f);

            dataLoader = null;
            // Get the data loader
            dataLoader = GetComponent<Skyr_data_loader>();
        }

    }

    



    IEnumerator VisualizeData()
    {
        GameObject testPoint = Instantiate(dataPointPrefab);
        testPoint.transform.position = new Vector3(-1,0,-1);
        testPoint.transform.rotation = Quaternion.Euler(new Vector3(0.2f,0,0));
    
        // Loop over the data
        for (int i = 0; i < dataLoader.data.x.Count; i++)
        {
            if (!(dataLoader.data.x_Rot[i] == 0 && dataLoader.data.y_Rot[i] == 90))
            {
                if(first)
                {
                    UnityEngine.Debug.Log("first data point");
                    UnityEngine.Debug.Log("x: " + dataLoader.data.x[i]);
                    UnityEngine.Debug.Log("y: " + dataLoader.data.y[i]);
                    UnityEngine.Debug.Log("z: " + dataLoader.data.z[i]);
                    UnityEngine.Debug.Log("x_Rot: " + dataLoader.data.x_Rot[i]);
                    UnityEngine.Debug.Log("y_Rot: " + dataLoader.data.y_Rot[i]);
                    UnityEngine.Debug.Log("scalefactor: " + dataLoader.data.scalefactor[i]);
                }

                first = false;

                // Create a new data point
                GameObject dataPoint = Instantiate(dataPointPrefab);
                dataPointPool.Add(dataPoint);


                if(simulation_mode == "continuum")
                {
                    // Set its position
                    Vector3 position = new Vector3(
                        dataLoader.data.z[i],
                        0,
                        dataLoader.data.x[i]
                    );
                    dataPoint.transform.position = position;
                }
                else if(simulation_mode == "atomistic")
                {
                    float squashfactor = (float)(Math.Sqrt(3) / 2);

                    float z = dataLoader.data.x[i];
                    float x = dataLoader.data.z[i];

                    // Adjust x position based on the row (z) to create a hexagonal pattern
                    if (z % 2 == 1)
                    {
                        // For even rows, position as is
                        x *= 1/squashfactor;
                    }
                    else
                    {
                        // For odd rows, offset the x position
                        x = x * 1/squashfactor + 1/squashfactor / 2;
                    }

                    // Set its position
                    Vector3 position = new Vector3(x, 0, z);
                    dataPoint.transform.position = position;
                }
                else
                {
                    UnityEngine.Debug.Log("simulation_mode not set correctly");
                }

                // Set its rotation
                Quaternion direction = Quaternion.Euler(
                    dataLoader.data.x_Rot[i],
                    dataLoader.data.y_Rot[i],
                    0 // Ignore z rotation
                );
                // dataPoint.transform.rotation = direction; // old
                // this is new
                // Quaternion currentTarget = dataPointComponent.TargetRotation;
                direction = Quaternion.Normalize(direction);
                dataPoint.transform.rotation = direction;

                // turn off right now
                // dataPoint.transform.localScale = new Vector3(dataLoader.data.scalefactor[i], dataLoader.data.scalefactor[i], dataLoader.data.scalefactor[i]); 

                // Set its color based on y-value
                Renderer renderer = dataPoint.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    Color new_color = Colorpicker(dataLoader.data.x_Rot[i]);
                    renderer.material.color = new_color;
                }
            }
        }

        yield return null;
    }

    IEnumerator AnimateRotation(GameObject dataPoint, Quaternion targetRotation, Color startColor, Color targetColor, float duration)
    {
        animationCounter++;
        Quaternion startRotation = dataPoint.transform.rotation;
        Renderer   renderer      = dataPoint.GetComponentInChildren<Renderer>();
        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            dataPoint.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(startColor, targetColor, t);
            }
            yield return null;
        }
        // dataPoint.transform.rotation = targetRotation; // Ensure final rotation is set
        animationCounter--;
    }

    IEnumerator AnimationCounterCheck()
    {
        // wait for 1 sec
        yield return new WaitForSeconds(1.0f);
        UnityEngine.Debug.Log("animations finished");
    }

    void UpdateData()
    {
        // // Reset all data points
        // foreach (GameObject dataPoint in dataPointPool)
        // {
        //     dataPoint.SetActive(false);
        //     // UnityEngine.Debug.Log("data_detected");
        // }

        // Reset the next data point index
        nextDataPointIndex = 0;
        
        // // start a separate coroutine checking for animationCounter == 0
        // StartCoroutine(AnimationCounterCheck());      

        // Loop over the data
        for (int i = 0; i < dataLoader.data.x.Count; i++)
        {
            if (!(dataLoader.data.x_Rot[i] == 0 && dataLoader.data.y_Rot[i] == 90))
            {
                // if (i==10000)
                // {
                //     PrintGameObjectInfo(dataPointPool[i]);
                // }
                // Get the next data point from the pool
                GameObject dataPoint = dataPointPool[nextDataPointIndex];
                
                // Check if a data point can be taken from the pool
                if (nextDataPointIndex < dataPointPool.Count)
                {
                    dataPoint = dataPointPool[nextDataPointIndex];
                    nextDataPointIndex++;
                }
                // Set its rotation
                Quaternion direction = Quaternion.Euler(
                    dataLoader.data.x_Rot[i],
                    dataLoader.data.y_Rot[i],
                    0 // Ignore z rotation
                );
                // dataPoint.transform.rotation = Quaternion.Euler(direction); // old

                // dataPoint.transform.rotation = Quaternion.Slerp(dataPoint.transform.rotation, direction, animtime); // new --> does not work because animtime does not change
                Color current_color = dataPoint.GetComponentInChildren<Renderer>().material.color;
                Color next_color = Colorpicker(dataLoader.data.x_Rot[i]);

                if (current_color != next_color)
                {
                    StartCoroutine(AnimateRotation(dataPoint, direction, current_color, next_color, animtime)); // new_2 --> start coroutine for each animation
                }
                // dataPoint.transform.localScale = new Vector3(dataLoader.data.scalefactor[i], dataLoader.data.scalefactor[i], dataLoader.data.scalefactor[i]);

                if (i==1000)
                {
                    UnityEngine.Debug.Log("scalefactor: " + dataLoader.data.scalefactor[i]);
                }

                // // Set its color based on y-value
                // Renderer renderer = dataPoint.GetComponentInChildren<Renderer>();
                // if (renderer != null)
                // {
                    
                //     renderer.material.color = color;
                // }

                // // Make it active
                // dataPoint.SetActive(true);
            }
        }
    }

    Color Colorpicker(float y_Rot)
    {
        // maps -180, 180 to 0, 1
        float normalizedY = Mathf.InverseLerp(-90, 90, y_Rot);

        Color blue = Color.blue;
        Color white = Color.white;
        Color red = Color.red;

        Color next_color;

        if (normalizedY < 0.5f)
        {
            // Interpolate between blue and white in the first half of the gradient
            next_color = Color.Lerp(blue, white, 2 * normalizedY);
        }
        else
        {
            // Interpolate between white and red in the second half of the gradient
            next_color = Color.Lerp(white, red, normalizedY * 2 - 1.0f);
        }

        return next_color;
    }

    void PrintGameObjectInfo(GameObject obj)
    {
        UnityEngine.Debug.Log("GameObject name: " + obj.name);
        UnityEngine.Debug.Log("Position: " + obj.transform.position);
        UnityEngine.Debug.Log("Rotation: " + obj.transform.rotation);
        UnityEngine.Debug.Log("Scale: " + obj.transform.localScale);

        foreach (Component comp in obj.GetComponents<Component>())
        {
            UnityEngine.Debug.Log("Component: " + comp.GetType().ToString());
        }
    }

    void SetupRecorder()
    {
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        recorderController = new RecorderController(controllerSettings);

        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "My Video Recorder";
        videoRecorder.Enabled = true;

        // Use Game View Input Settings for general screen capture
        var gameViewInputSettings = new GameViewInputSettings()
        {
            OutputHeight = 908,  // Ensure this is an even number
            OutputWidth = 1920 // Ensure this is an even number
        };
        videoRecorder.ImageInputSettings = gameViewInputSettings;

        // Configure output file path to include currentFileNumber
        string fileName = $"video_{currentFileNumber.ToString("D4")}";

        // Configure other settings like output file path, frame rate, etc.
        videoRecorder.OutputFile = Path.Combine(Application.streamingAssetsPath, "OUTPUT", fileName);

        controllerSettings.AddRecorderSettings(videoRecorder);
        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 30.0f;  // Example frame rate

        recorderController.PrepareRecording();
    }

    void transformCam()
    {
        // Set the camera position
        Camera.main.transform.position = new Vector3(62.26f, 18.82f, 54.27f);

        // Set the camera rotation
        Camera.main.transform.rotation = Quaternion.Euler(new Vector3(34.90f, 303.49f, -2.81e-05f));
    }
}
