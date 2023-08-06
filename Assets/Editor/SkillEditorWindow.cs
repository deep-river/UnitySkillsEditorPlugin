using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

public class SkillEditorWindow : EditorWindow
{
    // ��������
    public int skillID; // ����ID
    public string skillName; // ��������
    public string skillDescription; // ��������
    public int targetDetectType; // Ŀ��������
    private string[] targetDetectTypesArray = new string[]
    {
        "�жԽ�ɫ",
        "�ѷ���ɫ",
        "�ѷ����Լ�",
        "ȫ���ɫ",
        "ȫ����Լ�"
    };
    public int skillTargetType; // ����Ŀ������
    private string[] skillTargetTypesArray = new string[]
    {
        "������",
        "����",
        "��������",
        "���Լ�"
    };

    private Texture skillIcon;
    public string icon; // ����ͼ��
    public int skillTypeIndex; // ��������
    private string[] skillTypesArray = new string[]
    {
        "�չ�",
        "��������",
        "��������"
    };
    public float coolDownDuration; // CDʱ��
    public float castDistance; // ʩ������
    public int skillIndicatorType; // ����ָʾ����״
    private string[] skillIndicatorTypesArray = new string[]
    {
        "Բ��",
        "����",
        "����"
    };
    public float rangeParam1; // ��Χ����1
    public float rangeParam2; // ��Χ����2
    public float offsetX, offsetY, offsetZ;

    public int resourceType; // ��Դ��������
    private string[] resourceTypesArray = new string[]
    {
        "ħ��ֵ"
    };
    public float resourceAmout; // ��Դ��������
    public int levelRequired; // ��������ȼ�
    public bool castFacingDir; // ����ʩ������
    public bool isRotateWhenCast; // ʩ��ʱ��ת��
    public bool isMoveWhenCast; // ʩ��ʱ���ƶ�

    bool m_Foldout1;
    GUIContent m_Content1 = new GUIContent("������Ϣ");
    bool m_Foldout2;
    GUIContent m_Content2 = new GUIContent("����ʩ������");
    bool m_Foldout3;
    GUIContent m_Content3 = new GUIContent("��������");

    string path = "Assets/Resources/";
    string configName = "skillConfig";
    string ext = ".asset";
    public SkillConfigSto configFile;
    public SkillConfigSto lastConfigFile; // ��¼��һ�δ򿪵������ļ�

    // ��������
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
        "����",
        "�ƶ�"
    };
    List<Global.TransitionCondition> transitionConditionsList = new List<Global.TransitionCondition>();

    List<Global.attackDetection> attackDetectionList = new List<Global.attackDetection>();
    bool isDrawAttackRange = false;
    bool isOverwriteDetection = false;



    [MenuItem("������/���ܱ༭��")]
    static void Open()
    {
        SkillEditorWindow window = (SkillEditorWindow)GetWindow(typeof(SkillEditorWindow));
        window.Show();
    }

    private void OnGUI()
    { 
        // �½������ļ�
        if (GUILayout.Button("�½�����", GUILayout.Width(200)))
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

        // ��ȡ�����ļ�
        configFile = EditorGUILayout.ObjectField("�����ļ�", configFile, typeof(ScriptableObject), true) as SkillConfigSto;
        if (configFile != lastConfigFile)
        {
            configFile = lastConfigFile;
            LoadConfig();
        }

        // ���������ļ�
        if (GUILayout.Button("��������", GUILayout.Width(200)))
        {
            SaveConfig();
        }

        // �������Ա༭
        EditorGUILayout.BeginVertical(GUI.skin.box);
        // EditorGUI.indentLevel++;
        m_Foldout1 = EditorGUILayout.Foldout(m_Foldout1, m_Content1);
        if (m_Foldout1)
        {
            skillIcon = EditorGUILayout.ObjectField("����ͼ��", skillIcon, typeof(Texture), true) as Texture;
            skillID = EditorGUILayout.IntField("����ID:", skillID);
            skillName = EditorGUILayout.TextField("��ʾ����:", skillName);
            EditorGUILayout.LabelField("��������");
            skillDescription = EditorGUILayout.TextArea(skillDescription, GUILayout.Height(35));
            skillTypeIndex = EditorGUILayout.Popup("��������:", skillTypeIndex, skillTypesArray);
            skillTargetType = EditorGUILayout.Popup("����Ŀ������:", skillTargetType, skillTargetTypesArray);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        m_Foldout2 = EditorGUILayout.Foldout(m_Foldout2, m_Content2);
        if (m_Foldout2)
        {
            targetDetectType = EditorGUILayout.Popup("����Ŀ��������:", targetDetectType, targetDetectTypesArray);
            resourceType = EditorGUILayout.Popup("��Դ��������:", resourceType, resourceTypesArray);
            resourceAmout = EditorGUILayout.FloatField("��Դ��������:", resourceAmout);
            coolDownDuration = EditorGUILayout.FloatField("CDʱ��:", coolDownDuration);
            castDistance = EditorGUILayout.FloatField("ʩ������:", castDistance);
            skillIndicatorType = EditorGUILayout.Popup("�����ж���Χ��״:", skillIndicatorType, skillIndicatorTypesArray);
            rangeParam1 = EditorGUILayout.FloatField("��Χ����1:", rangeParam1);
            rangeParam2 = EditorGUILayout.FloatField("��Χ����2:", rangeParam2);
            offsetX = EditorGUILayout.FloatField("offsetX:", offsetX);
            offsetY = EditorGUILayout.FloatField("offsetY:", offsetY);
            offsetZ = EditorGUILayout.FloatField("offsetZ:", offsetZ);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        m_Foldout3 = EditorGUILayout.Foldout(m_Foldout3, m_Content3);
        if (m_Foldout3)
        {
            levelRequired = EditorGUILayout.IntField("��������ȼ�:", levelRequired);
            castFacingDir = EditorGUILayout.Toggle("����ʩ������", castFacingDir);
            isRotateWhenCast = EditorGUILayout.Toggle("ʩ��ʱ��ת��", isRotateWhenCast);
            isMoveWhenCast = EditorGUILayout.Toggle("ʩ��ʱ���ƶ�", isMoveWhenCast);
        }
        EditorGUILayout.EndVertical();

        // �����༭
        model = EditorGUILayout.ObjectField("��ӽ�ɫ", model, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("Ӧ�ý�ɫ", GUILayout.Width(200)))
        {
            if (model == null)
            {
                Debug.LogError("δѡ���κ�ģ��");
                return;
            }
            animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("ģ��δ��Animator���");
                return;
            }
        }

        if (animator != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            animationIndex = Mathf.Clamp(animationIndex, 0, clips.Length);

            EditorGUILayout.BeginHorizontal();

            string[] clipNamesArray = clips.Select(t => t.name).ToArray();
            animationIndex = EditorGUILayout.Popup("����Ƭ��", animationIndex, clipNamesArray);

            if (GUILayout.Button("��Ӷ���", GUILayout.Width(80)))
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
            if (GUILayout.Button("��������", GUILayout.Width(80)))
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

            EditorGUILayout.LabelField("ʹ�ö���Ƭ��", selectedAnimationName, GUIStyles.textField);

            if (GUILayout.Button("�����ת����", GUILayout.Width(200)))
            {
                Global.TransitionCondition transitionCondition= new Global.TransitionCondition();
                transitionConditionsList.Add(transitionCondition);
            }

            for (int i = 0; i < transitionConditionsList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("ɾ��", GUILayout.Width(40)))
                {
                    transitionConditionsList.RemoveAt(i);
                }

                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("��תĿ�궯��"));
                transitionConditionsList[i].targetAnimation = EditorGUILayout.Popup("��תĿ�궯��", transitionConditionsList[i].targetAnimation, clipNamesArray, GUILayout.MaxWidth(150));
                AnimationClip clip_ = clips[transitionConditionsList[i].targetAnimation];
                int animFrames = (int)(clip_.length * clip_.frameRate);
                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("  ��ʼ֡"));
                transitionConditionsList[i].beginAtFrame = EditorGUILayout.IntField("  ��ʼ֡", transitionConditionsList[i].beginAtFrame, GUILayout.MaxWidth(150));
                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("  ����֡"));
                transitionConditionsList[i].endAtFrame = EditorGUILayout.IntField("  ����֡", transitionConditionsList[i].endAtFrame, GUILayout.MaxWidth(150));

                transitionConditionsList[i].beginAtFrame = Mathf.Clamp(transitionConditionsList[i].beginAtFrame, 0, animFrames - 1);
                transitionConditionsList[i].endAtFrame = Mathf.Clamp(transitionConditionsList[i].endAtFrame, 0, animFrames - 1);
                if (transitionConditionsList[i].endAtFrame < transitionConditionsList[i].beginAtFrame)
                {
                    transitionConditionsList[i].endAtFrame = transitionConditionsList[i].beginAtFrame;
                }
                if (GUILayout.Button("�������", GUILayout.Width(60)))
                {
                    transitionConditionsList[i].executions.Add(0);
                }
                for (int j = 0; j < transitionConditionsList[i].executions.Count; j++)
                {
                    transitionConditionsList[i].executions[j] = EditorGUILayout.Popup("ѡ�����", transitionConditionsList[i].executions[j], executionsArray, GUILayout.MaxWidth(150));
                }

                EditorGUILayout.EndHorizontal();
            }

            // �����ж�
            if (GUILayout.Button("��ӹ����ж���", GUILayout.Width(200)))
            {
                Global.attackDetection attackDetection = new Global.attackDetection();
                attackDetection.frameIndex = frameIndex;
                attackDetectionList.Add(attackDetection);
            }
            for (int i = 0; i < attackDetectionList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("ɾ��", GUILayout.Width(40)))
                {
                    attackDetectionList.RemoveAt(i);
                }
                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("֡��"));
                attackDetectionList[i].frameIndex = EditorGUILayout.IntField("֡��", attackDetectionList[i].frameIndex, GUILayout.MaxWidth(70));

                EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("�Ƿ���д��Χ"));
                attackDetectionList[i].isOverwrite = EditorGUILayout.Toggle("�Ƿ���д��Χ", attackDetectionList[i].isOverwrite, GUILayout.MaxWidth(100));

                if (attackDetectionList[i].isOverwrite)
                {
                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("�ж���״"));
                    attackDetectionList[i].rangeShape = EditorGUILayout.Popup("�ж���״", attackDetectionList[i].rangeShape, skillIndicatorTypesArray, GUILayout.MaxWidth(70));

                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("����1"));
                    attackDetectionList[i].param1 = EditorGUILayout.FloatField("����1", attackDetectionList[i].param1, GUILayout.MaxWidth(70));

                    EditorGUIUtility.labelWidth = calcLabelWidth(new GUIContent("����2"));
                    attackDetectionList[i].param2 = EditorGUILayout.FloatField("����2", attackDetectionList[i].param2, GUILayout.MaxWidth(70));

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
            // �϶�slider��Scene����Ԥ������֡
            // clip.SampleAnimation(animator.gameObject, frameTimeFloat);
            // frameTimeFloat = EditorGUILayout.Slider(frameTimeFloat, 0, clip.length);
            clip.SampleAnimation(animator.gameObject, currentFrame);
            frameIndex = EditorGUILayout.IntSlider(frameIndex, 0, (int)(clip.length * clip.frameRate - 1));

            EditorGUILayout.LabelField("����ʱ����" + clip.length);
            DrawFramesView(clip);
        }
    }

    void DrawFramesView(AnimationClip clip)
    {
        int frameCount = (int)(clip.length * clip.frameRate); // ��������֡��
        float frameViewWidth = 40;
        float frameInfoSectionWidth = 600;
        float frameInfoSectionHeight = 60;

        scrollView = EditorGUILayout.BeginScrollView(scrollView, true, true, GUILayout.Width(frameInfoSectionWidth), GUILayout.Height(frameInfoSectionHeight));
        EditorGUILayout.BeginHorizontal();

        // ����֡������ͼ
        for (int i = 0; i < frameCount; i++)
        {
            bool selected = frameIndex == i;
            // string title = "" + i;
            int id = GetFrameIndexInAttackDetectionList(i);
            string title = string.Format("{0}\n{1}", i, IsFrameHasAttackDetection(id)? "��" : "");

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

    // ���뼼������
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
                Debug.LogError("ģ��δ��Animator���");
                return;
            }
        }
    }

    // ���漼������
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

    // ��������������ʾ���
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