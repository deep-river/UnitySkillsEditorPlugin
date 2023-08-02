using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global 
{
    // combo transition condition 技能跳转条件
    public class TransitionCondition
    {
        public int targetAnimation;
        public int beginAtFrame;
        public int endAtFrame;
        public List<int> executions = new List<int>();
    }
}
