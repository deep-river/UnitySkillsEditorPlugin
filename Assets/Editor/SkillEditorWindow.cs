using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class SkillEditorWindow : EditorWindow
{
    // 技能属性
    public int skillID; // 技能ID
    public string skillName; // 技能名称
    public string skillDescription; // 技能描述
    public int targetDetectType; // 目标检测类型
    private string[] targetDetectTypesArray = new string[]
    {
        "敌对角色",
        "友方角色",
        "友方除自己",
        "全体角色",
        "全体除自己"
    };
    public int skillTargetType; // 技能目标类型
    private string[] skillTargetTypesArray = new string[]
    {
        "面向方向",
        "区域",
        "单体锁定",
        "对自己"
    };

    private Texture skillIcon;
    public string icon; // 技能图标
    public int skillTypeIndex; // 技能类型
    private string[] skillTypesArray = new string[]
    {
        "普攻",
        "主动技能",
        "被动技能"
    };
    public float coolDownDuration; // CD时间
    public float castDistance; // 施法距离
    public int skillIndicatorType; // 技能指示器形状
    private string[] skillIndicatorTypesArray = new string[]
    {
        "圆形",
        "矩形",
        "扇形"
    };
    public float rangeParam1; // 范围参数1
    public float rangeParam2; // 范围参数2
    public int resourceType; // 资源消耗类型
    private string[] resourceTypesArray = new string[]
    {
        "魔法值"
    };
    public float resourceAmout; // 资源消耗数量
    public int levelRequired; // 升级所需等级
    public bool castFacingDir; // 面向施法方向
    public bool isRotateWhenCast; // 施法时可转向
    public bool isMoveWhenCast; // 施法时可移动

    bool m_Foldout1;
    GUIContent m_Content1 = new GUIContent("基本信息");
    bool m_Foldout2;
    GUIContent m_Content2 = new GUIContent("技能施放与检测");
    bool m_Foldout3;
    GUIContent m_Content3 = new GUIContent("其他配置");

    string path = "Assets/Resources/";
    string configName = "skillConfig";
    string ext = ".asset";
    public SkillConfigSto configFile;
    public SkillConfigSto lastConfigFile; // 记录上一次打开的配置文件

    // 动画控制
    public GameObject model;
    Animator animator;
    private int animationIndex = 0;
    float currentFrame;
    private Vector2 scrollView = new Vector2(0, 0);
    int frameIndex;
    float frameTimeFloat;

    [MenuItem("工具箱/技能编辑器")]
    static void Open()
    {
        SkillEditorWindow window = (SkillEditorWindow)GetWindow(typeof(SkillEditorWindow));
        window.Show();
    }

    private void OnGUI()
    { 
        // 新建配置文件
        if (GUILayout.Button("新建配置", GUILayout.Width(200)))
        {
            ScriptableObject scriptable = ScriptableObject.CreateInstance<SkillConfigSto>();
            int index = 0;
            string url = "";
            while(true)
            {
                url = path + configName + index + ext;
                if (!File.Exists(url))
                {
                    break;
                }
                ++index;
            }
            AssetDatabase.CreateAsset(scriptable, url);
            configFile = Resources.Load(configName + index) as SkillConfigSto;
        }

        // 读取配置文件
        configFile = EditorGUILayout.ObjectField("配置文件", configFile, typeof(ScriptableObject), true) as SkillConfigSto;
        if (configFile != lastConfigFile)
        {
            configFile = lastConfigFile;
            LoadConfig();
        }

        // 保存配置文件
        if (GUILayout.Button("保存配置", GUILayout.Width(200)))
        {
            SaveConfig();
        }

        // 技能属性编辑
        EditorGUILayout.BeginVertical(GUI.skin.box);
        // EditorGUI.indentLevel++;
        m_Foldout1 = EditorGUILayout.Foldout(m_Foldout1, m_Content1);
        if (m_Foldout1)
        {
            skillIcon = EditorGUILayout.ObjectField("技能图标", skillIcon, typeof(Texture), true) as Texture;
            skillID = EditorGUILayout.IntField("技能ID:", skillID);
            skillName = EditorGUILayout.TextField("显示名称:", skillName);
            EditorGUILayout.LabelField("技能描述");
            skillDescription = EditorGUILayout.TextArea(skillDescription, GUILayout.Height(35));
            skillTypeIndex = EditorGUILayout.Popup("技能类型:", skillTypeIndex, skillTypesArray);
            skillTargetType = EditorGUILayout.Popup("技能目标类型:", skillTargetType, skillTargetTypesArray);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        m_Foldout2 = EditorGUILayout.Foldout(m_Foldout2, m_Content2);
        if (m_Foldout2)
        {
            targetDetectType = EditorGUILayout.Popup("技能目标检测类型:", targetDetectType, targetDetectTypesArray);
            resourceType = EditorGUILayout.Popup("资源消耗类型:", resourceType, resourceTypesArray);
            resourceAmout = EditorGUILayout.FloatField("资源消耗数量:", resourceAmout);
            coolDownDuration = EditorGUILayout.FloatField("CD时间:", coolDownDuration);
            castDistance = EditorGUILayout.FloatField("施法距离:", castDistance);
            skillIndicatorType = EditorGUILayout.Popup("技能指示器形状:", skillIndicatorType, skillIndicatorTypesArray);
            rangeParam1 = EditorGUILayout.FloatField("范围参数1:", rangeParam1);
            rangeParam2 = EditorGUILayout.FloatField("范围参数2:", rangeParam2);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        m_Foldout3 = EditorGUILayout.Foldout(m_Foldout3, m_Content3);
        if (m_Foldout3)
        {
            levelRequired = EditorGUILayout.IntField("升级所需等级:", levelRequired);
            castFacingDir = EditorGUILayout.Toggle("面向施法方向", castFacingDir);
            isRotateWhenCast = EditorGUILayout.Toggle("施法时可转向", isRotateWhenCast);
            isMoveWhenCast = EditorGUILayout.Toggle("施法时可移动", isMoveWhenCast);
        }
        EditorGUILayout.EndVertical();

        // 动画编辑
        model = EditorGUILayout.ObjectField("添加角色", model, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("应用角色", GUILayout.Width(200)))
        {
            if (model == null)
            {
                Debug.LogError("未选中任何模型");
                return;
            }
            animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("模型未绑定Animator组件");
                return;
            }
        }

        if (animator != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            animationIndex = Mathf.Clamp(animationIndex, 0, clips.Length);
            string[] clipNamesArray = clips.Select(t => t.name).ToArray();
            animationIndex = EditorGUILayout.Popup("动画片段", animationIndex, clipNamesArray);

            AnimationClip clip = clips[animationIndex];
            // 拖动slider在Scene窗口预览动画帧
            // clip.SampleAnimation(animator.gameObject, frameTimeFloat);
            // frameTimeFloat = EditorGUILayout.Slider(frameTimeFloat, 0, clip.length);
            clip.SampleAnimation(animator.gameObject, currentFrame);
            frameIndex = EditorGUILayout.IntSlider(frameIndex, 0, (int)(clip.length * clip.frameRate - 1));

            EditorGUILayout.LabelField("动画时长：" + clip.length);
            DrawFramesView(clip);
        }
    }

    void DrawFramesView(AnimationClip clip)
    {
        int frameCount = (int)(clip.length * clip.frameRate); // 动画长度帧数
        float frameViewWidth = 40;
        float frameInfoSectionWidth = 600;
        float frameInfoSectionHeight = 60;

        scrollView = EditorGUILayout.BeginScrollView(scrollView, true, true, GUILayout.Width(frameInfoSectionWidth), GUILayout.Height(frameInfoSectionHeight));
        EditorGUILayout.BeginHorizontal();

        // 绘制帧序列视图
        for (int i = 0; i < frameCount; i++)
        {
            bool selected = frameIndex == i;
            string title = "" + i;
            if (GUILayout.Button(title, selected?GUIStyles.item_select:GUIStyles.item_normal, GUILayout.Width(frameViewWidth)))
            {
                frameIndex = selected ? -1 : i;
            }
            currentFrame = frameIndex / clip.frameRate;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    // 载入技能配置
    void LoadConfig()
    {
        if (configFile==null)
        {
            return;
        }

        skillIcon = Resources.Load("skillIcon/" + configFile.icon) as Texture;
        skillID = configFile.skillID;
        skillName = configFile.skillName;
        skillDescription = configFile.skillDescription;
        skillTypeIndex = configFile.skillType;
        skillTargetType = configFile.skillTargetType;

        targetDetectType = configFile.targetDetectType;
        resourceType = configFile.resourceType;
        resourceAmout = configFile.resourceAmout;
        coolDownDuration = configFile.coolDownDuration;
        castDistance = configFile.castDistance;
        skillIndicatorType = configFile.skillIndicatorType;
        rangeParam1 = configFile.rangeParam1;
        rangeParam2 = configFile.rangeParam2;

        levelRequired = configFile.levelRequired;
        castFacingDir = configFile.castFacingDir;
        isRotateWhenCast = configFile.isRotateWhenCast;
        isMoveWhenCast = configFile.isMoveWhenCast;
    }

    // 保存技能配置
    void SaveConfig()
    {
        if (configFile == null)
        {
            return;
        }

        if (skillIcon != null)
        {
            configFile.icon = skillIcon.name;
        }
        configFile.skillID = skillID;
        configFile.skillName = skillName;
        configFile.skillDescription = skillDescription;
        configFile.skillType = skillTypeIndex;
        configFile.skillTargetType = skillTargetType;

        configFile.targetDetectType = targetDetectType;
        configFile.resourceType = resourceType;
        configFile.resourceAmout = resourceAmout;
        configFile.coolDownDuration = coolDownDuration;
        configFile.castDistance = castDistance;
        configFile.skillIndicatorType = skillIndicatorType;
        configFile.rangeParam1 = rangeParam1;
        configFile.rangeParam2 = rangeParam2;

        configFile.levelRequired = levelRequired;
        configFile.castFacingDir = castFacingDir;
        configFile.isRotateWhenCast = isRotateWhenCast;
        configFile.isMoveWhenCast = isMoveWhenCast;
    }
}

public static class GUIStyles
{
    public static GUIStyle item_select = "MeTransitionSelectHead";
    public static GUIStyle item_normal = "MeTransitionSelect";
    public static GUIStyle box = "HelpBox";
}