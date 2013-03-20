using System;
using System.Collections.Generic;
using CubeWorld.World.Objects;
using UnityEngine;
using CubeWorld.Tiles;

public class VisualDefinitionRenderUnity : MonoBehaviour
{
    static private List<Vector3> vertices = new List<Vector3>();
    static private List<Vector3> normals = new List<Vector3>();
    static private List<Color> colors = new List<Color>();
    static private List<int> trianglesNormal = new List<int>();

    public CWVisualDefinition visualDefinition;
    public Material material;
    public CubeWorld.World.CubeWorld world; 

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    private int currentAmbientLuminance;
    private int currentLightSourceLuminance;
    private int currentGlobalAmbientLuminance;

    private void Awake()
    {
        meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        meshRenderer = (MeshRenderer)gameObject.AddComponent(typeof(MeshRenderer));

        meshRenderer.castShadows = false;
        meshRenderer.receiveShadows = false;
    }


    void Start()
    {
        currentLightSourceLuminance = Tile.MAX_LUMINANCE - 1;
        currentAmbientLuminance = Tile.MAX_LUMINANCE - 1;

        TilePosition tilePos = GraphicsUnity.Vector3ToTilePosition(transform.position);

        if (world != null && world.tileManager.IsValidTile(tilePos))
        {
            Tile tileInfo = world.tileManager.GetTile(tilePos);

            currentAmbientLuminance = tileInfo.AmbientLuminance;
            currentLightSourceLuminance = tileInfo.LightSourceLuminance;
            currentGlobalAmbientLuminance = world.dayCycleManager.ambientLightLuminance;
        }

        UpdateMesh();
    }

    public void Update()
    {
        if (world != null)
        {
            TilePosition tilePos = GraphicsUnity.Vector3ToTilePosition(transform.position);

            if (world.tileManager.IsValidTile(tilePos))
            {
                Tile tileInfo = world.tileManager.GetTile(tilePos);

                if (tileInfo.AmbientLuminance != currentAmbientLuminance ||
                    tileInfo.LightSourceLuminance != currentLightSourceLuminance ||
                    world.dayCycleManager.ambientLightLuminance != currentGlobalAmbientLuminance)
                {
                    currentAmbientLuminance = tileInfo.AmbientLuminance;
                    currentLightSourceLuminance = tileInfo.LightSourceLuminance;
                    currentGlobalAmbientLuminance = world.dayCycleManager.ambientLightLuminance;

                    UpdateLighting();
                }
            }
        }
    }

    private float GetLightIntensity()
    {
        return Math.Min(MeshUtils.luminanceMapper[currentGlobalAmbientLuminance] *
                MeshUtils.luminanceMapper[currentAmbientLuminance] +
                MeshUtils.luminanceMapper[currentLightSourceLuminance], 1.0f);
    }

    private void UpdateMesh()
    {
        float lightIntensity = GetLightIntensity();

        meshFilter.sharedMesh = GetMeshForDefinition(visualDefinition, lightIntensity);

        meshRenderer.sharedMaterial = material;
    }

    private void UpdateLighting()
    {
        float lightIntensity = GetLightIntensity();

        meshFilter.sharedMesh = GetMeshForDefinition(visualDefinition, lightIntensity);
    }

    static private Dictionary<CWVisualDefinition, Mesh> cacheBaseMeshes = new Dictionary<CWVisualDefinition, Mesh>();
    static private Dictionary<CWVisualDefinition, Dictionary<int, Mesh>> cacheMeshes = new Dictionary<CWVisualDefinition, Dictionary<int, Mesh>>();

    static private Mesh GetMeshForDefinition(CWVisualDefinition visualDefinition, float lightIntensity)
    {
        int lightIntensityI = (int)(lightIntensity * 10);
        lightIntensity = (float) lightIntensityI / 10.0f;

        Mesh mesh = null;

        if (cacheMeshes.ContainsKey(visualDefinition))
            if (cacheMeshes[visualDefinition].ContainsKey(lightIntensityI))
                mesh = cacheMeshes[visualDefinition][lightIntensityI];

        if (mesh == null)
        {
            Mesh baseMesh = GetBaseMeshForDefinition(visualDefinition);
            Color[] colors = baseMesh.colors;

            for (int i = 0; i < colors.Length; i++)
                colors[i] *= lightIntensity;

            mesh = new Mesh();

            mesh.vertices = baseMesh.vertices;
            mesh.colors = colors;
            mesh.normals = baseMesh.normals;
            mesh.triangles = baseMesh.triangles;

            if (cacheMeshes.ContainsKey(visualDefinition) == false)
                cacheMeshes[visualDefinition] = new Dictionary<int, Mesh>();

            cacheMeshes[visualDefinition][lightIntensityI] = mesh;
        }

        return mesh;
    }

    static private Mesh GetBaseMeshForDefinition(CWVisualDefinition visualDefinition)
    {
        Mesh mesh;

        if (cacheBaseMeshes.TryGetValue(visualDefinition, out mesh) == false)
        {
            float pixelScale = 1.0f / (float)visualDefinition.scale;
            Vector3 pivot = GraphicsUnity.CubeWorldVector3ToVector3(visualDefinition.pivot);

            mesh = new Mesh();
            Texture2D[] images = LoadImages(visualDefinition);

            colors.Clear();
            vertices.Clear();
            normals.Clear();
            trianglesNormal.Clear();

            int index = 0;

            TilePosition mainNormal;
            TilePosition d1Normal;
            TilePosition d2Normal;

            switch (visualDefinition.plane)
            {
                case "x":
                    mainNormal = new TilePosition(1, 0, 0);
                    d1Normal = new TilePosition(0, 0, 1);
                    d2Normal = new TilePosition(0, 1, 0);
                    break;

                case "y":
                    mainNormal = new TilePosition(0, 1, 0);
                    d1Normal = new TilePosition(1, 0, 0);
                    d2Normal = new TilePosition(0, 0, 1);
                    break;

                case "z":
                default:
                    mainNormal = new TilePosition(0, 0, 1);
                    d1Normal = new TilePosition(1, 0, 0);
                    d2Normal = new TilePosition(0, 1, 0);
                    break;
            }

            float halfDmain = images.Length / 2.0f;
            float halfD1 = images[0].width / 2.0f;
            float halfD2 = images[0].height / 2.0f;

            for (int dmain = 0; dmain < images.Length; dmain++)
            {
                Texture2D image = images[dmain];

                for (int d1 = 0; d1 < image.width; d1++)
                {
                    for (int d2 = 0; d2 < image.height; d2++)
                    {
                        Color colorPixel = image.GetPixel(d1, d2);

                        if (colorPixel.a == 0.0f)
                            continue;

                        Vector3 offset = new Vector3();

                        offset += GraphicsUnity.TilePositionToVector3(d1Normal) * (((d1Normal * d1).GetSumComponents() - halfD1) - halfD1 * pivot.x);
                        offset += GraphicsUnity.TilePositionToVector3(d2Normal) * (((d2Normal * d2).GetSumComponents() - halfD2) - halfD2 * pivot.y);
                        offset += GraphicsUnity.TilePositionToVector3(mainNormal) * (((mainNormal * dmain).GetSumComponents() - halfDmain) - halfDmain * pivot.z);

                        offset += new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

                        offset *= pixelScale;

                        for (int face = 0; face < 6; face++)
                        {
                            TilePosition normalInt = MeshUtils.faceNormalsTile[face];

                            int near1 = d1 + (d1Normal * normalInt).GetSumComponents();
                            int near2 = d2 + (d2Normal * normalInt).GetSumComponents();
                            int nearMain = dmain + (mainNormal * normalInt).GetSumComponents();

                            if (near1 >= 0 && near2 >= 0 && nearMain >= 0 &&
                                near1 < image.width && near2 < image.height && nearMain < images.Length)
                            {
                                if (images[nearMain].GetPixel(near1, near2).a == 1.0f)
                                    continue;
                            }

                            Color faceColor = colorPixel * MeshUtils.faceBright[face];

                            Vector3 faceNormal = MeshUtils.faceNormals[face];

                            for (int i = 0; i < 4; i++)
                            {
                                vertices.Add(MeshUtils.faceVectorsNormal[(face << 2) + i] * pixelScale + offset);
                                normals.Add(faceNormal);
                                colors.Add(faceColor);
                            }

                            trianglesNormal.Add(index + 0);
                            trianglesNormal.Add(index + 1);
                            trianglesNormal.Add(index + 2);

                            trianglesNormal.Add(index + 2);
                            trianglesNormal.Add(index + 3);
                            trianglesNormal.Add(index + 0);

                            index += 4;
                        }
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.colors = colors.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = trianglesNormal.ToArray();

            cacheBaseMeshes[visualDefinition] = mesh;
        }

        return mesh;
    }


    static private Texture2D[] LoadImages(CWVisualDefinition visualDefinition)
    {
        Texture2D[] images = new Texture2D[visualDefinition.materialCount];
        for (int i = 0; i < visualDefinition.materialCount; i++)
        {
            string resourceName;

            if (i == 0 && visualDefinition.materialCount == 1)
                resourceName = "Items/" + visualDefinition.material;
            else
                resourceName = "Items/" + visualDefinition.material + "_z" + (i + 1).ToString();

            Texture2D image = (Texture2D) Resources.Load(resourceName, typeof(Texture2D));

            if (image == null)
                Debug.LogError("Image not found: " + resourceName);

            images[i] = image;
        }

        return images;
    }
}

