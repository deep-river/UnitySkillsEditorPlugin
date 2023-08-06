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

    public class attackDetection
    {
        public int frameIndex;
        public bool isOverwrite;
        public int rangeShape;
        public float param1;
        public float param2;
        public Vector3 offset;
    }
}
