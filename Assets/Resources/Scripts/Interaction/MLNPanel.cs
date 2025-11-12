using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// UI panel for interacting with an MLN: toggles colouring, edge styling, layer arrangements/dimensions,
/// saving/loading, and running example setups/layouts.
/// </summary>
public class MLNPanel : MonoBehaviour
{
    /// <summary>Optional debug object/reference.</summary>
    public GameObject debug;

    /// <summary>Active multilayer network.</summary>
    public MLN mln;

    /// <summary>Toggle: colour nodes by degree.</summary>
    public Toggle colorByDegreeToggle;

    /// <summary>Toggle: map edge weight to line thickness (relative to layer).</summary>
    public Toggle edgeWeightToThickness;

    /// <summary>Toggle: colour nodes by "sex" attribute.</summary>
    public Toggle sexToColor;

    /// <summary>Toggle: align node positions across layers by label.</summary>
    public Toggle globalLayout;

    /// <summary>Toggle: show/hide self-edges.</summary>
    public Toggle selfEdges;

    /// <summary>Toggle: optional custom colouring (unused here).</summary>
    public Toggle ginoColorToggle;

    /// <summary>Button: export graphs (MLN and super-graph).</summary>
    public Button forceDirectedLayout;

    /// <summary>Button: arrange layers in 2D strip.</summary>
    public Button layerArrangement2D;

    /// <summary>Button: arrange layers stacked in 2.5D.</summary>
    public Button layerArrangement25D;

    /// <summary>Button: arrange layers around a 3D circle.</summary>
    public Button layerArrangement3D;

    /// <summary>Button: set layer dimensions to 1D.</summary>
    public Button layerDimensionalityButton1D;

    /// <summary>Button: set layer dimensions to 2D.</summary>
    public Button layerDimensionalityButton2D;

    /// <summary>Button: set layer dimensions to 3D.</summary>
    public Button layerDimensionalityButton3D;

    /// <summary>Button: export current MLN to GraphML.</summary>
    public Button storeButton;

    /// <summary>Current layer arrangement identifier.</summary>
    public string currentLayerArrangement;

    /// <summary>Flag indicating if a layout algorithm has been applied.</summary>
    public bool isLayoutAlgorithmApplied = false;

    // UI list headers / legend entries
    public GameObject liLA;
    public GameObject liLD;
    public GameObject liCbD;
    public GameObject liStC;
    public GameObject liEwtT;
    public GameObject liFDLA;
    public GameObject liSE;
    public GameObject liGL;
    public GameObject liGC;

    /// <summary>Initial layer arrangement (2D / 2.5D / 3D).</summary>
    public string initalLayerArrangement;

    /// <summary>Initial layer dimension (1D / 2D / 3D).</summary>
    public string initalLayerDimension;

    private bool isInit = false;
    private int frameCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        registerInteractables();
    }

    // Update is called once per frame
    void Update() { }

    private void LateUpdate()
    {
        // Delay initialisation to ensure scene objects are ready.
        if (!isInit && frameCounter > 10) { isInit = true; exampleSetupMandrills(); } else { frameCounter++; }
    }

    /// <summary>
    /// Wires up UI elements to handler methods.
    /// </summary>
    private void registerInteractables()
    {
        colorByDegreeToggle.onValueChanged.AddListener(applyColorByDegree);
        edgeWeightToThickness.onValueChanged.AddListener(applyRelativeEdgeWeightToThickness);
        selfEdges.onValueChanged.AddListener(toggleSelfEdges);
        globalLayout.onValueChanged.AddListener(applyGlobalLayout);
        forceDirectedLayout.onClick.AddListener(writeGraphAndSuperGraph);
        sexToColor.onValueChanged.AddListener(applySexToColor);
        layerArrangement2D.onClick.AddListener(arrangeLayers2D);
        layerArrangement25D.onClick.AddListener(arrangeLayers25D);
        layerArrangement3D.onClick.AddListener(arrangeLayers3D);
        layerDimensionalityButton1D.onClick.AddListener(layerDim1D);
        layerDimensionalityButton2D.onClick.AddListener(layerDim2D);
        layerDimensionalityButton3D.onClick.AddListener(layerDim3D);
        storeButton.onClick.AddListener(storeGraph);
    }

    /// <summary>
    /// Runs the Stress Minimisation layout on the current MLN.
    /// </summary>
    public void applyStressMinLayout()
    {
        StressMin stressMin = new StressMin(mln);
    }

    /// <summary>
    /// Writes the current MLN as GraphML to disk.
    /// </summary>
    public void storeGraph()
    {
        WriteGraphML writer = new WriteGraphML(mln, mln.label);
        writer.writeGraphML();
    }

    /// <summary>
    /// Writes both the MLN and its super-graph to disk.
    /// </summary>
    public void writeGraphAndSuperGraph()
    {
        WriteGraphML writer = new WriteGraphML(mln, mln.label);
        writer.writeGraphML();
        writer.writeSuperGraph();
    }

    /// <summary>
    /// Reads a super-graph, normalises its node positions to [0,1], applies them to the current MLN, and writes GraphML.
    /// </summary>
    public void readSuperGraphScaleAndApplyNodePositionsToMainGraph()
    {
        ReadGraphML reader = new ReadGraphML();
        MLN sgGraph = reader.readMonoLayerGraph(@"C:\0main\0_researchProjects\0_MLNforCB\OGDF\OGDFVS\graphName.graphml");
        Dictionary<string, Vector3> nodeLabelToPosition = new Dictionary<string, Vector3>();
        float maxX = float.MinValue, minX = float.MaxValue;
        float maxY = float.MinValue, minY = float.MaxValue;
        float maxZ = float.MinValue, minZ = float.MaxValue;

        float xPos, yPos, zPos;

        foreach (Node node in sgGraph.layers[0].nodes)
        {
            maxX = Mathf.Max(node.initialPosition.x, maxX);
            minX = Mathf.Min(node.initialPosition.x, minX);
            maxY = Mathf.Max(node.initialPosition.y, maxY);
            minY = Mathf.Min(node.initialPosition.y, minY);
            maxZ = Mathf.Max(node.initialPosition.z, maxZ);
            minZ = Mathf.Min(node.initialPosition.z, minZ);
        }

        foreach (Node node in sgGraph.layers[0].nodes)
        {
            xPos = (node.initialPosition.x - minX) / (0.00001f + (maxX - minX));
            yPos = (node.initialPosition.y - minY) / (0.00001f + (maxY - minY));
            zPos = (node.initialPosition.z - minZ) / (0.00001f + (maxZ - minZ));
            Vector3 position = new Vector3(xPos - 0.5f, yPos - 0.5f, zPos);
            nodeLabelToPosition.Add(node.label, position);
        }

        foreach (Node node in mln.nodes)
        {
            node.nodeVis.setPosition(nodeLabelToPosition[node.label]);
        }

        WriteGraphML graphWriter = new WriteGraphML(mln, mln.id);
        graphWriter.writeGraphML();
    }

    /// <summary>
    /// Colours nodes by the value of their "sex" attribute.
    /// </summary>
    /// <param name="active">True to enable, false to reset to default colour.</param>
    public void applySexToColor(bool active)
    {
        if (active)
        {
            colorByDegreeToggle.isOn = false;
            foreach (Node node in mln.nodes)
            {
                switch (node.getAttribute("sex").ToUpper())
                {
                    case "MALE": node.nodeVis.setColour(Color.blue); break;
                    case "FEMALE": node.nodeVis.setColour(Color.red); break;
                    default: node.nodeVis.setColour(Color.blue); break;
                }
            }
        }
        else
        {
            foreach (Node node in mln.nodes)
            {
                node.nodeVis.setColour(Color.red);
            }
        }
    }

    /// <summary>
    /// Colours nodes by degree using a two-stop gradient.
    /// </summary>
    /// <param name="active">True to enable, false to reset to default colour.</param>
    public void applyColorByDegree(bool active)
    {
        if (active)
        {
            sexToColor.isOn = false;
            foreach (Node node in mln.nodes)
            {
                Gradient gradient = new Gradient();

                GradientColorKey[] colors = new GradientColorKey[2];
                colors[0] = new GradientColorKey(Color.yellow, 0.0f);
                colors[1] = new GradientColorKey(Color.red, 1.0f);

                GradientAlphaKey[] alphas = new GradientAlphaKey[2];
                alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
                alphas[1] = new GradientAlphaKey(1.0f, 1.0f);
                gradient.SetKeys(colors, alphas);

                float value = ((float)node.getDegree() - (float)mln.getMinNodeDegree()) / ((float)mln.getMaxNodeDegree() - (float)mln.getMinNodeDegree());
                node.nodeVis.setColour(gradient.Evaluate(value));
            }
        }
        else
        {
            foreach (Node node in mln.nodes)
            {
                node.nodeVis.setColour(Color.red);
            }
        }
    }

    /// <summary>
    /// Maps edge weights to thickness within each layer.
    /// </summary>
    /// <param name="active">True to enable relative thickness mapping; false resets style.</param>
    public void applyRelativeEdgeWeightToThickness(bool active)
    {
        if (active)
        {
            foreach (Layer layer in mln.layers)
            {
                foreach (Edge edge in layer.edges)
                {
                    edge.edgeVis.setEdgeWidthByWeightRelative();
                }
            }
        }
        else
        {
            foreach (Edge edge in mln.edges)
            {
                edge.edgeVis.setWidthRelative01(0f);
                edge.edgeVis.setStandardColor();
            }
        }
    }

    /// <summary>
    /// Maps edge weights to thickness relative to the whole MLN.
    /// </summary>
    /// <param name="active">True to enable absolute thickness mapping; false resets style.</param>
    public void applyAbsoluteEdgeWeightToThickness(bool active)
    {
        if (active)
        {
            foreach (Layer layer in mln.layers)
            {
                foreach (Edge edge in layer.edges)
                {
                    edge.edgeVis.setEdgeWidthByWeightAbsolute();
                }
            }
        }
        else
        {
            foreach (Edge edge in mln.edges)
            {
                edge.edgeVis.setWidthRelative01(0f);
                edge.edgeVis.setStandardColor();
            }
        }
    }

    /// <summary>Applies the 2D layer arrangement.</summary>
    public void arrangeLayers2D() => mln.mlnVis.set2DLayerArrangement();

    /// <summary>Applies the 2.5D layer arrangement.</summary>
    public void arrangeLayers25D() => mln.mlnVis.set25DLayerArrangement();

    /// <summary>Applies the 3D layer arrangement.</summary>
    public void arrangeLayers3D() => mln.mlnVis.set3DLayerArrangement();

    /// <summary>
    /// Runs a force-directed layout variant on the MLN (aggregated approach).
    /// </summary>
    public void applyForceDirectedLayout()
    {
        ForceDirected5 f5 = new ForceDirected5(mln);
    }

    /// <summary>
    /// Places nodes on each layer on a circle.
    /// </summary>
    public void applyCircularLayout()
    {
        CircularLayout cl = new CircularLayout(mln);
    }

    /// <summary>
    /// Shows/hides all self-edges across the MLN.
    /// </summary>
    /// <param name="active">True to show, false to hide.</param>
    public void toggleSelfEdges(bool active)
    {
        if (active)
        {
            foreach (Edge edge in mln.selfEdges) edge.edgeVis.activate();
        }
        else
        {
            foreach (Edge edge in mln.selfEdges) edge.edgeVis.deactivate();
        }
    }

    /// <summary>
    /// Aligns node positions across layers (by label) to match layer 0 positions.
    /// </summary>
    /// <param name="active">True to apply alignment; false does nothing.</param>
    public void applyGlobalLayout(bool active)
    {
        if (active)
        {
            foreach (Layer layer in mln.layers)
            {
                foreach (Node node in layer.nodes)
                {
                    foreach (Node firstLayerNode in mln.layers[0].nodes)
                    {
                        if (node.label == firstLayerNode.label && node.layer != firstLayerNode.layer)
                        {
                            node.nodeVis.transform.localPosition = firstLayerNode.nodeVis.transform.localPosition;
                        }
                    }
                }
            }
        }
        else { }
    }

    /// <summary>Sets all layers to 1D dimension.</summary>
    private void layerDim1D()
    {
        foreach (Layer layer in mln.layers) layer.layerVis.set1DLayerDimension();
    }

    /// <summary>Sets all layers to 2D dimension.</summary>
    private void layerDim2D()
    {
        foreach (Layer layer in mln.layers) layer.layerVis.set2DLayerDimension();
    }

    /// <summary>Sets all layers to 3D dimension.</summary>
    private void layerDim3D()
    {
        foreach (Layer layer in mln.layers) layer.layerVis.set3DLayerDimension();
    }

    /// <summary>
    /// Demonstration setup for the “Mandrills” dataset: scales, colours, sizes, and arranges layers based on initial settings.
    /// </summary>
    public void exampleSetupMandrills()
    {
        Debug.Log("Init View");

        Variables.minEdgeWidth = 0.0125f;
        Variables.maxEdgeWidth = 0.025f;

        mln.mlnVis.setGlobalLayerScale(0.5f);
        applyCircularLayout();
        applyGlobalLayout(true);

        layerDim2D();
        foreach (Node node in mln.nodes)
        {
            node.nodeVis.setSize(0.025f);
            node.nodeVis.setColour(Color.red);
        }
        layerDim2D();

        foreach (Node node in mln.nodes)
        {
            node.nodeVis.setSize(0.05f);
            node.nodeVis.setColour(Color.red);
        }

        applyRelativeEdgeWeightToThickness(true);

        if (initalLayerArrangement == Variables.ID2DLA)
        {
            arrangeLayers2D();
            mln.mlnVis.shiftPosition(new Vector3(-1.5f, 0.5f, 0));
        }

        if (initalLayerArrangement == Variables.ID25DLA)
        {
            arrangeLayers25D();
            mln.mlnVis.shiftPosition(new Vector3(-3f, 0.25f, -1f));
        }

        this.gameObject.SetActive(false);
    }
}
