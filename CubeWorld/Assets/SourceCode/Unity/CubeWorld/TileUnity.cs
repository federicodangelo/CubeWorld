using UnityEngine;
using System.Collections.Generic;
using CubeWorld.Tiles;

public class TileUnity : MonoBehaviour
{
    public GameManagerUnity gameManagerUnity;
    public DynamicTile tile;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private bool meshValid;

    private int currentAmbientLuminance;
    private int currentLightSourceLuminance;
    private int currentGlobalAmbientLuminance;

    private bool useFullLightSimulation;

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

        TilePosition tilePos;

        if (tile.IsProxy)
            tilePos = tile.tilePosition;
        else
            tilePos = GraphicsUnity.Vector3ToTilePosition(transform.position);

        if (gameManagerUnity.world.tileManager.IsValidTile(tilePos))
        {
            Tile tileInfo = gameManagerUnity.world.tileManager.GetTile(tilePos);

            if (tile.IsProxy)
                useFullLightSimulation = true;

            currentAmbientLuminance = tileInfo.AmbientLuminance;
            currentLightSourceLuminance = tileInfo.LightSourceLuminance;
            currentGlobalAmbientLuminance = gameManagerUnity.world.dayCycleManager.ambientLightLuminance;
        }

        UpdateMesh();
    }

    public void Update()
    {
		if (tile.IsProxy)
			transform.position = GraphicsUnity.CubeWorldVector3ToVector3(tile.position);
			
        TilePosition tilePos = GraphicsUnity.Vector3ToTilePosition(transform.position);

        if (gameManagerUnity.world.tileManager.IsValidTile(tilePos))
        {
            Tile tileInfo = gameManagerUnity.world.tileManager.GetTile(tilePos);

            if (tile.Invalidated)
            {
                currentAmbientLuminance = tileInfo.AmbientLuminance;
                currentLightSourceLuminance = tileInfo.LightSourceLuminance;
                currentGlobalAmbientLuminance = gameManagerUnity.world.dayCycleManager.ambientLightLuminance;

                UpdateMesh();

                tile.Invalidated = false;
            }
            else if (tileInfo.AmbientLuminance != currentAmbientLuminance ||
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
    static private List<Vector2> uvs2 = new List<Vector2>();
    static private List<Vector3> normals = new List<Vector3>();
    static private List<Color> colors = new List<Color>();

    static private List<int> trianglesNormal = new List<int>();
    static private List<int> trianglesTransparent = new List<int>();
    static private List<int> trianglesTranslucid = new List<int>();
    static private List<int> trianglesDamage = new List<int>();

    public void UpdateMesh()
    {
        mesh.Clear();

        colors.Clear();
        vertices.Clear();
        uvs.Clear();
        uvs2.Clear();
        normals.Clear();
        trianglesNormal.Clear();
        trianglesTransparent.Clear();
        trianglesTranslucid.Clear();
        trianglesDamage.Clear();

        int index = 0;
        float uvdelta = 1.0f / GraphicsUnity.TILE_PER_MATERIAL_ROW;

        float ambientLightIntensity = MeshUtils.luminanceMapper[currentGlobalAmbientLuminance];
        float lightIntensity = ambientLightIntensity *
                            MeshUtils.luminanceMapper[currentAmbientLuminance] +
                            MeshUtils.luminanceMapper[currentLightSourceLuminance];

        if (lightIntensity > 1.0f)
            lightIntensity = 1.0f;

        if (tile.tileDefinition.tileType != TileDefinition.EMPTY_TILE_TYPE)
        {
            TileDefinition tileDefinition = tile.tileDefinition;

            List<int> triangles;
            Vector3[] faceVectors;
            TileDefinition.DrawMode drawMode = tileDefinition.drawMode;

            if (drawMode == TileDefinition.DrawMode.SOLID)
            {
                if (tile.Energy == tileDefinition.energy)
                    triangles = trianglesNormal;
                else
                    triangles = trianglesDamage;
            }
            else if (drawMode == TileDefinition.DrawMode.SOLID_ALPHA)
                triangles = trianglesTranslucid;
            else
                triangles = trianglesTransparent;

            //TODO: Implemented liquid
            //if (drawMode == TileDefinition.DrawMode.LIQUID)
            //    faceVectors = MeshUtils.faceVectorsLiquid[0];
            //else
                faceVectors = MeshUtils.faceVectorsNormal;

            for (int face = 0; face < 6; face++)
            {
                int material = tileDefinition.materials[face];
                if (material < 0)
                    continue;

                if (useFullLightSimulation == true)
                {
                    TilePosition normalInt = MeshUtils.faceNormalsTile[face];
                    TilePosition near = tile.tilePosition + normalInt;

                    if (tile.world.tileManager.IsValidTile(near))
                    {
                        lightIntensity = ambientLightIntensity *
                                        MeshUtils.luminanceMapper[tile.world.tileManager.GetTileAmbientLuminance(near)] +
                                        MeshUtils.luminanceMapper[tile.world.tileManager.GetTileLightSourceLuminance(near)];

                        if (lightIntensity > 1.0f)
                            lightIntensity = 1.0f;
                    }
                }

                Color faceColor = new Color(lightIntensity * MeshUtils.faceBright[face],
                                            lightIntensity * MeshUtils.faceBright[face],
                                            lightIntensity * MeshUtils.faceBright[face]);

                Vector3 faceNormal = MeshUtils.faceNormals[face];

                if (tile.OnFire && MeshUtils.faceVectorsFireAvailable[face])
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vertices.Add(MeshUtils.faceVectorsFire[(face << 2) + i]);
                        normals.Add(faceNormal);
                        colors.Add(faceColor);
                    }

                    trianglesTranslucid.Add(index + 0);
                    trianglesTranslucid.Add(index + 1);
                    trianglesTranslucid.Add(index + 2);

                    trianglesTranslucid.Add(index + 2);
                    trianglesTranslucid.Add(index + 3);
                    trianglesTranslucid.Add(index + 0);

                    int fireMaterial = gameManagerUnity.extraMaterials.fireMaterials[face % gameManagerUnity.extraMaterials.fireMaterials.Length];

                    float uvxFire = uvdelta * (fireMaterial % GraphicsUnity.TILE_PER_MATERIAL_ROW);
                    float uvyFire = 1.0f - uvdelta * (fireMaterial / GraphicsUnity.TILE_PER_MATERIAL_ROW);

                    uvs.Add(new Vector2(uvxFire, uvyFire - uvdelta));
                    uvs.Add(new Vector2(uvxFire, uvyFire));
                    uvs.Add(new Vector2(uvxFire + uvdelta, uvyFire));
                    uvs.Add(new Vector2(uvxFire + uvdelta, uvyFire - uvdelta));

                    uvs2.Add(Vector2.zero);
                    uvs2.Add(Vector2.zero);
                    uvs2.Add(Vector2.zero);
                    uvs2.Add(Vector2.zero);

                    index += 4;
                }

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

                if (drawMode == TileDefinition.DrawMode.SOLID && tile.Energy < tileDefinition.energy)
                {
                    int materialDamageIndex = ((tileDefinition.energy - tile.Energy) * gameManagerUnity.extraMaterials.damageMaterials.Length) / tileDefinition.energy;
                    if (materialDamageIndex >= gameManagerUnity.extraMaterials.damageMaterials.Length)
                        materialDamageIndex = gameManagerUnity.extraMaterials.damageMaterials.Length - 1;

                    int materialDamage = gameManagerUnity.extraMaterials.damageMaterials[materialDamageIndex];

                    uvx = uvdelta * (materialDamage % GraphicsUnity.TILE_PER_MATERIAL_ROW);
                    uvy = 1.0f - uvdelta * (materialDamage / GraphicsUnity.TILE_PER_MATERIAL_ROW);

                    uvs2.Add(new Vector2(uvx, uvy - uvdelta));
                    uvs2.Add(new Vector2(uvx, uvy));
                    uvs2.Add(new Vector2(uvx + uvdelta, uvy));
                    uvs2.Add(new Vector2(uvx + uvdelta, uvy - uvdelta));
                }
                else
                {
                    uvs2.Add(Vector2.zero);
                    uvs2.Add(Vector2.zero);
                    uvs2.Add(Vector2.zero);
                    uvs2.Add(Vector2.zero);
                }

                index += 4;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uvs2.ToArray();
        mesh.subMeshCount = 4;

        List<Material> materials = new List<Material>();

        if (trianglesNormal.Count > 0)
            materials.Add(gameManagerUnity.material);

        if (trianglesTranslucid.Count > 0)
            materials.Add(gameManagerUnity.materialTranslucid);

        if (trianglesTransparent.Count > 0)
            materials.Add(gameManagerUnity.materialTransparent);

        if (trianglesDamage.Count > 0)
            materials.Add(gameManagerUnity.materialDamaged);

        meshRenderer.sharedMaterials = materials.ToArray();

        int trianglesGroupIndex = 0;

        if (trianglesNormal.Count > 0)
            mesh.SetTriangles(trianglesNormal.ToArray(), trianglesGroupIndex++);
        if (trianglesTranslucid.Count > 0)
            mesh.SetTriangles(trianglesTranslucid.ToArray(), trianglesGroupIndex++);
        if (trianglesTransparent.Count > 0)
            mesh.SetTriangles(trianglesTransparent.ToArray(), trianglesGroupIndex++);
        if (trianglesDamage.Count > 0)
            mesh.SetTriangles(trianglesDamage.ToArray(), trianglesGroupIndex++);
		
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

            if (tile.tileDefinition.tileType != TileDefinition.EMPTY_TILE_TYPE)
            {
                TileDefinition tileDefinition = tile.tileDefinition;
                
                for (int face = 0; face < 6; face++)
                {
                    int material = tileDefinition.materials[face];
                    if (material < 0)
                        continue;

                    if (useFullLightSimulation == true)
                    {
                        TilePosition normalInt = MeshUtils.faceNormalsTile[face];
                        TilePosition near = tile.tilePosition + normalInt;

                        if (tile.world.tileManager.IsValidTile(near))
                        {
                            lightIntensity = ambientLightIntensity *
                                            MeshUtils.luminanceMapper[tile.world.tileManager.GetTileAmbientLuminance(near)] +
                                            MeshUtils.luminanceMapper[tile.world.tileManager.GetTileLightSourceLuminance(near)];

                            if (lightIntensity > 1.0f)
                                lightIntensity = 1.0f;
                        }
                    }

                    Color faceColor = new Color(lightIntensity * MeshUtils.faceBright[face],
                                                lightIntensity * MeshUtils.faceBright[face],
                                                lightIntensity * MeshUtils.faceBright[face]);

                    if (tile.OnFire && MeshUtils.faceVectorsFireAvailable[face])
                        for (int i = 0; i < 4; i++)
                            colors[index++] = faceColor;

                    for (int i = 0; i < 4; i++)
                        colors[index++] = faceColor;
                }
            }

            mesh.colors = colors;
        }
    }
}

