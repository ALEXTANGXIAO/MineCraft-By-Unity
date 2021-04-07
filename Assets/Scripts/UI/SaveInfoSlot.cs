using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SaveInfoSlot : MonoBehaviour
{
    public TMP_Text saveNameText;
    public string saveDir = "Default";
    public ArchiveData bindArchive;
    public GameObject maskImage;
}
