using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
    protected List<Node> _children = new List<Node>();
    
    public Selector(RangedWarrior owner, string nodeName) : base(owner, nodeName)
    {

    }
    
    public void AddChild(Node child)
    {
        _children.Add(child);
    }
    
    // 选择器会评估子节点直到一个返回成功，如果所有子节点都失败，则返回失败
    public override bool Evaluate()
    {
        foreach (Node child in _children)
        {
            if (child.Evaluate())
            {
                child.IsSuccessful = true;
                return true; 
            }
            else
            {
                child.IsSuccessful = false;
            }
        }
        return false;
    }
    
    public override List<string> GetDebugPath()
    {
        List<string> path = new List<string> { _nodeName };
        foreach (Node child in _children)
        {
            if (child.IsSuccessful)
            {
                path.AddRange(child.GetDebugPath());
                break;
            }
        }
        return path;
    }
}
