using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Runtime.ConstrainedExecution;
using UnityEditor.PackageManager;
using UnityEngine.InputSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Visual component for a layer: draws a boundary cube, label, exposes scaling/activation helpers,
/// and provides simple geometric metrics such as crossing counts.
/// </summary>
public class LayerVis : MonoBehaviour
{
    /// <summary>Back-reference to the logical layer.</summary>
    public Layer layer;

    /// <summary>Width used for label/layout decisions (not actively modified).</summary>
    public float layerWidth;

    /// <summary>Global layer scale factor (mirrors <see cref="Variables.layerScaleFactor"/>).</summary>
    private float layerScaleFactor = Variables.layerScaleFactor;

    /// <summary>Sorting layer index used when layered drawing is enabled.</summary>
    public int layerDrawingLayerNumber;

    /// <summary>Tracks current dimension mode ("1D", "2D", "3D").</summary>
    private string currentLayerDim = "not initialised";

    /// <summary>Boundary renderer.</summary>
    public MeshRenderer meshRenderer;

    /// <summary>Parent object for edge visuals (optional).</summary>
    public GameObject edgeParent;

    GameObject layerInfo;
    GameObject label;
    GameObject mainCam;

    // Start is called before the first frame update
    void Start()
    {
        initialise();
        createLabel();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        // createLayerInfo();
        // initXRGrabInteractable();
    }

    // Update is called once per frame
    void Update()
    {
        // makeLabelLookAtCamera();
    }

    /// <summary>Positions the label and faces it toward the main camera.</summary>
    private void makeLabelLookAtCamera()
    {
        Vector3 pos = transform.position;
        label.transform.position = new Vector3(pos.x, pos.y - 0.6f, pos.z);
        label.transform.LookAt(mainCam.transform);
        label.transform.Rotate(0, 180, 0);
    }

    /// <summary>Sets the sorting layer index used for draw order management.</summary>
    public void setDrawingLayerNumber(int _layerDrawingLayerNumber)
    {
        layerDrawingLayerNumber = _layerDrawingLayerNumber;
    }

    /// <summary>Adds XR grab behaviour and sets basic rigidbody params (disabled by default).</summary>
    private void initXRGrabInteractable()
    {
        XRGrabInteractable gi = this.gameObject.AddComponent<XRGrabInteractable>();
        Rigidbody rb = this.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.drag = 10f;
        rb.mass = 10f;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// Creates boundary mesh, associates references and applies sorting-layer settings if active.
    /// </summary>
    public void initialise()
    {
        this.name = layer.label;
        layer.layerVis = this;
        // createEdgeParent();

        GameObject boundary = new GameObject("boundary");
        boundary.transform.SetParent(this.transform);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boundary.AddComponent<MeshFilter>().mesh = go.GetComponent<MeshFilter>().mesh;
        MeshRenderer boundaryMeshRenderer = boundary.AddComponent<MeshRenderer>();
        boundaryMeshRenderer.material = Resources.Load<Material>("Materials/layerMaterial");
        // boundary.AddComponent<BoxCollider>();

        if (Variables.isLayerDrawingHandlingActive)
        {
            boundaryMeshRenderer.sortingLayerName = "L" + layerDrawingLayerNumber;
            boundaryMeshRenderer.sortingOrder = Variables.layerBoundaryDrawingSortingOrder;
        }

        Destroy(go);
    }

    /// <summary>Sets local position of the layer object.</summary>
    public void setPosition(Vector3 _position)
    {
        this.transform.localPosition = _position;
    }

    /// <summary>Sets local rotation (Euler) of the layer object.</summary>
    public void setRotation(Vector3 _rotation)
    {
        this.transform.localEulerAngles = _rotation;
    }

    /// <summary>Sets uniform scale by factor, updating both the global and local scale settings.</summary>
    public void setScaleByFactor(float _scaleFactor)
    {
        Variables.layerScaleFactor = _scaleFactor;
        layerScaleFactor = _scaleFactor;
        Vector3 scale = Vector3.one * layerScaleFactor;
        setScale(scale);
    }

    /// <summary>Returns the current scale factor.</summary>
    public float getScaleFactor()
    {
        return layerScaleFactor;
    }

    private void createLabel2()
    {
        label = GameObject.CreatePrimitive(PrimitiveType.Cube);
        label.transform.SetParent(this.transform);
        label.transform.localPosition = new Vector3(0, -0.6f, 0);
        label.transform.localScale = new Vector3(1f, 1f, 1f / this.transform.localScale.z);
    }

    /// <summary>Creates a TextMeshPro label anchored to the layer.</summary>
    private void createLabel()
    {
        label = new GameObject("layer label");
        label.transform.SetParent(this.transform);

        label.name = "layer label";
        label.transform.SetParent(label.transform);
        TextMeshPro tmp = label.AddComponent<TextMeshPro>();
        tmp.text = layer.label;
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.rectTransform.sizeDelta = new Vector2(1f, 0.4f);
        tmp.rectTransform.localScale = new Vector3(1, 1, 1);
        tmp.rectTransform.localPosition = new Vector3(0, -0.6f, 0);
        tmp.fontSize = 1.0f;
        MeshRenderer labelMeschRenderer = label.GetComponent<MeshRenderer>();
        if (Variables.isLayerDrawingHandlingActive)
        {
            labelMeschRenderer.sortingLayerName = "L" + layerDrawingLayerNumber;
            labelMeschRenderer.sortingOrder = Variables.labelDrawingSortingOrder;
        }
    }

    /// <summary>Sets scale for a "1D" layer footprint and optionally randomises node positions.</summary>
    public void set1DLayerDimension()
    {
        currentLayerDim = "1D";
        float currentNodeScale = layer.nodes.ToArray()[0].nodeVis.getSize();
        this.transform.localScale = new Vector3(1 * layerScaleFactor, currentNodeScale * layerScaleFactor, currentNodeScale * layerScaleFactor);
        // randomiseNodePositions();
    }

    /// <summary>Sets scale for a "2D" layer footprint and optionally randomises node positions.</summary>
    public void set2DLayerDimension()
    {
        currentLayerDim = "2D";
        float currentNodeScale = layer.nodes.ToArray()[0].nodeVis.getSize();
        this.transform.localScale = new Vector3(1 * layerScaleFactor, 1 * layerScaleFactor, currentNodeScale * layerScaleFactor);
        // randomiseNodePositions();
    }

    /// <summary>Sets scale for a "3D" layer footprint and optionally randomises node positions.</summary>
    public void set3DLayerDimension()
    {
        currentLayerDim = "3D";
        this.transform.localScale = Vector3.one * layerScaleFactor;
        // randomiseNodePositions();
    }

    /// <summary>Gets the current local scale.</summary>
    public Vector3 getScale()
    {
        return this.transform.localScale;
    }

    /// <summary>Sets the local scale.</summary>
    public void setScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    /// <summary>Randomises node positions according to current layer dimension mode.</summary>
    public void randomiseNodePositions()
    {
        switch (currentLayerDim)
        {
            case "1D":
                foreach (Node node in layer.nodes)
                {
                    node.nodeVis.transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, 0);
                }
                break;
            case "2D":
                foreach (Node node in layer.nodes)
                {
                    node.nodeVis.transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0);
                }
                break;
            case "3D":
                foreach (Node node in layer.nodes)
                {
                    node.nodeVis.transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Counts geometric crossings among all edge segments on the layer (2D projection using local XY).
    /// </summary>
    public int CountEdgeCrossings()
    {
        int crossings = 0;

        for (int i = 0; i < layer.edges.Count; i++)
        {
            Edge edge1 = layer.edges[i];

            for (int j = i + 1; j < layer.edges.Count; j++)
            {
                Edge edge2 = layer.edges[j];

                if (DoEdgesIntersect(edge1, edge2))
                {
                    crossings++;
                }
            }
        }

        return crossings;
    }

    /// <summary>Returns true if two edge segments intersect using a 2D orientation test.</summary>
    private bool DoEdgesIntersect(Edge edge1, Edge edge2)
    {
        Vector3 p1 = edge1.sourceNode.nodeVis.transform.localPosition;
        Vector3 q1 = edge1.targetNode.nodeVis.transform.localPosition;
        Vector3 p2 = edge2.sourceNode.nodeVis.transform.localPosition;
        Vector3 q2 = edge2.targetNode.nodeVis.transform.localPosition;

        return (IsCounterClockwise(p1, p2, q2) != IsCounterClockwise(q1, p2, q2)) &&
               (IsCounterClockwise(p1, q1, p2) != IsCounterClockwise(p1, q1, q2));
    }

    /// <summary>Helper: returns true if a,b,c are in counter-clockwise order (2D test on XY components).</summary>
    private bool IsCounterClockwise(Vector3 a, Vector3 b, Vector3 c)
    {
        return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
    }

    /// <summary>
    /// Returns average edge length for the layer (currently ignores <paramref name="threshold"/>).
    /// </summary>
    public float calculateAvergageEdgeLengthForEdgesWithWeightAbove(float threshold)
    {
        float averageLength = 0f;
        foreach (Edge edge in layer.edges)
        {
            averageLength += edge.edgeVis.length;
        }
        return averageLength / layer.nodes.Count;
    }

    /// <summary>Creates a small text panel with layer metrics (disabled by default).</summary>
    private void createLayerInfo()
    {
        layerInfo = new GameObject();
        layerInfo.transform.SetParent(this.transform);
        layerInfo.name = "layer info";
        layerInfo.AddComponent<TextMeshPro>().text = "edge crossings: " + CountEdgeCrossings();
        layerInfo.GetComponent<TextMeshPro>().horizontalAlignment = HorizontalAlignmentOptions.Center;
        layerInfo.GetComponent<TextMeshPro>().verticalAlignment = VerticalAlignmentOptions.Middle;
        layerInfo.GetComponent<TextMeshPro>().rectTransform.sizeDelta = new Vector2(1f, 0.1f);
        layerInfo.GetComponent<TextMeshPro>().rectTransform.localScale = new Vector3(1, 1, 1);
        layerInfo.GetComponent<TextMeshPro>().rectTransform.localPosition = new Vector3(0, 0.6f, 0);
        layerInfo.GetComponent<TextMeshPro>().fontSize = 0.5f;
        layerInfo.GetComponent<TextMeshPro>().transform.LookAt(GameObject.FindGameObjectWithTag("MainCamera").transform);
    }

    /// <summary>Updates the layer info panel with current crossing count and average length.</summary>
    public void updateLayerInfo(float threshold)
    {
        layerInfo.GetComponent<TextMeshPro>().text =
            "edge crossings: " + CountEdgeCrossings() + "\n" +
            "avg l imp edges: " + calculateAvergageEdgeLengthForEdgesWithWeightAbove(threshold);
    }

    /// <summary>Returns the current layer scale factor.</summary>
    public float getLayerScaleFactor()
    {
        return layerScaleFactor;
    }

    /// <summary>Sets the layer scale factor and updates global variable.</summary>
    public void setLayerScaleFactor(float _factor)
    {
        Variables.layerScaleFactor = _factor;
        layerScaleFactor = _factor;
    }

    /// <summary>Disables this layer and all its edge visuals.</summary>
    public void deactivate()
    {
        this.gameObject.SetActive(false);
        foreach (Edge edge in layer.edges)
        {
            edge.edgeVis.deactivate();
        }
    }

    /// <summary>Enables this layer and all its edge visuals.</summary>
    public void activate()
    {
        this.gameObject.SetActive(true);
        foreach (Edge edge in layer.edges)
        {
            edge.edgeVis.activate();
        }
    }

    /// <summary>Creates a container object intended to group edge visuals (optional).</summary>
    private void createEdgeParent()
    {
        edgeParent.name = "edges";
        edgeParent.transform.SetParent(this.transform);
    }
}
