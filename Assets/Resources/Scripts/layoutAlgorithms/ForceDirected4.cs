using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Force-directed layout operating on the first layer of an MLN.
/// Applies limited-distance repulsion, edge-weighted attraction, optional centring, and damping.
/// </summary>
public class ForceDirected4
{
    /// <summary>Owning MLN.</summary>
    public MLN mln;

    //public List<Vector3> nodes; // List of node positions
    //public List<(int, int)> edges; // List of edges as pairs of node indices

    /// <summary>Strength of node-node repulsion.</summary>
    public float repulsionStrength = 1.0f;

    /// <summary>Strength of edge attraction.</summary>
    public float attractionStrength = 10.0f;

    /// <summary>Damping factor to stabilise motion.</summary>
    public float damping = 0.85f;

    /// <summary>Maximum per-step displacement.</summary>
    public float maxMovement = 0.02f;

    /// <summary>Soft pull toward origin to keep nodes central; 0 disables.</summary>
    public float centeringStrength = 0.0001f;

    /// <summary>Repulsion only applies below this distance.</summary>
    float minRepulsionDistance = 0.15f;

    /// <summary>Number of iterations to run.</summary>
    public int iterations = 100;

    /// <summary>Multiplier for how strongly edge weight affects attraction.</summary>
    public float edgeWeightMultiplyer = 1f;

    /// <summary>
    /// Constructs the layout and runs it immediately on the provided MLN.
    /// </summary>
    public ForceDirected4(MLN mln)
    {
        // Debug.Log(mln.edges.Count);
        applyLayout(mln);
    }

    /// <summary>
    /// Executes the layout for a fixed number of iterations.
    /// </summary>
    /// <param name="mln">MLN whose first layer is laid out.</param>
    public void applyLayout(MLN mln)
    {
        for (int i = 0; i <= iterations; i++)
        {
            ApplyForces(mln);
        }
    }

    /// <summary>
    /// Applies one iteration of repulsion, attraction and centring forces, then integrates positions.
    /// </summary>
    /// <param name="mln">Target MLN.</param>
    public void ApplyForces(MLN mln)
    {
        // reset all forces
        foreach (Node node in mln.layers[0].nodes)
        {
            node.nodeVis.force = Vector3.zero;
        }

        // Repulsive forces (node-node)
        foreach (Node node0 in mln.layers[0].nodes)
        {
            foreach (Node node1 in mln.layers[0].nodes)
            {
                Vector3 delta = node0.nodeVis.transform.localPosition - node1.nodeVis.transform.localPosition;
                float distance = delta.magnitude + 0.001f; // avoid div by zero

                if (distance < minRepulsionDistance)
                {
                    // Reduce repulsion near the bounds.
                    float boundaryFactor = Mathf.Clamp01(1f - delta.magnitude / 0.5f);
                    Vector3 repulsion = (repulsionStrength * boundaryFactor / distance) * delta.normalized;

                    node0.nodeVis.force += repulsion;
                    node1.nodeVis.force -= repulsion;
                }
            }
        }

        // Attractive forces (edges)
        foreach (Edge edge in mln.edges)
        {
            float edgeMaxWeight = edge.sourceNode.layer.getMaxEdgeWeight();
            float edgeMinWeight = edge.sourceNode.layer.getMinEdgeWeight();
            float edgeRelativeWeight = (edge.getWeight() - edgeMinWeight) / (0.0000001f + (edgeMaxWeight - edgeMinWeight));

            Vector3 delta = edge.sourceNode.nodeVis.transform.localPosition - edge.targetNode.nodeVis.transform.localPosition;
            float distance = delta.magnitude + 0.00001f;

            float averageDegree = (float)mln.edges.Count / mln.nodes.Count; // reduce force in sparse graphs
            float dynamicAttractionStrength = attractionStrength * Mathf.Clamp(1f / averageDegree, 0.5f, 2f);

            // Using simple attraction scaled by relative weight:
            Vector3 attraction = (distance * delta.normalized) * (attractionStrength * edgeRelativeWeight * edgeWeightMultiplyer);

            edge.sourceNode.nodeVis.force -= attraction;
            edge.targetNode.nodeVis.force += attraction;
        }

        // Integrate
        foreach (Node node in mln.layers[0].nodes)
        {
            Vector3 toCenter = Vector3.zero - node.nodeVis.transform.localPosition;
            node.nodeVis.force += centeringStrength * toCenter;
            node.nodeVis.force = Vector3.ClampMagnitude(node.nodeVis.force * damping, maxMovement);
            node.nodeVis.transform.localPosition += node.nodeVis.force;
        }
    }

    /// <summary>
    /// Aggregates all layers into a working layer by reconnecting edges to the first layer's nodes.
    /// </summary>
    /// <remarks>Not used by <see cref="applyLayout(MLN)"/>; provided for experimentation.</remarks>
    private Layer aggregateLayers(MLN mln)
    {
        Layer aggregatedLayer = new Layer("aggregatedLayer");
        aggregatedLayer.nodes = mln.layers[0].nodes;

        foreach (Layer layer in mln.layers)
        {
            foreach (Edge edge in layer.edges)
            {
                edge.sourceNode = aggregatedLayer.getNodeByLabel(edge.sourceNode.label);
                edge.targetNode = aggregatedLayer.getNodeByLabel(edge.targetNode.label);
            }
        }
        return aggregatedLayer;
    }
}
