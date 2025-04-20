using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    protected RangedWarrior _owner;
    protected string _nodeName;
    public bool IsSuccessful;
    
    public Node(RangedWarrior owner, string name)
    {
        _owner = owner;
        _nodeName = name;
    }
    
    public abstract bool Evaluate();
    
    public string GetNodeName()
    {
        return _nodeName;
    }
    
    public virtual List<string> GetDebugPath()
    {
        return new List<string> { _nodeName };
    }
}
