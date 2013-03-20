using UnityEngine;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.Items;

public class ItemTileUnity : MonoBehaviour
{
    public const float ITEM_TILE_SCALE = CubeWorld.Utils.Graphics.ITEM_TILE_SIZE / CubeWorld.Utils.Graphics.TILE_SIZE;
    public const float ROTATION_SPEED = 180.0f;

    public GameManagerUnity gameManagerUnity;
    public ItemTile item;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private bool meshValid;

    private int currentAmbientLuminance;
    private int currentLightSourceLuminance;
    private int currentGlobalAmbientLuminance;

    void Awake()
    {
        mesh = new Mesh();
        meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        meshRenderer = (MeshRenderer)gameObject.AddComponent(typeof(MeshRenderer));

        meshRenderer.castShadows = false;
        meshRenderer.receiveShadows = false;

        meshFilter.mesh = mesh;
    }

    void Start()
    {
        currentLightSourceLuminance = Tile.MAX_LUMINANCE - 1;
        currentAmbientLuminance = Tile.MAX_LUMINANCE - 1;

        transform.position = GraphicsUnity.CubeWorldVector3ToVector3(item.position);
        transform.localScale = new Vector3(ITEM_TILE_SCALE, ITEM_TILE_SCALE, ITEM_TILE_SCALE);
        transform.localRotation = Quaternion.Euler(45.0f, 0.0f, 0.0f);

        TilePosition tilePos = GraphicsUnity.Vector3ToTilePosition(GraphicsUnity.CubeWorldVector3ToVector3(item.position));

        if (gameManagerUnity.world.tileManager.IsValidTile(tilePos))
        {
            Tile tileInfo = gameManagerUnity.world.tileManager.GetTile(tilePos);

            currentAmbientLuminance = tileInfo.AmbientLuminance;
            currentLightSourceLuminance = tileInfo.LightSourceLuminance;
            currentGlobalAmbientLuminance = gameManagerUnity.world.dayCycleManager.ambientLightLuminance;
        }

        UpdateMesh();
    }

    public void Update()
    {
        transform.position = GraphicsUnity.CubeWorldVector3ToVector3(item.position);
        transform.localRotation = Quaternion.Euler(0.0f, ROTATION_SPEED * Time.deltaTime, 0.0f) * transform.localRotation;

        TilePosition tilePos = GraphicsUnity.Vector3ToTilePosition(transform.position);

        if (gameManagerUnity.world.tileManager.IsValidTile(tilePos))
        {
            Tile tileInfo = gameManagerUnity.world.tileManager.GetTile(tilePos);

            if (tileInfo.AmbientLuminance != currentAmbientLuminance ||
                    tileInfo.LightSourceLuminance != currentLightSourceLuminance ||
                    gameManagerUnity.world.dayCycleManager.ambientLightLuminance != currentGlobalAmbientLuminance)
            {
                currentAmbientLuminance = tileInfo.AmbientLuminance;
                currentLightSourceLuminance = tileInfo.LightSourceLuminance;
                currentGlobalAmbientLuminance = gameManagerUnity.world.dayCycleManager.ambientLightLuminance;

                UpdateLight();
            }
        }
    }
	
    static private List<Vector3> vertices = new List<Vector3>();
    static private List<Vector2> uvs = new List<Vector2>();
    static private List<Vector3> normals = new List<Vector3>();
    static private List<Color> colors = new List<Color>();

    static private List<int> trianglesNormal = new List<int>();
    static private List<int> trianglesTransparent = new List<int>();
    static private List<int> trianglesTranslucid = new List<int>();

    public void UpdateMesh()
    {
        mesh.Clear();

        colors.Clear();
        vertices.Clear();
        uvs.Clear();
        normals.Clear();
        trianglesNormal.Clear();
        trianglesTransparent.Clear();
        trianglesTranslucid.Clear();
        
        int index = 0;
        float uvdelta = 1.0f / GraphicsUnity.TILE_PER_MATERIAL_ROW;

        float ambientLightIntensity = MeshUtils.luminanceMapper[currentGlobalAmbientLuminance];
        float lightIntensity = ambientLightIntensity *
                            MeshUtils.luminanceMapper[currentAmbientLuminance] +
                            MeshUtils.luminanceMapper[currentLightSourceLuminance];

        if (lightIntensity > 1.0f)
            lightIntensity = 1.0f;

        TileDefinition tileDefinition = item.itemTileDefinition.tileDefinition;

        List<int> triangles;
        Vector3[] faceVectors;
        TileDefinition.DrawMode drawMode = tileDefinition.drawMode;

        if (drawMode == TileDefinition.DrawMode.SOLID)
            triangles = trianglesNormal;
        else if (drawMode == TileDefinition.DrawMode.SOLID_ALPHA)
            triangles = trianglesTranslucid;
        else
            triangles = trianglesTransparent;

        //TODO: Implemented liquid!
        //if (drawMode == TileDefinition.DrawMode.LIQUID)
        //    faceVectors = MeshUtils.faceVectorsLiquid[0];
        //else
            faceVectors = MeshUtils.faceVectorsNormal;

        for (int face = 0; face < 6; face++)
        {
            int material = tileDefinition.materials[face];
            if (material < 0)
                continue;

            Color faceColor = new Color(lightIntensity * MeshUtils.faceBright[face],
                                        lightIntensity * MeshUtils.faceBright[face],
                                        lightIntensity * MeshUtils.faceBright[face]);

            Vector3 faceNormal = MeshUtils.faceNormals[face];

            for (int i = 0; i < 4; i++)
            {
                vertices.Add(faceVectors[(face << 2) + i]);
                normals.Add(faceNormal);
                colors.Add(faceColor);
            }

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 0);

            float uvx = uvdelta * (material % GraphicsUnity.TILE_PER_MATERIAL_ROW);
            float uvy = 1.0f - uvdelta * (material / GraphicsUnity.TILE_PER_MATERIAL_ROW);

            uvs.Add(new Vector2(uvx, uvy - uvdelta));
            uvs.Add(new Vector2(uvx, uvy));
            uvs.Add(new Vector2(uvx + uvdelta, uvy));
            uvs.Add(new Vector2(uvx + uvdelta, uvy - uvdelta));

            index += 4;
        }

        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.subMeshCount = 4;

        List<Material> materials = new List<Material>();

        if (trianglesNormal.Count > 0)
            materials.Add(gameManagerUnity.material);

        if (trianglesTranslucid.Count > 0)
            materials.Add(gameManagerUnity.materialTranslucid);

        if (trianglesTransparent.Count > 0)
            materials.Add(gameManagerUnity.materialTransparent);

        meshRenderer.sharedMaterials = materials.ToArray();

        int trianglesGroupIndex = 0;

        if (trianglesNormal.Count > 0)
            mesh.SetTriangles(trianglesNormal.ToArray(), trianglesGroupIndex++);
        if (trianglesTranslucid.Count > 0)
            mesh.SetTriangles(trianglesTranslucid.ToArray(), trianglesGroupIndex++);
        if (trianglesTransparent.Count > 0)
            mesh.SetTriangles(trianglesTransparent.ToArray(), trianglesGroupIndex++);
		
        meshValid = true;
    }

    /**
     * Only update lighting, it works only if the mesh hasn't changed, so we can change only the color components.
     */
    public void UpdateLight()
    {
        if (meshValid)
        {
            Color[] colors = mesh.colors;

            int index = 0;

            float ambientLightIntensity = MeshUtils.luminanceMapper[currentGlobalAmbientLuminance];
            float lightIntensity = ambientLightIntensity * 
                                MeshUtils.luminanceMapper[currentAmbientLuminance] +
                                MeshUtils.luminanceMapper[currentLightSourceLuminance];

            if (lightIntensity > 1.0f)
                lightIntensity = 1.0f;

            TileDefinition tileDefinition = item.itemTileDefinition.tileDefinition;
            
            for (int face = 0; face < 6; face++)
            {
                int material = tileDefinition.materials[face];
                if (material < 0)
                    continue;

                Color faceColor = new Color(lightIntensity * MeshUtils.faceBright[face],
                                            lightIntensity * MeshUtils.faceBright[face],
                                            lightIntensity * MeshUtils.faceBright[face]);

                for (int i = 0; i < 4; i++)
                    colors[index++] = faceColor;
            }

            mesh.colors = colors;
        }
    }
}

