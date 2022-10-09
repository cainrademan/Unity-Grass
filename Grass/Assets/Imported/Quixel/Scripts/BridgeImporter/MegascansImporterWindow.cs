#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Quixel
{
    public class MegascansImporterWindow : EditorWindow
    {

        public static string version = "4.0";

        static private int texPack;
        static private int texPackUpdate;
        static private string[] texPacking = new string[] {
            "Metallic",
            "Specular",
        };
        static private int dispType;
        static private int dispTypeUpdate;
        static private string[] dispTypes = new string[] {
            "None",
            "Vertex",
            "Pixel",
        };
        static private int shaderType;
        static private int shaderTypeUpdate;
        static private string[] shaderTypes = new string[] {
            "HDRP",
            "URP (LWRP)",
            "Legacy",
            "Auto-Detect",
        };

        static private int importResolution;
        static private int importResolutionUpdate;
        static private string[] importResolutions = new string[] {
            "512",
            "1024",
            "2048",
            "4096",
            "8192",
        };

        static private int lodFadeMode;
        static private int lodFadeModeUpdate;
        static private string[] lodFadeModeSettings = new string[] {
            "None",
            "Cross Fade",
            "Speed Tree"
        };

        static private string path;
        static private string pathUpdate;

        static private Texture2D MSLogo;
        static private Texture2D BridgeLogo;
        static private Texture2D HelpLogo;

        static private GUIStyle MSLogoStyle;
        static private GUIStyle HelpLogoStyle;
        static private Texture2D MSBackground;
        static private GUIStyle MSField;
        static private GUIStyle MSPopup;
        static private GUIStyle MSText;
        static private GUIStyle MSCheckBox;
        static private GUIStyle MSHelpStyle;
        static private GUIStyle MSNormalTextStyle;
        static private GUIStyle MSWarningTextStyle;
        static private GUIStyle MSHeadingTextStyle;
        static private GUIStyle MSTabsStyle;
        static private GUIStyle MSStrechedWidthStyle;
        static private bool connection;
        static private bool connectionUpdate;
        static private bool setupCollision;
        static private bool applyToSelection;
        static private bool addAssetToScene;
        static private bool importLODs;
        static private bool setupLOD;
        static private bool setupPrefabs;
        static private bool setupCollisionUpdate;
        static private bool applyToSelectionUpdate;
        static private bool addAssetToSceneUpdate;
        static private bool importLODsUpdate;
        static private bool setupLODUpdate;
        static private bool setupPrefabsUpdate;
        static private bool importAllTextures;
        static private bool importAllTexturesUpdate;

        static private bool SuperHD;

        static private Vector2 size;
        static private Vector2 logoSize;
        static private Vector2 textSize;
        static private Vector2 textHeadingSize;
        static private Vector2 fieldSize;
        static private Rect collisionLoc;
        static private Rect applyToSelectionLoc;
        static private Rect addAssetToSceneLoc;
        static private Rect importLODsLoc;
        static private Rect setupLODLoc;
        static private Rect setupPrefabsLoc;
        static private Rect importAllTexturesLoc;
        static private Rect connectionLoc;

        static private float lineYLoc;

        //Decal Properties
        static private string decalBlend = "100";
        static private string decalSize = "1";

        //Decal Properties
        static private string decalBlendUpdate = "100";
        static private string decalSizeUpdate = "1";

        private int tab = 0;
        //Terrain tools properties
        static private string terrainMaterialName = "Terrain Material";
        static private string terrainMaterialPath = "Quixel/";
        static private string tiling = "10";

        static private string terrainMaterialNameUpdate = "Terrain Material";
        static private string terrainMaterialPathUpdate = "Quixel/";
        static private string tilingUpdate = "10";

        [MenuItem("Window/Quixel/Megascans Importer", false, 10)]
        public static void Init()
        {
            MegascansImporterWindow window = (MegascansImporterWindow)EditorWindow.GetWindow(typeof(MegascansImporterWindow));
            GUIContent header = new GUIContent();
            header.text = " Bridge Plugin v" + version;
            header.image = (Texture)MSLogo;
            header.tooltip = "Megascans Bridge Plugin.";
            window.titleContent = header;
            window.maxSize = size * 20f;
            window.minSize = size;
            window.Show();
        }

        void OnGUI()
        {

            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), MSBackground, ScaleMode.StretchToFill);

            GUILayout.BeginHorizontal();

            GUILayout.Box("Bridge Plugin v" + version, MSHeadingTextStyle, GUILayout.Height(textHeadingSize.y));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(MSLogo, MSLogoStyle, GUILayout.Height(logoSize.y), GUILayout.Width(logoSize.x)))
                Application.OpenURL("https://quixel.com/megascans/library/latest");

            if (GUILayout.Button(BridgeLogo, MSLogoStyle, GUILayout.Height(logoSize.y), GUILayout.Width(logoSize.x)))
                Application.OpenURL("https://quixel.com/bridge");

            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();

            //tab = GUILayout.Toolbar(tab, new string[] { "Settings", "Utilities" }, MSTabsStyle, GUILayout.Height(textSize.y));

            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Handles.color = Color.white;
            Handles.DrawLine(new Vector3(0f, lineYLoc), new Vector3(maxSize.x, lineYLoc));
            GUILayout.EndHorizontal();

            if (tab == 0)
            {

                GUILayout.BeginHorizontal();

                GUILayout.Label("Workflow", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                texPack = EditorGUILayout.Popup(texPack, texPacking, MSPopup, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Displacement", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                dispType = EditorGUILayout.Popup(dispType, dispTypes, MSPopup, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Box("Shader Type", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                shaderType = EditorGUILayout.Popup(shaderType, shaderTypes, MSPopup, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Box("Import Resolution", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                importResolution = EditorGUILayout.Popup(importResolution, importResolutions, MSPopup, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Box("LOD Fade Mode", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                lodFadeMode = EditorGUILayout.Popup(lodFadeMode, lodFadeModeSettings, MSPopup, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Box("Import Path", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                path = EditorGUILayout.TextField(path, MSField, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                setupCollision = EditorGUI.Toggle(collisionLoc, setupCollision, MSCheckBox);
                GUILayout.Box("Setup Collision", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                applyToSelection = EditorGUI.Toggle(applyToSelectionLoc, applyToSelection, MSCheckBox);
                GUILayout.Box("Apply To Selection (2D Surfaces)", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                addAssetToScene = EditorGUI.Toggle(addAssetToSceneLoc, addAssetToScene, MSCheckBox);
                GUILayout.Box("Add Asset to the Scene", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                setupPrefabs = EditorGUI.Toggle(setupPrefabsLoc, setupPrefabs, MSCheckBox);
                GUILayout.Box("Create Prefabs", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                importLODs = EditorGUI.Toggle(importLODsLoc, importLODs, MSCheckBox);
                GUILayout.Box("Import Lower LODs", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                setupLOD = EditorGUI.Toggle(setupLODLoc, setupLOD, MSCheckBox);
                GUILayout.Box("Create LOD Groups", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                importAllTextures = EditorGUI.Toggle(importAllTexturesLoc, importAllTextures, MSCheckBox);
                GUILayout.Box("Import All Textures", MSNormalTextStyle, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                connection = EditorGUI.Toggle(connectionLoc, connection, MSCheckBox);
                GUILayout.Box("Enable Plugin", MSNormalTextStyle, GUILayout.Height(textSize.y));
                if (GUILayout.Button("Help...", MSHelpStyle, GUILayout.Width(textSize.x)))
                    Application.OpenURL("https://docs.google.com/document/d/1XeK2nlkO6NSm34IBYJT8Kon0IxzGwCIv-tACs8i_X58");

                GUILayout.EndHorizontal();
            }
            else
            {
                /*
#if (UNITY_2018 || UNITY_2019 || UNITY_2020 || UNITY_2021)
                GUILayout.BeginHorizontal();

                GUILayout.Box("Terrain Tools (Beta)", MSHeadingTextStyle, GUILayout.Height(textHeadingSize.y));

                GUILayout.EndHorizontal();

                if (MegascansUtilities.isLegacy())
                {

                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Material Name", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                    terrainMaterialName = EditorGUILayout.TextField(terrainMaterialName, MSField, GUILayout.Height(fieldSize.y), GUILayout.Width(fieldSize.x));

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Material Path", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                    terrainMaterialPath = EditorGUILayout.TextField(terrainMaterialPath, MSField, GUILayout.Height(fieldSize.y), GUILayout.Width(fieldSize.x));

                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();

                GUILayout.Box("Texture Tiling", MSText, GUILayout.Height(textSize.y), GUILayout.Width(textSize.x));
                tiling = EditorGUILayout.TextField(tiling, MSField, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Setup Paint Layers", MSStrechedWidthStyle, GUILayout.Height(textSize.y)))
                    MegascansTerrainTools.SetupTerrain();

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                string warningMsg = "Warning: This feature works properly with the metallic workflow only.";
                if (MegascansUtilities.isLegacy())
                    warningMsg += "This feature requires HD Render Pipeline.";

                GUI.skin.label.wordWrap = true;
                GUILayout.Label(warningMsg, MSWarningTextStyle, GUILayout.Height(textSize.y));

                GUILayout.EndHorizontal();

#endif

#if (UNITY_2018_3 || UNITY_2018_4 || UNITY_2019 || UNITY_2020 || UNITY_2021)

                GUILayout.BeginHorizontal();

                GUILayout.Box("Material Tools (Beta)", MSHeadingTextStyle, GUILayout.Height(textHeadingSize.y));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Create Terrain Paint Layer", MSStrechedWidthStyle, GUILayout.Height(textSize.y)))
                    MegascansTerrainTools.CreateTerrainLayerFromMat();

                GUILayout.EndHorizontal();

#endif

#if HDRP && (UNITY_2018_2 || UNITY_2018_3 || UNITY_2018_4 || UNITY_2019 || UNITY_2020 || UNITY_2021)

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Decal Setup (Beta)", MSHeadingTextStyle, GUILayout.Height(textHeadingSize.y));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Global Opacity (%)", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                decalBlend = EditorGUILayout.TextField (decalBlend, MSField, GUILayout.Height(fieldSize.y));

                GUILayout.EndHorizontal ();

                if (!MegascansUtilities.isLegacy ()) {
                    GUILayout.BeginHorizontal ();

                    GUILayout.Box ("Scale", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                    decalSize = EditorGUILayout.TextField (decalSize, MSField, GUILayout.Height(fieldSize.y));

                    GUILayout.EndHorizontal ();
                }

                GUILayout.BeginHorizontal ();

                if (GUILayout.Button ("Create Decal Projector", MSStrechedWidthStyle, GUILayout.Height (textSize.y)))
                    MegascansDecalTools.SetupDecalProjector ();

                GUILayout.EndHorizontal ();
#endif
                */
            }
            

            if (!MSLogo)
            {
                InitStyle();
                Repaint();
            }
        }

        void OnEnable()
        {

            SuperHD = (Display.main.systemHeight > 1500);

            size = SuperHD ? new Vector2(750, 1400) : new Vector2(308, 796);
            textSize = SuperHD ? new Vector2(200, 54) : new Vector2(100, 30);
            textHeadingSize = SuperHD ? new Vector2(555, 64) : new Vector2(308, 40);
            fieldSize = SuperHD ? new Vector2(290, 54) : new Vector2(152, 30);
            
            collisionLoc = SuperHD ? new Rect(25, 632, 32, 32) : new Rect(13, 340, 17, 17);
            applyToSelectionLoc = SuperHD ? new Rect(25, 715, 32, 32) : new Rect(13, 384, 17, 17);
            addAssetToSceneLoc = SuperHD ? new Rect(25, 794, 32, 32) : new Rect(13, 427, 17, 17);
            setupPrefabsLoc = SuperHD ? new Rect(25, 875, 32, 32) : new Rect(13, 470, 17, 17);
            importLODsLoc = SuperHD ? new Rect(25, 957, 32, 32) : new Rect(13, 513, 17, 17);
            setupLODLoc = SuperHD ? new Rect(25, 1038, 32, 32) : new Rect(13, 556, 17, 17);
            importAllTexturesLoc = SuperHD ? new Rect(25, 1118, 32, 32) : new Rect(13, 599, 17, 17);
            connectionLoc = SuperHD ? new Rect(25, 1199, 32, 32) : new Rect(13, 642, 17, 17);

            lineYLoc = SuperHD ? 185f : 102f;

            logoSize = SuperHD ? new Vector2(64, 64) : new Vector2(34, 34);
            InitStyle();
            GetDefaults();
            Repaint();
        }

        //If the values dont exist in editor prefs they are replaced with the default values.
        internal static void GetDefaults()
        {
            path = EditorPrefs.GetString("QuixelDefaultPath", "Quixel/Megascans/");
            dispType = EditorPrefs.GetInt("QuixelDefaultDisplacement", 0);
            texPack = EditorPrefs.GetInt("QuixelDefaultTexPacking", 0);
            shaderType = EditorPrefs.GetInt("QuixelDefaultShader", 3);
            importResolution = EditorPrefs.GetInt("QuixelDefaultImportResolution", 4);
            lodFadeMode = EditorPrefs.GetInt("QuixelDefaultLodFadeMode", 1);
            connection = EditorPrefs.GetBool("QuixelDefaultConnection", true);
            setupCollision = EditorPrefs.GetBool("QuixelDefaultSetupCollision", true);
            applyToSelection = EditorPrefs.GetBool("QuixelDefaultApplyToSelection", false);
            addAssetToScene = EditorPrefs.GetBool("QuixelDefaultAddAssetToScene", false);
            importLODs = EditorPrefs.GetBool("QuixelDefaultImportLODs", true);
            setupLOD = EditorPrefs.GetBool("QuixelDefaultSetupLOD", true);
            setupPrefabs = EditorPrefs.GetBool("QuixelDefaultSetupPrefabs", true);
            importAllTextures = EditorPrefs.GetBool("QuixelDefaultImportAllTextures", false);

            decalBlend = EditorPrefs.GetString("QuixelDefaultDecalBlend", "100");
            decalSize = EditorPrefs.GetString("QuixelDefaultDecalSize", "1");

            terrainMaterialName = EditorPrefs.GetString("QuixelDefaultMaterialName", "Terrain Material");
            terrainMaterialPath = EditorPrefs.GetString("QuixelDefaultMaterialPath", "Quixel/");
            tiling = EditorPrefs.GetString("QuixelDefaultTiling", "10");

            pathUpdate = path;
            dispTypeUpdate = dispType;
            texPackUpdate = texPack;
            shaderTypeUpdate = shaderType;
            connectionUpdate = connection;
            setupCollisionUpdate = setupCollision;
            applyToSelectionUpdate = applyToSelection;
            addAssetToSceneUpdate = addAssetToScene;
            setupPrefabsUpdate = setupPrefabs;
            importLODsUpdate = importLODs;
            setupLODUpdate = setupLOD;
            importResolutionUpdate = importResolution;
            lodFadeModeUpdate = lodFadeMode;
            importAllTexturesUpdate = importAllTextures;

            //Decal Properties
            decalBlendUpdate = decalBlend;
            decalSizeUpdate = decalSize;

            //Terrain tool properties
            terrainMaterialNameUpdate = terrainMaterialName;
            terrainMaterialPathUpdate = terrainMaterialPath;
            tilingUpdate = tiling;

            if (connection)
                MegascansBridgeLink.ToggleServer();
        }

        static void SaveDefaults()
        {

            if (connection != connectionUpdate)
            {
                connectionUpdate = connection;
                MegascansBridgeLink.ToggleServer(connection);
            }

            EditorPrefs.SetString("QuixelDefaultPath", path);
            EditorPrefs.SetInt("QuixelDefaultDisplacement", dispType);
            EditorPrefs.SetInt("QuixelDefaultTexPacking", texPack);
            EditorPrefs.SetInt("QuixelDefaultShader", shaderType);
            EditorPrefs.SetBool("QuixelDefaultConnection", connection);
            EditorPrefs.SetBool("QuixelDefaultSetupCollision", setupCollision);
            EditorPrefs.SetBool("QuixelDefaultApplyToSelection", applyToSelection);
            EditorPrefs.SetBool("QuixelDefaultAddAssetToScene", addAssetToScene);
            EditorPrefs.SetBool("QuixelDefaultImportLODs", importLODs);
            EditorPrefs.SetBool("QuixelDefaultSetupLOD", setupLOD);
            EditorPrefs.SetBool("QuixelDefaultSetupPrefabs", setupPrefabs);
            EditorPrefs.SetInt("QuixelDefaultImportResolution", importResolution);
            EditorPrefs.SetInt("QuixelDefaultLodFadeMode", lodFadeMode);
            EditorPrefs.SetBool("QuixelDefaultImportAllTextures", importAllTextures);

            pathUpdate = path;
            dispTypeUpdate = dispType;
            texPackUpdate = texPack;
            shaderTypeUpdate = shaderType;
            importResolutionUpdate = importResolution;
            setupCollisionUpdate = setupCollision;
            applyToSelectionUpdate = applyToSelection;
            addAssetToSceneUpdate = addAssetToScene;
            setupPrefabsUpdate = setupPrefabs;
            importLODsUpdate = importLODs;
            setupLODUpdate = setupLOD;
            lodFadeModeUpdate = lodFadeMode;
            importAllTexturesUpdate = importAllTextures;

            //Decal Properties

            EditorPrefs.SetString("QuixelDefaultDecalBlend", decalBlend);
            EditorPrefs.SetString("QuixelDefaultDecalSize", decalSize);

            decalBlendUpdate = decalBlend;
            decalSizeUpdate = decalSize;

            //Terrain tool properties

            EditorPrefs.SetString("QuixelDefaultMaterialName", terrainMaterialName);
            EditorPrefs.SetString("QuixelDefaultMaterialPath", terrainMaterialPath);
            EditorPrefs.SetString("QuixelDefaultTiling", tiling);

            terrainMaterialNameUpdate = terrainMaterialName;
            terrainMaterialPathUpdate = terrainMaterialPath;
            tilingUpdate = tiling;
        }

        void ConstructPopUp()
        {
            MSPopup = new GUIStyle();
            MSPopup.normal.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSPopup.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Text_Background.png");

            MSPopup.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSPopup.fontSize = SuperHD ? 24 : 13;
            MSPopup.padding = SuperHD ? new RectOffset(20, 0, 10, 0) : new RectOffset(10, 5, 7, 4);
            MSPopup.margin = SuperHD ? new RectOffset(0, 20, 13, 7) : new RectOffset(0, 10, 6, 5);
            //MSPopup.
        }

        void ConstructText()
        {
            MSText = new GUIStyle();
            MSText.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
            MSText.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSText.fontSize = SuperHD ? 24 : 13;
            MSText.padding = SuperHD ? new RectOffset(5, 0, 10, 0) : new RectOffset(5, 5, 7, 4);
            MSText.margin = SuperHD ? new RectOffset(20, 0, 13, 7) : new RectOffset(10, 20, 6, 5);
        }

        void ConstructBackground()
        {
            MSBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Background.png");
        }

        void ConstructLogo()
        {
            MSLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/M.png");
            BridgeLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/B.png");
            MSLogoStyle = new GUIStyle();
            MSLogoStyle.margin = SuperHD ? new RectOffset(25, 0, 27, 33) : new RectOffset(15, 0, 15, 15);
        }

        void ConstructField()
        {
            MSField = new GUIStyle();
            MSField.normal.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSField.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSField.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSField.clipping = TextClipping.Clip;
            MSField.fontSize = SuperHD ? 24 : 13;
            MSField.padding = SuperHD ? new RectOffset(20, 0, 10, 0) : new RectOffset(10, 5, 7, 4);
            MSField.margin = SuperHD ? new RectOffset(0, 20, 13, 7) : new RectOffset(0, 10, 6, 5);
        }

        void ConstructCheckBox()
        {
            MSCheckBox = new GUIStyle();
            MSCheckBox.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/CheckBoxOff.png");
            MSCheckBox.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/CheckBoxHover.png");
            MSCheckBox.onNormal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/CheckBoxOn.png");
        }

        void ConstructHelp()
        {
            MSHelpStyle = new GUIStyle();
            MSHelpStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Help.png");
            MSHelpStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSHelpStyle.margin = SuperHD ? new RectOffset(152, 20, 35, 15) : new RectOffset(102, 0, 16, 5);
            MSHelpStyle.padding = SuperHD ? new RectOffset(20, 20, 10, 10) : new RectOffset(10, 10, 5, 5);
            MSHelpStyle.fontSize = SuperHD ? 24 : 12;
            MSHelpStyle.normal.textColor = new Color(0.16796875f, 0.59375f, 0.9375f);
        }

        void ConstructNormalText()
        {
            MSNormalTextStyle = new GUIStyle();
            MSNormalTextStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSNormalTextStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSNormalTextStyle.fontSize = SuperHD ? 24 : 13;
            MSNormalTextStyle.padding = SuperHD ? new RectOffset(5, 0, 15, 15) : new RectOffset(5, 5, 7, 4);
            MSNormalTextStyle.margin = SuperHD ? new RectOffset(72, 0, 27, 10) : new RectOffset(37, 20, 13, 5);
        }

        void ConstructWarningText()
        {
            MSWarningTextStyle = new GUIStyle();
            MSWarningTextStyle.normal.textColor = new Color(1.0f, 1.0f, 0.0f);
            MSWarningTextStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSWarningTextStyle.fontSize = SuperHD ? 24 : 13;
            MSWarningTextStyle.padding = SuperHD ? new RectOffset(5, 0, 15, 15) : new RectOffset(5, 5, 7, 4);
            MSWarningTextStyle.margin = SuperHD ? new RectOffset(10, 0, 27, 10) : new RectOffset(10, 0, 13, 5);
            MSWarningTextStyle.wordWrap = true;
        }

        void ConstructHeadingText()
        {
            MSHeadingTextStyle = new GUIStyle();
            MSHeadingTextStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSHeadingTextStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSHeadingTextStyle.fontSize = SuperHD ? 30 : 16;
            MSHeadingTextStyle.alignment = TextAnchor.MiddleCenter;
        }

        void ContrauctTabs()
        {
            MSTabsStyle = new GUIStyle();
            MSTabsStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSTabsStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Text_Background.png");
            MSTabsStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSTabsStyle.hover.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSTabsStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSTabsStyle.active.textColor = new Color(0.5f, 0.5f, 0.5f);
            MSTabsStyle.fontSize = SuperHD ? 26 : 15;
            MSTabsStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSTabsStyle.margin = new RectOffset(5, 5, 10, 10);
            MSTabsStyle.alignment = TextAnchor.MiddleCenter;
        }

        void ContrauctStrechedWidth()
        {
            MSStrechedWidthStyle = new GUIStyle();
            MSStrechedWidthStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSStrechedWidthStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Text_Background.png");
            MSStrechedWidthStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSStrechedWidthStyle.hover.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSStrechedWidthStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSStrechedWidthStyle.active.textColor = new Color(0.5f, 0.5f, 0.5f);
            MSStrechedWidthStyle.fontSize = SuperHD ? 26 : 15;
            MSStrechedWidthStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f);
            MSStrechedWidthStyle.margin = new RectOffset(0, 0, 10, 10);
            MSStrechedWidthStyle.alignment = TextAnchor.MiddleCenter;
        }

        void InitStyle()
        {
            ConstructBackground();
            ConstructLogo();
            ConstructPopUp();
            ConstructText();
            ConstructField();
            ConstructCheckBox();
            ConstructHelp();
            ConstructNormalText();
            ConstructWarningText();
            ConstructHeadingText();
            ContrauctTabs();
            ContrauctStrechedWidth();
        }

        private void Update()
        {
            if (
                (dispType != dispTypeUpdate) ||
                (shaderType != shaderTypeUpdate) ||
                (texPack != texPackUpdate) ||
                (path != pathUpdate) ||
                (connection != connectionUpdate) ||
                (importResolution != importResolutionUpdate) ||
                (lodFadeMode != lodFadeModeUpdate) ||
                (setupCollision != setupCollisionUpdate) ||
                (applyToSelection != applyToSelectionUpdate) ||
                (addAssetToScene != addAssetToSceneUpdate) ||
                (importLODs != importLODsUpdate) ||
                (setupLOD != setupLODUpdate) ||
                (setupPrefabs != setupPrefabsUpdate) ||
                (decalBlendUpdate != decalBlend) ||
                (decalSizeUpdate != decalSize) ||
                (terrainMaterialNameUpdate != terrainMaterialName) ||
                (terrainMaterialPathUpdate != terrainMaterialPath) ||
                (importAllTextures != importAllTexturesUpdate) ||
                (tilingUpdate != tiling)
            )
            {
                SaveDefaults();
            }
        }
    }
}

#endif