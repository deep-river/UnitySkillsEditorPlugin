using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillConfigSto : ScriptableObject
{
    public int skillID; // ����ID
    public string skillName; // ��������
    public string skillDescription; // ��������
    public int targetDetectType; // Ŀ��������
    public int skillTargetType; // ����Ŀ������
    public string icon; // ����ͼ��
    public int skillType; // ��������
    public float coolDownDuration; // CDʱ��
    public float castDistance; // ʩ������
    public int skillIndicatorType; // ����ָʾ����״
    public float rangeParam1; // ��Χ����1
    public float rangeParam2; // ��Χ����2
    public int resourceType; // ��Դ��������
    public float resourceAmout; // ��Դ��������
    public int levelRequired; // ��������ȼ�
    public bool castFacingDir; // ����ʩ������
    public bool isRotateWhenCast; // ʩ��ʱ��ת��
    public bool isMoveWhenCast; // ʩ��ʱ���ƶ�

    public string selectedAnimationName;
    public List<Global.TransitionCondition> transitionConditionsList = new List<Global.TransitionCondition>();

    public string modelName;
}
