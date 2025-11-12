using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight force-directed layout with optional centring force and edge-weight influence.
/// Operates in local space; updates node positions in place across a fixed number of iterations.
/// </summary>
public class ForceDirected3
{
    /// <summary>Owning MLN (optional; not required for core step).</summary>
    public MLN mln;

    /// <summary>Strength of node-node repulsive force.</summary>
    public float repulsionStrength = 0.01f;

    /// <summary>Strength of edge attraction.</summary>
    public float attractionStrength = 0.01f;

    /// <summary>Velocity damping factor applied each step.</summary>
    public float damping = 0.85f;

    /// <summary>Clamp for per-step node displacement.</summary>
    public float maxMovement = 0.02f;

    /// <summary>
    /// Additional pull towards the origin to keep nodes within view (0 disables).
    /// </summary>
    public float centeringStrength = 0.01f;

    /// <summary>Repulsion is considered only within this distance.</summary>
    float minRepulsionDistance = 0.2f;

    /// <summary>Number of iterations to run.</summary>
    public int iterations = 100;

    /// <summary>Multiplier controlling how strongly edge weight affects attraction.</summary>
    public float edgeWeightMultiplyer = 10f;

    /// <summary>Constructs the layout helper.</summary>
    public ForceDirected3() { }

    /// <summary>
    /// Executes the layout for a single layer.
    /// </summary>
    /// <param name="layer">Target layer whose nodes will be updated.</param>
    public void applyLayout(Layer layer)
    {
        for (int i = 0; i <= iterations; i++)
        {
            ApplyForces(layer, layer.nodes);
        }
    }

    /// <summary>
    /// Applies one iteration of forces to a set of nodes (repulsion, attraction, centring), then updates positions.
    /// </summary>
    /// <param name="layer">Layer providing edges and weight ranges.</param>
    /// <param name="nodes">Nodes to process.</param>
    public void ApplyForces(Layer layer, List<Node> nodes)
    {
        // Reset all forces
        foreach (Node node in nodes)
        {
            node.nodeVis.force = Vector3.zero;
        }

        // Repulsive forces (node-node)
        foreach (Node node0 in nodes)
        {
            foreach (Node node1 in nodes)
            {
                Vector3 delta = node0.nodeVis.transform.localPosition - node1.nodeVis.transform.localPosition;
                float distance = delta.magnitude + 0.001f; // Prevent division by zero

                if (distance < minRepulsionDistance)
                {
                    // Reduce repulsion near the boundaries to limit drift
                    float boundaryFactor = Mathf.Clamp01(1f - delta.magnitude / 0.5f);
                    Vector3 repulsion = (repulsionStrength * boundaryFactor / distance) * delta.normalized;

                    node0.nodeVis.force += repulsion;
                    node1.nodeVis.force -= repulsion; // Equal and opposite
                }
            }
        }

        // Attractive forces (edges, weighted)
        foreach (Edge edge in layer.edges)
        {
            float edgeMaxWeight = edge.sourceNode.layer.getMaxEdgeWeight();
            float edgeMinWeight = edge.sourceNode.layer.getMinEdgeWeight();
            Vector3 delta = edge.sourceNode.nodeVis.transform.localPosition - edge.targetNode.nodeVis.transform.localPosition;
            float distance = delta.magnitude + 0.01f;

            float edgeRelativeWeight01 =
                (edge.getWeight() - edgeMinWeight) / (0.0001f + (edgeMaxWeight - edgeMinWeight));

            float averageDegree = (float)layer.edges.Count / layer.nodes.Count; // Reduce force in sparse graphs
            float dynamicAttractionStrength = attractionStrength * Mathf.Clamp(1f / averageDegree, 0.5f, 2f);

            // Option A (commented in original): dynamic attraction strength
            // Vector3 attraction = ((distance * dynamicAttractionStrength) * delta.normalized) * (edgeRelativeWeight01 * edgeWeightMultiplyer);

            // Option B (active): fixed attraction strength scaled by edge weight
            Vector3 attraction = ((distance * attractionStrength) * delta.normalized) * (edgeRelativeWeight01 * edgeWeightMultiplyer);

            edge.sourceNode.nodeVis.force -= attraction;
            edge.targetNode.nodeVis.force += attraction;
        }

        // Integrate and clamp displacement
        foreach (Node node in nodes)
        {
            Vector3 toCenter = Vector3.zero - node.nodeVis.transform.localPosition;
            node.nodeVis.force += centeringStrength * toCenter;
            node.nodeVis.force = Vector3.ClampMagnitude(node.nodeVis.force * damping, maxMovement);
            node.nodeVis.transform.localPosition += node.nodeVis.force;
        }
    }
}
