using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCSchedule
{
    public int hour;
    public List<Transform> waypoints;   
    public string actionAnim = "";
   
}