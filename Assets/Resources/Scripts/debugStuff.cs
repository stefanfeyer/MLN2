using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class debugStuff : MonoBehaviour
{
    public MLN mln;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            arrange();
        }
    }

    private void arrange()
    {
        float x = 0;
        foreach (Layer layer in mln.layers)
        {
            layer.layerVis.setPosition(new Vector3(x, 1, 0));
            x++;
        }
    }
}
