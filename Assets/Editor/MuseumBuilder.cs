using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Builds the whole "Lucy Mission VR Space Museum" scene from code.
// Run from the menu: Museum > Build Scene  (Unity Cloud Build calls MuseumBuilder.PreExport automatically).
public static class MuseumBuilder
{
    const string GenDir = "Assets/Generated";
    const string ScenePath = "Assets/Scenes/Museum.unity";
    static Font font;

    [MenuItem("Museum/Build Scene")]
    public static void PreExport()
    {
        Directory.CreateDirectory(GenDir);
        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.Refresh();
        Random.InitState(42);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Environment ----------
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.44f, 0.6f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.01f, 0.012f, 0.04f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;

        var sunLight = new GameObject("Directional Light").AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.intensity = 0.7f;
        sunLight.transform.rotation = Quaternion.Euler(50, -30, 0);

        // ---------- Materials ----------
        var mFloor = Mat("Floor", new Color(0.07f, 0.09f, 0.22f), GridTex());
        mFloor.mainTextureScale = new Vector2(8, 8);
        var mWall = Mat("Wall", new Color(0.08f, 0.1f, 0.24f));
        var mPedestal = Mat("Pedestal", new Color(0.17f, 0.2f, 0.45f));
        var mNeon = Mat("Neon", new Color(0.3f, 0.75f, 1f), null, new Color(0.3f, 0.75f, 1f) * 2f);
        var mStar = Mat("Star", Color.white, null, Color.white * 1.5f);
        var mCeil = Mat("Ceiling", new Color(0.02f, 0.025f, 0.07f));
        var mSun = Mat("Sun", new Color(1f, 0.7f, 0.1f), NoiseTex("SunTex", new Color(1f, 0.55f, 0f), new Color(1f, 0.88f, 0.3f)), new Color(1f, 0.55f, 0.05f) * 1.6f);
        var mEarth = Mat("Earth", Color.white, EarthTex());
        var mMoon = Mat("Moon", new Color(0.75f, 0.75f, 0.75f));
        var mMars = Mat("Mars", Color.white, NoiseTex("MarsTex", new Color(0.56f, 0.23f, 0.12f), new Color(0.82f, 0.47f, 0.3f)));
        var mJupiter = Mat("Jupiter", Color.white, JupiterTex());
        var mRock = Mat("Rock", Color.white, NoiseTex("RockTex", new Color(0.29f, 0.27f, 0.24f), new Color(0.55f, 0.51f, 0.47f)));
        var mGold = Mat("Gold", new Color(0.83f, 0.69f, 0.22f)); mGold.SetFloat("_Metallic", 0.8f); mGold.SetFloat("_Glossiness", 0.7f);
        var mPanel = Mat("SolarPanel", new Color(0.12f, 0.24f, 0.6f), null, new Color(0.03f, 0.07f, 0.25f)); mPanel.SetFloat("_Metallic", 0.6f);
        var mGrey = Mat("Grey", new Color(0.6f, 0.6f, 0.6f));

        // ---------- Hall ----------
        var hall = new GameObject("Museum Hall").transform;
        Prim(PrimitiveType.Plane, "Floor", hall, new Vector3(0, 0, 0), new Vector3(3f, 1, 2.8f), mFloor);
        Prim(PrimitiveType.Cube, "Wall Back", hall, new Vector3(0, 3, -14), new Vector3(30, 6, 0.3f), mWall);
        Prim(PrimitiveType.Cube, "Wall Front", hall, new Vector3(0, 3, 14), new Vector3(30, 6, 0.3f), mWall);
        Prim(PrimitiveType.Cube, "Wall Left", hall, new Vector3(-15, 3, 0), new Vector3(0.3f, 6, 28), mWall);
        Prim(PrimitiveType.Cube, "Wall Right", hall, new Vector3(15, 3, 0), new Vector3(0.3f, 6, 28), mWall);
        Prim(PrimitiveType.Cube, "Ceiling", hall, new Vector3(0, 6.1f, 0), new Vector3(30, 0.2f, 28), mCeil);
        Prim(PrimitiveType.Cube, "Neon Back", hall, new Vector3(0, 0.06f, -13.8f), new Vector3(30, 0.08f, 0.06f), mNeon, false);
        Prim(PrimitiveType.Cube, "Neon Left", hall, new Vector3(-14.8f, 0.06f, 0), new Vector3(0.06f, 0.08f, 28), mNeon, false);
        Prim(PrimitiveType.Cube, "Neon Right", hall, new Vector3(14.8f, 0.06f, 0), new Vector3(0.06f, 0.08f, 28), mNeon, false);
        var stars = new GameObject("Stars").transform; stars.SetParent(hall);
        for (int i = 0; i < 160; i++)
        {
            float s = Random.Range(0.04f, 0.1f);
            Prim(PrimitiveType.Sphere, "Star", stars, new Vector3(Random.Range(-14.5f, 14.5f), 5.95f, Random.Range(-13.5f, 13.5f)), Vector3.one * s, mStar, false);
        }
        Label("LUCY MISSION — VR SPACE MUSEUM", new Vector3(0, 4.8f, -13.8f), 0.32f, new Color(1f, 0.83f, 0.42f), hall);
        Label("Journey to the Trojan Asteroids of Jupiter", new Vector3(0, 4.1f, -13.8f), 0.16f, new Color(0.73f, 0.77f, 1f), hall);

        // ---------- Exhibits ----------
        // 1. Sun
        var sun = Pedestal("The Sun", new Vector3(0, 0, -9), 1.8f, mPedestal);
        var sunBall = Prim(PrimitiveType.Sphere, "Sun", sun, sun.position + new Vector3(0, 2.4f, 0), Vector3.one * 2.2f, mSun);
        AddExhibit(sunBall, "The Sun", "Lucy is solar powered. Near Jupiter the Sun is about 5x farther away than from Earth, so sunlight is about 25x weaker. That is why Lucy has two huge solar arrays.", 15);
        var pl = new GameObject("Sun Light").AddComponent<Light>();
        pl.type = LightType.Point; pl.range = 14; pl.intensity = 2.2f; pl.color = new Color(1f, 0.72f, 0.3f);
        pl.transform.SetParent(sun); pl.transform.localPosition = new Vector3(0, 2.4f, 0);
        Label("THE SUN", sun.position + new Vector3(0, 1.15f, 0.95f), 0.12f, new Color(1f, 0.83f, 0.42f), sun);

        // 2. Earth + Moon
        var earth = Pedestal("Earth", new Vector3(-8, 0, 3), 1.4f, mPedestal);
        var earthBall = Prim(PrimitiveType.Sphere, "Earth", earth, earth.position + new Vector3(0, 2f, 0), Vector3.one * 1.4f, mEarth);
        AddExhibit(earthBall, "Earth: Launch & Gravity Assists", "Lucy launched on 16 October 2021 on an Atlas V rocket from Cape Canaveral. It swings past Earth several times, using Earth's gravity like a slingshot to reach Jupiter's orbit.", 25);
        var moonPivot = new GameObject("Moon Orbit").transform; moonPivot.SetParent(earth); moonPivot.localPosition = new Vector3(0, 2.3f, 0);
        Prim(PrimitiveType.Sphere, "Moon", moonPivot, moonPivot.position + new Vector3(1.4f, 0, 0), Vector3.one * 0.3f, mMoon);
        var orbit = moonPivot.gameObject.AddComponent<Exhibit>(); orbit.title = "The Moon"; orbit.description = "Earth's Moon."; orbit.spin = new Vector3(0, 40, 0);
        Label("EARTH  (Launch: Oct 16, 2021)", earth.position + new Vector3(0, 1.15f, 0.75f), 0.09f, new Color(0.56f, 0.83f, 1f), earth);

        // 3. Mars
        var mars = Pedestal("Mars", new Vector3(-8, 0, -6), 1.2f, mPedestal);
        var marsBall = Prim(PrimitiveType.Sphere, "Mars", mars, mars.position + new Vector3(0, 1.8f, 0), Vector3.one * 1f, mMars);
        AddExhibit(marsBall, "Mars", "Lucy's path passes through the main asteroid belt between Mars and Jupiter, where it flies by its first targets before heading to the Trojans.", 25);
        Label("MARS", mars.position + new Vector3(0, 1.15f, 0.65f), 0.1f, new Color(1f, 0.6f, 0.45f), mars);

        // 4. Jupiter + Trojan swarms
        var jup = Pedestal("Jupiter", new Vector3(8, 0, -6), 2f, mPedestal);
        var jupBall = Prim(PrimitiveType.Sphere, "Jupiter", jup, jup.position + new Vector3(0, 2.6f, 0), Vector3.one * 2.4f, mJupiter);
        AddExhibit(jupBall, "Jupiter & the Trojan Asteroids", "The Trojans are two swarms of asteroids sharing Jupiter's orbit, one ahead of the planet and one behind it (the L4 and L5 points). They are leftovers from the birth of the planets - 'fossils' of the early Solar System.", 35);
        var swarm = new GameObject("Trojan Swarms").transform; swarm.SetParent(jup); swarm.localPosition = new Vector3(0, 2.6f, 0);
        foreach (float deg in new[] { 60f, -60f })
            for (int i = 0; i < 14; i++)
            {
                float a = (deg + Random.Range(-18f, 18f)) * Mathf.Deg2Rad, r = Random.Range(1.9f, 2.4f);
                var rock = Prim(PrimitiveType.Sphere, "Trojan", swarm, swarm.position + new Vector3(Mathf.Cos(a) * r, Random.Range(-0.25f, 0.25f), Mathf.Sin(a) * r),
                    new Vector3(Random.Range(0.1f, 0.25f), Random.Range(0.08f, 0.18f), Random.Range(0.1f, 0.22f)), mRock, false);
                rock.rotation = Random.rotation;
            }
        var sw = swarm.gameObject.AddComponent<Exhibit>(); sw.title = "Trojan Asteroids"; sw.description = "Two swarms of asteroids that share Jupiter's orbit."; sw.spin = new Vector3(0, 9, 0);
        Label("JUPITER & TROJANS", jup.position + new Vector3(0, 1.15f, 1.05f), 0.1f, new Color(1f, 0.81f, 0.6f), jup);

        // 5. Lucy spacecraft
        var lucyBase = Pedestal("Lucy Spacecraft", new Vector3(8, 0, 3), 1.6f, mPedestal);
        var lucy = new GameObject("Lucy Spacecraft").transform; lucy.SetParent(lucyBase); lucy.localPosition = new Vector3(0, 2.3f, 0);
        Prim(PrimitiveType.Cube, "Body", lucy, lucy.position, new Vector3(0.6f, 0.5f, 0.5f), mGold);
        Prim(PrimitiveType.Cylinder, "Antenna", lucy, lucy.position + new Vector3(0, 0.35f, 0), new Vector3(0.5f, 0.05f, 0.5f), mGrey);
        foreach (int side in new[] { -1, 1 })
        {
            var arm = Prim(PrimitiveType.Cylinder, "Boom", lucy, lucy.position + new Vector3(side * 0.75f, 0, 0), new Vector3(0.05f, 0.45f, 0.05f), mGrey);
            arm.rotation = Quaternion.Euler(0, 0, 90);
            var panel = Prim(PrimitiveType.Cylinder, "Solar Array", lucy, lucy.position + new Vector3(side * 1.55f, 0, 0), new Vector3(1.3f, 0.02f, 1.3f), mPanel);
            panel.rotation = Quaternion.Euler(0, 0, 90);
        }
        AddExhibit(lucy, "Lucy Spacecraft", "Lucy is the first mission to visit the Jupiter Trojans. It is named after the famous 'Lucy' human-ancestor fossil, because the Trojan asteroids may be fossils of planet formation. Each round solar array is about 7 m wide.", 20);
        Label("LUCY SPACECRAFT", lucyBase.position + new Vector3(0, 1.15f, 0.85f), 0.1f, new Color(1f, 0.83f, 0.42f), lucyBase);

        // 6. Asteroid Donaldjohanson
        var ast = Pedestal("Asteroid", new Vector3(0, 0, 1), 1.2f, mPedestal, 0.8f);
        var rockBig = Prim(PrimitiveType.Sphere, "Donaldjohanson", ast, ast.position + new Vector3(0, 1.6f, 0), new Vector3(1.3f, 0.8f, 0.9f), mRock);
        AddExhibit(rockBig, "Asteroid Donaldjohanson", "A main-belt asteroid Lucy flew past in April 2025. It is named after Donald Johanson, the scientist who discovered the 'Lucy' fossil in 1974.", 18);
        rockBig.GetComponent<Exhibit>().spin = new Vector3(12, 18, 0);
        Label("ASTEROID DONALDJOHANSON", ast.position + new Vector3(0, 0.95f, 0.65f), 0.08f, new Color(0.8f, 0.84f, 1f), ast);

        // ---------- Player ----------
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1.0f, 10f);
        player.transform.rotation = Quaternion.Euler(0, 180, 0); // face the exhibits
        var cc = player.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.35f;
        var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
        camGo.transform.SetParent(player.transform); camGo.transform.localPosition = new Vector3(0, 0.6f, 0);
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.01f, 0.012f, 0.04f);
        cam.fieldOfView = 70; cam.nearClipPlane = 0.05f;
        camGo.AddComponent<AudioListener>();
        player.AddComponent<PlayerController>();

        // ---------- UI ----------
        var canvasGo = new GameObject("UI Canvas");
        var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
        var ui = canvasGo.AddComponent<MuseumUI>();

        var help = Panel(canvasGo.transform, "Help", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(430, 120), new Vector2(0, 1));
        UIText(help, "<b><color=#FFD36B>Lucy Mission VR Space Museum</color></b>  (made in Unity)\n<b>W A S D</b> / arrows: walk     <b>Drag mouse</b>: look around\n<b>Click</b> an exhibit (or press <b>E</b>): show information", 16, TextAnchor.UpperLeft, new Vector2(12, -10), new Vector2(-12, 10));

        var cross = new GameObject("Crosshair", typeof(RectTransform)); cross.transform.SetParent(canvasGo.transform, false);
        var ct = cross.AddComponent<Text>(); ct.font = font; ct.text = "+"; ct.fontSize = 28; ct.alignment = TextAnchor.MiddleCenter; ct.color = new Color(1f, 0.83f, 0.42f);
        var crt = cross.GetComponent<RectTransform>(); crt.sizeDelta = new Vector2(40, 40);

        var info = Panel(canvasGo.transform, "Info Panel", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(760, 150), new Vector2(0.5f, 0));
        ui.infoTitle = UIText(info, "Title", 22, TextAnchor.UpperLeft, new Vector2(16, -10), new Vector2(-16, 106));
        ui.infoTitle.color = new Color(1f, 0.83f, 0.42f); ui.infoTitle.fontStyle = FontStyle.Bold;
        ui.infoBody = UIText(info, "Body", 17, TextAnchor.UpperLeft, new Vector2(16, -44), new Vector2(-16, 10));
        ui.infoPanel = info.gameObject;

        // ---------- Save + build settings ----------
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Lucy Mission VR Space Museum";
        PlayerSettings.companyName = "Student Project";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        AssetDatabase.SaveAssets();
        Debug.Log("Museum scene built: " + ScenePath);
    }

    // ================= helpers =================
    static Transform Pedestal(string name, Vector3 pos, float width, Material m, float height = 1f)
    {
        var root = new GameObject(name + " Exhibit").transform; root.position = pos;
        Prim(PrimitiveType.Cylinder, "Pedestal", root, pos + new Vector3(0, height / 2f, 0), new Vector3(width, height / 2f, width), m);
        return root;
    }

    static Transform Prim(PrimitiveType t, string name, Transform parent, Vector3 worldPos, Vector3 scale, Material m, bool keepCollider = true)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = worldPos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go.transform;
    }

    static void AddExhibit(Transform t, string title, string desc, float spinY)
    {
        var ex = t.gameObject.AddComponent<Exhibit>();
        ex.title = title; ex.description = desc; ex.spin = new Vector3(0, spinY, 0);
        if (t.GetComponent<Collider>() == null)
        {
            var bc = t.gameObject.AddComponent<BoxCollider>(); bc.size = new Vector3(4f, 1.2f, 1.2f);
        }
    }

    static void Label(string text, Vector3 pos, float size, Color color, Transform parent)
    {
        var go = new GameObject("Label: " + text);
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, 180, 0); // face the player
        var tm = go.AddComponent<TextMesh>();
        tm.text = text; tm.font = font; tm.fontSize = 64; tm.characterSize = size * 0.25f;
        tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = color;
        go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
    }

    static RectTransform Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = new Color(0.03f, 0.05f, 0.15f, 0.88f);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
        return rt;
    }

    static Text UIText(RectTransform parent, string text, int size, TextAnchor anchor, Vector2 offMin, Vector2 offMax)
    {
        var go = new GameObject("Text", typeof(RectTransform)); go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>(); t.font = font; t.text = text; t.fontSize = size; t.alignment = anchor; t.color = Color.white; t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(offMin.x, offMax.y); rt.offsetMax = new Vector2(offMax.x, offMin.y);
        return t;
    }

    static Material Mat(string name, Color color, Texture2D tex = null, Color? emission = null)
    {
        var m = new Material(Shader.Find("Standard")) { color = color };
        if (tex != null) m.mainTexture = tex;
        m.SetFloat("_Glossiness", 0.2f);
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        AssetDatabase.CreateAsset(m, GenDir + "/" + name + ".mat");
        return m;
    }

    static Texture2D SaveTex(string name, Texture2D t)
    {
        t.Apply();
        AssetDatabase.CreateAsset(t, GenDir + "/" + name + ".asset");
        return t;
    }

    static Texture2D NoiseTex(string name, Color a, Color b)
    {
        var t = new Texture2D(256, 128);
        float ox = Random.Range(0f, 100f);
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 256; x++)
            {
                float n = Mathf.PerlinNoise(ox + x * 0.05f, y * 0.05f) * 0.7f + Mathf.PerlinNoise(x * 0.2f, ox + y * 0.2f) * 0.3f;
                t.SetPixel(x, y, Color.Lerp(a, b, n));
            }
        return SaveTex(name, t);
    }

    static Texture2D JupiterTex()
    {
        var t = new Texture2D(256, 128);
        Color[] bands = { new Color(0.85f, 0.71f, 0.54f), new Color(0.69f, 0.48f, 0.31f), new Color(0.91f, 0.84f, 0.72f), new Color(0.61f, 0.42f, 0.27f), new Color(0.94f, 0.89f, 0.78f) };
        for (int y = 0; y < 128; y++)
        {
            Color c = bands[(int)(Mathf.PerlinNoise(0.5f, y * 0.06f) * 10) % bands.Length];
            for (int x = 0; x < 256; x++)
            {
                float wob = Mathf.PerlinNoise(x * 0.04f, y * 0.3f) * 0.15f;
                float dx = (x - 160) / 22f, dy = (y - 44) / 10f;
                t.SetPixel(x, y, (dx * dx + dy * dy < 1f) ? new Color(0.71f, 0.28f, 0.18f) : c * (0.92f + wob));
            }
        }
        return SaveTex("JupiterTex", t);
    }

    static Texture2D EarthTex()
    {
        var t = new Texture2D(256, 128);
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 256; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.035f + 7, y * 0.05f + 3);
                Color c = n > 0.55f ? new Color(0.25f, 0.56f, 0.23f) : new Color(0.11f, 0.37f, 0.72f);
                if (n > 0.7f) c = new Color(0.65f, 0.55f, 0.31f);
                if (y < 8 || y > 119) c = Color.white;
                float cloud = Mathf.PerlinNoise(x * 0.08f + 50, y * 0.15f);
                if (cloud > 0.68f) c = Color.Lerp(c, Color.white, 0.6f);
                t.SetPixel(x, y, c);
            }
        return SaveTex("EarthTex", t);
    }

    static Texture2D GridTex()
    {
        var t = new Texture2D(64, 64);
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                t.SetPixel(x, y, (x < 2 || y < 2) ? new Color(0.25f, 0.32f, 0.75f) : new Color(0.07f, 0.09f, 0.22f));
        t.wrapMode = TextureWrapMode.Repeat;
        return SaveTex("GridTex", t);
    }
}
