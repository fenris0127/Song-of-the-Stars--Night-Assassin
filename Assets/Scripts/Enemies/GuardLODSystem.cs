using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 경비병 LOD (Level of Detail) 시스템
/// 플레이어와 거리에 따라 업데이트 빈도 조절
/// </summary>
public class GuardLODSystem : MonoBehaviour
{
    public static GuardLODSystem Instance { get; private set; }

    [Header("▶ LOD 거리 임계값")]
    [Tooltip("이 거리 이상이면 Low LOD")]
    public float lowLODDistanceSqr = 400f; // 20타일
    [Tooltip("이 거리 이상이면 Medium LOD")]
    public float mediumLODDistanceSqr = 100f; // 10타일
    
    [Header("▶ LOD별 업데이트 간격")]
    public int highLODInterval = 1;    // 매 프레임
    public int mediumLODInterval = 2;  // 2프레임마다
    public int lowLODInterval = 5;     // 5프레임마다

    private List<GuardRhythmPatrol> _allGuards = new List<GuardRhythmPatrol>();
    private Transform _playerTransform;
    private int _frameCounter = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        _playerTransform = GameServices.Player?.transform;
        RegisterAllGuards();
    }

    void RegisterAllGuards()
    {
        GuardRhythmPatrol[] guards = FindObjectsOfType<GuardRhythmPatrol>();
        _allGuards.AddRange(guards);
    }

    public void RegisterGuard(GuardRhythmPatrol guard)
    {
        if (!_allGuards.Contains(guard))
            _allGuards.Add(guard);
    }

    public void UnregisterGuard(GuardRhythmPatrol guard)
    {
        _allGuards.Remove(guard);
    }

    void Update()
    {
        if (_playerTransform == null) return;

        _frameCounter++;
        Vector2 playerPos = _playerTransform.position;

        for (int i = _allGuards.Count - 1; i >= 0; i--)
        {
            if (_allGuards[i] == null)
            {
                _allGuards.RemoveAt(i);
                continue;
            }

            float sqrDist = ((Vector2)_allGuards[i].transform.position - playerPos).sqrMagnitude;
            int interval = GetUpdateInterval(sqrDist);

            if (_frameCounter % interval == 0)
                _allGuards[i].enabled = true;
            else
                _allGuards[i].enabled = false;
        }
    }

    int GetUpdateInterval(float sqrDistance)
    {
        if (sqrDistance >= lowLODDistanceSqr)
            return lowLODInterval;
        else if (sqrDistance >= mediumLODDistanceSqr)
            return mediumLODInterval;
        else
            return highLODInterval;
    }
}