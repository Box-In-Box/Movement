using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Istate
{
    public void OperateEnter();
    public void OperateUpdate();
    public void OperateExit();
    public void OperateFixedUpdate();
}