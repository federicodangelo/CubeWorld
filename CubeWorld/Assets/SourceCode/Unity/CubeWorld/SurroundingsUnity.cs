using UnityEngine;
using System.Collections.Generic;
using CubeWorld.Configuration;

public class SurroundingsUnity : MonoBehaviour
{
    public Material daySkybox;
    public Material nightSkybox;
    public Material materialSurrounding;

    public GameManagerUnity gameManagerUnity;

    private float deltaColorTransition;
    private Color currentColor = Color.green;
    private Color newColor = Color.green;
    private Color oldSurroundingFaceColor;

    private List<GameObject> goSurroundings = new List<GameObject>();
    private GameObject goContainer;
	
    public void Start()
    {
        RenderSettings.skybox = daySkybox;
    }

    public void Clear()
    {
        oldSurroundingFaceColor = new Color();

        foreach (GameObject goSurrounding in goSurroundings)
            GameObject.DestroyImmediate(goSurrounding);

        if (goContainer)
        {
            GameObject.DestroyImmediate(goContainer);
            goContainer = null;
        }

        goSurroundings.Clear();
    }

    public void UpdateSkyColor()
    {
        float ambientLight = MeshUtils.luminanceMapper[gameManagerUnity.world.dayCycleManager.ambientLightLuminance];

        Color surroundingFaceColor = new Color(ambientLight * MeshUtils.faceBright[(int)CubeWorld.Utils.Graphics.Faces.Top],
                                                ambientLight * MeshUtils.faceBright[(int)CubeWorld.Utils.Graphics.Faces.Top],
                                                ambientLight * MeshUtils.faceBright[(int)CubeWorld.Utils.Graphics.Faces.Top]);

        if (surroundingFaceColor != oldSurroundingFaceColor)
        {
            foreach (GameObject go in goSurroundings)
                go.GetComponent<Renderer>().material.color = surroundingFaceColor;

            oldSurroundingFaceColor = surroundingFaceColor;
        }

        if (newColor != GraphicsUnity.CubeWorldColorToColor(gameManagerUnity.world.dayCycleManager.skyColor))
        {
            currentColor = RenderSettings.skybox.GetColor("_Tint");
            newColor = GraphicsUnity.CubeWorldColorToColor(gameManagerUnity.world.dayCycleManager.skyColor);
            deltaColorTransition = 0.0f;
        }

        if (deltaColorTransition < 1.0f)
        {
            Color interpolated = Color.Lerp(currentColor, newColor, deltaColorTransition);

            if (interpolated.r > 0.3 && RenderSettings.skybox != daySkybox)
                RenderSettings.skybox = daySkybox;
            else if (interpolated.r <= 0.3 && RenderSettings.skybox != nightSkybox)
                RenderSettings.skybox = nightSkybox;

            RenderSettings.skybox.SetColor("_Tint", interpolated);
            deltaColorTransition += Time.deltaTime;
            if (deltaColorTransition >= 1.0f)
            {
                RenderSettings.skybox.SetColor("_Tint", newColor);
                currentColor = newColor;
            }
        }
    }
	
    public void CreateSurroundings(ConfigSurroundings configSurroundings)
    {
        if (configSurroundings.surroundingMaterial >= 0)
        {
            float SURROUNDING_PLANE_SIZE = 1000.0f;
            float SURROUNDING_PLANE_SCALE = 10.0f;

            float surroundingLevel = configSurroundings.surroundingLevel.EvaluateInt(gameManagerUnity.world);
            surroundingLevel += configSurroundings.surroundingOffsetY;

            CubeWorld.World.CubeWorld world = gameManagerUnity.world;

            goContainer = new GameObject();
            goContainer.name = "Surroundings";
            goContainer.transform.position = new Vector3(0, 0, 0);
            goContainer.transform.rotation = Quaternion.identity;
            goContainer.transform.localScale = new Vector3(1, 1, 1);

            GameObject goPlane1 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            goPlane1.name = "A1";
            goPlane1.transform.parent = goContainer.transform;
            GameObject goPlane2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            goPlane2.name = "A2";
            goPlane2.transform.parent = goContainer.transform;
            GameObject goPlane3 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            goPlane3.name = "A3";
            goPlane3.transform.parent = goContainer.transform;
            GameObject goPlane4 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            goPlane4.name = "A4";
            goPlane4.transform.parent = goContainer.transform;

            goPlane1.GetComponent<Renderer>().material = new Material(materialSurrounding);
            goPlane2.GetComponent<Renderer>().material = new Material(materialSurrounding);
            goPlane3.GetComponent<Renderer>().material = new Material(materialSurrounding);
            goPlane4.GetComponent<Renderer>().material = new Material(materialSurrounding);
			
            Texture2D surroundingTexture = GraphicsUnity.GetTilesetTexture((Texture2D) gameManagerUnity.materialTransparent.mainTexture, configSurroundings.surroundingMaterial);
            surroundingTexture.wrapMode = TextureWrapMode.Repeat;

            goPlane1.GetComponent<Renderer>().material.mainTexture = surroundingTexture;
            goPlane2.GetComponent<Renderer>().material.mainTexture = surroundingTexture;
            goPlane3.GetComponent<Renderer>().material.mainTexture = surroundingTexture;
            goPlane4.GetComponent<Renderer>().material.mainTexture = surroundingTexture;

            goPlane1.transform.localScale = new Vector3(world.sizeX / SURROUNDING_PLANE_SCALE, 0, SURROUNDING_PLANE_SIZE / SURROUNDING_PLANE_SCALE);
            goPlane1.transform.position = new Vector3(world.sizeX / 2.0f - 0.5f, surroundingLevel - 0.5f, -SURROUNDING_PLANE_SIZE / 2.0f - 0.5f);
            goPlane1.GetComponent<Renderer>().material.mainTextureScale = new Vector2(goPlane1.transform.localScale.x * SURROUNDING_PLANE_SCALE, goPlane1.transform.localScale.z * SURROUNDING_PLANE_SCALE);

            goPlane2.transform.localScale = new Vector3(world.sizeX / SURROUNDING_PLANE_SCALE, 0, SURROUNDING_PLANE_SIZE / SURROUNDING_PLANE_SCALE);
            goPlane2.transform.position = new Vector3(world.sizeX / 2.0f - 0.5f, surroundingLevel - 0.5f, world.sizeZ - 0.5f + SURROUNDING_PLANE_SIZE / 2.0f);
            goPlane2.GetComponent<Renderer>().material.mainTextureScale = new Vector2(goPlane2.transform.localScale.x * SURROUNDING_PLANE_SCALE, goPlane2.transform.localScale.z * SURROUNDING_PLANE_SCALE);

            goPlane3.transform.localScale = new Vector3(SURROUNDING_PLANE_SIZE / SURROUNDING_PLANE_SCALE, 0, SURROUNDING_PLANE_SIZE / SURROUNDING_PLANE_SCALE * 2.0f + world.sizeX / SURROUNDING_PLANE_SCALE);
            goPlane3.transform.position = new Vector3(world.sizeX + SURROUNDING_PLANE_SIZE / 2.0f - 0.5f, surroundingLevel - 0.5f, world.sizeZ / 2.0f - 0.5f);
            goPlane3.GetComponent<Renderer>().material.mainTextureScale = new Vector2(goPlane3.transform.localScale.x * SURROUNDING_PLANE_SCALE, goPlane3.transform.localScale.z * SURROUNDING_PLANE_SCALE);

            goPlane4.transform.localScale = new Vector3(SURROUNDING_PLANE_SIZE / SURROUNDING_PLANE_SCALE, 0, SURROUNDING_PLANE_SIZE / SURROUNDING_PLANE_SCALE * 2.0f + world.sizeX / SURROUNDING_PLANE_SCALE);
            goPlane4.transform.position = new Vector3(-SURROUNDING_PLANE_SIZE / 2.0f - 0.5f, surroundingLevel - 0.5f, world.sizeZ / 2.0f - 0.5f);
            goPlane4.GetComponent<Renderer>().material.mainTextureScale = new Vector2(goPlane4.transform.localScale.x * SURROUNDING_PLANE_SCALE, goPlane4.transform.localScale.z * SURROUNDING_PLANE_SCALE);

            goSurroundings.Add(goPlane1);
            goSurroundings.Add(goPlane2);
            goSurroundings.Add(goPlane3);
            goSurroundings.Add(goPlane4);
        }
    }


}

