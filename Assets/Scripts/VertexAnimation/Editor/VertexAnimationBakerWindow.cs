#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VertexAnimationBakerWindow : EditorWindow
{
    private GameObject          root;
    private SkinnedMeshRenderer renderer;

    [SerializeField]
    private AnimationClip[]     clips;

    private int                 samples;
    private Vector3             rotation;
    private bool                spawnDebugMesh;

    private SerializedObject    target;
    private SerializedProperty  clipsProperty;

    private GUIContent rootContent;
    private GUIContent rendererContent;
    private GUIContent samplesContent;
    private GUIContent spawnDebugMeshContent;
    private GUIContent rotationContent;

    [MenuItem("Tools/Vertex Animation Baker")]
    public static void OpenWindow()
    {
        var window = GetWindow<VertexAnimationBakerWindow>();
        window.titleContent = new GUIContent()
        {
            text = "Vertex Animation Baker",
        };
        window.Show();
    }

    private void OnEnable()
    {
        target          = new SerializedObject(this);
        clipsProperty   = target.FindProperty("clips");

        rootContent             = new GUIContent("Root");
        rendererContent         = new GUIContent("Renderer");
        samplesContent          = new GUIContent("Samples");
        spawnDebugMeshContent   = new GUIContent("Spawn Debug Mesh");
        rotationContent         = new GUIContent("Rotation");
    }

    private void OnGUI()
    {
        root            = EditorGUILayout.ObjectField(rootContent, root, typeof(GameObject), true) as GameObject;
        renderer        = EditorGUILayout.ObjectField(rendererContent, renderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
        samples         = EditorGUILayout.IntField(samplesContent, samples);
        rotation        = EditorGUILayout.Vector3Field(rotationContent, rotation);
        spawnDebugMesh  = EditorGUILayout.Toggle(spawnDebugMeshContent, spawnDebugMesh);
        EditorGUILayout.PropertyField(clipsProperty);

        if(target.hasModifiedProperties)
        {
            target.ApplyModifiedProperties();
        }

        if(GUILayout.Button("Bake"))
        {
            string path = GetSaveFilePath();
            if(!string.IsNullOrEmpty(path))
            {
                BakeVertexAnimations(path);
            }
            //BakeVertexAnimations();
        }
    }

    private string GetSaveFilePath()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save as", root.name + "_Animations", "asset", "Ok");
        Debug.Log(path);
        return path;
    }

    public void BakeVertexAnimations(string filePath)
    {
        MeshVertexAnimations meshAnimations = ScriptableObject.CreateInstance<MeshVertexAnimations>();
        meshAnimations.mesh = renderer.sharedMesh;
        meshAnimations.animations = new VertexAnimation[clips.Length];

        int meshVertexCount = renderer.sharedMesh.vertices.Length;

        Debug.Log($"Baking vertex animations. {meshVertexCount} mesh vertices.");

        GameObject debugRoot = null;
        if (spawnDebugMesh)
        {
            debugRoot = new GameObject(root.name + "_Animations");
        }

        Quaternion rot = Quaternion.Euler(rotation);

        for (int c = 0; c < clips.Length; ++c)
        {
            VertexAnimation vertexAnimation = new VertexAnimation();
            vertexAnimation.name = clips[c].name;
            vertexAnimation.frames = samples;
            vertexAnimation.vertices = new Vector3[samples * meshVertexCount];

            float interval = clips[c].length / samples;

            GameObject animRoot = null;
            if (spawnDebugMesh)
            {
                animRoot = new GameObject(clips[c].name);
                animRoot.transform.SetParent(debugRoot.transform);
            }

            for (int i = 0; i < samples; ++i)
            {
                clips[c].SampleAnimation(root, i * interval);

                var mesh = new Mesh();
                renderer.BakeMesh(mesh, false);

                var vertices = mesh.vertices;
                for (int v = 0; v < vertices.Length; ++v)
                {
                    vertices[v] = rot * vertices[v];
                }
                System.Array.Copy(vertices, 0, vertexAnimation.vertices, i * meshVertexCount, vertices.Length);

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.SetParent(animRoot.transform);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            meshAnimations.animations[c] = vertexAnimation;

            Debug.Log($"Baked animation \'{vertexAnimation.name}\'. {vertexAnimation.vertices.Length} vertices. ({vertexAnimation.vertices.Length * 3} bytes)");
        }
        AssetDatabase.CreateAsset(meshAnimations, filePath);
        AssetDatabase.SaveAssets();

    }
}

#endif