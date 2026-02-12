using UnityEngine;

/// <summary>
/// 任务系统诊断工具 - 检查任务系统配置是否正确
/// 使用方法：将此脚本附加到任意 GameObject，运行游戏后在 Console 查看诊断结果
/// </summary>
public class TaskSystemDiagnostic : MonoBehaviour
{
    [Header("自动诊断")]
    [SerializeField] private bool runOnStart = true;
    
    [ContextMenu("Run Diagnostic")]
    public void RunDiagnostic()
    {
        Debug.Log("========== 任务系统诊断开始 ==========");
        
        CheckTaskManager();
        CheckTaskDisplayUI();
        CheckGameModeManager();
        CheckFishSpawnManager();
        CheckConfirmButton();
        
        Debug.Log("========== 任务系统诊断完成 ==========");
    }
    
    private void Start()
    {
        if (runOnStart)
        {
            Invoke(nameof(RunDiagnostic), 1f); // 延迟1秒确保所有对象初始化
        }
    }
    
    private void CheckTaskManager()
    {
        Debug.Log("--- 检查 TaskManager ---");
        
        TaskManager taskManager = ServiceLocator.Instance.Get<TaskManager>();
        
        if (taskManager == null)
        {
            Debug.LogError("❌ TaskManager 未找到！请创建 TaskManager GameObject");
            return;
        }
        
        Debug.Log($"✅ TaskManager 存在: {taskManager.gameObject.name}");
        Debug.Log($"   - GameObject 激活: {taskManager.gameObject.activeInHierarchy}");
        Debug.Log($"   - 组件启用: {taskManager.enabled}");
    }
    
    private void CheckTaskDisplayUI()
    {
        Debug.Log("--- 检查 TaskDisplayUI ---");
        
        TaskDisplayUI taskDisplayUI = ServiceLocator.Instance.Get<TaskDisplayUI>();
        
        if (taskDisplayUI == null)
        {
            Debug.LogError("❌ TaskDisplayUI 未找到！请创建 TaskPanel 并添加 TaskDisplayUI 组件");
            return;
        }
        
        Debug.Log($"✅ TaskDisplayUI 存在: {taskDisplayUI.gameObject.name}");
        Debug.Log($"   - GameObject 激活: {taskDisplayUI.gameObject.activeInHierarchy}");
        Debug.Log($"   - 组件启用: {taskDisplayUI.enabled}");
        
        // 使用反射检查引用（因为字段是 SerializeField private）
        var taskDescriptionTextField = taskDisplayUI.GetType().GetField("taskDescriptionText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (taskDescriptionTextField != null)
        {
            var taskDescriptionText = taskDescriptionTextField.GetValue(taskDisplayUI);
            if (taskDescriptionText == null)
            {
                Debug.LogError("❌ Task Description Text 引用为空！请在 Inspector 中设置");
            }
            else
            {
                Debug.Log($"✅ Task Description Text 已设置");
            }
        }
    }
    
    private void CheckGameModeManager()
    {
        Debug.Log("--- 检查 GameModeManager ---");
        
        GameModeManager gameModeManager = ServiceLocator.Instance.Get<GameModeManager>();
        
        if (gameModeManager == null)
        {
            Debug.LogError("❌ GameModeManager 未找到！");
            return;
        }
        
        Debug.Log($"✅ GameModeManager 存在: {gameModeManager.gameObject.name}");
        Debug.Log($"   - GameObject 激活: {gameModeManager.gameObject.activeInHierarchy}");
        Debug.Log($"   - 组件启用: {gameModeManager.enabled}");
    }
    
    private void CheckFishSpawnManager()
    {
        Debug.Log("--- 检查 FishSpawnManager ---");
        
        FishSpawnManager fishSpawnManager = ServiceLocator.Instance.Get<FishSpawnManager>();
        
        if (fishSpawnManager == null)
        {
            Debug.LogError("❌ FishSpawnManager 未找到！");
            return;
        }
        
        Debug.Log($"✅ FishSpawnManager 存在: {fishSpawnManager.gameObject.name}");
        Debug.Log($"   - GameObject 激活: {fishSpawnManager.gameObject.activeInHierarchy}");
        Debug.Log($"   - 组件启用: {fishSpawnManager.enabled}");
    }
    
    private void CheckConfirmButton()
    {
        Debug.Log("--- 检查 ConfirmButton ---");
        
        ConfirmButtonHandler confirmButton = ServiceLocator.Instance.Get<ConfirmButtonHandler>();
        
        if (confirmButton == null)
        {
            Debug.LogWarning("⚠️ ConfirmButtonHandler 未找到！确认按钮可能未创建");
            return;
        }
        
        Debug.Log($"✅ ConfirmButtonHandler 存在: {confirmButton.gameObject.name}");
        Debug.Log($"   - GameObject 激活: {confirmButton.gameObject.activeInHierarchy}");
        Debug.Log($"   - 组件启用: {confirmButton.enabled}");
    }
}
