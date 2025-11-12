using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// Visual representation of an <see cref="Edge"/> using a <see cref="LineRenderer"/> in local space.
/// Handles width/colour mapping, dynamic redraw on node movement, and layer-aware sorting.
/// </summary>
public class EdgeVis : MonoBehaviour
{
    /// <summary>Back-reference to the logical edge this visual represents.</summary>
    public Edge edge;

    /// <summary>Owning MLN (used for weight range queries, etc.).</summary>
    public MLN mln;

    /// <summary>Layer reference for intra-layer edges.</summary>
    public Layer layer;

    /// <summary>Preferred rest length (not actively used by drawing).</summary>
    public float restLength = 1f;

    private float minEdgeWidth = Variables.minEdgeWidth;
    private float maxEdgeWidth = Variables.maxEdgeWidth;

    private Vector3 lastFrameSourceNodePosition;
    private Vector3 lastFrameTargetNodePosition;

    /// <summary>Current geometric length (updated when requested).</summary>
    public float length;

    /// <summary>Current line width (units).</summary>
    public float width;

    /// <summary>Cached edge weight.</summary>
    public float weight;

    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        initialise();
    }

    // Update is called once per frame
    void Update()
    {
        drawEdgeIfPositionChanged();
    }

    /// <summary>
    /// Sets up the line renderer, associates references and draws once.
    /// </summary>
    private void initialise()
    {
        edge.edgeVis = this;
        name = edge.id;
        if (edge.isEdgeOnLayer) { layer = edge.layer; }
        weight = edge.weight;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = Resources.Load<Material>("Materials/edgeMaterial");
        lineRenderer.startWidth = minEdgeWidth;
        lineRenderer.endWidth = minEdgeWidth;
        lineRenderer.positionCount = 2;

        draw();

        lineRenderer.generateLightingData = true;
        lineRenderer.useWorldSpace = false;

        if (Variables.isLayerDrawingHandlingActive)
        {
            lineRenderer.sortingLayerName = "L" + edge.sourceNode.layer.layerVis.layerDrawingLayerNumber;
            lineRenderer.sortingOrder = Variables.edgeDrawingSortingOrder;
        }
    }

    /// <summary>
    /// Draws the edge segment between source and target nodes, clipping to node radii.
    /// </summary>
    private void draw()
    {
        Vector3 sourceNodePosition = edge.sourceNode.nodeVis.transform.localPosition;
        Vector3 targetNodePosition = edge.targetNode.nodeVis.transform.localPosition;
        Vector3 direction = (targetNodePosition - sourceNodePosition).normalized;
        Vector3 startPoint = sourceNodePosition + direction * (edge.sourceNode.nodeVis.size / 2);
        Vector3 endPoint = targetNodePosition - direction * (edge.targetNode.nodeVis.size / 2);

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    /// <summary>
    /// Redraws the edge if either endpoint moved since last frame.
    /// </summary>
    private void drawEdgeIfPositionChanged()
    {
        if (lastFrameSourceNodePosition != edge.sourceNode.nodeVis.transform.position ||
            lastFrameTargetNodePosition != edge.targetNode.nodeVis.transform.position)
        {
            draw();
            //calculateLength(); // Optional: enable if length is needed per frame
        }

        lastFrameSourceNodePosition = edge.sourceNode.nodeVis.transform.position;
        lastFrameTargetNodePosition = edge.targetNode.nodeVis.transform.position;
    }

    /// <summary>Shows the edge visual.</summary>
    public void activate() => gameObject.SetActive(true);

    /// <summary>Hides the edge visual.</summary>
    public void deactivate() => gameObject.SetActive(false);

    /// <summary>
    /// Sets width by a 0..1 value linearly mapped to global min/max widths in <see cref="Variables"/>.
    /// </summary>
    /// <param name="_width">Normalised width in [0,1].</param>
    public void setWidthRelative01(float _width)
    {
        width = Variables.minEdgeWidth + (Variables.maxEdgeWidth - Variables.minEdgeWidth) * _width;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    /// <summary>
    /// Sets an absolute width for the line.
    /// </summary>
    /// <param name="_width">Width in world units.</param>
    public void setEdgeWidthAbsolute(float _width)
    {
        width = _width;
        lineRenderer.startWidth = _width;
        lineRenderer.endWidth = _width;
    }

    /// <summary>Sets the current material colour.</summary>
    public void setColor(Color color) => lineRenderer.material.color = color;

    /// <summary>Restores the standard material for edges.</summary>
    public void setStandardColor()
    {
        lineRenderer.material = Resources.Load<Material>("Materials/edgeMaterial");
    }

    /// <summary>
    /// Updates <see cref="length"/> using current node positions (local space).
    /// </summary>
    public void calculateLength()
    {
        Vector3 delta = edge.sourceNode.nodeVis.transform.localPosition - edge.targetNode.nodeVis.transform.localPosition;
        length = delta.magnitude;
    }

    /// <summary>Sets the global minimum edge width and syncs the local cache.</summary>
    public void setMinEdgeWidth(float _minEdgeWidth)
    {
        Variables.minEdgeWidth = _minEdgeWidth;
        minEdgeWidth = _minEdgeWidth;
    }

    /// <summary>Sets the global maximum edge width and syncs the local cache.</summary>
    public void setMaxEdgeWidth(float _maxEdgeWidth)
    {
        Variables.maxEdgeWidth = _maxEdgeWidth;
        maxEdgeWidth = _maxEdgeWidth;
    }

    /// <summary>Returns the cached minimum edge width.</summary>
    public float getMinEdgeWidth() => minEdgeWidth;

    /// <summary>Returns the cached maximum edge width.</summary>
    public float getMaxEdgeWidth() => maxEdgeWidth;

    /// <summary>
    /// Sets width and colour based on the edge weight relative to the whole MLN.
    /// </summary>
    public void setEdgeWidthByWeightAbsolute()
    {
        float mlnMinWeight = mln.getMinEdgeWeight();
        float mlnMaxWeight = mln.getMaxEdgeWeight();
        float edgeThickness = (edge.getWeight() - mlnMinWeight) / (mlnMaxWeight - mlnMinWeight);

        setWidthRelative01(edgeThickness);
        setColor(calcColorGradient(edgeThickness));
    }

    /// <summary>
    /// Sets width and colour based on the edge weight relative to its layer only.
    /// </summary>
    public void setEdgeWidthByWeightRelative()
    {
        float layerMinWeight = layer.getMinEdgeWeight();
        float layerMaxWeight = layer.getMaxEdgeWeight();
        float edgeThickness = (edge.getWeight() - layerMinWeight) / (layerMaxWeight - layerMinWeight);

        setWidthRelative01(edgeThickness);
        setColor(calcColorGradient(edgeThickness));
    }

    /// <summary>
    /// Maps a 0..1 value to a gradient colour using low/high edge colour settings in <see cref="Variables"/>.
    /// </summary>
    /// <param name="value">Position in gradient range, typically weight-normalised [0,1].</param>
    /// <returns>Interpolated colour.</returns>
    private Color calcColorGradient(float value)
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(Variables.LOWWEIGHTEDGECOLOR, 0.0f);
        colors[1] = new GradientColorKey(Variables.HIGHWEIGHTEDGECOLOR, 1.0f);

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);
        gradient.SetKeys(colors, alphas);

        return gradient.Evaluate(value);
    }
}
