using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public class RhythmAssetLoader : MonoBehaviour
{
    #region Inspector
    [Header("▶ Beat asset paths (Resources folder)")]
    [Tooltip("Paths to AudioClips inside Resources. Example: 'Audio/Beats/beat1' (omit 'Resources/')")]
    public string[] _resourcePaths = new string[0];

    [Header("▶ Loaded clips (read-only at runtime)")]
    [SerializeField] // Runtime에서 Inspector에 표시되도록 설정
    private AudioClip[] _loadedClips = new AudioClip[0];

    [Header("▶ Loaded clips (read-only at runtime)")]
    public AudioClip[] LoadedClips => _loadedClips;
    #endregion

    #region Public API
    /// <summary>
    /// 비트 동기화된 3개의 사운드 에셋을 비동기 로드하고, 모든 로드가 완료될 때까지 대기합니다.
    /// </summary>
    public async UniTask LoadBeatAssetsAsync(CancellationToken cancellationToken = default)
    {
        if (_resourcePaths == null || _resourcePaths.Length == 0)
        {
            Debug.LogWarning("RhythmAssetLoader: No resource paths assigned.");
            _loadedClips = new AudioClip[0];
            return;
        }

        // 유효한 경로만 필터링
        var pathsToLoad = _resourcePaths.Where(p => !string.IsNullOrEmpty(p)).ToArray();

        if (pathsToLoad.Length == 0)
        {
             Debug.LogWarning("RhythmAssetLoader: All resource paths are empty strings. Returning empty.");
            _loadedClips = new AudioClip[0];
            return;
        }
        try
        {
            // 각 경로에 대해 비동기 로드 작업을 시작합니다.
            var tasks = pathsToLoad.Select(p => LoadClipAsync(p, cancellationToken)).ToArray();

            // 모든 작업이 완료될 때까지 대기합니다.
            AudioClip[] results = await UniTask.WhenAll(tasks);

            _loadedClips = results;

            Debug.Log($"RhythmAssetLoader: Successfully loaded {results.Length} clip(s).");
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("RhythmAssetLoader: Load cancelled.");
            // 취소 예외는 호출자에게 전달하여 적절히 처리하도록 함
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"RhythmAssetLoader: Failed to load beat assets. Exception: {ex}");
            // 로드 실패 예외는 호출자에게 전달
            throw;
        }
    }
    #endregion

    #region Implementation
    private async UniTask<AudioClip> LoadClipAsync(string resourcesPath, CancellationToken cancellationToken)
    {
        // 1. 경로 유효성 검사 (이전 코드에 이미 포함됨)
        if (string.IsNullOrEmpty(resourcesPath))
            throw new ArgumentException("resourcesPath is null or empty", nameof(resourcesPath));

        // 2. Resources.LoadAsync 시작
        ResourceRequest request = Resources.LoadAsync<AudioClip>(resourcesPath);

        // 3. ✨ 가장 안전한 방식으로 로드가 완료될 때까지 대기 (취소 지원 포함)
        // 참고: 취소되었다면 이미 여기서 OperationCanceledException을 던지고 함수가 종료됨.
        await UniTask.WaitUntil(() => request.isDone, PlayerLoopTiming.Update, cancellationToken);
        
        // 4. 결과 검증
        var clip = request.asset as AudioClip;
        
        if (clip == null)
        {
            throw new InvalidOperationException(
                $"RhythmAssetLoader: Resource at '{resourcesPath}' is not an AudioClip or could not be loaded. Check resource path and type.");
        }

        return clip;
    }
    #endregion

    /*
    🛠️ 기술 노트 (UniTask vs 코루틴을 사용하는 이유)
    🗑️ GC(Garbage Collection) 할당 감소:
    UniTask는 값 형식(value-type)의 태스크 컨테이너를 사용하여, Unity 코루틴이 StartCoroutine 호출당 생성하는 클로저(closures)와 IEnumerator 객체의 할당을 피합니다.

    위의 구현은 UniTask.Yield()를 await하고 경량의 UniTask 태스크를 사용하여 프레임당 할당을 방지합니다. 이는 특히 로딩/await가 자주 발생할 때 게임 플레이 중 GC 부하를 낮춥니다.

    🛑 오류 처리 (try/catch):
    UniTask는 C#의 async/await 패턴과 통합되어 예외가 자연스럽게 전파되고 async 메서드 내에서 try/catch로 잡힐 수 있습니다.

    이것은 코루틴 콜백에서 예외를 캡처하는 것보다 중앙 집중식 오류 처리를 더 간단하고 오류 발생 가능성을 낮춥니다.

    ❌ 취소 및 합성(Composability):
    UniTask는 CancellationToken, UniTask.WhenAll, 그리고 결합자(combinators)를 지원하여 여러 비동기 작업을 (위에서 사용된 것처럼) 간단하게 합성(composing)할 수 있게 합니다.

    📚 Resources vs Addressables에 대한 참고 사항:
    이 예제는 간단함을 위해 Resources.LoadAsync를 사용합니다. 더 큰 프로젝트나 동적 콘텐츠 관리를 위해서는 Addressables를 선호해야 하며, UniTask는 Addressables의 비동기 작업도 유사하게 await 할 수 있습니다.
    */

}
