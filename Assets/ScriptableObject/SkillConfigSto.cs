using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillConfigSto : ScriptableObject
{
    public int skillID; // 技能ID
    public string skillName; // 技能名称
    public string skillDescription; // 技能描述
    public int targetDetectType; // 目标检测类型
    public int skillTargetType; // 技能目标类型
    public string icon; // 技能图标
    public int skillType; // 技能类型
    public float coolDownDuration; // CD时间
    public float castDistance; // 施法距离
    public int skillIndicatorType; // 技能指示器形状
    public float rangeParam1; // 范围参数1
    public float rangeParam2; // 范围参数2
    public int resourceType; // 资源消耗类型
    public float resourceAmout; // 资源消耗数量
    public int levelRequired; // 升级所需等级
    public bool castFacingDir; // 面向施法方向
    public bool isRotateWhenCast; // 施法时可转向
    public bool isMoveWhenCast; // 施法时可移动

    public string selectedAnimationName;
    public List<Global.TransitionCondition> transitionConditionsList = new List<Global.TransitionCondition>();

    public string modelName;
}
