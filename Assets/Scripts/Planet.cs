using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

    [Range(2, 255)]
    public int resolution = 10;
    //[Range(1, 7)]
    public int divisions = 1;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, None, Top, Bottom, Left, Right, Front, Back };
    public int maskException;
    public FaceRenderMask faceRenderMask;
    bool[] genFace;

    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;

    public GameObject playercam;

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    MeshFilter meshFilterLOD;
    TerrainFace terrainFaceLOD;


    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);

        //if (meshFilters == null || meshFilters.Length == 0 || meshFilters.Length < 6*divisions)
        //{
        var children = new List<GameObject>();
        foreach (Transform child in gameObject.transform)
        {
            children.Add(child.gameObject);
        }
        children.ForEach(child => DestroyImmediate(child));
        meshFilters = new MeshFilter[6*divisions * divisions];
        //}
        terrainFaces = new TerrainFace[6*divisions * divisions];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6*divisions * divisions; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh" + i);
                meshObj.transform.parent = transform;
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.transform.localScale = Vector3.one;
                meshObj.layer = meshObj.transform.parent.gameObject.layer;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;
            meshFilters[i].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshFilters[i].GetComponent<MeshRenderer>().receiveShadows = false;
            genFace = new bool[6 * divisions * divisions];
            genFace[i] = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 2 == i / (divisions * divisions) || maskException == i;
            if (genFace[i])
            {
                terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, divisions, directions[i/(divisions * divisions)]);
            }
            else
            {
                terrainFaces[i] = null;
            }
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 2 == i/(divisions * divisions) || maskException == i;
            //Debug.Log(renderFace);
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
    }
    public void GenerateLOD()
    {
        float camDist = Vector3.Distance(transform.position, playercam.transform.position);
        //float camAngle = 
        float scale = transform.lossyScale.x;
        //Mesh[] meshLODs = new Mesh[];
        int divisionsLOD = divisions;
        int meshCoords = divisionsLOD / 2; //Automatically rounded down because of int division.
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);

        /* Delete all children:
        var children = new List<GameObject>();
        foreach (Transform child in gameObject.transform)
        {
            children.Add(child.gameObject);
        }
        children.ForEach(child => DestroyImmediate(child));*/

        if (meshFilterLOD == null)
        {
            GameObject meshObj = new GameObject("meshLOD");
            meshObj.transform.parent = transform;
            meshObj.transform.localPosition = Vector3.zero;
            meshObj.transform.localScale = Vector3.one;
            meshObj.layer = meshObj.transform.parent.gameObject.layer;

            meshObj.AddComponent<MeshRenderer>();
            if (transform.lossyScale.magnitude < Mathf.Pow(90000, 2))
            {
                meshObj.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshObj.GetComponent<MeshRenderer>().receiveShadows = false;
            }
            meshFilterLOD = meshObj.AddComponent<MeshFilter>();
            meshFilterLOD.sharedMesh = new Mesh();
        }
        meshFilterLOD.GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;
        terrainFaceLOD = new TerrainFace(shapeGenerator, meshFilterLOD.sharedMesh, resolution, divisionsLOD, playercam.transform.position.normalized);
        terrainFaceLOD.ConstructMeshMono(meshCoords, divisionsLOD, colourGenerator, this);
        //StartCoroutine(terrainFaceLOD.ConstructMeshCo(meshCoords, meshCoords, divisionsLOD, Vector3.zero, colourGenerator)); Doesn't work for some reason? Had to pass the monobehaviour into terrainFaceLOD and start the coroutine there (see above).
        /* Non coroutine version:
        terrainFaceLOD.ConstructMeshCo(meshCoords, meshCoords, divisionsLOD, Vector3.zero, colourGenerator);
        terrainFaceLOD.UpdateUVs(colourGenerator, meshCoords, meshCoords); */
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
            GenerateColours();
        }
    }

    public void OnColourSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
            GenerateColours();
        }
    }

    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int y=0; y<divisions; y++)
            {
                for (int x=0; x<divisions; x++)
                {
                    if (meshFilters[i * divisions * divisions + y * divisions + x].gameObject.activeSelf)
                    {
                        terrainFaces[i*divisions*divisions + y*divisions + x].ConstructMesh(x,y, divisions, transform.position);
                    }
                }
            }
        }
        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColours()
    {
        colourGenerator.UpdateColours();
        for (int i = 0; i < 6; i++)
        {
            for (int y = 0; y < divisions; y++)
            {
                for (int x = 0; x < divisions; x++)
                {
                    if (meshFilters[i * divisions * divisions + y * divisions + x].gameObject.activeSelf)
                    {
                        terrainFaces[i*divisions*divisions + y*divisions + x].UpdateUVs(colourGenerator,x,y);
                    }
                }
            }
        }
    }
}
