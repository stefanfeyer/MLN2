using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Force-directed layout over an aggregated view: edges from all layers are projected onto layer 0's nodes,
/// preserving each edge's weight relative to its origin layer. Optionally ignores weak edges and dense layers.
/// </summary>
public class ForceDirected5
{
    /// <summary>Strength of node-node repulsion.</summary>
    public float repulsionStrength = 0.001f;

    /// <summary>Strength of edge attraction.</summary>
    public float attractionStrength = 1000.0f;

    /// <summary>Damping factor to stabilise motion.</summary>
    public float damping = 0.85f;

    /// <summary>Maximum per-step displacement.</summary>
    public float maxMovement = 0.15f;

    /// <summary>Soft pull toward origin to keep nodes central.</summary>
    public float centeringStrength = 0.01f;

    /// <summary>Repulsion only applies below this distance.</summary>
    float minRepulsionDistance = 0.1f;

    /// <summary>Number of iterations to run (randomised in <see cref="applyLayout(Layer)"/>).</summary>
    public int iterations = 100;

    /// <summary>Multiplier for how strongly edge weight affects attraction.</summary>
    public float edgeWeightMultiplyer = 1f;

    /// <summary>Edges below this per-layer relative weight are ignored.</summary>
    public float ignoreEdgesWithLayerRelativeWeightBelow = 0.01f;

    /// <summary>Layers with density above this threshold are ignored.</summary>
    public float ignoreLayersWithDensityAbove = 1.0f;

    /// <summary>Threshold above which edges are considered "important" for any metrics.</summary>
    public float importantEdgeWeightThreshold = 1.0f;

    /// <summary>
    /// Aggregates edges, runs the layout, then restores original edge wiring.
    /// </summary>
    public ForceDirected5(MLN mln)
    {
        Layer layer = aggregateLayers(mln);
        applyLayout(layer);
        devideLayers(mln);

        foreach (Layer layerO in mln.layers)
        {
            // layerO.layerVis.updateLayerInfo(importantEdgeWeightThreshold);
        }
    }

    /// <summary>
    /// Aggregates all layers into a temporary layer sharing the first layer's node set.
    /// </summary>
    private Layer aggregateLayers(MLN mln)
    {
        Layer aggregatedLayer = new Layer("aggregatedLayer");
        aggregatedLayer.nodes = mln.layers[0].nodes;

        foreach (Layer layer in mln.layers)
        {
            if (layer.layerDensity01 >= ignoreLayersWithDensityAbove) { continue; }

            foreach (Edge edge in layer.edges)
            {
                edge.originalSourceNode = edge.sourceNode;
                edge.sourceNode = aggregatedLayer.getNodeByLabel(edge.sourceNode.label);
                edge.originalTargetNode = edge.targetNode;
                edge.targetNode = aggregatedLayer.getNodeByLabel(edge.targetNode.label);
                aggregatedLayer.edges.Add(edge);
            }
        }
        return aggregatedLayer;
    }

    /// <summary>
    /// Restores edges to their original source/target nodes.
    /// </summary>
    private void devideLayers(MLN mln)
    {
        foreach (Layer layer in mln.layers)
        {
            if (layer.layerDensity01 >= ignoreLayersWithDensityAbove) { continue; }
            foreach (Edge edge in layer.edges)
            {
                edge.sourceNode = edge.originalSourceNode;
                edge.targetNode = edge.originalTargetNode;
            }
        }
    }

    /// <summary>
    /// Executes the layout on the aggregated layer.
    /// </summary>
    public void applyLayout(Layer layer)
    {
        iterations = Random.Range(50, 200);
        for (int i = 0; i <= iterations; i++)
        {
            ApplyForces(layer);
        }
    }

    /// <summary>
    /// Applies one iteration of forces to the aggregated layer.
    /// </summary>
    public void ApplyForces(Layer layer)
    {
        // reset all forces
        foreach (Node node in layer.nodes)
        {
            node.nodeVis.force = Vector3.zero;
        }

        // Repulsion
        foreach (Node node0 in layer.nodes)
        {
            foreach (Node node1 in layer.nodes)
            {
                Vector3 delta = node0.nodeVis.transform.localPosition - node1.nodeVis.transform.localPosition;
                float distance = delta.magnitude + 0.001f;

                if (distance < minRepulsionDistance)
                {
                    float boundaryFactor = Mathf.Clamp01(1f - delta.magnitude / 0.5f);
                    Vector3 repulsion = (repulsionStrength * boundaryFactor / distance) * delta.normalized;

                    node0.nodeVis.force += repulsion;
                    node1.nodeVis.force -= repulsion;
                }
            }
        }

        // Attraction (skip weak edges)
        foreach (Edge edge in layer.edges)
        {
            if (edge.weightRelativeToLayer01 <= ignoreEdgesWithLayerRelativeWeightBelow)
            {
                // edge.edgeVis.deactivate();
                continue;
            }

            Vector3 delta = edge.sourceNode.nodeVis.transform.localPosition - edge.targetNode.nodeVis.transform.localPosition;
            float distance = delta.magnitude + 0.00001f;

            float scaledStrength = attractionStrength * Mathf.Exp(edge.weightRelativeToLayer01 - importantEdgeWeightThreshold);
            Vector3 attraction = (distance * delta.normalized) * scaledStrength;

            edge.sourceNode.nodeVis.force -= attraction;
            edge.targetNode.nodeVis.force += attraction;
        }

        // Integrate
        foreach (Node node in layer.nodes)
        {
            Vector3 toCenter = Vector3.zero - node.nodeVis.transform.localPosition;
            node.nodeVis.force += centeringStrength * toCenter;
            node.nodeVis.force = Vector3.ClampMagnitude(node.nodeVis.force * damping, maxMovement);
            node.nodeVis.transform.localPosition += node.nodeVis.force;
        }
    }
}
