using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{

    ShapeGenerator shapeGenerator;
    Mesh mesh;
    int resolution;
    int hiddenResolution;
    int divisions;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, int divisions, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.hiddenResolution = resolution-1;
        this.divisions = divisions;
        //Quaternion rot = Quaternion.Euler(90, 0, 0);
        //localUp = rot * localUp;
        //localUp = new Vector3(0, 0, 1);
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMeshMono(int meshCoords, int divisionsLOD, ColourGenerator colourGenerator, MonoBehaviour mono)
    {
        mono.StopAllCoroutines();
        mono.StartCoroutine(ConstructMeshCo(meshCoords, meshCoords, divisionsLOD, Vector3.zero, colourGenerator));
    }

    public void ConstructMesh(int xMesh, int yMesh, int divisions, Vector3 center)
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int[] realTriangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        Vector2[] uv = (mesh.uv.Length == vertices.Length)?mesh.uv:new Vector2[vertices.Length];

        Vector3[] norms = new Vector3[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x-0, y-0) / (resolution - 1 - 0);
                Vector3 pointOnUnitCube = localUp + ((float)xMesh / divisions + percent.x / divisions - .5f) * 2 * axisA + ((float)yMesh / divisions + percent.y / divisions - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                uv[i].x = (unscaledElevation > 0) ? unscaledElevation : unscaledElevation * 1;
                if (unscaledElevation > 0.2f)
                {
                    //Debug.Log(unscaledElevation);
                }

                if (x < resolution - 1 && y < resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    if (x < hiddenResolution - 1 && y < hiddenResolution - 1)
                    {
                        realTriangles[triIndex] = i;
                        realTriangles[triIndex + 1] = i + resolution + 1;
                        realTriangles[triIndex + 2] = i + resolution;

                        realTriangles[triIndex + 3] = i;
                        realTriangles[triIndex + 4] = i + 1;
                        realTriangles[triIndex + 5] = i + resolution + 1;
                    }
                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        //mesh.triangles = realTriangles;
        mesh.uv = uv;

        norms = mesh.normals;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if (x == 0 || x == (resolution-1) || y == 0 || y == (resolution-1))
                {
                    int i = x + y * resolution;
                    Vector2 percent = new Vector2(x - 0, y - 0) / (resolution - 1 - 0);
                    Vector3 pointOnUnitCube = localUp + ((float)xMesh / divisions + percent.x / divisions - .5f) * 2 * axisA + ((float)yMesh / divisions + percent.y / divisions - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    norms[i] = pointOnUnitSphere;
                }
            }
        }
        mesh.normals = norms;

    }

    public IEnumerator ConstructMeshCo(int xMesh, int yMesh, int divisions, Vector3 center, ColourGenerator colourGenerator)
    {
        //Debug.Log("coroutine starting");
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];
        Vector2[] uvxy = new Vector2[vertices.Length]; //uv that gives the shader the x and y position of the current vertex.
        Vector2[] uvze = new Vector2[vertices.Length]; //z position and elevation of terrain. (I couldn't find a good way to pass anything other than a vector2 into a shader)

        Vector3[] norms = new Vector3[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + ((float)xMesh / divisions + percent.x / divisions - .5f) * 2 * axisA + ((float)yMesh / divisions + percent.y / divisions - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                uv[i].x = (unscaledElevation > 0) ? unscaledElevation +0.0002f : unscaledElevation - 0.00009f;
                //if (unscaledElevation > 0)
                //{
                uvxy[i].x = vertices[i].x;
                uvxy[i].y = vertices[i].y;
                uvze[i].x = vertices[i].z;
                uvze[i].y = unscaledElevation;
                //}

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
            if(y % 10 == 0)
            {
                //Debug.Log("loop" + y);
                yield return null;

            }
        }
        //Debug.Log("coroutine ending");
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = uv;
        mesh.uv2 = uvxy;
        mesh.uv3 = uvze;
        UpdateUVs(colourGenerator, xMesh, yMesh);

        norms = mesh.normals;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if (x == 0 || x == (resolution - 1) || y == 0 || y == (resolution - 1))
                {
                    int i = x + y * resolution;
                    Vector2 percent = new Vector2(x - 0, y - 0) / (resolution - 1 - 0);
                    Vector3 pointOnUnitCube = localUp + ((float)xMesh / divisions + percent.x / divisions - .5f) * 2 * axisA + ((float)yMesh / divisions + percent.y / divisions - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    norms[i] = pointOnUnitSphere;
                }
            }
        }
        mesh.normals = norms;

    }

    public void UpdateUVs(ColourGenerator colourGenerator, int xMesh, int yMesh)
    {
        Vector2[] uv = mesh.uv;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + ((float)xMesh/divisions + percent.x/divisions - .5f) * 2 * axisA + ((float)yMesh/divisions + percent.y/divisions - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                uv[i].y = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
            }
        }
        mesh.uv = uv;
    }

}
