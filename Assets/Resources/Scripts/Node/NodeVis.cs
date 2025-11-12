using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Visual representation for a <see cref="Node"/> using a sphere mesh and floating TextMeshPro label.
/// Handles initial size/colour/position, scaling compensation, and look-at behaviour for labels.
/// </summary>
public class NodeVis : MonoBehaviour
{
    /// <summary>Back-reference to the logical node.</summary>
    public Node node;

    /// <summary>Accumulated force from layout algorithms.</summary>
    public Vector3 force = new Vector3(0, 0, 0);

    /// <summary>Optional metric placeholder.</summary>
    public float stress = 0f;

    /// <summary>Current visual node size (before scale compensation).</summary>
    public float size = Variables.initialNodeScaleFactor;

    /// <summary>Renderer for the node mesh.</summary>
    public MeshRenderer meshRenderer;

    /// <summary>Current colour value (mirrors material colour).</summary>
    public Color color;

    /// <summary>Label GameObject.</summary>
    public GameObject label;

    GameObject mainCam;

    // Start is called before the first frame update
    void Start()
    {
        initialise();
        createLabel();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        // initXRGrabInteractable();
    }

    // Update is called once per frame
    void Update()
    {
        label.transform.LookAt(mainCam.transform);
        label.transform.Rotate(0, 180f, 0);
    }

    /// <summary>
    /// Optionally enables XR grabbing behaviour (disabled by default).
    /// </summary>
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
    /// Assigns mesh/material, sets initial size/colour/position, and configures sorting layers.
    /// </summary>
    private void initialise()
    {
        node.nodeVis = this;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        this.gameObject.AddComponent<MeshFilter>().mesh = go.GetComponent<MeshFilter>().mesh;
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/nodeMaterial");
        Destroy(go);

        setInitialNodeSize();
        setInitialColor();
        setInitialPosition();

        if (Variables.isLayerDrawingHandlingActive)
        {
            meshRenderer.sortingLayerName = "L" + node.layer.layerVis.layerDrawingLayerNumber;
            meshRenderer.sortingOrder = Variables.nodeDrawingSortingOrder;
        }
    }

    /// <summary>
    /// Applies the node's initial colour if specified.
    /// </summary>
    private void setInitialColor()
    {
        if (node.hasInitialColor) { setColour(node.initalColor); }
    }

    /// <summary>
    /// Creates a TextMeshPro label that follows the node.
    /// </summary>
    private void createLabel()
    {
        label = new GameObject("node label");
        label.transform.SetParent(this.transform);
        label.name = "node label";
        TextMeshPro tmp = label.AddComponent<TextMeshPro>();
        tmp.text = node.label;
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.rectTransform.sizeDelta = new Vector2(2f, 1f);
        tmp.rectTransform.localScale = new Vector3(1, 1, 1);
        tmp.rectTransform.localPosition = new Vector3(1f, 0.5f, -0.5f);
        tmp.fontSize = 5;
        MeshRenderer labelMeschRenderer = label.GetComponent<MeshRenderer>();
        if (Variables.isLayerDrawingHandlingActive)
        {
            labelMeschRenderer.sortingLayerName = "L" + node.layer.layerVis.layerDrawingLayerNumber;
            labelMeschRenderer.sortingOrder = Variables.labelDrawingSortingOrder;
        }
    }

    /// <summary>Sets the local position in the layer coordinate system.</summary>
    public void setPosition(Vector3 position)
    {
        this.transform.localPosition = position;
    }

    /// <summary>
    /// Applies the node's stored initial position if available, otherwise randomises.
    /// </summary>
    public void setInitialPosition()
    {
        if (node.hasInitialPosition)
        {
            setPosition(node.initialPosition);
        }
        else
        {
            Vector3 randomPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            this.transform.localPosition = randomPosition;
        }
    }

    /// <summary>
    /// Sets visual size while compensating for layer scaling to keep on-screen size consistent.
    /// </summary>
    /// <param name="_size">Base node size.</param>
    public void setSize(float _size)
    {
        size = _size;
        Vector3 layerScale = node.layer.layerVis.transform.localScale;
        transform.localScale = new Vector3(_size * (1 / layerScale.x), _size * (1 / layerScale.y), _size * (1 / layerScale.z));
    }

    /// <summary>Returns the current base node size.</summary>
    public float getSize() => size;

    /// <summary>Sets the material colour and caches it.</summary>
    public void setColour(Color _color)
    {
        color = _color;
        this.GetComponent<MeshRenderer>().material.color = _color;
    }

    /// <summary>Hides the node visual.</summary>
    public void deactivate() => this.gameObject.SetActive(false);

    /// <summary>Shows the node visual.</summary>
    public void activate() => this.gameObject.SetActive(true);

    /// <summary>
    /// Computes a default initial size from the per-layer node count.
    /// </summary>
    public void setInitialNodeSize()
    {
        setSize((float)(1f / ((float)node.layer.nodes.Count / 3f)));
    }
}
