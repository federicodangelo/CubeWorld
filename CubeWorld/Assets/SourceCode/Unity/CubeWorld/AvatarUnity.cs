using System.Collections.Generic;
using UnityEngine;
using CubeWorld.Avatars;

public class AvatarUnity : MonoBehaviour
{
    public GameManagerUnity gameManagerUnity;

    public CubeWorld.Avatars.Avatar avatar;

    private GameObject body;

    private List<GameObject> parts = new List<GameObject>();
    private List<GameObject> heads = new List<GameObject>();
    private List<GameObject> arms = new List<GameObject>();
    private List<GameObject> legs = new List<GameObject>();

    public float headRotationVertical;

    private bool bodyRenderEnabled = true;

    public void DisableBodyRender()
    {
        bodyRenderEnabled = false;

        foreach (GameObject part in parts)
            part.GetComponent<Renderer>().enabled = false;
    }

    public void EnableBodyRender()
    {
        bodyRenderEnabled = true;

        foreach (GameObject part in parts)
            part.GetComponent<Renderer>().enabled = true;
    }

    public virtual void Start()
    {
        Vector3 sizeInTiles = GraphicsUnity.TilePositionToVector3(((AvatarDefinition) avatar.definition).sizeInTiles);

        //float halfX = sizeInTiles.x / 2.0f;
        float halfY = sizeInTiles.y / 2.0f;
        //float halfZ = sizeInTiles.z / 2.0f;

        body = new GameObject();
        body.name = "Body";
        body.transform.parent = transform;
        body.transform.localPosition = new Vector3(
            0,
            halfY - CubeWorld.Utils.Graphics.HALF_TILE_SIZE,
            0);

        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3(1, 1, 1);

        foreach (AvatarPartDefinition avatarPart in ((AvatarDefinition) avatar.definition).parts)
        {
            Vector3 offset = new Vector3(
                                avatarPart.offset.x,
                                avatarPart.offset.y,
                                avatarPart.offset.z);

            Vector3 rotation = GraphicsUnity.CubeWorldVector3ToVector3(avatarPart.rotation);

            GameObject goPart = new GameObject();
            goPart.name = avatarPart.id;
            goPart.transform.parent = body.transform;
            goPart.transform.localPosition = offset;
            goPart.transform.localRotation = Quaternion.Euler(rotation);
            goPart.transform.localScale = new Vector3(1, 1, 1);

            VisualDefinitionRenderUnity visualDefinitionRenderer = goPart.AddComponent<VisualDefinitionRenderUnity>();
            visualDefinitionRenderer.world = gameManagerUnity.world;
            visualDefinitionRenderer.material = gameManagerUnity.materialItems;
            visualDefinitionRenderer.visualDefinition = avatarPart.visualDefinition;

            switch (avatarPart.id)
            {
                case "head":
                    heads.Add(goPart);
                    break;

                case "arm":
                    arms.Add(goPart);
                    break;

                case "leg":
                    legs.Add(goPart);
                    break;
            }

            goPart.GetComponent<Renderer>().enabled = bodyRenderEnabled;

            parts.Add(goPart);
        }
    }

    private float legRotationTimer;
    private float legRotation;
    //private float armRotation;

    public virtual void Update()
    {
        if (avatar.input.jump)
        {
            legRotation = 25.0f;
        }
        else
        {
            if (avatar.input.moveDirection.magnitude > 0)
            {
                legRotationTimer += Time.deltaTime;
                legRotation = Mathf.Cos(legRotationTimer * 30.0f) * 25.0f;
            }
            else
            {
                legRotationTimer = 0.0f;
                legRotation = 0.0f;
            }
        }

        for (int i = 0; i < legs.Count; i++)
        {
            GameObject go = legs[i];
            go.transform.localRotation = Quaternion.Euler(((i % 2) == 0 ? 1.0f : -1.0f) * legRotation, 0, 0);
        }
    }
}

