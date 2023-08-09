using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System;

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

    attackDetection.IDetectionRange IDetectionRange = null;
    DetectionInfo detectionInfo = new DetectionInfo();
    BoxBoundsHandle boxHandle = new BoxBoundsHandle();
    SphereBoundsHandle sphereHandle = new SphereBoundsHandle();
    attackDetection.BoxItem boxItem = new attackDetection.BoxItem();
    attackDetection.SphereItem sphereItem = new attackDetection.SphereItem();



    [MenuItem("������/���ܱ༭��")]
    static void Open()
    {
        SkillEditorWindow window = (SkillEditorWindow)GetWindow(typeof(SkillEditorWindow));
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        OnSceneGUI();
        sceneView.Repaint();
        Repaint();
    }

    void OnSceneGUI()
    {
        if (isDrawAttackRange)
        {
            DrawAttackRange();
        }
    }

    void DrawAttackRange()
    {
        if (!isOverwriteDetection)
        {
            switch (skillIndicatorType)
            {
                case 1:
                    IDetectionRange = boxItem;
                    IDetectionRange.SetValue(rangeParam1, 1, rangeParam2, offsetX, offsetY, offsetZ);
                    break;
                case 0:
                    IDetectionRange = sphereItem;
                    IDetectionRange.SetValue(rangeParam1, 0, 0, offsetX, offsetY, offsetZ);
                    break;
            }
        }
        else
        {
            int index = GetFrameIndexInAttackDetectionList(frameIndex);
            if (index >= 0)
            {
                switch(attackDetectionList[index].rangeShape)
                {
                    case 1:
                        IDetectionRange = boxItem;
                        IDetectionRange.SetValue(attackDetectionList[index].param1, 1, attackDetectionList[index].param2, attackDetectionList[index].offset.x, attackDetectionList[index].offset.y, attackDetectionList[index].offset.z);
                        break;
                    case 0:
                        IDetectionRange = sphereItem;
                        IDetectionRange.SetValue(attackDetectionList[index].param1, 0, 0, attackDetectionList[index].offset.x, attackDetectionList[index].offset.y, attackDetectionList[index].offset.z);
                        break;
                }
            }
        }
        detectionInfo.value = IDetectionRange;
        Matrix4x4 localToWorld = model.transform.localToWorldMatrix;
        DrawDetectionRange(detectionInfo, localToWorld, new Color(1, 0, 0, 0.25f));
    }

    void DrawDetectionRange(DetectionInfo detectionInfo, Matrix4x4 localToWorld, Color color)
    {
        Matrix4x4 temp = Matrix4x4.TRS(localToWorld.MultiplyPoint3x4(Vector3.zero), localToWorld.rotation, Vector3.one);
        Handles.matrix = temp;
        DrawRange(detectionInfo, color);
        DrawHandler(detectionInfo.value);
    }

    void DrawRange(DetectionInfo config, Color color)
    {
        DrawHandles.H.PushColor(color);
        DrawHandles.H.isFillVolume = true;
        switch (config.value)
        {
            case attackDetection.BoxItem v:
                DrawHandles.H.DrawBox(v.size, Matrix4x4.Translate(v.offset));
                break;
            case attackDetection.SphereItem v:
                DrawHandles.H.DrawSphere(v.radius, Matrix4x4.Translate(v.offset));
                break;
        }
        DrawHandles.H.isFillVolume = false;
        DrawHandles.H.PopColor();
    }

    void DrawHandler(attackDetection.IDetectionRange config)
    {
        Vector3 offset = Vector3.zero;
        Vector3 size = Vector3.one;

        switch (config)
        {
            case attackDetection.BoxItem v:
                offset = v.offset;
                size = v.size;
                break;
            case attackDetection.SphereItem v:
                offset = v.offset;
                size = new Vector2(v.radius, 0);
                break;
        }
        float handlerSize = HandleUtility.GetHandleSize(offset);
        switch (Tools.current)
        {
            case Tool.View:
                break;
            case Tool.Move:
                offset = Handles.DoPositionHandle(offset, Quaternion.identity);
                break;
            case Tool.Scale:
                size = Handles.DoScaleHandle(size, offset, Quaternion.identity, handlerSize);
                break;
            case Tool.Transform:
                Vector3 _offset = size;
                Vector3 _size = size;
                Handles.TransformHandle(ref _offset, Quaternion.identity, ref _size);
                offset = _offset; 
                size = _size; 
                break;
            case Tool.Rect:
                switch (config)
                {
                    case attackDetection.BoxItem v:
                        boxHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y | PrimitiveBoundsHandle.Axes.Z;
                        boxHandle.center = offset;
                        boxHandle.size = size;
                        boxHandle.DrawHandle();
                        offset = boxHandle.center;
                        size = boxHandle.size;
                        break;
                    case attackDetection.SphereItem v:
                        sphereHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y | PrimitiveBoundsHandle.Axes.Z;
                        sphereHandle.center = offset;
                        sphereHandle.radius = size.x;
                        sphereHandle.DrawHandle();
                        offset = sphereHandle.center;
                        size.x = sphereHandle.radius;
                        break;
                }
                break;
        }
        Func<Vector3> getOffset = () => new Vector3(offset.x, offset.y, offset.z);
        Func<Vector3> getSize = () => new Vector3(size.x, size.y, size.z);
        Func<float> getRadius = () => size.magnitude;
        switch (config)
        {
            case attackDetection.BoxItem v:
                v.offset = getOffset();
                v.size = getSize();
                if (!isOverwriteDetection)
                {
                    offsetX = v.offset.x;
                    offsetY = v.offset.y;
                    offsetZ = v.offset.z;
                    rangeParam1 = v.size.x;
                    rangeParam2 = v.size.z;
                }
                else
                {
                    int index = GetFrameIndexInAttackDetectionList(frameIndex);
                    if (index > 0)
                    {
                        attackDetectionList[index].offset.x = v.offset.x;
                        attackDetectionList[index].offset.y = v.offset.y;
                        attackDetectionList[index].offset.z = v.offset.z;
                        attackDetectionList[index].param1 = v.size.x;
                        attackDetectionList[index].param2 = v.size.z; 
                    }
                }
                break;
            case attackDetection.SphereItem v: 
                v.offset = getOffset();
                v.radius = getRadius();
                if (!isOverwriteDetection)
                {
                    offsetX = v.offset.x;
                    offsetY = v.offset.y;
                    offsetZ = v.offset.z;
                    rangeParam1 = v.radius;
                }
                else
                {
                    int index = GetFrameIndexInAttackDetectionList(frameIndex);
                    if (index > 0)
                    {
                        attackDetectionList[index].offset.x = v.offset.x;
                        attackDetectionList[index].offset.y = v.offset.y;
                        attackDetectionList[index].offset.z = v.offset.z;
                        attackDetectionList[index].param1 = v.radius;
                    }
                }
                break;
        }
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

public class DetectionInfo
{
    public attackDetection.IDetectionRange value;
}

namespace attackDetection
{
    public interface IDetectionRange
    {
        public void SetValue(float a1, float a2, float a3, float x, float y, float z);
    }

    public class BoxItem : IDetectionRange
    {
        public Vector3 offset = Vector3.zero;
        public Vector3 size = Vector3.one;

        public void SetValue(float a1, float a2, float a3, float x, float y, float z)
        {
            this.size.x = a1;
            this.size.y = a2;
            this.size.z = a3;
            this.offset.x = x;
            this.offset.y = y;
            this.offset.z = z;
        }
    }

    public class SphereItem : IDetectionRange
    {
        public Vector3 offset = Vector3.zero;
        public float radius = 1;

        public void SetValue(float a1, float a2, float a3, float x, float y, float z)
        {
            this.radius = a1;
            this.offset.x = x;
            this.offset.y = y;
            this.offset.z = z;
        }
    }
}

public class DrawHandles : DrawTool
{
    public static DrawHandles H = new DrawHandles();

    public override Color color { get => Handles.color; set => Handles.color = value; }

    public override void DrawLine(Vector3 start, Vector3 end)
    {
        Handles.DrawLine(start, end);
    }

    protected override void FillPolygon(Vector3[] vertices)
    {
        Handles.DrawAAConvexPolygon(vertices);
    }
}

public abstract class DrawTool
{
    public static Color defaultColor = Color.white;
    public abstract void DrawLine(Vector3 start, Vector3 end);
    public virtual Color color { get; set; }
    public Color outlineColor => new Color(1, 1, 1, color.a);
    public int sphereCutPrecision = 30; // �����и��
    public bool isFillVolume = false; // �Ƿ����������
    public bool isDrawOutline = false; // �Ƿ�����߿�
    Stack<Color> _colorStack = new Stack<Color>();

    public void PushColor(Color color)
    {
        _colorStack.Push(this.color);
        this.color = color;
    }

    public void PopColor()
    {
        this.color = _colorStack.Count > 0 ? _colorStack.Pop() : defaultColor;
    }

    public void DrawPolygon(Vector3[] vertices)
    {
        if (isFillVolume)
        {
            FillPolygon(vertices);
            if (isDrawOutline)
            {
                PushColor(outlineColor);
                for (int i = vertices.Length - 1, j = 0; j < vertices.Length; i = j, j++)
                {
                    DrawLine(vertices[i], vertices[j]);
                }
                PopColor();
            }
        }
        else
        {
            for (int i = vertices.Length - 1, j = 0; j < vertices.Length; i = j, j++)
            {
                DrawLine(vertices[i], vertices[j]);
            }
        }
    }

    protected virtual void FillPolygon(Vector3[] vertices)
    {
        for (int i = vertices.Length - 1, j = 0; j < vertices.Length; i = j, j++)
        {
            DrawLine(vertices[i], vertices[j]);
        }
    }

    public void DrawBox(Vector3 size, Matrix4x4 matrix)
    {
        Vector3[] points = MathUtility.CalcBoxVertex(size, matrix);
        int[] indexes = MathUtility.GetBoxSurfaceVertices();
        for (int i = 0; i < 6; i++)
        {
            Vector3[] polygon = new Vector3[]
            {
                points[indexes[i * 4]],
                points[indexes[i * 4 + 1]],
                points[indexes[i * 4 + 2]],
                points[indexes[i * 4 + 3]],
            };
            DrawPolygon(polygon);
        }
    }

    public void DrawSphere(float radius, Matrix4x4 matrix)
    {
        Matrix4x4 lookMatrix = Matrix4x4.identity;
        SceneView sceneView = SceneView.currentDrawingSceneView;
        if (sceneView != null)
        {
            Camera cam = sceneView.camera;
            var cameraTransform = cam.transform;
            var rotation = Quaternion.LookRotation(cameraTransform.position - matrix.MultiplyPoint(Vector3.zero));
            lookMatrix = Matrix4x4.TRS(matrix.MultiplyPoint(Vector3.zero), rotation, matrix.lossyScale);
            DrawCircle(radius, lookMatrix);
        }
        bool prevColorFill = isFillVolume;
        isFillVolume = false;
        PushColor(outlineColor);
        DrawCircle(radius, matrix);
        DrawCircle(radius, matrix * Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0)));
        DrawCircle(radius, matrix * Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0)));
        PopColor();
        isFillVolume = prevColorFill;
    }

    public void DrawCircle(float radius, Matrix4x4 lookMatrix)
    {
        Vector3[] vertices = MathUtility.CalcCircleVertex(radius, lookMatrix, sphereCutPrecision);
        DrawPolygon(vertices);
    }
}

public static class MathUtility
{
    public const float PI = Mathf.PI;
    public static int[] GetBoxSurfaceVertices()
    {
        return new int[]
        {
            0, 1, 2, 3, // ����
            4, 5, 6, 7, // ����
            2, 6, 5, 3, // ����
            0, 4, 7, 1, // ����
            1, 7, 6, 2, // ǰ��
            0, 3, 5, 4 // ����
        };
    }

    // ���㳤�����8������
    public static Vector3[] CalcBoxVertex(Vector3 size)
    {
        Vector3 halfSize = size / 2f;
        Vector3[] points = new Vector3[8];
        points[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
        points[1] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        points[2] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        points[3] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);

        points[4] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        points[5] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        points[6] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        points[7] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);

        return points;
    }

    public static Vector3[] CalcBoxVertex(Vector3 size, Matrix4x4 matrix)
    {
        Vector3[] points = CalcBoxVertex(size);
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = matrix.MultiplyPoint(points[i]);
        }
        return points;
    }

    public static Vector3[] CalcCircleVertex(float radius, Matrix4x4 matrix, int sphereCutPrecision = 30)
    {
        float deg = 2 * Mathf.PI;
        float deltaDeg = deg / sphereCutPrecision;
        Vector3[] vertices = new Vector3[sphereCutPrecision];
        for (int i = 0; i < sphereCutPrecision; i++)
        {
            Vector2 pos;
            float d = deg - deltaDeg * i;
            pos.x = radius * Mathf.Cos(d);
            pos.y = radius * Mathf.Sin(d);
            vertices[i] = matrix.MultiplyPoint(pos);
        }
        return vertices;
    }
}