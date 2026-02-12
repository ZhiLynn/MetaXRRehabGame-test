using UnityEngine;

/// <summary>
/// 鱼的数据组件 - 附加到每条鱼GameObject上，用于任务系统识别鱼的颜色
/// </summary>
public class FishData : MonoBehaviour
{
    public string prefabName; // "redFish", "blueFish", etc.
    
    /// <summary>
    /// 设置鱼的prefab名称
    /// </summary>
    public void SetPrefabName(string name)
    {
        prefabName = name;
    }
}
