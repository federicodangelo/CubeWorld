using UnityEngine;
using System.Collections.Generic;
using CubeWorld.Tiles;

public class ItemUnity : MonoBehaviour
{
    public GameManagerUnity gameManagerUnity;
    public CubeWorld.Items.Item item; 
	
    void Start()
    {
        VisualDefinitionRenderUnity visualDefinitionRenderer = gameObject.AddComponent<VisualDefinitionRenderUnity>();
        visualDefinitionRenderer.world = gameManagerUnity.world;
        visualDefinitionRenderer.material = gameManagerUnity.materialItems;
        visualDefinitionRenderer.visualDefinition = item.itemDefinition.visualDefinition;
    }
}

