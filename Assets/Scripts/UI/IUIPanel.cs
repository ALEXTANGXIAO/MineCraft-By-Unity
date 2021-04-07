using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SlotData
{
    public int slotType;
    public int slotIndex;
}

public interface IUIPanel
{
    bool IsOpen { get; }
    void HidePanel();
    void ShowPanel();
}
