using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Sequence : Node
{
    protected List<Node> _children = new List<Node>();
    
    public Sequence(RangedWarrior owner, string nodeName) : base(owner, nodeName)
    {

    }
    
    public void AddChild(Node child)
    {
        _children.Add(child);
    }
    
    // 序列会评估子节点直到一个返回失败，如果所有子节点都成功，则返回成功
    public override bool Evaluate()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i].Evaluate())
            {
                _children[i].IsSuccessful = true;
            }
            else
            {
                _children[i].IsSuccessful = false;
                return false;
            }
        }
        return true;
    }
    
    public override List<string> GetDebugPath()
    {
        List<string> path = new List<string> { _nodeName };
        foreach (Node child in _children)
        {
            if (child.IsSuccessful)
            {
                path.AddRange(child.GetDebugPath());
            }
        }
        return path;
    }
}
