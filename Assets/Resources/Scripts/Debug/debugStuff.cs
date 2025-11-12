using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// Ad-hoc debugging utilities for testing layouts, forces and layer spacing at runtime.
/// Attach to a scene object, assign <see cref="mln"/>, and use key presses to trigger actions.
/// </summary>
public class debugStuff : MonoBehaviour
{
    /// <summary>
    /// Reference to the active multilayer network in the scene.
    /// </summary>
    public MLN mln;

    /// <summary>Spring joint minimum distance (debug use).</summary>
    public float sMinDist;

    /// <summary>Spring joint maximum distance (debug use).</summary>
    public float sMaxDist;

    /// <summary>Spring damper (debug use).</summary>
    public float sDamp;

    /// <summary>Spring stiffness (debug use).</summary>
    public float sSpring;

    // Start is called before the first frame update
    void Start()
    {
        // Intentionally left blank – wire up debug actions via Update() key handling.
    }

    // Update is called once per frame
    void Update()
    {
        // Example hotkeys can be wired here as needed.
        if (Keyboard.current.numpad1Key.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.numpad2Key.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.numpad3Key.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.sKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.gKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.nKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.wKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.hKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.jKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.cKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.oKey.wasPressedThisFrame) { /* your action */ }
        if (Keyboard.current.qKey.wasPressedThisFrame) { /* your action */ }

        // NOTE: The same key is checked twice in the original code. Kept as-is to preserve behaviour.
        if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
        {
            // Increase spacing between layers.
            mln.mlnVis.setDistanceBetweenLayers(0.1f);
        }

        if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
        {
            // Decrease spacing between layers. (Overlaps previous block; original behaviour retained.)
            mln.mlnVis.setDistanceBetweenLayers(-0.1f);
        }
    }

    /// <summary>
    /// Sets all nodes on every layer to the same local positions as the corresponding nodes on layer 0 (by label).
    /// Useful to quickly align layers for visual comparison.
    /// </summary>
    public void globalLayout()
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

    /// <summary>
    /// Adds <see cref="Rigidbody"/> components and <see cref="SpringJoint"/> connections along each edge in every layer.
    /// This enables quick physics-based experimentation of spring layouts in play mode.
    /// </summary>
    public void springs()
    {
        // Ensure rigidbodies exist
        foreach (Layer layer in mln.layers)
        {
            foreach (Edge edge in layer.edges)
            {
                if (!edge.sourceNode.nodeVis.GetComponent<Rigidbody>())
                {
                    edge.sourceNode.nodeVis.AddComponent<Rigidbody>();
                    edge.sourceNode.nodeVis.GetComponent<Rigidbody>().useGravity = false;
                }
                if (!edge.targetNode.nodeVis.GetComponent<Rigidbody>())
                {
                    edge.targetNode.nodeVis.AddComponent<Rigidbody>();
                    edge.targetNode.nodeVis.GetComponent<Rigidbody>().useGravity = false;
                }
            }
        }

        // Connect springs
        foreach (Layer layer in mln.layers)
        {
            foreach (Edge edge in layer.edges)
            {
                edge.sourceNode.nodeVis.AddComponent<SpringJoint>().connectedBody =
                    edge.targetNode.nodeVis.GetComponent<Rigidbody>();

                foreach (SpringJoint joint in edge.sourceNode.nodeVis.GetComponents<SpringJoint>())
                {
                    joint.minDistance = 0.1f;
                    joint.maxDistance = 0.4f;
                    joint.spring = 10f;
                    joint.damper = 0.2f;
                    joint.autoConfigureConnectedAnchor = false;
                    joint.tolerance = 0f;
                }
            }
        }
    }
}
