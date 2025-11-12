using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple loader UI: lists GraphML files, spawns buttons to load them, and auto-loads two examples on first Update().
/// </summary>
public class loadPanel : MonoBehaviour
{
    /// <summary>Folder containing GraphML files to display.</summary>
    private string graphFolderPath = @"Assets/Resources/Graphs/CurrentGraphs";

    /// <summary>Prefab path for the button UI.</summary>
    private string graphButtonAssetPath = @"Assets/Resources/Prefabs/loadGraphButton.prefab";

    /// <summary>Parent transform to hold generated buttons.</summary>
    public GameObject graphButtonList;

    /// <summary>Optional debug button (unused).</summary>
    public Button debugButton;

    private bool init = false;

    // Start is called before the first frame update
    void Start()
    {
        string[] graphFileNames = Directory.GetFiles(graphFolderPath);
        foreach (string graphFileName in graphFileNames)
        {
            if (Path.GetExtension(graphFileName) == ".graphml")
            {
                generateGraphButton(graphFileName);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Automatically load example graphs on first frame
        if (!init) { loadNetwork1(); loadNetwork2(); init = true; }
    }

    /// <summary>
    /// Instantiates a button for the given graph path and wires its onClick to load the file.
    /// </summary>
    private void generateGraphButton(string graphPath)
    {
        GameObject graphButton = PrefabUtility.LoadPrefabContents(graphButtonAssetPath);
        graphButton.transform.SetParent(graphButtonList.transform, false);
        graphButton.transform.localScale = Vector3.one;
        graphButton.AddComponent<loadMLN>().path = graphPath;

        GameObject button = graphButton.transform.Find("theButtonItself").gameObject;
        button.GetComponent<Button>().onClick.AddListener(graphButton.GetComponent<loadMLN>().load);
        GameObject tmp = button.transform.Find("buttonFront").gameObject;
        GameObject buttonText = tmp.transform.Find("buttonText").gameObject;
        buttonText.GetComponent<TextMeshProUGUI>().text = Path.GetFileName(graphPath).Split(".")[0];
    }

    /// <summary>
    /// Loads a first example graph with a predefined arrangement/dimension.
    /// </summary>
    private void loadNetwork1()
    {
        Debug.Log("loading network");
        ReadGraphML reader = new ReadGraphML();
        MLN mln = reader.readMLNGraph(@"Assets/Resources/Graphs/CurrentGraphs/mandrillDirectedN.graphml");
        GameObject mlnVisGo = new GameObject();
        MLNVis mlnVis = mlnVisGo.AddComponent<MLNVis>();
        mlnVis.mln = mln;
        mlnVis.initialLayerArrangement = Variables.ID2DLA;
        mlnVis.initialLayerDimension = Variables.ID2DLD;
    }

    /// <summary>
    /// Loads a second example graph with a different initial arrangement/dimension, then hides this panel.
    /// </summary>
    private void loadNetwork2()
    {
        Debug.Log("loading network");
        ReadGraphML reader = new ReadGraphML();
        MLN mln = reader.readMLNGraph(@"Assets/Resources/Graphs/CurrentGraphs/mandrillDirectedN.graphml");
        GameObject mlnVisGo = new GameObject();
        MLNVis mlnVis = mlnVisGo.AddComponent<MLNVis>();
        mlnVis.mln = mln;
        mlnVis.initialLayerArrangement = Variables.ID25DLA;
        mlnVis.initialLayerDimension = Variables.ID2DLD;

        this.gameObject.SetActive(false);
    }
}
