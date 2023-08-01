using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

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
            skillIndicatorType = EditorGUILayout.Popup("����ָʾ����״:", skillIndicatorType, skillIndicatorTypesArray);
            rangeParam1 = EditorGUILayout.FloatField("��Χ����1:", rangeParam1);
            rangeParam2 = EditorGUILayout.FloatField("��Χ����2:", rangeParam2);
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
            string[] clipNamesArray = clips.Select(t => t.name).ToArray();
            animationIndex = EditorGUILayout.Popup("����Ƭ��", animationIndex, clipNamesArray);

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
    }
}

public static class GUIStyles
{
    public static GUIStyle item_select = "MeTransitionSelectHead";
    public static GUIStyle item_normal = "MeTransitionSelect";
    public static GUIStyle box = "HelpBox";
}