using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// Root visual for an MLN. Creates layer/node/edge visuals, manages global scale and arrangements,
/// and hosts the control panel.
/// </summary>
public class MLNVis : MonoBehaviour
{
    /// <summary>Optional debug object.</summary>
    public GameObject debug;

    /// <summary>Source multilayer network.</summary>
    public MLN mln;

    private bool init = false;

    /// <summary>Parent object for inter-layer edges.</summary>
    public GameObject edgeParent;

    private string currentLayerArrangement;

    private bool isViewInitialised = false;

    /// <summary>Global layer scale factor (mirrors <see cref="Variables.layerScaleFactor"/>).</summary>
    private float layerScaleFactor = Variables.layerScaleFactor;

    /// <summary>Panel controller instance.</summary>
    public MLNPanel mlnPanel;

    /// <summary>Panel GameObject instance.</summary>
    public GameObject mlnPanelGo;

    /// <summary>Input action used to interact with the view.</summary>
    public InputAction inputAction;

    /// <summary>Initial arrangement (2D/25D/3D).</summary>
    public string initialLayerArrangement = Variables.ID2DLA;

    /// <summary>Initial layer dimension (1D/2D/3D).</summary>
    public string initialLayerDimension = Variables.ID2DLD;

    /// <summary>Whether to draw self-edges on startup.</summary>
    public bool initialDrawSelfEdges = false;

    // Start is called before the first frame update
    void Start()
    {
        mln.mlnVis = this;
        setInitalPosition();

        createEdgeParent();
        initialisePanel();
        createMLNVis();

        InputActionAsset inputActionAsset = (InputActionAsset)Resources.Load("InputActionControler/mainInputActionControler");
        if (initialLayerArrangement == Variables.ID2DLA) { inputAction = inputActionAsset.FindActionMap("testContr").FindAction("vec2Right"); }
        if (initialLayerArrangement == Variables.ID25DLA) { inputAction = inputActionAsset.FindActionMap("testContr").FindAction("vec2Left"); }
    }

    /// <summary>
    /// Sets the initial world position for the MLN visual container.
    /// </summary>
    private void setInitalPosition()
    {
        float middle = mln.layers.Count / 2 * 0.5f;
        setPostion(new Vector3(-middle, 0.0f, 0f));
    }

    /// <summary>
    /// Creates a parent container for edges not attached to a specific layer.
    /// </summary>
    private void createEdgeParent()
    {
        edgeParent = new GameObject("edges");
        edgeParent.transform.SetParent(this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isViewInitialised)
        {
            isViewInitialised = true;
            // setInitialAppearence();
        }
    }

    /// <summary>
    /// Builds all layer, node, and initial edge visuals.
    /// </summary>
    public void createMLNVis()
    {
        this.name = mln.label;
        createLayers();
        createEdgesInitially();
    }

    /// <summary>
    /// Creates a <see cref="LayerVis"/> for each layer and spawns child <see cref="NodeVis"/> objects.
    /// </summary>
    public void createLayers()
    {
        int drawingLayerNumber = 1;
        foreach (Layer layer in mln.layers)
        {
            GameObject layerVisGo = new GameObject(layer.label);
            LayerVis layerVis = layerVisGo.AddComponent<LayerVis>();
            layerVis.layer = layer;
            layer.layerVis = layerVisGo.GetComponent<LayerVis>();
            layerVis.setDrawingLayerNumber(drawingLayerNumber++);
            layerVisGo.transform.SetParent(this.transform);
            createNodes(layer, layerVisGo);
        }
    }

    /// <summary>
    /// Creates <see cref="NodeVis"/> game objects for all nodes on a layer.
    /// </summary>
    /// <param name="layer">Layer to populate with node visuals.</param>
    /// <param name="layerVis">Layer visual parent.</param>
    public void createNodes(Layer layer, GameObject layerVis)
    {
        foreach (Node node in layer.nodes)
        {
            GameObject nodeVis = new GameObject(node.label);
            nodeVis.AddComponent<NodeVis>().node = node;
            nodeVis.transform.SetParent(layerVis.transform);
        }
    }

    /// <summary>
    /// Draws visuals for all edges once on startup.
    /// </summary>
    private void createEdgesInitially()
    {
        foreach (Edge edge in mln.edges)
        {
            drawEdge(edge);
        }
    }

    /// <summary>
    /// Creates a visual for a single edge and attaches it to the correct parent.
    /// </summary>
    /// <param name="edge">Edge to visualise.</param>
    public void drawEdge(Edge edge)
    {
        GameObject edgeVisGo = new GameObject(edge.label);
        if (edge.isEdgeOnLayer)
        {
            if (edge.layer.layerVis.edgeParent == null)
            {
                GameObject edgeParent = new GameObject("edges");
                edge.layer.layerVis.edgeParent = edgeParent;
                edgeParent.transform.SetParent(edge.layer.layerVis.transform);
                edgeVisGo.transform.SetParent(edgeParent.transform);
            }
            else
            {
                edgeVisGo.transform.SetParent(edge.layer.layerVis.edgeParent.transform);
            }
        }
        else
        {
            edgeVisGo.transform.SetParent(edgeParent.transform);
        }

        EdgeVis edgeVis = edgeVisGo.AddComponent<EdgeVis>();
        edgeVis.mln = mln;
        edgeVis.edge = edge;
    }

    /// <summary>
    /// Destroys an edge and removes it from MLN tracking lists.
    /// </summary>
    /// <param name="edge">Edge to remove.</param>
    public void destroyEdge(Edge edge)
    {
        mln.edgesBetweenLayers.Remove(edge);
        mln.selfEdges.Remove(edge);
        mln.edges.Remove(edge);
        Destroy(edge.edgeVis);
    }

    /// <summary>
    /// Sets node sizes inversely proportional to node count per layer.
    /// </summary>
    public void initalNodeSize()
    {
        foreach (Node node in mln.nodes)
        {
            node.nodeVis.setSize((float)(1f / ((float)node.layer.nodes.Count / 3f)));
        }
    }

    /// <summary>
    /// Instantiates the control panel prefab and wires it to this MLN.
    /// </summary>
    private void initialisePanel()
    {
        string mlnPanelPath = @"Assets/Resources/Prefabs/mlnPanel.prefab";
        mlnPanelGo = PrefabUtility.LoadPrefabContents(mlnPanelPath);
        mlnPanel = mlnPanelGo.GetComponent<MLNPanel>();
        mlnPanel.mln = mln;
        mlnPanel.initalLayerArrangement = initialLayerArrangement;
        mlnPanel.initalLayerDimension = initialLayerDimension;

        mlnPanelGo.transform.SetParent(this.transform, false);
        mlnPanelGo.transform.localPosition = new Vector3(-1f, 0.5f, -0.5f);
    }

    /// <summary>
    /// Places layers along X in a 2D strip and centres the scene.
    /// </summary>
    public void set2DLayerArrangement()
    {
        resetRotation();
        transform.position = Variables.CENTER;
        shiftPosition(new Vector3(0, Variables.layerScaleFactor * 1, 0));
        float x = 0;
        foreach (Layer layer in mln.layers)
        {
            layer.layerVis.setPosition(new Vector3(x, 0, 0));
            x += layer.layerVis.getLayerScaleFactor() * Variables.distanceBetweenLayer2DFactor;
        }
        currentLayerArrangement = Variables.ID2DLA;
    }

    /// <summary>
    /// Stacks layers along Y with a 90° rotation (2.5D).
    /// </summary>
    public void set25DLayerArrangement()
    {
        resetRotation();
        transform.position = Variables.CENTER;
        float y = 0f;
        foreach (Layer layer in mln.layers)
        {
            layer.layerVis.setPosition(new Vector3(0, y, 0));
            layer.layerVis.setRotation(new Vector3(90, 0, 0));
            y = y + layer.layerVis.getLayerScaleFactor() * Variables.distanceBetweenLayer25DFactor;
        }
        currentLayerArrangement = Variables.ID25DLA;
    }

    /// <summary>
    /// Places layers around a circle in XZ with fixed radius (3D).
    /// </summary>
    public void set3DLayerArrangement()
    {
        resetRotation();

        float radius = 2f;
        float layerNumber = 0f;
        foreach (Layer layer in mln.layers)
        {
            float angle = layerNumber * Mathf.PI * 2f / mln.layers.Count;
            layerNumber++;
            layer.layerVis.setPosition(new Vector3(Mathf.Cos(angle) * radius, 1f, Mathf.Sin(angle) * radius));
        }
        currentLayerArrangement = Variables.ID3DLA;
    }

    /// <summary>
    /// Resets rotations of the MLN container and all layers.
    /// </summary>
    private void resetRotation()
    {
        mln.mlnVis.transform.localEulerAngles = Vector3.zero;
        mln.mlnVis.transform.eulerAngles = Vector3.zero;

        foreach (Layer layer in mln.layers)
        {
            layer.layerVis.transform.localEulerAngles = Vector3.zero;
            layer.layerVis.transform.eulerAngles = Vector3.zero;
        }
    }

    /// <summary>
    /// Sets initial arrangement/dimension and optional self-edge visibility.
    /// </summary>
    private void setInitialAppearence()
    {
        switch (initialLayerArrangement)
        {
            case Variables.ID2DLA: set2DLayerArrangement(); break;
            case Variables.ID25DLA: set25DLayerArrangement(); break;
            case Variables.ID3DLA: set3DLayerArrangement(); break;
            default: set2DLayerArrangement(); break;
        }

        foreach (Layer layer in mln.layers)
        {
            switch (initialLayerDimension)
            {
                case Variables.ID1DLD: layer.layerVis.set1DLayerDimension(); break;
                case Variables.ID2DLD: layer.layerVis.set2DLayerDimension(); break;
                case Variables.ID3DLD: layer.layerVis.set3DLayerDimension(); break;
                default: layer.layerVis.set2DLayerDimension(); break;
            }
        }

        if (!initialDrawSelfEdges)
        {
            foreach (Edge edge in mln.selfEdges) edge.edgeVis.deactivate();
        }
    }

    /// <summary>
    /// Sets global scale for all layer visuals.
    /// </summary>
    /// <param name="_layerScaleFactor">Uniform scale factor.</param>
    public void setGlobalLayerScale(float _layerScaleFactor)
    {
        layerScaleFactor = _layerScaleFactor;
        Variables.layerScaleFactor = _layerScaleFactor;
        foreach (Layer layer in mln.layers)
        {
            layer.layerVis.setScaleByFactor(layerScaleFactor);
        }
    }

    /// <summary>
    /// Sets absolute edge width for all edges.
    /// </summary>
    /// <param name="_width">Line width in units.</param>
    public void setGlobalAbsoluteEdgeWidth(float _width)
    {
        foreach (Edge edge in mln.edges)
        {
            edge.edgeVis.setEdgeWidthAbsolute(_width);
        }
    }

    /// <summary>
    /// Sets uniform node size for all nodes (adjusted for layer scale).
    /// </summary>
    /// <param name="_size">Desired visual size.</param>
    public void setGlobalNodeSize(float _size)
    {
        foreach (Node node in mln.nodes)
        {
            node.nodeVis.setSize(_size);
        }
    }

    /// <summary>Sets world position of the MLN container.</summary>
    public void setPostion(Vector3 _position) => transform.position = _position;

    /// <summary>Returns world position of the MLN container.</summary>
    public Vector3 getPosition() => transform.position;

    /// <summary>Shifts world position of the MLN container.</summary>
    public void shiftPosition(Vector3 _shift) => transform.position += _shift;

    private float currentLayerDistance = 1.0f;

    /// <summary>
    /// Adjusts spacing between layers for the current arrangement.
    /// </summary>
    /// <param name="newLayerDistance">Delta distance to add.</param>
    public void setDistanceBetweenLayers(float newLayerDistance)
    {
        currentLayerDistance = currentLayerDistance + newLayerDistance;
        for (int i = 0; i < mln.layers.Count; i++)
        {
            mln.layers[i].layerVis.setPosition(new Vector3(
                mln.layers[i].layerVis.transform.position.x,
                currentLayerDistance * i,
                mln.layers[i].layerVis.transform.position.z));
        }
    }
}
