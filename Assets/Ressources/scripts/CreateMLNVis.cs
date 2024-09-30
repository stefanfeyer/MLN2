using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMLNVis : MonoBehaviour
{
    public MLN mln;
    bool init = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!init && mln != null)
        {
            createMLNVis();
            init = !init;
        }
    }

    public void createMLNVis()
    {
        this.name = mln.label;
        createLayers();
        createEdges();
    }


    public void createLayers()
    {
        foreach (Layer layer in mln.layers)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = layer.label;
            cube.GetComponent<MeshRenderer>().enabled = false;
            cube.transform.SetParent(this.transform);
            createNodes(layer, cube);
        }
    }

    public void createNodes(Layer layer, GameObject layerVis)
    {
        foreach ( Node node in layer.nodes )
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = node.label;
            sphere.transform.SetParent(layerVis.transform);
        }
    }

    public void createEdges()
    {
        foreach ( Edge edge in mln.edges )
        {
            
        }
    }
}
