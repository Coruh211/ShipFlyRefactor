//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using System.Runtime.Serialization.Formatters.Binary;

namespace FSG.MeshAnimator
{
    /// <summary>
    /// Editor Window for MeshAnimator baking
    /// </summary>
    public class MeshAnimationCreator : EditorWindow
    {
        private static readonly string version = "2.0.9";
        private static MeshAnimationCreator Instance;
        private enum MeshAnimatorType
        {
            Snapshot,
            ShaderAnimated
        }
        private enum ShaderTextureSize
        {
            Size_Smallest_Common = 0,
            Size_Largest_Common = 1,
            Size_Most_Common = 2,
            Size_32 = 32,
            Size_64 = 64,
            Size_128 = 128,
            Size_256 = 256,
            Size_512 = 512,
            Size_1024 = 1024,
            Size_2048 = 2048,
            Size_4096 = 4096
        }
        private enum ShaderTextureQuality
        {
            Quality_8_Bit__Low = TextureFormat.RGBA32,
            Quality_16_Bit__Medium = TextureFormat.RGBAHalf,
            Quality_32_Bit__High = TextureFormat.RGBAFloat,
        }
        private static readonly List<ShaderTextureSize> textureSizes = System.Enum.GetValues(typeof(ShaderTextureSize)).Cast<ShaderTextureSize>().ToList();
        private static readonly string[] textureSizeNames = System.Enum.GetNames(typeof(ShaderTextureSize)).Select(x => x.Remove(0, 5).Replace("__", " - ").Replace("_", " ")).ToArray();
        private static readonly List<ShaderTextureQuality> textureQualities = System.Enum.GetValues(typeof(ShaderTextureQuality)).Cast<ShaderTextureQuality>().ToList();
        private static readonly string[] textureQualityNames = System.Enum.GetNames(typeof(ShaderTextureQuality)).Select(x => x.Remove(0, 8).Replace("__", " - ").Replace("_", " ")).ToArray();
        private static readonly Dictionary<MeshAnimatorType, string[]> information = new Dictionary<MeshAnimatorType, string[]>()
        {
            {
                MeshAnimatorType.Snapshot, new string[]
                {
                    "Creates a copy of the mesh in memory at runtime for each frame of animation.",
                    "Requires more memory than a traditional SkinnedMeshRenderer, but CPU usage is much lower and more meshes can be displayed.",
                    "Does not require special shaders, and is compatible with all platforms and graphics devices.",
                    "Supports GPU instancing for identical meshes currently on the same frame of animation.",
                    "Compatible with all render pipelines."
                }
            },
            {
                MeshAnimatorType.ShaderAnimated, new string[]
                {
                    "Shader Animated mode creates textures that store animation data for each animation frame.",
                    "Requires special shader code to update the mesh vertex positions.",
                    "Requires 2D texture array support by the graphics device, and may not be compatible with older graphics hardware.",
                    "Supports GPU instancing and provides the highest level of performance.",
                }
            },
        };

        [System.Serializable]
        public class RenderSection : System.IDisposable
        {
            private static int _indentLevel = 0;
            public string key;
            public bool expanded;

            public void Dispose() { }

            public static RenderSection Draw(string key, GUIStyle bgStyle, System.Action drawAction, bool defaultExpand = false)
            {
                var section = Instance.sections.Find(x => x.key == key);
                if (section == null)
                {
                    section = new RenderSection() { key = key, expanded = defaultExpand };
                    Instance.sections.Add(section);
                }

                var buttonStyle = new GUIStyle(EditorStyles.foldout);
                buttonStyle.fontStyle = FontStyle.Bold;
                if (section.expanded)
                {
                    buttonStyle.normal = buttonStyle.onNormal;
                }
                _indentLevel++;
                GUIStyle bgCopy = new GUIStyle(bgStyle);
                bgCopy.margin = new RectOffset(_indentLevel * 10, _indentLevel * 10, 5, 5);
                using (new GUILayout.VerticalScope(bgCopy))
                {
                    if (GUILayout.Button(key, buttonStyle))
                        section.expanded = !section.expanded;

                    if (section.expanded)
                    {
                        drawAction();
                    }
                }
                _indentLevel--;
                return section;
            }

            public static implicit operator bool(RenderSection section)
            {
                return section.expanded;
            }
        }

        [System.Serializable]
        public class MeshBakePreferences
        {
            public int type { get; set; }
            public int texSize { get; set; }
            public int texQuality { get; set; }
            public int fps { get; set; }
            public int previousGlobalBake { get; set; }
            public int globalBake { get; set; }
            public int meshNormalMode { get; set; }
            public string[] customClips { get; set; }
            public bool customCompression { get; set; }
            public int rootMotionMode { get; set; }
            public string animController { get; set; }
            public string animAvatar { get; set; }
            public bool combineMeshes { get; set; }
            public string[] exposedTransforms { get; set; }
            public int[] lodDistanceKeys { get; set; }
            public float[] lodDistanceValues { get; set; }
            public string outputPath { get; set; }
        }
        private struct CombineInstanceMaterial
        {
            public CombineInstance combine;
            public Material material;
            public Mesh sharedMesh;
        }
        private const int menuIndex = 9999;

        [MenuItem("Assets/Create/Mesh Animator...", false, menuIndex)]
        static void MakeWindow()
        {
            window = GetWindow(typeof(MeshAnimationCreator)) as MeshAnimationCreator;
            window.orignalContentColor = GUI.contentColor;
            if (window.prefab != Selection.activeGameObject)
            {
                window.prefab = null;
                window.OnEnable();
            }
        }

        private static MeshAnimationCreator window;
        private Color orignalContentColor;
        private Vector2 scrollpos;
        private Dictionary<string, int> frameSkips = new Dictionary<string, int>();
        private Dictionary<string, bool> bakeAnims = new Dictionary<string, bool>();
        private List<KeyValuePair<int, float>> lodDistances = new List<KeyValuePair<int, float>>();

        private GUIStyle[] bgs;
        private static Color[] bgColors = new Color[]
        {
            new Color(0, 0, 0, 0.1f),
            new Color(0, 0, 0, 0.35f),
            new Color(0.25f, 0, 0, 0.35f),
            new Color(0, 0.25f, 0, 0.35f),
            new Color(0, 0, 0.25f, 0.35f),
            new Color(0.25f, 0.25f, 0, 0.35f),
            new Color(0, 0.25f, 0.25f, 0.35f),
            new Color(0, 1, 0, 0.5f),
        };

        [SerializeField]
        private Vector2 scroll;
        [SerializeField]
        private MeshAnimatorType selectedAnimatorType = MeshAnimatorType.Snapshot;
        [SerializeField]
        private ShaderTextureSize selectedTextureSize = ShaderTextureSize.Size_Smallest_Common;
        [SerializeField]
        private ShaderTextureQuality selectedTextureQuality = ShaderTextureQuality.Quality_16_Bit__Medium;
        [SerializeField]
        private int fps = 30;
        [SerializeField]
        private int previousGlobalBake = 1;
        [SerializeField]
        private int globalBake = 1;
        [SerializeField]
        private MeshNormalMode meshNormalMode = MeshNormalMode.Baked;
        [SerializeField]
        private bool useOriginalMesh = true;
        [SerializeField]
        private Texture2D tex;
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private List<AnimationClip> customClips = new List<AnimationClip>();
        [SerializeField]
        private List<MeshFilter> meshFilters = new List<MeshFilter>();
        [SerializeField]
        private List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
        [SerializeField]
        private GameObject previousPrefab;
        [SerializeField]
        private bool customCompression = false;
        [SerializeField]
        private GameObject spawnedAsset;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private RootMotionMode rootMotionMode = RootMotionMode.None;
        [SerializeField]
        private RuntimeAnimatorController animController;
        [SerializeField]
        private Avatar animAvatar;
        [SerializeField]
        private bool requiresAnimator;
        [SerializeField]
        private bool isHumanoid;
        [SerializeField]
        private List<string> exposedTransforms = new List<string>();
        [SerializeField]
        private List<RenderSection> sections = new List<RenderSection>();
        [SerializeField]
        private string outputPath;
        [SerializeField]
        private Object outputFolder;

        private bool clipsLoaded = false;

        #region Initialization
        /// Reload the target prefab
        private void OnEnable()
        {
            Instance = this;
            titleContent = new GUIContent("Mesh Animator");
            if (prefab == null && Selection.activeGameObject)
            {
                prefab = Selection.activeGameObject;
                OnPrefabChanged();
            }
        }

        /// Destroy the temporary prefab and texture
        private void OnDisable()
        {
            if (spawnedAsset)
            {
                DestroyImmediate(spawnedAsset.gameObject);
            }
            if (tex)
            {
                DestroyImmediate(tex);
            }
        }

        private void Update()
        {
            if (!clipsLoaded)
                GetClips();
        }

        /// Load the editor texture from bytes
        private void LoadTex()
        {
            tex = new Texture2D(250, 40);
            byte[] imgBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 250, 0, 0, 0, 40, 8, 6, 0, 0, 0, 215, 26, 231, 25, 0, 0, 16, 240, 73, 68, 65, 84, 120, 1, 237, 92, 11, 112, 149, 197, 21, 222, 132, 132, 144, 4, 4, 37, 34, 129, 86, 2, 50, 8, 1, 209, 242, 168, 84, 130, 164, 26, 65, 180, 3, 82, 81, 208, 161, 98, 229, 53, 140, 51, 84, 165, 206, 248, 160, 20, 31, 29, 29, 173, 12, 208, 118, 28, 16, 41, 50, 12, 169, 130, 85, 24, 32, 72, 97, 18, 144, 16, 228, 49, 24, 64, 164, 16, 241, 70, 203, 67, 132, 64, 4, 242, 32, 55, 73, 207, 183, 247, 238, 205, 222, 203, 238, 254, 255, 189, 252, 185, 228, 38, 255, 153, 156, 236, 254, 103, 207, 158, 221, 61, 187, 103, 247, 236, 254, 251, 223, 184, 250, 250, 122, 230, 130, 171, 1, 87, 3, 205, 91, 3, 241, 205, 187, 121, 110, 235, 92, 13, 184, 26, 128, 6, 92, 67, 119, 199, 129, 171, 129, 22, 160, 129, 132, 22, 208, 70, 167, 154, 216, 147, 4, 101, 17, 118, 215, 8, 252, 156, 232, 5, 132, 94, 77, 122, 172, 147, 115, 168, 1, 195, 12, 141, 104, 238, 237, 55, 52, 189, 233, 39, 185, 134, 110, 175, 143, 96, 228, 251, 9, 147, 45, 216, 191, 166, 244, 254, 132, 205, 205, 216, 55, 80, 155, 70, 89, 180, 29, 201, 205, 181, 253, 54, 154, 222, 180, 89, 92, 67, 183, 215, 63, 79, 18, 91, 114, 238, 145, 26, 246, 191, 75, 234, 195, 203, 33, 157, 91, 177, 97, 233, 173, 250, 16, 95, 54, 225, 102, 194, 230, 2, 240, 96, 70, 253, 240, 195, 15, 108, 251, 246, 237, 218, 54, 245, 238, 221, 155, 245, 237, 219, 55, 214, 219, 159, 67, 13, 108, 150, 94, 139, 107, 232, 218, 161, 27, 148, 192, 59, 127, 141, 199, 203, 246, 159, 173, 11, 74, 144, 31, 200, 208, 241, 8, 222, 230, 100, 232, 25, 104, 212, 153, 51, 103, 216, 166, 77, 155, 16, 213, 2, 25, 58, 210, 98, 181, 253, 48, 242, 255, 104, 27, 215, 144, 16, 147, 94, 139, 107, 232, 13, 29, 232, 198, 90, 182, 6, 254, 140, 230, 127, 252, 241, 199, 90, 45, 196, 178, 215, 18, 142, 161, 99, 198, 179, 114, 107, 60, 196, 147, 237, 231, 203, 160, 16, 224, 33, 196, 65, 205, 18, 66, 64, 26, 225, 67, 132, 144, 149, 65, 8, 240, 16, 202, 60, 160, 133, 66, 79, 34, 100, 19, 202, 249, 4, 207, 121, 138, 32, 255, 167, 132, 37, 130, 168, 9, 7, 17, 61, 155, 16, 114, 58, 16, 202, 32, 228, 20, 16, 113, 143, 156, 16, 102, 124, 13, 241, 11, 217, 30, 138, 163, 110, 203, 8, 189, 132, 225, 130, 169, 190, 144, 5, 217, 5, 132, 42, 47, 194, 78, 159, 161, 205, 217, 132, 178, 62, 190, 164, 103, 200, 93, 77, 24, 9, 188, 98, 200, 4, 185, 30, 194, 108, 66, 185, 47, 65, 67, 154, 19, 227, 228, 33, 191, 108, 209, 7, 244, 24, 0, 157, 190, 178, 192, 113, 21, 94, 139, 169, 159, 60, 36, 90, 148, 91, 130, 114, 66, 192, 78, 63, 33, 75, 54, 33, 116, 6, 89, 179, 9, 109, 67, 156, 205, 11, 51, 168, 136, 29, 183, 198, 84, 112, 30, 37, 254, 155, 240, 85, 194, 116, 13, 99, 17, 209, 199, 19, 126, 47, 165, 99, 98, 152, 79, 248, 91, 66, 171, 195, 48, 100, 91, 79, 56, 151, 48, 212, 80, 209, 6, 204, 218, 188, 67, 41, 180, 130, 237, 196, 128, 1, 187, 153, 112, 43, 225, 221, 19, 54, 85, 106, 93, 247, 223, 247, 78, 100, 207, 255, 162, 53, 177, 105, 1, 46, 223, 163, 132, 7, 181, 28, 193, 9, 24, 56, 115, 9, 31, 12, 38, 107, 159, 80, 223, 5, 132, 194, 56, 237, 30, 160, 105, 5, 82, 66, 46, 225, 211, 132, 101, 151, 46, 93, 98, 30, 143, 135, 162, 106, 72, 75, 75, 99, 55, 221, 116, 147, 58, 49, 60, 170, 221, 113, 50, 141, 196, 202, 186, 12, 87, 95, 24, 107, 24, 143, 127, 37, 196, 216, 224, 227, 123, 250, 244, 233, 20, 85, 195, 136, 17, 35, 216, 195, 15, 63, 140, 68, 232, 122, 6, 33, 202, 239, 73, 136, 241, 105, 183, 159, 66, 199, 103, 164, 253, 132, 250, 223, 69, 104, 11, 18, 108, 113, 249, 12, 132, 189, 189, 239, 178, 150, 125, 124, 207, 4, 118, 93, 235, 56, 150, 87, 234, 101, 187, 79, 215, 177, 19, 21, 190, 189, 108, 151, 148, 120, 246, 204, 237, 137, 236, 230, 118, 241, 163, 40, 51, 144, 173, 39, 158, 45, 223, 215, 6, 241, 60, 217, 39, 145, 245, 239, 24, 255, 43, 74, 254, 144, 80, 52, 160, 31, 197, 177, 49, 228, 19, 3, 14, 195, 100, 217, 68, 231, 208, 46, 49, 142, 221, 211, 181, 21, 27, 213, 45, 129, 117, 72, 138, 131, 194, 129, 83, 9, 151, 112, 6, 198, 198, 81, 184, 10, 241, 239, 46, 212, 177, 188, 239, 188, 108, 15, 213, 241, 66, 77, 240, 193, 26, 228, 12, 234, 20, 207, 198, 247, 76, 132, 28, 76, 8, 232, 252, 71, 144, 207, 14, 156, 175, 174, 103, 31, 150, 212, 4, 201, 70, 251, 199, 222, 146, 32, 14, 234, 208, 150, 59, 9, 229, 137, 76, 37, 122, 10, 17, 223, 67, 2, 12, 108, 235, 214, 173, 236, 232, 209, 163, 172, 162, 162, 226, 10, 222, 94, 189, 122, 177, 172, 172, 44, 24, 25, 234, 11, 204, 37, 124, 153, 208, 242, 0, 13, 249, 0, 123, 247, 238, 13, 146, 223, 181, 107, 87, 62, 160, 83, 83, 83, 31, 163, 228, 50, 240, 92, 188, 120, 145, 29, 62, 124, 24, 81, 37, 192, 173, 21, 134, 110, 114, 127, 81, 102, 219, 182, 109, 217, 238, 221, 187, 217, 145, 35, 71, 216, 217, 179, 103, 185, 188, 142, 29, 59, 178, 49, 99, 198, 64, 70, 96, 156, 128, 103, 223, 190, 125, 65, 60, 247, 221, 119, 31, 235, 222, 189, 59, 198, 201, 98, 66, 49, 78, 130, 244, 133, 124, 197, 197, 197, 90, 125, 13, 24, 48, 64, 200, 128, 156, 1, 132, 104, 103, 56, 0, 197, 125, 68, 136, 137, 123, 23, 97, 178, 169, 159, 208, 54, 244, 211, 224, 193, 131, 25, 233, 84, 140, 79, 140, 171, 189, 132, 182, 250, 9, 58, 219, 184, 113, 35, 215, 217, 132, 9, 19, 68, 253, 239, 160, 252, 95, 18, 90, 130, 221, 21, 157, 91, 68, 102, 238, 37, 173, 192, 183, 239, 74, 98, 239, 30, 184, 204, 142, 93, 8, 54, 30, 100, 72, 77, 136, 99, 243, 178, 146, 48, 216, 217, 115, 219, 171, 217, 198, 239, 189, 87, 200, 1, 207, 251, 247, 180, 129, 177, 35, 237, 31, 132, 152, 37, 249, 43, 45, 76, 12, 152, 100, 78, 87, 94, 41, 27, 204, 2, 32, 99, 74, 102, 2, 155, 222, 55, 176, 178, 194, 216, 207, 19, 114, 35, 135, 140, 127, 30, 174, 17, 236, 198, 48, 100, 133, 222, 70, 204, 198, 21, 125, 40, 157, 186, 31, 45, 175, 211, 214, 17, 250, 121, 144, 38, 34, 2, 172, 88, 15, 32, 162, 129, 192, 160, 221, 176, 97, 3, 251, 236, 179, 207, 88, 85, 85, 149, 134, 181, 129, 60, 116, 232, 80, 97, 156, 32, 66, 127, 79, 127, 245, 213, 87, 108, 225, 194, 133, 13, 76, 33, 177, 153, 51, 103, 178, 197, 139, 23, 43, 229, 119, 238, 220, 153, 189, 242, 74, 131, 7, 110, 37, 75, 90, 237, 152, 105, 85, 156, 50, 101, 10, 91, 183, 110, 29, 59, 117, 234, 84, 72, 109, 24, 107, 211, 166, 13, 27, 59, 118, 44, 203, 206, 206, 230, 245, 194, 4, 164, 130, 23, 94, 120, 1, 3, 29, 73, 104, 39, 6, 58, 159, 20, 195, 209, 87, 102, 102, 38, 67, 93, 200, 240, 32, 135, 239, 205, 173, 220, 118, 206, 72, 255, 160, 55, 255, 193, 99, 37, 61, 38, 23, 20, 20, 176, 79, 62, 249, 68, 169, 71, 145, 7, 33, 218, 55, 114, 228, 72, 246, 192, 3, 129, 238, 127, 132, 200, 171, 172, 116, 59, 111, 222, 60, 246, 214, 91, 111, 5, 116, 38, 149, 127, 15, 229, 207, 135, 108, 43, 224, 86, 101, 197, 100, 39, 125, 238, 46, 181, 145, 35, 239, 37, 111, 61, 55, 240, 169, 249, 85, 74, 35, 23, 60, 47, 21, 5, 6, 244, 83, 68, 195, 8, 229, 175, 180, 158, 223, 81, 173, 53, 32, 228, 21, 128, 114, 22, 236, 175, 225, 101, 249, 105, 175, 82, 248, 7, 196, 231, 80, 253, 236, 26, 57, 248, 193, 139, 60, 126, 72, 20, 17, 93, 88, 120, 170, 214, 88, 71, 232, 7, 222, 4, 1, 86, 172, 63, 106, 228, 164, 17, 29, 117, 230, 3, 125, 205, 154, 53, 150, 131, 71, 200, 41, 44, 44, 228, 131, 193, 255, 12, 253, 89, 130, 206, 200, 145, 17, 134, 104, 90, 153, 45, 133, 107, 24, 86, 172, 88, 17, 24, 176, 161, 44, 152, 208, 96, 48, 11, 22, 44, 224, 94, 70, 104, 186, 120, 94, 182, 108, 153, 136, 162, 157, 17, 233, 235, 208, 161, 67, 178, 190, 132, 188, 112, 67, 110, 228, 185, 185, 185, 182, 250, 9, 237, 67, 159, 46, 95, 190, 92, 148, 195, 199, 166, 120, 208, 133, 152, 172, 85, 19, 163, 142, 95, 69, 143, 87, 17, 35, 161, 193, 200, 76, 128, 116, 24, 131, 9, 224, 13, 192, 61, 39, 72, 38, 28, 5, 195, 152, 87, 108, 111, 5, 150, 229, 194, 99, 128, 23, 64, 144, 78, 152, 5, 57, 171, 191, 9, 95, 14, 242, 248, 95, 167, 193, 197, 187, 42, 64, 251, 95, 219, 19, 152, 56, 112, 160, 162, 130, 39, 137, 152, 14, 215, 83, 183, 154, 169, 50, 9, 26, 6, 3, 86, 23, 2, 232, 207, 18, 172, 60, 5, 187, 43, 156, 101, 65, 18, 131, 85, 153, 72, 135, 17, 154, 32, 164, 157, 87, 165, 47, 120, 1, 145, 2, 238, 22, 192, 200, 195, 5, 76, 202, 200, 75, 224, 219, 59, 89, 8, 48, 157, 141, 88, 100, 13, 36, 59, 102, 232, 157, 146, 227, 88, 143, 118, 113, 1, 193, 186, 136, 21, 143, 124, 33, 5, 123, 105, 221, 4, 2, 55, 221, 36, 11, 219, 8, 1, 133, 39, 245, 19, 12, 234, 237, 223, 46, 8, 246, 160, 112, 245, 55, 87, 110, 51, 130, 24, 164, 7, 147, 28, 176, 73, 103, 2, 29, 164, 108, 114, 116, 12, 30, 118, 236, 216, 33, 211, 130, 226, 25, 25, 25, 220, 5, 12, 34, 74, 15, 69, 69, 56, 163, 177, 7, 237, 219, 183, 103, 64, 19, 192, 173, 116, 18, 224, 190, 98, 91, 96, 5, 86, 60, 98, 111, 15, 57, 38, 125, 65, 14, 202, 212, 1, 246, 242, 145, 2, 86, 103, 29, 64, 175, 232, 43, 29, 44, 93, 186, 84, 151, 212, 40, 116, 199, 12, 125, 122, 102, 34, 91, 247, 155, 20, 163, 241, 141, 187, 197, 199, 131, 253, 172, 29, 200, 63, 174, 55, 80, 236, 197, 81, 158, 14, 224, 29, 248, 93, 101, 237, 109, 54, 228, 125, 237, 206, 36, 246, 175, 17, 250, 5, 240, 200, 121, 125, 29, 228, 178, 97, 228, 144, 131, 189, 189, 14, 78, 85, 4, 188, 158, 12, 13, 15, 159, 225, 117, 43, 26, 6, 237, 139, 47, 190, 200, 247, 177, 154, 252, 97, 185, 120, 51, 102, 204, 96, 179, 102, 205, 210, 137, 106, 20, 58, 246, 224, 216, 251, 155, 12, 25, 231, 13, 224, 25, 56, 112, 160, 173, 58, 28, 59, 118, 76, 201, 7, 99, 131, 156, 137, 19, 39, 42, 211, 65, 20, 171, 37, 78, 211, 23, 45, 90, 164, 229, 67, 2, 206, 33, 192, 227, 223, 159, 179, 146, 146, 18, 45, 255, 164, 73, 147, 248, 94, 94, 199, 128, 114, 253, 171, 186, 142, 197, 81, 186, 99, 134, 254, 179, 118, 62, 81, 109, 233, 228, 93, 7, 237, 252, 54, 208, 171, 131, 190, 216, 11, 146, 135, 253, 77, 121, 192, 48, 174, 16, 153, 121, 131, 111, 178, 48, 173, 234, 165, 23, 245, 249, 67, 5, 90, 173, 198, 161, 252, 161, 207, 56, 93, 183, 2, 233, 48, 241, 102, 5, 47, 63, 169, 83, 208, 3, 36, 177, 50, 221, 120, 227, 141, 1, 90, 104, 196, 202, 53, 150, 249, 113, 160, 37, 78, 202, 101, 122, 99, 198, 69, 221, 69, 91, 84, 101, 137, 3, 50, 156, 86, 235, 0, 39, 246, 2, 116, 109, 190, 254, 250, 235, 57, 75, 74, 138, 126, 65, 16, 50, 34, 9, 203, 203, 203, 141, 217, 68, 59, 116, 76, 184, 109, 24, 45, 176, 30, 157, 209, 170, 137, 191, 28, 121, 5, 213, 185, 237, 114, 149, 76, 19, 203, 78, 139, 51, 1, 200, 57, 84, 102, 111, 197, 150, 203, 84, 197, 211, 83, 245, 19, 156, 138, 95, 65, 27, 6, 154, 211, 174, 178, 162, 28, 151, 228, 128, 6, 162, 185, 26, 59, 80, 93, 102, 185, 138, 56, 81, 72, 99, 200, 16, 6, 122, 241, 178, 253, 85, 91, 85, 143, 37, 135, 188, 12, 222, 193, 75, 3, 147, 88, 185, 66, 86, 123, 131, 135, 162, 146, 23, 13, 90, 105, 105, 105, 52, 138, 105, 54, 101, 252, 248, 227, 143, 142, 183, 37, 154, 171, 177, 168, 60, 246, 252, 184, 227, 128, 203, 73, 126, 240, 136, 136, 85, 24, 179, 134, 142, 215, 104, 140, 73, 126, 190, 161, 165, 153, 55, 232, 29, 23, 120, 13, 211, 11, 170, 216, 253, 63, 79, 16, 23, 123, 84, 146, 112, 194, 117, 213, 39, 239, 42, 193, 18, 237, 28, 226, 112, 51, 117, 135, 56, 232, 100, 172, 248, 95, 124, 241, 133, 148, 205, 141, 154, 244, 133, 149, 55, 63, 63, 223, 168, 36, 240, 68, 123, 11, 99, 172, 144, 34, 81, 186, 59, 32, 82, 113, 31, 227, 91, 241, 96, 21, 198, 172, 161, 91, 53, 76, 78, 199, 69, 21, 188, 199, 54, 109, 5, 240, 74, 14, 168, 48, 248, 169, 36, 235, 119, 178, 60, 196, 235, 200, 145, 0, 210, 31, 195, 207, 238, 213, 249, 127, 123, 175, 150, 66, 120, 25, 1, 186, 63, 29, 207, 224, 55, 0, 46, 126, 20, 225, 214, 23, 14, 220, 66, 97, 231, 206, 157, 108, 237, 218, 181, 108, 245, 106, 223, 13, 87, 221, 213, 101, 153, 94, 87, 87, 199, 142, 31, 63, 30, 42, 138, 197, 197, 5, 111, 51, 78, 156, 56, 193, 121, 4, 93, 14, 33, 3, 128, 80, 94, 25, 193, 35, 243, 213, 212, 52, 76, 186, 216, 187, 138, 244, 208, 80, 212, 175, 182, 182, 150, 33, 143, 156, 30, 31, 175, 159, 144, 121, 37, 52, 255, 84, 250, 194, 43, 202, 205, 155, 55, 179, 57, 115, 230, 104, 114, 53, 144, 177, 58, 55, 101, 67, 199, 141, 65, 255, 5, 33, 44, 56, 187, 9, 63, 39, 244, 13, 132, 134, 102, 24, 99, 45, 194, 208, 161, 129, 185, 191, 108, 205, 112, 241, 6, 246, 88, 75, 200, 141, 148, 199, 235, 121, 156, 27, 46, 241, 173, 60, 90, 195, 62, 253, 214, 203, 38, 247, 73, 96, 47, 15, 74, 66, 214, 247, 8, 183, 33, 82, 86, 85, 207, 142, 209, 237, 55, 97, 224, 160, 9, 168, 246, 111, 245, 47, 83, 120, 178, 225, 116, 93, 36, 219, 13, 239, 34, 198, 215, 9, 95, 70, 6, 92, 171, 196, 43, 28, 12, 216, 211, 167, 79, 131, 100, 11, 172, 86, 40, 97, 108, 66, 152, 120, 22, 161, 160, 135, 134, 151, 47, 55, 188, 178, 12, 77, 19, 19, 2, 232, 184, 46, 171, 3, 81, 6, 120, 66, 219, 36, 140, 94, 148, 131, 16, 175, 209, 64, 199, 36, 32, 210, 17, 23, 60, 84, 14, 250, 230, 110, 148, 7, 125, 229, 229, 229, 177, 93, 187, 118, 49, 171, 131, 50, 240, 199, 10, 116, 235, 214, 77, 84, 21, 227, 34, 95, 60, 132, 19, 198, 172, 161, 227, 180, 61, 53, 49, 142, 175, 156, 194, 240, 132, 241, 98, 13, 130, 65, 167, 167, 4, 86, 174, 147, 180, 170, 167, 183, 37, 254, 113, 121, 149, 236, 188, 98, 47, 46, 43, 237, 44, 73, 157, 187, 155, 110, 250, 253, 84, 207, 175, 229, 82, 26, 31, 72, 152, 32, 128, 141, 8, 57, 36, 155, 27, 249, 202, 149, 43, 185, 145, 99, 240, 134, 11, 48, 160, 166, 188, 66, 233, 218, 131, 73, 64, 32, 120, 16, 215, 157, 168, 87, 86, 226, 246, 41, 7, 222, 55, 152, 16, 161, 51, 124, 15, 32, 38, 5, 132, 50, 98, 162, 104, 213, 170, 85, 16, 13, 233, 45, 1, 154, 180, 161, 87, 208, 71, 39, 48, 44, 47, 89, 174, 48, 50, 30, 146, 69, 191, 59, 60, 133, 13, 239, 98, 235, 125, 60, 70, 196, 8, 194, 143, 136, 191, 207, 209, 137, 169, 108, 86, 97, 53, 91, 254, 223, 26, 203, 254, 5, 79, 55, 154, 80, 230, 12, 230, 43, 187, 37, 191, 3, 12, 207, 64, 6, 6, 44, 80, 7, 247, 222, 123, 47, 219, 178, 101, 139, 46, 185, 197, 209, 67, 245, 133, 109, 129, 12, 86, 250, 146, 189, 17, 120, 16, 242, 228, 32, 79, 12, 194, 27, 145, 101, 199, 74, 60, 202, 134, 238, 91, 14, 177, 159, 197, 62, 22, 183, 102, 97, 196, 194, 144, 189, 100, 192, 93, 164, 215, 84, 199, 53, 63, 219, 36, 43, 119, 57, 221, 73, 255, 64, 99, 180, 147, 110, 77, 100, 79, 244, 78, 196, 109, 152, 3, 34, 15, 125, 149, 198, 87, 233, 153, 253, 91, 179, 231, 10, 171, 216, 182, 19, 193, 131, 66, 240, 137, 112, 33, 29, 250, 69, 209, 208, 135, 161, 92, 211, 141, 171, 169, 83, 167, 178, 30, 61, 122, 184, 134, 46, 58, 136, 66, 108, 109, 116, 16, 174, 190, 116, 30, 4, 228, 75, 219, 5, 238, 109, 148, 149, 149, 113, 15, 1, 147, 129, 152, 16, 68, 168, 171, 207, 181, 162, 59, 110, 232, 48, 90, 172, 196, 8, 107, 96, 196, 1, 99, 174, 103, 149, 94, 95, 51, 177, 159, 213, 237, 99, 225, 114, 135, 3, 30, 186, 199, 174, 51, 86, 177, 226, 151, 18, 143, 71, 241, 85, 29, 12, 184, 244, 167, 58, 246, 42, 221, 65, 7, 143, 10, 240, 202, 173, 248, 76, 29, 187, 61, 173, 209, 93, 60, 244, 197, 117, 168, 131, 201, 93, 199, 193, 204, 129, 3, 129, 121, 75, 85, 229, 22, 71, 11, 221, 235, 203, 10, 104, 44, 125, 97, 117, 151, 182, 15, 114, 145, 252, 44, 65, 120, 9, 231, 206, 157, 11, 154, 12, 228, 73, 33, 40, 83, 35, 63, 132, 109, 232, 190, 21, 184, 222, 103, 196, 146, 49, 99, 127, 12, 56, 71, 223, 100, 219, 89, 137, 125, 220, 209, 249, 255, 1, 173, 250, 210, 7, 37, 65, 133, 110, 30, 147, 194, 246, 60, 146, 194, 6, 173, 170, 208, 26, 187, 213, 158, 62, 72, 96, 228, 15, 124, 53, 55, 25, 113, 191, 126, 248, 60, 223, 5, 187, 26, 184, 86, 250, 146, 93, 124, 213, 111, 8, 136, 179, 2, 193, 135, 16, 124, 152, 4, 18, 18, 18, 120, 104, 183, 141, 118, 249, 194, 50, 116, 15, 173, 126, 88, 165, 99, 13, 224, 174, 155, 0, 233, 163, 187, 39, 176, 191, 237, 215, 159, 42, 155, 242, 187, 105, 45, 79, 3, 157, 58, 117, 138, 184, 209, 48, 108, 175, 215, 203, 221, 127, 8, 193, 51, 86, 126, 1, 194, 216, 69, 40, 38, 4, 145, 30, 73, 24, 150, 161, 71, 219, 200, 187, 209, 253, 121, 157, 75, 109, 167, 177, 194, 192, 111, 79, 179, 62, 180, 43, 39, 79, 196, 5, 87, 3, 118, 53, 224, 196, 91, 13, 221, 93, 120, 76, 2, 192, 234, 234, 106, 94, 29, 39, 12, 189, 209, 55, 158, 118, 21, 167, 226, 51, 237, 139, 215, 208, 187, 110, 252, 116, 211, 86, 195, 97, 218, 240, 46, 214, 243, 88, 241, 25, 223, 97, 220, 151, 254, 80, 89, 15, 223, 175, 222, 168, 146, 156, 164, 121, 32, 204, 180, 82, 28, 60, 120, 144, 127, 241, 164, 251, 90, 203, 201, 202, 52, 7, 89, 66, 95, 225, 220, 75, 215, 25, 31, 244, 17, 122, 63, 192, 212, 87, 40, 211, 212, 79, 40, 7, 135, 170, 209, 2, 107, 75, 136, 86, 77, 20, 229, 204, 161, 11, 43, 107, 201, 160, 85, 0, 55, 219, 228, 106, 223, 77, 175, 222, 196, 68, 33, 14, 229, 84, 114, 240, 170, 13, 168, 3, 120, 21, 194, 51, 208, 241, 56, 68, 255, 22, 114, 172, 86, 138, 201, 147, 39, 59, 84, 92, 203, 16, 19, 174, 190, 112, 3, 13, 19, 132, 10, 112, 51, 17, 136, 189, 255, 155, 111, 190, 201, 114, 114, 114, 180, 175, 65, 231, 207, 159, 175, 18, 17, 160, 225, 144, 48, 154, 208, 148, 87, 244, 34, 24, 234, 19, 244, 138, 44, 18, 88, 74, 191, 63, 231, 135, 237, 8, 35, 149, 243, 196, 173, 81, 157, 11, 191, 67, 93, 77, 43, 5, 210, 93, 8, 214, 128, 255, 122, 104, 48, 49, 194, 39, 171, 137, 86, 22, 11, 99, 53, 121, 0, 50, 175, 28, 71, 30, 215, 208, 27, 52, 194, 111, 180, 188, 51, 52, 201, 248, 11, 48, 13, 236, 13, 177, 247, 127, 221, 134, 46, 186, 240, 57, 12, 70, 254, 1, 82, 34, 145, 131, 111, 212, 241, 190, 61, 138, 128, 59, 204, 124, 165, 48, 149, 233, 228, 192, 54, 149, 19, 43, 105, 86, 70, 19, 142, 190, 166, 77, 155, 102, 219, 120, 97, 176, 120, 79, 31, 14, 32, 207, 179, 207, 62, 43, 202, 88, 31, 78, 222, 171, 225, 109, 202, 43, 58, 218, 149, 11, 183, 121, 11, 189, 2, 251, 211, 160, 214, 204, 234, 147, 81, 24, 247, 234, 251, 147, 113, 73, 6, 121, 113, 35, 110, 6, 225, 18, 66, 46, 103, 239, 163, 169, 92, 142, 127, 18, 32, 178, 26, 80, 14, 202, 67, 185, 126, 183, 125, 155, 154, 211, 113, 234, 50, 72, 124, 252, 241, 199, 217, 232, 209, 163, 149, 194, 49, 104, 225, 54, 186, 16, 208, 64, 37, 92, 104, 232, 76, 5, 97, 234, 107, 61, 12, 17, 63, 198, 136, 219, 116, 54, 128, 151, 61, 123, 246, 108, 91, 94, 24, 92, 254, 55, 222, 120, 131, 13, 25, 50, 4, 162, 79, 18, 190, 99, 163, 12, 71, 88, 236, 254, 220, 51, 86, 154, 44, 71, 74, 180, 47, 228, 47, 196, 58, 155, 16, 119, 65, 31, 19, 217, 112, 248, 134, 3, 52, 28, 196, 9, 128, 49, 226, 224, 77, 236, 201, 137, 14, 37, 226, 218, 171, 188, 217, 250, 59, 61, 63, 77, 200, 1, 151, 96, 182, 158, 240, 29, 232, 9, 26, 194, 225, 93, 19, 66, 175, 214, 230, 18, 249, 24, 33, 191, 131, 14, 30, 7, 0, 158, 6, 127, 111, 174, 144, 53, 142, 104, 171, 64, 199, 129, 14, 222, 171, 227, 66, 8, 6, 224, 109, 183, 221, 22, 206, 1, 206, 215, 36, 162, 15, 228, 56, 0, 78, 202, 114, 160, 58, 65, 34, 48, 78, 120, 223, 56, 160, 175, 145, 36, 107, 17, 225, 168, 160, 18, 244, 15, 40, 251, 41, 194, 116, 176, 224, 240, 13, 24, 122, 129, 7, 91, 49, 244, 157, 180, 45, 40, 34, 246, 241, 132, 24, 167, 248, 73, 115, 59, 253, 132, 62, 232, 79, 168, 62, 180, 162, 4, 35, 224, 232, 222, 6, 246, 35, 158, 207, 9, 77, 128, 244, 215, 9, 77, 124, 72, 203, 177, 193, 3, 57, 114, 189, 144, 103, 29, 97, 57, 161, 9, 74, 41, 113, 5, 97, 26, 161, 156, 95, 196, 33, 7, 233, 224, 51, 1, 202, 65, 121, 224, 23, 121, 95, 167, 248, 213, 182, 141, 68, 112, 25, 208, 167, 144, 171, 10, 199, 25, 202, 42, 54, 164, 81, 18, 7, 212, 211, 110, 159, 153, 218, 4, 97, 225, 200, 130, 190, 236, 200, 115, 66, 151, 40, 7, 114, 160, 63, 83, 185, 225, 232, 75, 244, 197, 20, 139, 118, 200, 101, 99, 172, 173, 32, 180, 26, 83, 196, 194, 117, 35, 234, 44, 202, 178, 219, 79, 86, 99, 70, 200, 83, 134, 118, 87, 116, 227, 100, 17, 197, 68, 156, 140, 97, 37, 188, 131, 176, 131, 84, 238, 121, 138, 195, 235, 216, 35, 209, 172, 162, 131, 136, 1, 178, 100, 57, 200, 3, 57, 5, 132, 145, 205, 156, 148, 209, 65, 232, 73, 178, 224, 73, 117, 39, 140, 164, 141, 14, 86, 37, 38, 68, 93, 107, 125, 221, 65, 90, 2, 162, 191, 100, 192, 27, 21, 120, 113, 37, 50, 49, 154, 241, 88, 51, 244, 104, 234, 198, 45, 203, 213, 64, 179, 209, 64, 83, 63, 140, 107, 54, 138, 118, 27, 226, 106, 224, 90, 106, 192, 53, 244, 107, 169, 125, 183, 108, 87, 3, 81, 210, 192, 255, 1, 84, 13, 23, 233, 200, 91, 117, 156, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130, };
            tex.LoadImage(imgBytes);
            tex.hideFlags = HideFlags.HideAndDontSave;
        }
        #endregion

        #region GUI
        /// Draw the UI
        private void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            if (bgs == null || bgs.Length < bgColors.Length)
            {
                bgs = new GUIStyle[bgColors.Length];
                for (int i = 0; i < bgColors.Length; i++)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(1, 1, bgColors[i]);
                    tex.Apply();
                    tex.hideFlags = HideFlags.HideAndDontSave;

                    bgs[i] = new GUIStyle(GUI.skin.box);
                    bgs[i].normal.background = tex;
                }
            }

            bgs[6].padding = new RectOffset();
            bgs[6].alignment = TextAnchor.MiddleCenter;
            bgs[6].fontSize = 8;
            if (tex == null)
            {
                LoadTex();
            }
            if (tex != null)
            {
                GUI.DrawTexture(new Rect(position.width * 0.5f - tex.width * 0.5f, 0, tex.width, tex.height), tex);
                GUILayout.Space(tex.height + 10);
            }
            GUI.skin.label.richText = true;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("<b>Version " + version + "</b>");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<color=#29a6f4ff><b>View Documentation</b></color>", GUI.skin.label))
                {
                    Application.OpenURL("http://www.jacobschieck.com/projects/meshanimator/documentation/documentation.html");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                prefab = EditorGUILayout.ObjectField("Asset to Bake", prefab, typeof(GameObject), true) as GameObject;
                if (prefab != null)
                {
                    if (GUILayout.Button("Reset Settings", GUILayout.Width(100)))
                    {
                        ClearPreferencesForAsset();
                    }
                }
            }
            if (prefab == null)
            {
                DrawWarning("Specify an asset to bake.");
            }
            if (prefab != null)
            {
                if (previousPrefab != prefab)
                    OnPrefabChanged();
            }
            var clips = GetClips();
            selectedAnimatorType = (MeshAnimatorType)EditorGUILayout.EnumPopup("Mesh Animator Type", selectedAnimatorType);
            if (prefab != null && !string.IsNullOrEmpty(GetPrefabPath()))
            {
                if (outputFolder == null)
                {
                    if (string.IsNullOrEmpty(outputPath))
                        outputPath = GetAssetPath(new FileInfo(GetPrefabPath()).Directory.FullName);
                    outputFolder = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
                }
                outputFolder = EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(Object), false);
                if (outputFolder != null)
                {
                    outputPath = AssetDatabase.GetAssetPath(outputFolder);
                    var files = new DirectoryInfo(outputPath).GetFiles("*.prefab");
                    bool existing = false;
                    for (int f = 0; f < files.Length; f++)
                    {
                        var existingMa = AssetDatabase.LoadAssetAtPath<MeshAnimatorBase>(GetAssetPath(files[f].FullName));
                        if (existingMa != null)
                        {
                            existing = true;
                            break;
                        }
                    }
                    if (existing)
                    {
                        string message = "Baking to this folder may overwrite existing Mesh Animator assets sharing the same name.";
                        DrawWarning(message);
                    }
                }
            }
            var scrollScope = new GUILayout.ScrollViewScope(scroll);
            using (scrollScope)
            {
                using (RenderSection.Draw("Information", bgs[0], () =>
                {
                    var wrapLabel = new GUIStyle(GUI.skin.label);
                    wrapLabel.wordWrap = true;
                    string[] info;
                    if (information.TryGetValue(selectedAnimatorType, out info))
                    {
                        foreach (var line in info)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField("•", GUILayout.Width(15));
                                EditorGUILayout.LabelField(line, wrapLabel);
                            }
                        }
                    }
                })) ;
                EditorGUI.BeginChangeCheck();
                if (prefab != null)
                {
                    if (string.IsNullOrEmpty(GetPrefabPath()))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var style = new GUIStyle(GUI.skin.FindStyle("CN EntryErrorIcon"));
                            style.margin = new RectOffset();
                            style.contentOffset = new Vector2();
                            GUILayout.Box("", style, GUILayout.Width(15), GUILayout.Height(15));
                            var textStyle = new GUIStyle(GUI.skin.label);
                            textStyle.contentOffset = new Vector2(10, 0);
                            GUILayout.Label("Cannot find asset path. The target object must be linked to a prefab or animated asset (FBX, etc...)", textStyle);
                        }
                        return;
                    }
                    if (previousPrefab != prefab)
                        OnPrefabChanged();
                    if (spawnedAsset == null)
                    {
                        OnPrefabChanged();
                    }
                    using (RenderSection.Draw("Animation Setup", bgs[0], () =>
                    {
                        if (animController == null)
                        {
                            DrawInformation("Specify an Animation Controller to auto-populate animation clips.");
                        }
                        animController = EditorGUILayout.ObjectField("Animation Controller", animController, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
                        if (requiresAnimator)
                        {
                            if (animAvatar == null)
                                GetAvatar();
                            if (isHumanoid)
                                animAvatar = EditorGUILayout.ObjectField("Avatar", animAvatar, typeof(Avatar), true) as Avatar;
                            if (isHumanoid && animAvatar == null)
                            {
                                DrawError("For humanoid and optimized rigs, you must specify an Avatar.");
                            }
                            rootMotionMode = (RootMotionMode)EditorGUILayout.EnumPopup("Root Motion Mode", rootMotionMode);
                            switch (rootMotionMode)
                            {
                                case RootMotionMode.Baked:
                                    {
                                        DrawInformation("Root Motion will be baked into vertices.");
                                        break;
                                    }
                                case RootMotionMode.AppliedToTransform:
                                    {
                                        DrawInformation("Root Motion will move the MeshAnimator at runtime.");
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            rootMotionMode = RootMotionMode.None;
                        }

                        using (RenderSection.Draw("Bake Animations", bgs[0], () =>
                        {
                            bool globalSkipChanged = false;
                            if (selectedAnimatorType == MeshAnimatorType.Snapshot)
                            {
                                globalBake = EditorGUILayout.IntSlider("Global Frame Skip", globalBake, 1, fps);
                                globalSkipChanged = globalBake != previousGlobalBake;
                                previousGlobalBake = globalBake;
                            }
                            EditorGUILayout.LabelField("Custom Clips");
                            for (int i = 0; i < customClips.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    customClips[i] = (AnimationClip)EditorGUILayout.ObjectField(customClips[i], typeof(AnimationClip), false);
                                    if (GUILayout.Button("X", GUILayout.Width(32)))
                                    {
                                        customClips.RemoveAt(i);
                                        GUILayout.EndHorizontal();
                                        break;
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }

                            if (GUILayout.Button("Add Custom Animation Clip"))
                                customClips.Add(null);
                            if (GUILayout.Button("Add Selected Animation Clips"))
                            {
                                foreach (var o in Selection.objects)
                                {
                                    string p = AssetDatabase.GetAssetPath(o);
                                    if (string.IsNullOrEmpty(p) == false)
                                    {
                                        AnimationClip clipAtPath = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
                                        if (clipAtPath != null)
                                            customClips.Add(clipAtPath);
                                        AnimationClip[] clipsToAdd = AssetDatabase.LoadAllAssetRepresentationsAtPath(p).Where(q => q is AnimationClip).Cast<AnimationClip>().ToArray();
                                        customClips.AddRange(clipsToAdd);
                                    }
                                }
                            }
                            var clipNames = bakeAnims.Keys.ToArray();
                            bool modified = false;
                            try
                            {
                                EditorGUI.indentLevel++;
                                GUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button("Select All", GUILayout.Width(100)))
                                    {
                                        foreach (var clipName in clipNames)
                                            bakeAnims[clipName] = true;
                                    }
                                    if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                                    {
                                        foreach (var clipName in clipNames)
                                            bakeAnims[clipName] = false;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Bake", GUILayout.MaxWidth(50));
                                    if (selectedAnimatorType == MeshAnimatorType.Snapshot)
                                    {
                                        GUILayout.Label("Frame Skip", GUILayout.MaxWidth(100));
                                    }
                                    GUILayout.Label("Animation");
                                }
                                GUILayout.EndHorizontal();
                                foreach (var clipName in clipNames)
                                {
                                    if (frameSkips.ContainsKey(clipName) == false)
                                        frameSkips.Add(clipName, globalBake);
                                    float frameSkip = selectedAnimatorType != MeshAnimatorType.Snapshot ? 1 : frameSkips[clipName];
                                    AnimationClip clip = clips.Find(q => q.name == clipName);
                                    int framesToBake = clip ? (int)(clip.length * fps / frameSkip) : 0;
                                    GUILayout.BeginHorizontal();
                                    {
                                        bakeAnims[clipName] = EditorGUILayout.Toggle(bakeAnims[clipName], GUILayout.MaxWidth(40));
                                        GUI.enabled = bakeAnims[clipName];
                                        if (selectedAnimatorType == MeshAnimatorType.Snapshot)
                                        {
                                            frameSkips[clipName] = Mathf.Clamp(EditorGUILayout.IntField(frameSkips[clipName], GUILayout.MaxWidth(100)), 1, fps);
                                        }
                                        GUILayout.Space(10);
                                        GUI.enabled = true;
                                        GUILayout.Label(string.Format("{0} ({1} frames)", clipName, framesToBake));
                                    }
                                    GUILayout.EndHorizontal();
                                    if (framesToBake > 500)
                                    {
                                        GUI.skin.label.richText = true;
                                        EditorGUILayout.LabelField("<color=red>Long animations degrade performance, consider using a higher frame skip value.</color>", GUI.skin.label);
                                    }

                                    if (selectedAnimatorType == MeshAnimatorType.Snapshot)
                                    {
                                        if (globalSkipChanged) frameSkips[clipName] = globalBake;
                                        if (frameSkips[clipName] != 1)
                                            modified = true;
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e);
                            }
                            if (modified)
                            {
                                DrawInformation("Skipping more frames during baking will result in a smaller asset size, but potentially degrade animation quality.");
                            }
                        }, true)) ;
                    }, true)) ;

                    using (RenderSection.Draw("Mesh Setup", bgs[0], () =>
                    {
                        for (int i = 0; i < meshFilters.Count; i++)
                        {
                            bool remove = false;
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.ObjectField("Mesh Filter " + i, meshFilters[i], typeof(MeshFilter), true);
                                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                                    remove = true;
                            }
                            GUILayout.EndHorizontal();
                            if (remove)
                            {
                                meshFilters.RemoveAt(i);
                                break;
                            }
                        }
                        if (GUILayout.Button("+ Add MeshFilter"))
                            meshFilters.Add(null);

                        for (int i = 0; i < skinnedRenderers.Count; i++)
                        {
                            bool remove = false;
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.ObjectField("Skinned Mesh " + i, skinnedRenderers[i], typeof(SkinnedMeshRenderer), true);
                                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                                    remove = true;
                            }
                            GUILayout.EndHorizontal();
                            if (remove)
                            {
                                skinnedRenderers.RemoveAt(i);
                                break;
                            }
                        }
                        if (GUILayout.Button("+ Add SkinnedMeshRenderer"))
                            skinnedRenderers.Add(null);
                        // combine meshes
                        if (meshFilters.Count + skinnedRenderers.Count <= 1)
                            useOriginalMesh = EditorGUILayout.Toggle("Use Original Mesh", useOriginalMesh);
                        else
                            useOriginalMesh = false;
                    })) ;


                    using (RenderSection.Draw("Expose Transforms", bgs[0], () =>
                    {
                        for (int i = 0; i < exposedTransforms.Count; i++)
                        {
                            bool remove = false;
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(exposedTransforms[i]);
                                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                                    remove = true;
                            }
                            GUILayout.EndHorizontal();
                            if (remove)
                            {
                                exposedTransforms.RemoveAt(i);
                                break;
                            }
                        }
                        string[] extrasExposed = GetExtraExposedTranforms();
                        if (extrasExposed != null)
                        {
                            bool anyMissing = false;
                            List<string> missing = new List<string>();
                            for (int i = 0; i < exposedTransforms.Count; i++)
                            {
                                bool ok = false;
                                for (int j = 0; j < extrasExposed.Length; j++)
                                {
                                    if (extrasExposed[j] == exposedTransforms[i])
                                    {
                                        ok = true;
                                        break;
                                    }
                                }
                                if (!ok)
                                {
                                    missing.Add(exposedTransforms[i]);
                                    anyMissing = true;
                                    //break;
                                }
                            }
                            if (anyMissing)
                            {
                                string message = "Missing Exposed transforms in the Rig Import Settings:";
                                for (int i = 0; i < missing.Count; i++)
                                {
                                    message += string.Format("\n    - {0}", missing[i]);
                                }
                                DrawWarning(message.Trim().TrimEnd(','));
                                GUILayout.Space(15);
                            }
                        }
                        if (GUILayout.Button("+ Expose Transform"))
                        {
                            var allTransforms = GetAllTranforms();
                            GenericMenu m = new GenericMenu();
                            for (int i = 0; i < allTransforms.Length; i++)
                            {
                                string s = allTransforms[i];
                                m.AddItem(new GUIContent(s), false, () =>
                                {
                                    if (exposedTransforms.Contains(s) == false)
                                    {
                                        exposedTransforms.Add(s);
                                        CleanExposedTransforms();
                                    }
                                });
                            }
                            m.ShowAsContext();
                        }
                        if (GUILayout.Button("Expose All Transforms"))
                        {
                            var allTransforms = GetAllTranforms();
                            exposedTransforms.AddRange(allTransforms);
                            CleanExposedTransforms();
                        }
                    })) ;

                    using (RenderSection.Draw("Bake Preferences", bgs[0], () =>
                    {
                        fps = EditorGUILayout.IntSlider("Bake FPS", fps, 1, 500);
                        if (selectedAnimatorType == MeshAnimatorType.ShaderAnimated)
                        {
                            int selectedIndex = textureSizes.IndexOf(selectedTextureSize);
                            if (selectedIndex == -1)
                                selectedIndex = 0;
                            selectedIndex = EditorGUILayout.Popup("Bake Texture Size", selectedIndex, textureSizeNames);
                            selectedTextureSize = textureSizes[selectedIndex];

                            selectedIndex = textureQualities.IndexOf(selectedTextureQuality);
                            if (selectedIndex == -1)
                                selectedIndex = 0;
                            selectedIndex = EditorGUILayout.Popup("Bake Texture Quality", selectedIndex, textureQualityNames);
                            selectedTextureQuality = textureQualities[selectedIndex];

                            DrawInformation("Higher texture quality provides more data for animation textures at the expense of file size. Use a higher quality if jitter is noticed when viewing models up close.");
                        }
                        if (selectedAnimatorType == MeshAnimatorType.Snapshot)
                        {
                            int[] optionsValues = new int[5] { 1, 10, 100, 1000, 10000 };
                            string[] options = new string[5] { "0", "0.1", "0.01", "0.001", "0.0001" };
                            int selected = 3;
                            for (int i = 0; i < optionsValues.Length; i++)
                            {
                                if (optionsValues[i] == Snapshot.DeltaCompressedFrameData.compressionAccuracy)
                                    selected = i;
                            }
                            customCompression = EditorGUILayout.Toggle("Custom Compression", customCompression);
                            if (customCompression == false)
                            {
                                Snapshot.DeltaCompressedFrameData.compressionAccuracy = optionsValues[EditorGUILayout.Popup("Compression Accuracy", selected, options)];
                                DrawInformation("Higher compression values reduce the accuracy of vertex positions. Files sizes will be smaller, but may result in mesh jitter.");
                            }
                            else
                            {
                                Snapshot.DeltaCompressedFrameData.compressionAccuracy = EditorGUILayout.Slider("Compression Accuracy",
                                    Snapshot.DeltaCompressedFrameData.compressionAccuracy, 1, 10000);
                                GUI.skin.label.wordWrap = true;
                                DrawInformation("Lower custom compression values reduce the accuracy of vertex positions. Files sizes will be smaller, but may result in mesh jitter.");
                            }
                        }

                        List<string> meshNormalOptions = System.Enum.GetNames(typeof(MeshNormalMode)).ToList();
                        if (selectedAnimatorType == MeshAnimatorType.ShaderAnimated)
                        {
                            meshNormalOptions.RemoveAt(2);
                            if (meshNormalMode == MeshNormalMode.Recalculated)
                                meshNormalMode = MeshNormalMode.UseOriginal;
                        }
                        int meshNormalIndex = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            if (meshNormalMode == (MeshNormalMode)i)
                            {
                                meshNormalIndex = i;
                                break;
                            }
                        }
                        meshNormalIndex = EditorGUILayout.Popup("Mesh Normal Mode", meshNormalIndex, meshNormalOptions.ToArray());
                        meshNormalMode = (MeshNormalMode)meshNormalIndex;

                        string meshNormalText = "";
                        switch (meshNormalMode)
                        {
                            case MeshNormalMode.Baked:
                                {
                                    if (selectedAnimatorType == MeshAnimatorType.Snapshot)
                                        meshNormalText = "Mesh normals will be stored in bake data. Best results but larger asset sizes.";
                                    else
                                        meshNormalText = "Mesh normals will be stored in bake textures. Recommended for best results.";
                                    break;
                                }
                            case MeshNormalMode.Recalculated:
                                {
                                    meshNormalText = "Mesh normals will be recalculated at runtime by the Mesh.RecalculateNormals() method.";
                                    break;
                                }
                            case MeshNormalMode.UseOriginal:
                                {
                                    meshNormalText = "Original mesh normals will be used. Lighting may not be correct when rotating or lighting shifts.";
                                    break;
                                }
                        }
                        DrawInformation(meshNormalText);

                        using (RenderSection.Draw("Level of Detail (LOD)", bgs[0], () =>
                        {
                            DrawInformation("LOD can be used to update far away meshes less frequently, reducing overall CPU usage.");
                            bool usingLOD = lodDistances.Count > 0;
                            usingLOD = EditorGUILayout.Toggle("Use LOD", usingLOD);
                            if (usingLOD == false)
                            {
                                lodDistances.Clear();
                            }
                            else
                            {
                                if (GUILayout.Button("Add LOD Level") || lodDistances.Count == 0)
                                {
                                    lodDistances.Add(new KeyValuePair<int, float>(fps, 20));
                                }
                                for (int l = 0; l < lodDistances.Count; l++)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        if (GUILayout.Button("X"))
                                        {
                                            lodDistances.RemoveAt(l);
                                            GUILayout.EndHorizontal();
                                            break;
                                        }
                                        int key = EditorGUILayout.IntField("Playback FPS", lodDistances[l].Key);
                                        float value = EditorGUILayout.FloatField("Distance", lodDistances[l].Value);
                                        lodDistances[l] = new KeyValuePair<int, float>(key, value);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        })) ;
                    }, true)) ;
                }
                using (RenderSection.Draw("Utilities", bgs[0], () =>
                {
                    if (GUILayout.Button("Batch Bake Selected Objects"))
                    {
                        previousPrefab = null;
                        foreach (var obj in Selection.gameObjects)
                        {
                            try
                            {
                                prefab = obj;
                                OnPrefabChanged();
                                var toBakeClips = GetClips();
                                foreach (var clip in toBakeClips)
                                {
                                    frameSkips[clip.name] = 1;
                                }
                                CreateSnapshots();
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }
                })) ;
                EditorGUI.EndChangeCheck();
                if (GUI.changed)
                    Repaint();
                scroll = scrollScope.scrollPosition;
            }
            if (prefab != null)
            {
                GUILayout.Space(10);
                int bakeCount = bakeAnims.Count(q => q.Value);
                GUI.enabled = bakeCount > 0;
                var c = GUI.color;
                GUI.color = new Color(128 / 255f, 234 / 255f, 255 / 255f, 1);
                if (GUILayout.Button(string.Format("Bake {0} animation{1}", bakeCount, bakeCount > 1 ? "s" : string.Empty), GUILayout.Height(30)))
                    CreateSnapshots();
                GUI.color = c;
                GUI.enabled = true;
            }

            SavePreferencesForAsset();
        }

        /// Draw a UI text area
        private void DrawText(string text)
        {
            DrawText(text, GUI.color);
        }

        /// Draw a UI text area of a certain color
        private void DrawText(string text, Color color)
        {
            Color c = GUI.color;
            GUI.color = color;
            GUI.skin.label.wordWrap = true;
            GUILayout.Label(text);
            GUI.color = c;
        }

        private void DrawError(string text)
        {
            int w = (int)Mathf.Lerp(300, 900, text.Length / 200f);
            using (new EditorGUILayout.HorizontalScope(GUILayout.MinHeight(30)))
            {
                var style = new GUIStyle(GUI.skin.FindStyle("CN EntryErrorIcon"));
                style.margin = new RectOffset();
                style.contentOffset = new Vector2();
                GUILayout.Box("", style, GUILayout.Width(15), GUILayout.Height(15));
                var textStyle = new GUIStyle(GUI.skin.label);
                textStyle.contentOffset = new Vector2(10, Instance.position.width < w ? 0 : 5);
                GUILayout.Label(text, textStyle);
            }
        }

        private void DrawWarning(string text)
        {
            int w = (int)Mathf.Lerp(300, 900, text.Length / 200f);
            using (new EditorGUILayout.HorizontalScope(GUILayout.MinHeight(30)))
            {
                var style = new GUIStyle(GUI.skin.FindStyle("CN EntryWarnIcon"));
                style.margin = new RectOffset();
                style.contentOffset = new Vector2();
                GUILayout.Box("", style, GUILayout.Width(15), GUILayout.Height(15));
                var textStyle = new GUIStyle(GUI.skin.label);
                textStyle.contentOffset = new Vector2(10, Instance.position.width < w ? 0 : 5);
                GUILayout.Label(text, textStyle);
            }
        }

        private void DrawInformation(string text)
        {
            int w = (int)Mathf.Lerp(300, 900, text.Length / 200f);
            using (new EditorGUILayout.HorizontalScope(GUILayout.MinHeight(30)))
            {
                var style = new GUIStyle(GUI.skin.FindStyle("CN EntryInfoIcon"));
                style.margin = new RectOffset();
                style.contentOffset = new Vector2();
                GUILayout.Box("", style, GUILayout.Width(15), GUILayout.Height(15));
                var textStyle = new GUIStyle(GUI.skin.label);
                textStyle.contentOffset = new Vector2(10, Instance.position.width < w ? 0 : 5);
                GUILayout.Label(text, textStyle);
            }
        }
        #endregion

        #region Utility Methods
        /// Get a project relative path to an asset
        private string GetAssetPath(string s)
        {
            string path = s;
            string[] split = path.Split('\\');
            path = string.Empty;
            int startIndex = 0;
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i] == "Assets")
                    break;
                startIndex++;
            }
            for (int i = startIndex; i < split.Length; i++)
                path += split[i] + "\\";
            path = path.TrimEnd("\\".ToCharArray());
            path = path.Replace("\\", "/");
            return path;
        }

        /// Return the Avatar if available from the prefab
        private Avatar GetAvatar()
        {
            if (animAvatar)
                return animAvatar;
            var objs = EditorUtility.CollectDependencies(new Object[] { prefab }).ToList();
            foreach (var obj in objs.ToArray())
                objs.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(obj)));
            objs.RemoveAll(q => q is Avatar == false || q == null);
            if (objs.Count > 0)
                animAvatar = objs[0] as Avatar;
            return animAvatar;
        }

        /// Get all clips related to or attached to the prefab
        private List<AnimationClip> GetClips()
        {
            if (prefab == null)
                return new List<AnimationClip>();
            var clips = EditorUtility.CollectDependencies(new Object[] { prefab }).ToList();
            foreach (var obj in clips.ToArray())
                clips.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(obj)));
            clips.AddRange(customClips.Select(q => (Object)q));
            clips.RemoveAll(q => q is AnimationClip == false || q == null);
            foreach (AnimationClip clip in clips)
            {
                if (bakeAnims.ContainsKey(clip.name) == false)
                    bakeAnims.Add(clip.name, true);
            }
            clips.RemoveAll(q => bakeAnims.ContainsKey(q.name) == false);
            clips.RemoveAll(q => bakeAnims[q.name] == false);

            var distinctClips = clips.Select(q => (AnimationClip)q).Distinct().ToList();
            requiresAnimator = false;
            isHumanoid = false;
            var humanoidCheck = new List<AnimationClip>(distinctClips);
            if (animController)
            {
                humanoidCheck.AddRange(animController.animationClips);
                distinctClips.AddRange(animController.animationClips);
                distinctClips = distinctClips.Distinct().ToList();
            }
            foreach (var c in humanoidCheck)
            {
                if (c && (c.isHumanMotion || !c.legacy))
                {
                    requiresAnimator = true;
                    if (c.isHumanMotion)
                    {
                        isHumanoid = true;
                    }
                }
            }
            try
            {
                if (requiresAnimator == false)
                {
                    var importer = GetImporter(GetPrefabPath());
                    if (importer && importer.animationType == ModelImporterAnimationType.Human)
                    {
                        requiresAnimator = true;
                        isHumanoid = true;
                    }
                }
            }
            catch { }
            try
            {
                if (requiresAnimator == false && IsOptimizedAnimator())
                    requiresAnimator = true;
            }
            catch { }
            for (int i = 0; i < distinctClips.Count; i++)
            {
                if (bakeAnims.ContainsKey(distinctClips[i].name) == false)
                    bakeAnims.Add(distinctClips[i].name, true);
            }
            return distinctClips;
        }

        /// Return the path of the specified prefab
        private string GetPrefabPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
                assetPath = AssetDatabase.GetAssetPath(parentObject);
            }
            return assetPath;
        }

        /// Return the path of the original FBX or imported mesh
        private string GetSourcePrefabPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
                assetPath = AssetDatabase.GetAssetPath(parentObject);
            }
            return assetPath;
        }

        /// Callback to refresh what's needed when the user changes which prefab is baking
        private void OnPrefabChanged()
        {
            if (spawnedAsset != null)
            {
                DestroyImmediate(spawnedAsset.gameObject);
            }
            if (Application.isPlaying)
            {
                return;
            }
            animator = null;
            animAvatar = null;
            if (prefab != null)
            {
                if (spawnedAsset == null)
                {
                    spawnedAsset = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    SetChildFlags(spawnedAsset.transform, HideFlags.HideAndDontSave);
                }
                bakeAnims.Clear();
                frameSkips.Clear();
                AutoPopulateFiltersAndRenderers();
                AutoPopulateAnimatorAndController();
                AutoPopulateExposedTransforms();
                LoadPreferencesForAsset();
            }
            previousPrefab = prefab;
        }

        /// Find all renderers in the prefab
        private void AutoPopulateFiltersAndRenderers()
        {
            meshFilters.Clear();
            skinnedRenderers.Clear();
            MeshFilter[] filtersInPrefab = spawnedAsset.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < filtersInPrefab.Length; i++)
            {
                if (meshFilters.Contains(filtersInPrefab[i]) == false)
                    meshFilters.Add(filtersInPrefab[i]);
                if (filtersInPrefab[i].GetComponent<MeshRenderer>())
                    filtersInPrefab[i].GetComponent<MeshRenderer>().enabled = false;
            }
            SkinnedMeshRenderer[] renderers = spawnedAsset.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (skinnedRenderers.Contains(renderers[i]) == false)
                    skinnedRenderers.Add(renderers[i]);
                renderers[i].enabled = false;
            }
            useOriginalMesh = meshFilters.Count + skinnedRenderers.Count <= 1;
        }

        /// Find the animator and controllers in the prefab
        private void AutoPopulateAnimatorAndController()
        {
            animator = spawnedAsset.GetComponent<Animator>();
            if (animator == null)
                animator = spawnedAsset.GetComponentInChildren<Animator>();
            if (animator && animController == null)
                animController = animator.runtimeAnimatorController;
        }

        /// Find all exposed transforms of the imported rig
        private void AutoPopulateExposedTransforms()
        {
            exposedTransforms.Clear();
            exposedTransforms.AddRange(GetExtraExposedTranforms());
            CleanExposedTransforms();
        }

        /// Find all materials associated with the renderers
        private List<Material> GatherMaterials()
        {
            List<Material> mats = new List<Material>();
            MeshRenderer mr = null;
            foreach (MeshFilter mf in meshFilters)
                if (mf && ((mr = mf.GetComponent<MeshRenderer>())))
                    mats.AddRange(mr.sharedMaterials);
            foreach (SkinnedMeshRenderer sm in skinnedRenderers)
                if (sm) mats.AddRange(sm.sharedMaterials);
            mats.RemoveAll(q => q == null);
            mats = mats.Distinct().ToList();
            return mats;
        }

        /// Finds a matching transform in the hierarchy of another transform
        private Transform FindMatchingTransform(Transform parent, Transform source, Transform newParent)
        {
            List<int> stepIndexing = new List<int>();
            while (source != parent && source != null)
            {
                if (source.parent == null)
                    break;
                for (int i = 0; i < source.parent.childCount; i++)
                {
                    if (source.parent.GetChild(i) == source)
                    {
                        stepIndexing.Add(i);
                        source = source.parent;
                        break;
                    }
                }
            }
            stepIndexing.Reverse();
            for (int i = 0; i < stepIndexing.Count; i++)
            {
                newParent = newParent.GetChild(stepIndexing[i]);
            }
            return newParent;
        }

        /// Loads preferences associated with the target prefab
        private void LoadPreferencesForAsset()
        {
            try
            {
                string path = GetPrefabPath();
                if (string.IsNullOrEmpty(path))
                    return;
                string guid = AssetDatabase.AssetPathToGUID(path);
                string prefsPath = string.Format("MeshAnimator_BakePrefs_{0}", guid);
                prefsPath = Path.Combine(Path.GetTempPath(), prefsPath);
                MeshBakePreferences bakePrefs = null;
                using (FileStream fs = new FileStream(prefsPath, FileMode.Open))
                {
                    BinaryFormatter br = new BinaryFormatter();
                    bakePrefs = (MeshBakePreferences)br.Deserialize(fs);
                }
                animAvatar = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(bakePrefs.animAvatar), typeof(Avatar)) as Avatar;
                animController = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(bakePrefs.animController), typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                customClips.AddRange(bakePrefs.customClips.Select(q => (AnimationClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(q), typeof(AnimationClip))));
                customClips = customClips.Distinct().ToList();
                customCompression = bakePrefs.customCompression;
                exposedTransforms.AddRange(bakePrefs.exposedTransforms);
                exposedTransforms = exposedTransforms.Distinct().ToList();
                fps = bakePrefs.fps;
                globalBake = bakePrefs.globalBake;
                previousGlobalBake = bakePrefs.previousGlobalBake;
                rootMotionMode = (RootMotionMode)bakePrefs.rootMotionMode;
                meshNormalMode = (MeshNormalMode)bakePrefs.meshNormalMode;
                useOriginalMesh = bakePrefs.combineMeshes;
                selectedAnimatorType = (MeshAnimatorType)bakePrefs.type;
                selectedTextureSize = (ShaderTextureSize)bakePrefs.texSize;
                selectedTextureQuality = (ShaderTextureQuality)bakePrefs.texQuality;
                outputPath = bakePrefs.outputPath;
                for (int i = 0; i < bakePrefs.lodDistanceKeys.Length; i++)
                {
                    lodDistances.Add(new KeyValuePair<int, float>(bakePrefs.lodDistanceKeys[i], bakePrefs.lodDistanceValues[i]));
                }
            }
            catch { }
        }

        /// Saves preferences associated with the target prefab
        private void SavePreferencesForAsset()
        {
            try
            {
                string path = GetPrefabPath();
                if (string.IsNullOrEmpty(path))
                    return;
                string guid = AssetDatabase.AssetPathToGUID(path);
                MeshBakePreferences preferences = new MeshBakePreferences();
                preferences.animAvatar = animAvatar != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animAvatar)) : string.Empty;
                preferences.animController = animController != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animController)) : string.Empty;
                preferences.customClips = customClips.Select(q => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(q))).ToArray();
                preferences.customCompression = customCompression;
                preferences.exposedTransforms = exposedTransforms.ToArray();
                preferences.fps = fps;
                preferences.texSize = (int)selectedTextureSize;
                preferences.texQuality = (int)selectedTextureQuality;
                preferences.type = (int)selectedAnimatorType;
                preferences.globalBake = globalBake;
                preferences.previousGlobalBake = previousGlobalBake;
                preferences.rootMotionMode = (int)rootMotionMode;
                preferences.meshNormalMode = (int)meshNormalMode;
                preferences.lodDistanceKeys = new int[lodDistances.Count];
                preferences.lodDistanceValues = new float[lodDistances.Count];
                preferences.combineMeshes = useOriginalMesh;
                preferences.outputPath = outputPath;
                for (int i = 0; i < lodDistances.Count; i++)
                {
                    preferences.lodDistanceKeys[i] = lodDistances[i].Key;
                    preferences.lodDistanceValues[i] = lodDistances[i].Value;
                }
                string prefsPath = string.Format("MeshAnimator_BakePrefs_{0}", guid);
                prefsPath = Path.Combine(Path.GetTempPath(), prefsPath);
                // save prefs
                using (FileStream fs = new FileStream(prefsPath, FileMode.OpenOrCreate))
                {
                    BinaryFormatter br = new BinaryFormatter();
                    br.Serialize(fs, preferences);
                }
            }
            catch { }
        }

        /// Clears preferences associated with the target prefab
        private void ClearPreferencesForAsset()
        {
            try
            {
                string path = GetPrefabPath();
                if (string.IsNullOrEmpty(path))
                    return;
                string guid = AssetDatabase.AssetPathToGUID(path);
                string prefsPath = string.Format("MeshAnimator_BakePrefs_{0}", guid);
                prefsPath = Path.Combine(Path.GetTempPath(), prefsPath);
                if (File.Exists(prefsPath))
                    File.Delete(prefsPath);
                Selection.activeGameObject = prefab;
                Close();
                GetWindow<MeshAnimationCreator>();
            }
            catch { }
        }

        /// Gets extra exposed transforms
        private string[] GetExtraExposedTranforms()
        {
            if (prefab == null)
                return new string[0];
            var importers = GetAllImporters();
            List<string> output = new List<string>();
            for (int i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                var paths = importer.extraExposedTransformPaths;
                if (importer.optimizeGameObjects)
                {
                    for (int j = 0; j < paths.Length; j++)
                    {
                        var split = paths[j].Split('/');
                        paths[j] = split[split.Length - 1];
                    }
                }
                output.AddRange(paths);
            }
            return output.Distinct().ToArray();
        }

        /// Gets all transforms from the prefab
        private string[] GetAllTranforms()
        {
            if (prefab == null)
                return new string[0];
            var importers = GetAllImporters();
            List<string> output = new List<string>();
            for (int i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                output.AddRange(importer.transformPaths);
            }
            return output.Distinct().ToArray();
        }

        /// Cleans up the exposed transform list
        private void CleanExposedTransforms()
        {
            if (IsOptimizedAnimator())
            {
                for (int j = 0; j < exposedTransforms.Count; j++)
                {
                    var split = exposedTransforms[j].Split('/');
                    exposedTransforms[j] = split[split.Length - 1];
                }
            }
        }

        /// Returns true if the prefab uses an optimized rig
        private bool IsOptimizedAnimator()
        {
            var i = GetAllImporters();
            if (i.Count > 0)
                return i.Any(q => q.optimizeGameObjects);
            return false;
        }

        /// Transfers mesh properties from one mesh to another
        private void TransferMesh(Mesh from, Mesh to)
        {
            to.vertices = from.vertices;
            to.subMeshCount = from.subMeshCount;
            for (int i = 0; i < from.subMeshCount; i++)
            {
                to.SetTriangles(from.GetTriangles(i), i);
            }
            to.normals = from.normals;
            to.tangents = from.tangents;
            to.colors = from.colors;
            to.uv = from.uv;
            to.uv2 = from.uv2;
            to.uv3 = from.uv3;
            to.uv4 = from.uv4;
        }

        /// Returns a model importer from the specified asset path
        private ModelImporter GetImporter(string p)
        {
            return ModelImporter.GetAtPath(p) as ModelImporter;
        }

        /// Returns all model importers used on the prefab
        private List<ModelImporter> GetAllImporters()
        {
            List<ModelImporter> importers = new List<ModelImporter>();
            importers.Add(GetImporter(GetPrefabPath()));
            foreach (var mf in meshFilters)
            {
                if (mf && mf.sharedMesh)
                {
                    importers.Add(GetImporter(AssetDatabase.GetAssetPath(mf.sharedMesh)));
                }
            }
            foreach (var sr in skinnedRenderers)
            {
                if (sr && sr.sharedMesh)
                {
                    importers.Add(GetImporter(AssetDatabase.GetAssetPath(sr.sharedMesh)));
                }
            }
            importers.RemoveAll(q => q == null);
            importers = importers.Distinct().ToList();
            return importers;
        }

        /// Sets flags on a transform and all it's children
        private void SetChildFlags(Transform t, HideFlags flags)
        {
            Queue<Transform> q = new Queue<Transform>();
            q.Enqueue(t);
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                q.Enqueue(c);
                SetChildFlags(c, flags);
            }
            while (q.Count > 0)
            {
                q.Dequeue().gameObject.hideFlags = flags;
            }
        }
        #endregion

        #region Baking Methods
        private void CreateSnapshots()
        {
            switch (selectedAnimatorType)
            {
                case MeshAnimatorType.Snapshot:
                    {
                        CreateSnapshots<Snapshot.SnapshotMeshAnimator, Snapshot.SnapshotMeshAnimation>();
                        break;
                    }
                case MeshAnimatorType.ShaderAnimated:
                    {
                        CreateSnapshots<ShaderAnimated.ShaderMeshAnimator, ShaderAnimated.ShaderMeshAnimation>();
                        break;
                    }
            }
        }
        /// Create the MeshAnimator prefab, baking all the animations
        private void CreateSnapshots<MeshAnimatorType, MeshAnimationType>()
            where MeshAnimatorType : MeshAnimatorBase where MeshAnimationType : MeshAnimationBase
        {
            AnimatorController bakeController = null;
            try
            {
                string assetPath = GetPrefabPath();
                if (string.IsNullOrEmpty(assetPath))
                {
                    EditorUtility.DisplayDialog("Mesh Animator", "Unable to locate the asset path for prefab: " + prefab.name, "OK");
                    return;
                }

                HashSet<string> allAssets = new HashSet<string>();

                var clips = GetClips();
                foreach (var clip in clips)
                    allAssets.Add(AssetDatabase.GetAssetPath(clip));

                string assetFolder = AssetDatabase.GetAssetPath(outputFolder);
                var sampleGO = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                if (meshFilters.Count(q => q) == 0 && skinnedRenderers.Count(q => q) == 0)
                {
                    throw new System.Exception("Bake Error! No MeshFilter's or SkinnedMeshRender's found to bake!");
                }
                else
                {
                    animator = sampleGO.GetComponent<Animator>();
                    if (animator == null)
                    {
                        animator = sampleGO.GetComponentInChildren<Animator>();
                    }
                    if (requiresAnimator)
                    {
                        bakeController = CreateBakeController();
                        if (animator == null)
                        {
                            animator = sampleGO.AddComponent<Animator>();
                            animator.runtimeAnimatorController = bakeController;
                            animator.avatar = GetAvatar();
                            if (isHumanoid && animator.avatar == null)
                            {
                                EditorUtility.DisplayDialog("Mesh Animator", "Bake Error! Error loading avatar!.", "OK");
                                Debug.Log("Error loading avatar");
                            }
                        }
                        else
                        {
                            animator.runtimeAnimatorController = bakeController;
                            animator.avatar = GetAvatar();
                        }
                    }
                    if (animator != null)
                    {
                        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                        animator.applyRootMotion = rootMotionMode != RootMotionMode.None;
                    }
                    GameObject asset = new GameObject(prefab.name + "_AnimatedMesh");
                    MeshAnimatorType ma = asset.AddComponent<MeshAnimatorType>();
                    List<Material> mats = GatherMaterials();
                    asset.AddComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
                    List<MeshAnimationType> createdAnims = new List<MeshAnimationType>();
                    int animCount = 0;
                    int vertexCount = 0;
                    Transform rootMotionBaker = new GameObject().transform;
                    foreach (AnimationClip animClip in clips)
                    {
                        if (bakeAnims.ContainsKey(animClip.name) && bakeAnims[animClip.name] == false) continue;
                        if (selectedAnimatorType == MeshAnimationCreator.MeshAnimatorType.Snapshot)
                        {
                            if (!frameSkips.ContainsKey(animClip.name))
                                frameSkips.Add(animClip.name, 1);
                        }
                        string meshAnimationPath = string.Format("{0}/{1}.asset", assetFolder, FormatClipName(animClip.name));
                        MeshAnimationType meshAnim = AssetDatabase.LoadAssetAtPath(meshAnimationPath, typeof(MeshAnimationType)) as MeshAnimationType;
                        bool create = false;
                        if (meshAnim == null || meshAnim.Mode != (int)selectedAnimatorType)
                        {
                            if (meshAnim != null)
                            {
                                DestroyImmediate(meshAnim, true);
                            }
                            meshAnim = ScriptableObject.CreateInstance<MeshAnimationType>();
                            create = true;
                        }
                        meshAnim.name = animClip.name;
                        meshAnim.length = animClip.length;
                        meshAnim.Mode = (int)selectedAnimatorType;
                        // create exposed transforms
                        List<string> foundExposed = new List<string>();
                        for (int i = 0; i < exposedTransforms.Count; i++)
                        {
                            string exposedName = exposedTransforms[i];
                            Transform t = sampleGO.transform.Find(exposedName);
                            if (t != null)
                            {
                                string[] splitT = exposedName.Split('/');
                                string lastName = splitT[splitT.Length - 1];
                                foundExposed.Add(lastName);
                            }
                            else
                            {
                                Debug.LogWarningFormat("Unable to find \"{0}\" transform during bake. Is it exposed in the Rig settings?", exposedName);
                            }
                        }
                        meshAnim.exposedTransforms = foundExposed.ToArray();
                        int frameSkip = selectedAnimatorType != MeshAnimationCreator.MeshAnimatorType.Snapshot ? 1 : frameSkips[animClip.name];
                        int bakeFrames = Mathf.CeilToInt(animClip.length * fps / (float)frameSkip);
                        int frame = 0;
                        List<MeshFrameDataBase> verts = new List<MeshFrameDataBase>();
                        List<Vector3> meshesInFrame = new List<Vector3>();
                        List<Vector3> normalsInFrame = new List<Vector3>();
                        float lastFrameTime = 0;
                        meshAnim.SetFrameData(Enumerable.Range(0, bakeFrames + 1).Select(x => new MeshFrameDataBase()).ToArray());
                        List<List<Vector3>> framePositions = new List<List<Vector3>>();
                        List<List<Vector3>> frameNormals = new List<List<Vector3>>();
                        for (int i = 0; i <= bakeFrames; i++)
                        {
                            float bakeDelta = Mathf.Clamp01((float)i / bakeFrames);
                            EditorUtility.DisplayProgressBar("Baking Animation", string.Format("Processing: {0} Frame: {1}", animClip.name, i), bakeDelta);
                            float animationTime = bakeDelta * animClip.length;
                            if (requiresAnimator)
                            {
                                float normalizedTime = animationTime / animClip.length;
                                string stateName = FindStateNameForClip(animClip, animator);
                                if (stateName == null)
                                {
                                    stateName = animClip.name;
                                }
                                animator.Play(stateName, 0, normalizedTime);
                                if (lastFrameTime == 0)
                                {
                                    float nextBakeDelta = Mathf.Clamp01(((float)(i + 1) / bakeFrames));
                                    float nextAnimationTime = nextBakeDelta * animClip.length;
                                    lastFrameTime = animationTime - nextAnimationTime;
                                }
                                animator.Update(animationTime - lastFrameTime);
                                lastFrameTime = animationTime;
                            }
                            else
                            {
                                GameObject sampleObject = sampleGO;
                                Animation legacyAnimation = sampleObject.GetComponentInChildren<Animation>();
                                if (animator && animator.gameObject != sampleObject)
                                    sampleObject = animator.gameObject;
                                else if (legacyAnimation && legacyAnimation.gameObject != sampleObject)
                                    sampleObject = legacyAnimation.gameObject;
                                animClip.SampleAnimation(sampleObject, animationTime);
                            }
                            meshesInFrame.Clear();
                            normalsInFrame.Clear();

                            Mesh m = null;
                            List<MeshFilter> sampleMeshFilters = new List<MeshFilter>();
                            List<SkinnedMeshRenderer> sampleSkinnedRenderers = new List<SkinnedMeshRenderer>();
                            for (int j = 0; j < meshFilters.Count; j++)
                            {
                                var sampleMF = FindMatchingTransform(prefab.transform, meshFilters[j].transform, sampleGO.transform).GetComponent<MeshFilter>();
                                sampleMeshFilters.Add(sampleMF);
                                var sampleMR = sampleMF.gameObject.GetComponent<MeshRenderer>();
                                bool filterEnabled = sampleMR == null || sampleMR.enabled;
                                m = Instantiate(sampleMF.sharedMesh) as Mesh;
                                Vector3[] v = m.vertices;
                                Vector3[] n = m.normals;
                                for (int vIndex = 0; vIndex < v.Length; vIndex++)
                                {
                                    if (!filterEnabled)
                                    {
                                        v[vIndex] = sampleMR.bounds.center;
                                    }
                                    else
                                    {
                                        v[vIndex] = sampleMF.transform.TransformPoint(v[vIndex]);
                                    }

                                    if (selectedAnimatorType == MeshAnimationCreator.MeshAnimatorType.ShaderAnimated &&
                                       meshNormalMode == MeshNormalMode.UseOriginal)
                                    {
                                        n[vIndex] = Vector3.zero;
                                    }
                                    else
                                    {
                                        n[vIndex] = sampleMF.transform.TransformDirection(n[vIndex]);
                                    }
                                }
                                meshesInFrame.AddRange(v);
                                normalsInFrame.AddRange(n);
                                DestroyImmediate(m);
                            }
                            for (int j = 0; j < skinnedRenderers.Count; j++)
                            {
                                var sampleSR = FindMatchingTransform(prefab.transform, skinnedRenderers[j].transform, sampleGO.transform).GetComponent<SkinnedMeshRenderer>();
                                sampleSkinnedRenderers.Add(sampleSR);
                                bool filterEnabled = sampleSR.enabled;
                                m = new Mesh();
                                sampleSR.BakeMesh(m);
                                Vector3[] v = m.vertices;
                                Vector3[] n = m.normals;
                                sampleSR.transform.localScale = Vector3.one;
                                for (int vIndex = 0; vIndex < v.Length; vIndex++)
                                {
                                    if (!filterEnabled)
                                    {
                                        v[vIndex] = sampleSR.bounds.center;
                                    }
                                    else
                                    {
                                        v[vIndex] = sampleSR.transform.TransformPoint(v[vIndex]);
                                    }

                                    if (selectedAnimatorType == MeshAnimationCreator.MeshAnimatorType.ShaderAnimated &&
                                       meshNormalMode == MeshNormalMode.UseOriginal)
                                    {
                                        n[vIndex] = Vector3.zero;
                                    }
                                    else
                                    {
                                        n[vIndex] = sampleSR.transform.TransformDirection(n[vIndex]);
                                    }
                                }
                                meshesInFrame.AddRange(v);
                                normalsInFrame.AddRange(n);
                                DestroyImmediate(m);
                            }
                            bool combinedMeshes = false;
                            var combinedInFrame = GenerateCombinedMesh(sampleMeshFilters, sampleSkinnedRenderers, out combinedMeshes);
                            if (combinedMeshes)
                            {
                                meshesInFrame = combinedInFrame.vertices.ToList();
                                normalsInFrame = combinedInFrame.normals.ToList();
                                DestroyImmediate(combinedInFrame);
                            }
                            vertexCount = meshesInFrame.Count;

                            MeshFrameDataBase data = new MeshFrameDataBase();
                            switch (rootMotionMode)
                            {
                                case RootMotionMode.Baked:
                                    {
                                        rootMotionBaker.position = animator.rootPosition;
                                        rootMotionBaker.rotation = animator.rootRotation;
                                        for (int j = 0; j < meshesInFrame.Count; j++)
                                        {
                                            meshesInFrame[j] = rootMotionBaker.TransformPoint(meshesInFrame[j]);
                                        }
                                        break;
                                    }
                                case RootMotionMode.AppliedToTransform:
                                    {
                                        rootMotionBaker.position = animator.rootPosition;
                                        rootMotionBaker.rotation = animator.rootRotation;
                                        for (int j = 0; j < meshesInFrame.Count; j++)
                                        {
                                            meshesInFrame[j] = rootMotionBaker.InverseTransformPoint(meshesInFrame[j]);
                                        }
                                        data.rootMotionPosition = animator.deltaPosition;
                                        data.rootMotionRotation = animator.targetRotation;
                                        break;
                                    }
                            }
                            verts.Add(data);
                            // bake exposed tranforms
                            data.exposedTransforms = new Matrix4x4[exposedTransforms.Count];
                            for (int j = 0; j < exposedTransforms.Count; j++)
                            {
                                Transform t = sampleGO.transform.Find(exposedTransforms[j]);
                                if (t)
                                {
                                    data.exposedTransforms[j] = Matrix4x4.TRS(
                                        rootMotionBaker.InverseTransformPoint(t.position),
                                        Quaternion.Inverse(rootMotionBaker.rotation) * t.rotation,
                                        t.localScale);
                                }
                            }

                            // debug only
                            //Instantiate(sampleGO, frame * Vector3.right, Quaternion.identity);
                            meshAnim.SetFrameData(i, data);
                            meshAnim.frameSkip = frameSkip;
                            framePositions.Add(meshesInFrame.ToList());
                            frameNormals.Add(normalsInFrame.ToList());
                            frame++;
                        }
                        meshAnim.rootMotionMode = rootMotionMode;
                        meshAnim.animationName = animClip.name;
                        if (animClip.isLooping)
                        {
                            meshAnim.wrapMode = WrapMode.Loop;
                        }
                        else
                        {
                            meshAnim.wrapMode = animClip.wrapMode;
                        }
                        if (selectedAnimatorType == MeshAnimationCreator.MeshAnimatorType.Snapshot)
                        {
                            Snapshot.SnapshotMeshAnimation snapshot = meshAnim as Snapshot.SnapshotMeshAnimation;
                            snapshot.meshNormalMode = meshNormalMode;
                        }
                        if (create)
                        {
                            AssetDatabase.CreateAsset(meshAnim, meshAnimationPath);
                        }

                        meshAnim.CreateBakedAssets(meshAnimationPath, framePositions, frameNormals);

                        createdAnims.Add(meshAnim);

                        EditorUtility.SetDirty(meshAnim);

                        animCount++;
                    }

                    object[] parameters = null;
                    switch (selectedAnimatorType)
                    {
                        case MeshAnimationCreator.MeshAnimatorType.ShaderAnimated:
                            {
                                var shaderAnims = createdAnims.Cast<ShaderAnimated.ShaderMeshAnimation>();
                                Dictionary<Vector2Int, int> commonCounts = new Dictionary<Vector2Int, int>();
                                foreach (var anim in shaderAnims)
                                {
                                    if (!commonCounts.ContainsKey(anim.textureSize))
                                        commonCounts.Add(anim.textureSize, 1);
                                    else
                                        commonCounts[anim.textureSize]++;
                                }
                                Vector2Int textureSize;
                                switch (selectedTextureSize)
                                {
                                    case ShaderTextureSize.Size_Smallest_Common:
                                        textureSize = commonCounts.OrderBy(x => x.Key.x).First().Key;
                                        break;
                                    case ShaderTextureSize.Size_Largest_Common:
                                        textureSize = commonCounts.OrderByDescending(x => x.Key.x).First().Key;
                                        break;
                                    case ShaderTextureSize.Size_Most_Common:
                                        textureSize = commonCounts.OrderByDescending(x => x.Value).First().Key;
                                        break;
                                    default:
                                        textureSize = new Vector2Int((int)selectedTextureSize, (int)selectedTextureSize);
                                        break;
                                }
                                foreach (var anim in shaderAnims)
                                    anim.textureSize = textureSize;
                                parameters = new object[] { (int)selectedTextureQuality };
                                break;
                            }
                    }
                    foreach (var meshAnim in createdAnims)
                    {
                        meshAnim.CompleteBake(createdAnims.Cast<IMeshAnimation>().ToArray(), parameters);
                        EditorUtility.SetDirty(meshAnim);
                    }
                    AssetDatabase.SaveAssets();

                    DestroyImmediate(rootMotionBaker.gameObject);

                    ma.SetAnimations(createdAnims.ToArray());
                    string meshPath = string.Format("{0}/{1}_combinedMesh.asset", assetFolder, asset.name);
                    bool combined = false;

                    Mesh combinedMesh = GenerateCombinedMesh(out combined);
                    Mesh existingMesh = null;
                    if (useOriginalMesh == false)
                    {
                        if (combined)
                        {
                            if ((existingMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh))))
                            {
                                TransferMesh(combinedMesh, existingMesh);
                                combinedMesh = existingMesh;
                            }
                            else
                            {
                                existingMesh = new Mesh() { name = combinedMesh.name };
                                TransferMesh(combinedMesh, existingMesh);
                                combinedMesh = existingMesh;
                                AssetDatabase.CreateAsset(combinedMesh, meshPath);
                            }
                        }
                    }
                    ma.baseMesh = combinedMesh;
                    ma.meshFilter = ma.GetComponent<MeshFilter>();
                    ma.meshFilter.sharedMesh = ma.baseMesh;
                    ma.StoreAdditionalMeshData(ma.baseMesh);

                    ma.FPS = fps;
                    ma.LODLevels = new MeshAnimatorBase.MeshAnimatorLODLevel[lodDistances.Count];
                    for (int i = 0; i < lodDistances.Count; i++)
                    {
                        ma.LODLevels[i] = new MeshAnimatorBase.MeshAnimatorLODLevel()
                        {
                            fps = lodDistances[i].Key,
                            distance = lodDistances[i].Value
                        };
                    }
                    // create exposed children objects
                    for (int i = 0; i < exposedTransforms.Count; i++)
                    {
                        string[] splitT = exposedTransforms[i].Split('/');
                        string lastName = splitT[splitT.Length - 1];
                        var child = new GameObject(lastName);
                        child.transform.SetParent(asset.transform);
                        Transform t = sampleGO.transform.Find(exposedTransforms[i]);
                        if (t)
                        {
                            child.transform.position = t.position;
                        }
                        else
                        {
                            child.transform.localPosition = Vector3.zero;
                        }
                    }
                    string maPrefabPath = string.Format("{0}/{1}.prefab", assetFolder, asset.name);
                    var maPrefab = AssetDatabase.LoadAssetAtPath(maPrefabPath, typeof(GameObject));
                    if (maPrefab != null)
                    {
                        PrefabUtility.ReplacePrefab(asset, maPrefab);
                    }
                    else
                    {
                        PrefabUtility.CreatePrefab(maPrefabPath, asset);
                    }
                    GameObject.DestroyImmediate(asset);
                }
                GameObject.DestroyImmediate(sampleGO);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Mesh Animator", string.Format("Baked {0} animation{1} successfully!", clips.Count
                    , clips.Count > 1 ? "s" : string.Empty), "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Bake Error", string.Format("There was a problem baking the animations.\n\n{0}\n\nIf the problem persists, email jschieck@gmail.com for support.", e), "OK");
                Debug.LogException(e);
            }
            finally
            {
                if (bakeController)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bakeController));
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// Generate combined mesh
        private Mesh GenerateCombinedMesh(out bool combined)
        {
            GameObject tempCombinePrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            List<MeshFilter> combineMeshFilters = new List<MeshFilter>();
            List<SkinnedMeshRenderer> combineSkinnedRenderers = new List<SkinnedMeshRenderer>();
            for (int j = 0; j < meshFilters.Count; j++)
            {
                var sampleMF = FindMatchingTransform(prefab.transform, meshFilters[j].transform, tempCombinePrefab.transform).GetComponent<MeshFilter>();
                combineMeshFilters.Add(sampleMF);
            }
            for (int j = 0; j < skinnedRenderers.Count; j++)
            {
                var sampleSR = FindMatchingTransform(prefab.transform, skinnedRenderers[j].transform, tempCombinePrefab.transform).GetComponent<SkinnedMeshRenderer>();
                combineSkinnedRenderers.Add(sampleSR);
            }
            Mesh combinedMesh = GenerateCombinedMesh(combineMeshFilters, combineSkinnedRenderers, out combined);
            DestroyImmediate(tempCombinePrefab);
            return combinedMesh;
        }

        /// Combines the specified filters and renderers into a single mesh, only combining if needed
        private Mesh GenerateCombinedMesh(List<MeshFilter> filters, List<SkinnedMeshRenderer> renderers, out bool combined)
        {
            int totalMeshes = filters.Count + renderers.Count;
            combined = false;
            if (totalMeshes == 1)
            {
                foreach (MeshFilter mf in filters)
                {
                    return mf.sharedMesh;
                }
                foreach (SkinnedMeshRenderer sr in renderers)
                {
                    return sr.sharedMesh;
                }
            }
            List<Mesh> tempMeshes = new List<Mesh>();
            List<CombineInstanceMaterial> combineInstances = new List<CombineInstanceMaterial>();
            foreach (MeshFilter mf in filters)
            {
                Material[] materials = new Material[0];
                if (mf == null)
                    continue;
                Mesh m = mf.sharedMesh;
                if (m == null) m = mf.mesh;
                if (m == null)
                    continue;
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.sharedMaterials.Where(q => q != null).ToArray();
                }
                var matrix = mf.transform.localToWorldMatrix;
                if (mr && !mr.enabled)
                {
                    matrix = Matrix4x4.zero;
                }
                for (int i = 0; i < m.subMeshCount; i++)
                {
                    combineInstances.Add(new CombineInstanceMaterial()
                    {
                        combine = new CombineInstance()
                        {
                            mesh = m,
                            transform = matrix,
                            subMeshIndex = i
                        },
                        material = materials.Length > i ? materials[i] : null,
                        sharedMesh = m,
                    });
                }
            }
            foreach (SkinnedMeshRenderer sr in renderers)
            {
                Material[] materials = sr.sharedMaterials.Where(q => q != null).ToArray();

                if (sr == null || sr.sharedMesh == null)
                    continue;

                for (int i = 0; i < sr.sharedMesh.subMeshCount; i++)
                {
                    Mesh t = new Mesh();
                    sr.BakeMesh(t);
                    tempMeshes.Add(t);
                    var m = sr.transform.localToWorldMatrix;
                    Matrix4x4 scaledMatrix = Matrix4x4.TRS(MatrixUtils.GetPosition(m), MatrixUtils.GetRotation(m), sr.enabled ? Vector3.one : Vector3.zero);
                    combineInstances.Add(new CombineInstanceMaterial()
                    {
                        combine = new CombineInstance()
                        {
                            mesh = t,
                            transform = scaledMatrix,
                            subMeshIndex = i
                        },
                        material = materials.Length > i ? materials[i] : null,
                        sharedMesh = sr.sharedMesh,
                    });
                }
            }
            Dictionary<Material, Mesh> materialMeshes = new Dictionary<Material, Mesh>();
            Mesh mesh = null;
            Material nullMaterial = new Material(Shader.Find("Standard"));
            while (combineInstances.Count > 0)
            {
                Material cMat = combineInstances[0].material;
                var combines = combineInstances.Where(q => q.material == cMat).Select(q => q.combine).ToArray();
                combineInstances.RemoveAll(q => q.material == cMat);
                mesh = new Mesh();
                mesh.CombineMeshes(combines, true, true);
                if (cMat == null)
                    cMat = nullMaterial;
                materialMeshes.Add(cMat, mesh);
                tempMeshes.Add(mesh);
            }
            CombineInstance[] finalCombines = materialMeshes.Select(q => new CombineInstance() { mesh = q.Value }).ToArray();
            mesh = new Mesh();
            mesh.CombineMeshes(finalCombines, false, false);
            mesh.RecalculateBounds();
            combined = true;
            foreach (Mesh m in tempMeshes)
            {
                DestroyImmediate(m);
            }
            return mesh;
        }

        /// Weld vertices occupying close space into a single vertex
        private void AutoWeld(Mesh mesh, float threshold, float bucketStep)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            int[] old2new = new int[oldVertices.Length];
            int newSize = 0;

            // Find AABB
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++)
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
                int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
                int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

                // Check to see if it's already been added
                if (buckets[x, y, z] == null)
                    buckets[x, y, z] = new List<int>(); // Make buckets lazily

                for (int j = 0; j < buckets[x, y, z].Count; j++)
                {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude(to) < threshold)
                    {
                        old2new[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[newSize] = oldVertices[i];
                buckets[x, y, z].Add(newSize);
                old2new[i] = newSize;
                newSize++;

            skip:;
            }

            // Make new triangles
            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length];
            for (int i = 0; i < oldTris.Length; i++)
            {
                newTris[i] = old2new[oldTris[i]];
            }

            Vector3[] finalVertices = new Vector3[newSize];
            for (int i = 0; i < newSize; i++)
                finalVertices[i] = newVertices[i];

            mesh.Clear();
            mesh.vertices = finalVertices;
            mesh.triangles = newTris;
            mesh.RecalculateNormals();
#if UNITY_5
            mesh.Optimize();
#endif
        }

        /// Creates a temporary AnimatorController for baking
        private UnityEditor.Animations.AnimatorController CreateBakeController()
        {
            // Creates the controller automatically containing all animation clips
            string tempPath = "Assets/TempBakeController.controller";
            var bakeName = AssetDatabase.GenerateUniqueAssetPath(tempPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(bakeName);
            var baseStateMachine = controller.layers[0].stateMachine;
            var clips = GetClips();
            foreach (var clip in clips)
            {
                var state = baseStateMachine.AddState(clip.name);
                state.motion = clip;
            }
            return controller;
        }

        /// Formats an animation clip name to have valid asset characters only
        private string FormatClipName(string name)
        {
            string badChars = "!@#$%%^&*()=+}{[]'\";:|";
            for (int i = 0; i < badChars.Length; i++)
            {
                name = name.Replace(badChars[i], '_');
            }
            return name;
        }

        private string FindStateNameForClip(AnimationClip clip, Animator animator)
        {
            AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;

            var layers = ac.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                var states = layers[i].stateMachine.states;
                for (int j = 0; j < states.Length; j++)
                {
                    if (states[j].state.motion == clip)
                        return states[j].state.name;
                }
            }
            return null;
        }

        #endregion
    }
}