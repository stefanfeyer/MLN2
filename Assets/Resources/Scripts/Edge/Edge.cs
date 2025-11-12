using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logical edge between two nodes of an MLN. Handles registration with nodes/layers and basic weight bookkeeping.
/// </summary>
public class Edge
{
    /// <summary>Unique identifier (also used as display label by default).</summary>
    public string id;

    /// <summary>Source node of the edge.</summary>
    public Node sourceNode;

    /// <summary>Target node of the edge.</summary>
    public Node targetNode;

    /// <summary>Absolute weight of the edge (domain-specific scale; defaults to 1).</summary>
    public float weight = 1;

    /// <summary>
    /// Weight normalised to [0,1] relative to other edges on the same layer (NaN for inter-layer edges).
    /// </summary>
    public float weightRelativeToLayer01 = float.NaN;

    /// <summary>Display label; initialised from <see cref="id"/>.</summary>
    public string label;

    /// <summary>Visual component associated with this edge (set by <see cref="EdgeVis"/>).</summary>
    public EdgeVis edgeVis;

    /// <summary>Owning MLN instance.</summary>
    public MLN mln;

    /// <summary>True if the edge connects nodes from different layers.</summary>
    public bool isEdgeBetweenLayer = false;

    /// <summary>True if the edge connects nodes within the same layer.</summary>
    public bool isEdgeOnLayer = false;

    /// <summary>True if the edge connects two nodes with the same label across layers.</summary>
    public bool isSelfEdge = false;

    /// <summary>The layer this edge belongs to (for intra-layer edges only).</summary>
    public Layer layer;

    /// <summary>Original source node reference (used if nodes are aggregated/rewired temporarily).</summary>
    public Node originalSourceNode;

    /// <summary>Original target node reference (used if nodes are aggregated/rewired temporarily).</summary>
    public Node originalTargetNode;

    /// <summary>
    /// Constructs an edge, registers it with nodes/layers/MLN and sets type flags.
    /// </summary>
    /// <param name="_id">Unique identifier.</param>
    /// <param name="_sourceNode">Source node.</param>
    /// <param name="_targetNode">Target node.</param>
    /// <param name="_mln">Owning MLN instance.</param>
    public Edge(string _id, Node _sourceNode, Node _targetNode, MLN _mln)
    {
        id = _id;
        label = _id;
        mln = _mln;
        sourceNode = _sourceNode;
        targetNode = _targetNode;

        sourceNode.addEdge(this);
        targetNode.addEdge(this);

        if (sourceNode.layer == targetNode.layer)
        {
            isEdgeOnLayer = true;
            sourceNode.layer.addEdge(this);
            mln.addEdge(this);
            layer = sourceNode.layer;
        }
        else
        {
            isEdgeBetweenLayer = true;
            mln.addEdgeBetweenLayers(this);
            mln.addEdge(this);

            if (sourceNode.label == targetNode.label)
            {
                isSelfEdge = true;
                mln.addSelfEdge(this);
            }
        }
    }

    /// <summary>
    /// Sets edge weight from string input.
    /// </summary>
    /// <param name="_weight">String representation of the weight (parsed via <see cref="float.Parse(string)"/>).</param>
    public void setWeight(string _weight)
    {
        weight = float.Parse(_weight);
    }

    /// <summary>
    /// Sets edge weight.
    /// </summary>
    /// <param name="_weight">New absolute weight.</param>
    public void setWeight(float _weight)
    {
        weight = _weight;
    }

    /// <summary>
    /// Returns the absolute weight.
    /// </summary>
    public float getWeight()
    {
        return weight;
    }

    /// <summary>
    /// Computes <see cref="weightRelativeToLayer01"/> as a [0,1] value relative to other edges on the same layer.
    /// Leaves it as NaN for inter-layer edges.
    /// </summary>
    public void calculateWeightRelativeToLayer01()
    {
        if (isEdgeOnLayer)
        {
            float edgeMaxWeight = sourceNode.layer.getMaxEdgeWeight();
            float edgeMinWeight = sourceNode.layer.getMinEdgeWeight();
            weightRelativeToLayer01 =
                (weight - edgeMinWeight) / (0.0001f + (edgeMaxWeight - edgeMinWeight)); // Avoid /0
        }
        else
        {
            weightRelativeToLayer01 = float.NaN;
        }
    }
}
