using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

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
    public float offsetX, offsetY, offsetZ;

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

    string selectedAnimationName;
    string[] executionsArray = new string[]
    {
        "攻击",
        "移动"
    };
    List<Global.TransitionCondition> transitionConditionsList = new List<Global.TransitionCondition>();

    List<Global.attackDetection> attackDetectionList = new List<Global.attackDetection>();
    bool isDrawAttackRange = false;
    bool isOverwriteDetection = false;



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
            skillIndicatorType = EditorGUILayout.Popup("攻击判定范围形状:", skillIndicatorType, skillIndicatorTypesArray);
            rangeParam1 = EditorGUILayout.FloatField("范围参数1:", rangeParam1);
            rangeParam2 = EditorGUILayout.FloatField("范围参数2:", rangeParam2);
            offsetX = EditorGUILayout.FloatField("offsetX:", offsetX);
            offsetY = EditorGUILayout.FloatField("offsetY:", offsetY);
            offsetZ = EditorGUILayout.FloatField("offsetZ:", offsetZ);
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

            EditorGUILayout.BeginHorizontal();

            string[] clipNamesArray = clips.Select(t => t.name).ToArray();
            animationIndex = EditorGUILayout.Popup("动画片段", animationIndex, clipNamesArray);

            if (GUILayout.Button("添加动画", GUILayout.Width(80)))
            {
                if (selectedAnimationName == "" || selectedAnimationName == null)
                {
                    selectedAnimationName = clipNamesArray[animationIndex];
                }
                else
                {
                    selectedAnimationName += "|" + clipNamesArray[animationIndex];
                }
            }
            if (GUILayout.Button("撤销动画", GUILayout.Width(80)))
            {
                if (selectedAnimationName != null && selectedAnimationName.Length > 0)
                {
                    int pos = selectedAnimationName.LastIndexOf("|");
                    if (pos > -1)
                    {
                        selectedAnimationName = selectedAnimationName.Substring(0, pos);
                    }
                    else
                    {
                        selectedAnimationName = "";
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("使用动画片段", selectedAnimationName, GUIStyles.textField);

            if (GUILayout.Button("添加跳转条件", GUILayout.Width(200)))
            {
                Global.TransitionCondition transitionCondition= new Global.TransitionCondition();
                transitionConditionsList.Add(transitionCondition);
            }

            for (int i = 0; i < transitionConditionsList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("删除", GUILayout.Width(40)))
                {
                    transitionConditionsList.RemoveAt(i);
                }

                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("跳转目标动画"));
                transitionConditionsList[i].targetAnimation = EditorGUILayout.Popup("跳转目标动画", transitionConditionsList[i].targetAnimation, clipNamesArray, GUILayout.MaxWidth(150));
                AnimationClip clip_ = clips[transitionConditionsList[i].targetAnimation];
                int animFrames = (int)(clip_.length * clip_.frameRate);
                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("  开始帧"));
                transitionConditionsList[i].beginAtFrame = EditorGUILayout.IntField("  开始帧", transitionConditionsList[i].beginAtFrame, GUILayout.MaxWidth(150));
                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("  结束帧"));
                transitionConditionsList[i].endAtFrame = EditorGUILayout.IntField("  结束帧", transitionConditionsList[i].endAtFrame, GUILayout.MaxWidth(150));

                transitionConditionsList[i].beginAtFrame = Mathf.Clamp(transitionConditionsList[i].beginAtFrame, 0, animFrames - 1);
                transitionConditionsList[i].endAtFrame = Mathf.Clamp(transitionConditionsList[i].endAtFrame, 0, animFrames - 1);
                if (transitionConditionsList[i].endAtFrame < transitionConditionsList[i].beginAtFrame)
                {
                    transitionConditionsList[i].endAtFrame = transitionConditionsList[i].beginAtFrame;
                }
                if (GUILayout.Button("添加输入", GUILayout.Width(60)))
                {
                    transitionConditionsList[i].executions.Add(0);
                }
                for (int j = 0; j < transitionConditionsList[i].executions.Count; j++)
                {
                    transitionConditionsList[i].executions[j] = EditorGUILayout.Popup("选择操作", transitionConditionsList[i].executions[j], executionsArray, GUILayout.MaxWidth(150));
                }

                EditorGUILayout.EndHorizontal();
            }

            // 攻击判定
            if (GUILayout.Button("添加攻击判定框", GUILayout.Width(200)))
            {
                Global.attackDetection attackDetection = new Global.attackDetection();
                attackDetection.frameIndex = frameIndex;
                attackDetectionList.Add(attackDetection);
            }
            for (int i = 0; i < attackDetectionList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("删除", GUILayout.Width(40)))
                {
                    attackDetectionList.RemoveAt(i);
                }
                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("帧号"));
                attackDetectionList[i].frameIndex = EditorGUILayout.IntField("帧号", attackDetectionList[i].frameIndex, GUILayout.MaxWidth(70));

                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("是否重写范围"));
                attackDetectionList[i].isOverwrite = EditorGUILayout.Toggle("是否重写范围", attackDetectionList[i].isOverwrite, GUILayout.MaxWidth(100));

                if (attackDetectionList[i].isOverwrite)
                {
                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("判定形状"));
                    attackDetectionList[i].rangeShape = EditorGUILayout.Popup("判定形状", attackDetectionList[i].rangeShape, skillIndicatorTypesArray, GUILayout.MaxWidth(70));

                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("参数1"));
                    attackDetectionList[i].param1 = EditorGUILayout.FloatField("参数1", attackDetectionList[i].param1, GUILayout.MaxWidth(70));

                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("参数2"));
                    attackDetectionList[i].param2 = EditorGUILayout.FloatField("参数2", attackDetectionList[i].param2, GUILayout.MaxWidth(70));

                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("offset:"));
                    EditorGUILayout.LabelField("offset:", GUILayout.Width(45));

                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("X"));
                    attackDetectionList[i].offset.x = EditorGUILayout.FloatField("X", attackDetectionList[i].offset.x, GUILayout.MaxWidth(45));
                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("Y"));
                    attackDetectionList[i].offset.y = EditorGUILayout.FloatField("Y", attackDetectionList[i].offset.y, GUILayout.MaxWidth(45));
                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("Z"));
                    attackDetectionList[i].offset.z = EditorGUILayout.FloatField("Z", attackDetectionList[i].offset.z, GUILayout.MaxWidth(45));
                }
                EditorGUILayout.EndHorizontal();
            }



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
            // string title = "" + i;
            int id = GetFrameIndexInAttackDetectionList(i);
            string title = string.Format("{0}\n{1}", i, IsFrameHasAttackDetection(id)? "□" : "");

            if (GUILayout.Button(title, selected?GUIStyles.item_select:GUIStyles.item_normal, GUILayout.Width(frameViewWidth)))
            {
                frameIndex = selected ? -1 : i;
            }
            currentFrame = frameIndex / clip.frameRate;
        }
        int index = GetFrameIndexInAttackDetectionList(frameIndex);
        if (IsFrameHasAttackDetection(index))
        {
            isDrawAttackRange = true;
        }
        else
        {
            isDrawAttackRange = false;
        }

        if (IsFrameOverwriteAttackDetection(index))
        {
            isOverwriteDetection = true;
        }
        else
        {
            isOverwriteDetection = false;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    int GetFrameIndexInAttackDetectionList(int num)
    {
        int id = 0;
        foreach (var t in attackDetectionList)
        {
            if (t.frameIndex == num)
            {
                return id;
            }
            ++id;
        }
        return -1;
    }

    bool IsFrameHasAttackDetection(int num)
    {
        if (attackDetectionList.Count - 1 >= num && num >= 0)
        {
            return true;
        }
        return false;
    }

    bool IsFrameOverwriteAttackDetection(int num)
    {
        if (IsFrameHasAttackDetection(num))
        {
            return attackDetectionList[num].isOverwrite;
        }
        return false;
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

        selectedAnimationName = configFile.selectedAnimationName;
        transitionConditionsList = configFile.transitionConditionsList;
        GameObject model_ = GameObject.Find(configFile.modelName);
        if (model_ == null)
        {
            // search in Resource folder
        }
        else
        {
            model = model_;
            animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("模型未绑定Animator组件");
                return;
            }
        }
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

        configFile.selectedAnimationName = selectedAnimationName;
        configFile.transitionConditionsList = transitionConditionsList;
        configFile.modelName = model.name;
    }

    // 计算插件子区域显示宽度
    public static float calcLabelWidth(GUIContent label)
    {
        return GUI.skin.label.CalcSize(label).x + EditorGUI.indentLevel * GUI.skin.label.fontSize * 2;
    }
}

public static class GUIStyles
{
    public static GUIStyle item_select = "MeTransitionSelectHead";
    public static GUIStyle item_normal = "MeTransitionSelect";
    public static GUIStyle box = "HelpBox";
    public static GUIStyle textField = "TextField";
}