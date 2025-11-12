using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Computes simple circular positions for nodes on one or more layers of an MLN (Multilayer Network).
/// Places nodes of a given layer evenly on a circle in local space.
/// </summary>
public class CircularLayout
{
    private MLN mln;

    /// <summary>
    /// Initialises a circular layout helper and lays out the first layer immediately.
    /// </summary>
    /// <param name="_mln">The MLN instance containing layers and nodes to lay out.</param>
    public CircularLayout(MLN _mln)
    {
        mln = _mln;
        // By default, position only the first layer to avoid surprising global changes.
        calculateNodePositionsPerLayer(mln.layers[0]);
        // To lay out all layers instead, call calculateNodePositionsForAllLayers().
    }

    /// <summary>
    /// Applies a circular layout to all layers in the MLN.
    /// </summary>
    public void calculateNodePositionsForAllLayers()
    {
        foreach (Layer layer in mln.layers)
        {
            calculateNodePositionsPerLayer(layer);
        }
    }

    /// <summary>
    /// Evenly distributes nodes of the provided layer on a circle (XY plane, Z=0).
    /// </summary>
    /// <param name="layer">The target layer whose nodes will be positioned.</param>
    public void calculateNodePositionsPerLayer(Layer layer)
    {
        int numberOfNodes = layer.nodesDict.Count;
        if (numberOfNodes == 0) return;

        const float radius = 0.45f;
        int iterations = 0;

        foreach (Node node in layer.nodesDict.Values)
        {
            float angle = iterations++ * Mathf.PI * 2f / numberOfNodes; // Even distribution
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            node.nodeVis.setPosition(new Vector3(x, y, 0));
        }
    }
}
