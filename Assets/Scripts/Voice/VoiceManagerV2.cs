using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 語音管理器 V2 - 使用 ScriptableObject 資料庫
/// - 簡單模式：直接播放完整語音
/// - 中級模式：直接播放完整語音
/// - 高級模式：使用中級語音串接播放
/// </summary>
public class VoiceManagerV2 : MonoBehaviour
{
    [Header("音源組件")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("語音資料庫")]
    [SerializeField] private SimpleVoiceDatabase simpleVoiceDB;
    [SerializeField] private IntermediateVoiceDatabase intermediateVoiceDB;
    
    [Header("播放設定")]
    [SerializeField] private float delayBetweenClips = 0.15f;    // 高級模式階段之間的延遲
    [SerializeField] private bool enableVoice = true;
    
    // 管理器引用
    private TaskManager taskManager;
    private MultiBucketManager multiBucketManager;
    private HardModeManager hardModeManager;
    
    // 當前播放隊列
    private Queue<AudioClip> playbackQueue = new Queue<AudioClip>();
    private bool isPlaying = false;
    
    private void Awake()
    {
        // 自動添加 AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 配置 AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // 驗證資料庫
        ValidateDatabases();
    }
    
    private void Start()
    {
        // 訂閱事件
        if (ServiceLocator.Instance.TryGet(out taskManager))
        {
            taskManager.OnTaskGenerated.AddListener(OnTaskGenerated);
            Debug.Log("[VoiceManagerV2] 已訂閱 TaskManager 事件");
        }
        
        if (ServiceLocator.Instance.TryGet(out multiBucketManager))
        {
            multiBucketManager.OnBucketStageCompleted.AddListener(OnStageCompleted);
            Debug.Log("[VoiceManagerV2] 已訂閱 MultiBucketManager 事件");
        }
        
        hardModeManager = HardModeManager.Instance;
        if (hardModeManager != null)
        {
            hardModeManager.OnTaskGenerated.AddListener(OnHardModeTaskGenerated);
            Debug.Log("[VoiceManagerV2] 已訂閱 HardModeManager 事件");
        }
    }
    
    private void OnDestroy()
    {
        // 取消訂閱
        if (taskManager != null)
            taskManager.OnTaskGenerated.RemoveListener(OnTaskGenerated);
        
        if (multiBucketManager != null)
            multiBucketManager.OnBucketStageCompleted.RemoveListener(OnStageCompleted);
        
        if (hardModeManager != null)
            hardModeManager.OnTaskGenerated.RemoveListener(OnHardModeTaskGenerated);
    }
    
    /// <summary>
    /// 驗證所有資料庫
    /// </summary>
    private void ValidateDatabases()
    {
        if (simpleVoiceDB == null)
        {
            Debug.LogError("[VoiceManagerV2] SimpleVoiceDatabase 未設置！");
            return;
        }
        
        if (intermediateVoiceDB == null)
        {
            Debug.LogError("[VoiceManagerV2] IntermediateVoiceDatabase 未設置！");
            return;
        }
        
        // 驗證音檔完整性
        bool simpleValid = simpleVoiceDB.ValidateDatabase();
        bool intermediateValid = intermediateVoiceDB.ValidateDatabase();
        
        if (simpleValid && intermediateValid)
        {
            Debug.Log("[VoiceManagerV2] 所有語音資料庫驗證通過");
        }
    }
    
    /// <summary>
    /// 任務生成時播放語音
    /// </summary>
    private void OnTaskGenerated(TaskData task)
    {
        if (!enableVoice || task == null) return;
        
        switch (task.taskType)
        {
            case TaskType.CountOnly:
                PlaySimpleTask(task.targetCount);
                break;
                
            case TaskType.ColorCount:
                PlayIntermediateTask(task.targetCount, task.targetColor);
                break;
                
            case TaskType.MultiStage:
                PlayAdvancedTask(task);
                break;
        }
    }
    
    /// <summary>
    /// 困難模式任務生成
    /// </summary>
    private void OnHardModeTaskGenerated(HardModeTask hardTask)
    {
        if (!enableVoice || hardTask == null) return;
        PlayAdvancedHardModeTask(hardTask);
    }
    
    /// <summary>
    /// 階段完成時播放下一階段提示
    /// </summary>
    private void OnStageCompleted(int completedStageIndex)
    {
        if (!enableVoice || taskManager == null) return;
        
        TaskData currentTask = taskManager.GetCurrentTask();
        if (currentTask == null || currentTask.taskType != TaskType.MultiStage) return;
        
        SubTask nextStage = currentTask.GetCurrentSubTask();
        if (nextStage != null)
        {
            Debug.Log($"[VoiceManagerV2] 階段 {completedStageIndex} 完成，播放下一階段");
            PlayIntermediateTask(nextStage.count, nextStage.color);
        }
    }
    
    /// <summary>
    /// 播放簡單模式語音：「請幫我撈 X 隻魚」
    /// </summary>
    private void PlaySimpleTask(int count)
    {
        if (simpleVoiceDB == null) return;
        
        AudioClip clip = simpleVoiceDB.GetVoiceClip(count);
        if (clip != null)
        {
            PlaySingleClip(clip);
        }
        else
        {
            Debug.LogWarning($"[VoiceManagerV2] 找不到簡單模式語音: {count} 隻魚");
        }
    }
    
    /// <summary>
    /// 播放中級模式語音：「請幫我撈 X 隻 [顏色] 的魚」
    /// </summary>
    private void PlayIntermediateTask(int count, string colorKey)
    {
        if (intermediateVoiceDB == null) return;
        
        AudioClip clip = intermediateVoiceDB.GetVoiceClip(count, colorKey);
        if (clip != null)
        {
            PlaySingleClip(clip);
        }
        else
        {
            Debug.LogWarning($"[VoiceManagerV2] 找不到中級模式語音: {count} 隻 {colorKey} 魚");
        }
    }
    
    /// <summary>
    /// 播放高級模式語音：使用中級語音串接
    /// 「請幫我撈 X 隻 [顏色] 的魚、Y 隻 [顏色] 的魚、...」
    /// </summary>
    private void PlayAdvancedTask(TaskData task)
    {
        if (intermediateVoiceDB == null || task.subTasks == null || task.subTasks.Count == 0)
            return;
        
        List<AudioClip> clips = new List<AudioClip>();
        
        for (int i = 0; i < task.subTasks.Count; i++)
        {
            SubTask stage = task.subTasks[i];
            AudioClip stageClip = intermediateVoiceDB.GetVoiceClip(stage.count, stage.color);
            
            if (stageClip != null)
            {
                clips.Add(stageClip);
                
                // 如果不是最後一個階段，加上連接詞
                if (i < task.subTasks.Count - 1 && intermediateVoiceDB.connector_And != null)
                {
                    clips.Add(intermediateVoiceDB.connector_And);
                }
            }
        }
        
        if (clips.Count > 0)
        {
            PlayClipSequence(clips);
        }
    }
    
    /// <summary>
    /// 播放困難模式高級語音（HardModeTask）
    /// </summary>
    private void PlayAdvancedHardModeTask(HardModeTask hardTask)
    {
        if (intermediateVoiceDB == null || hardTask.stages == null || hardTask.stages.Count == 0)
            return;
        
        List<AudioClip> clips = new List<AudioClip>();
        
        for (int i = 0; i < hardTask.stages.Count; i++)
        {
            TaskStage stage = hardTask.stages[i];
            AudioClip stageClip = intermediateVoiceDB.GetVoiceClip(stage.count, stage.targetColor);
            
            if (stageClip != null)
            {
                clips.Add(stageClip);
                
                // 添加連接詞
                if (i < hardTask.stages.Count - 1 && intermediateVoiceDB.connector_And != null)
                {
                    clips.Add(intermediateVoiceDB.connector_And);
                }
            }
        }
        
        if (clips.Count > 0)
        {
            PlayClipSequence(clips);
        }
    }
    
    /// <summary>
    /// 播放單個音檔
    /// </summary>
    private void PlaySingleClip(AudioClip clip)
    {
        StopCurrentPlayback();
        audioSource.PlayOneShot(clip);
        Debug.Log($"[VoiceManagerV2] 播放語音: {clip.name}");
    }
    
    /// <summary>
    /// 播放音檔序列（用於高級模式）
    /// </summary>
    private void PlayClipSequence(List<AudioClip> clips)
    {
        StopCurrentPlayback();
        
        playbackQueue.Clear();
        foreach (AudioClip clip in clips)
        {
            if (clip != null)
            {
                playbackQueue.Enqueue(clip);
            }
        }
        
        if (playbackQueue.Count > 0)
        {
            StartCoroutine(PlayQueueCoroutine());
        }
    }
    
    /// <summary>
    /// 播放隊列協程
    /// </summary>
    private IEnumerator PlayQueueCoroutine()
    {
        isPlaying = true;
        
        while (playbackQueue.Count > 0)
        {
            AudioClip clip = playbackQueue.Dequeue();
            
            audioSource.clip = clip;
            audioSource.Play();
            
            Debug.Log($"[VoiceManagerV2] 播放階段語音: {clip.name}");
            
            yield return new WaitForSeconds(clip.length);
            
            // 階段之間的延遲
            if (playbackQueue.Count > 0)
            {
                yield return new WaitForSeconds(delayBetweenClips);
            }
        }
        
        isPlaying = false;
        Debug.Log("[VoiceManagerV2] 高級模式語音播放完成");
    }
    
    /// <summary>
    /// 停止當前播放
    /// </summary>
    private void StopCurrentPlayback()
    {
        if (isPlaying)
        {
            StopAllCoroutines();
            audioSource.Stop();
            playbackQueue.Clear();
            isPlaying = false;
        }
    }
    
    /// <summary>
    /// 手動播放任務語音
    /// </summary>
    public void PlayTaskVoice(TaskData task)
    {
        OnTaskGenerated(task);
    }
    
    /// <summary>
    /// 啟用/停用語音
    /// </summary>
    public void SetVoiceEnabled(bool enabled)
    {
        enableVoice = enabled;
        if (!enabled)
        {
            StopCurrentPlayback();
        }
    }
}
