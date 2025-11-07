using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// Rhythm-synced asset loader using UniTask instead of Unity Coroutines.
/// - Loads three AudioClip assets (beat-synced sound assets) asynchronously from Resources.
/// - Awaits all loads concurrently and completes when every clip is ready.
///
/// Usage:
/// 1. Place your three AudioClip assets under a `Resources/` folder and set their paths (without the Resources/ prefix) in the inspector.
/// 2. Attach this script to a GameObject (for example `AudioManager` or `GameManager`).
/// 3. Call `LoadBeatAssetsAsync().Forget();` from `Start()` or another initialization routine, or `await` it from another UniTask.
///
/// Note: Requires the UniTask package (com.cysharp.unitask). Install via OpenUPM or add to Packages/manifest.json:
///   "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git#upm"
///
/// Integration: This project uses Korean comments for gameplay notes and English for technical docs — follow the repo convention.
/// Important state changes and errors are logged with Debug.Log / Debug.LogWarning / Debug.LogError to aid debugging.
/// </summary>
public class RhythmAssetLoader : MonoBehaviour
{
    #region Inspector
    [Header("▶ Beat asset paths (Resources folder)")]
    [Tooltip("Paths to AudioClips inside Resources. Example: 'Audio/Beats/beat1' (omit 'Resources/')")]
    public string[] resourcePaths = new string[3];

    [Header("▶ Loaded clips (read-only at runtime)")]
    public AudioClip[] loadedClips = new AudioClip[0];
    #endregion

    #region Public API
    /// <summary>
    /// 비트 동기화된 3개의 사운드 에셋을 비동기 로드하고, 모든 로드가 완료될 때까지 대기합니다.
    /// Returns a UniTask that completes when all three clips are loaded into `loadedClips`.
    /// </summary>
    public async UniTask LoadBeatAssetsAsync(CancellationToken cancellationToken = default)
    {
        if (resourcePaths == null || resourcePaths.Length == 0)
        {
            Debug.LogWarning("RhythmAssetLoader: No resource paths assigned.");
            loadedClips = new AudioClip[0];
            return;
        }

        // Limit to 3 assets (project requirement); still support fewer/more gracefully
        var paths = resourcePaths.Take(3).ToArray();

        try
        {
            // Create an array of UniTasks that load each clip concurrently
            var tasks = paths.Select(p => LoadClipAsync(p, cancellationToken)).ToArray();

            // Await all loads in parallel; this avoids sequential blocking and reduces frame stalls
            AudioClip[] results = await UniTask.WhenAll(tasks);

            // Assign results to public array for other systems to use
            loadedClips = results;

            Debug.Log($"RhythmAssetLoader: Successfully loaded {results.Length} clip(s).");
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("RhythmAssetLoader: Load cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            // Centralized error handling: log and rethrow if caller needs it
            Debug.LogError($"RhythmAssetLoader: Failed to load beat assets. Exception: {ex}");
            throw;
        }
    }
    #endregion

    #region Implementation
    /// <summary>
    /// Load a single AudioClip from Resources using Unity's ResourceRequest but awaited via UniTask.
    /// We use a small polling loop with UniTask.Yield to avoid allocations that coroutines/continuations sometimes cause.
    /// </summary>
    private async UniTask<AudioClip> LoadClipAsync(string resourcesPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(resourcesPath))
            throw new ArgumentException("resourcesPath is null or empty", nameof(resourcesPath));

        // Start the asynchronous load operation
        ResourceRequest request = Resources.LoadAsync<AudioClip>(resourcesPath);

        // Await until done, honoring cancellation
        while (!request.isDone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.Yield();
        }

        // Convert to AudioClip and validate
        var clip = request.asset as AudioClip;
        if (clip == null)
            throw new InvalidOperationException($"RhythmAssetLoader: Resource at '{resourcesPath}' is not an AudioClip or could not be loaded.");

        return clip;
    }
    #endregion

    #region Example usage
    // Example: auto-start loading on Start and fire-and-forget.
    // Attach this component to an object and ensure resourcePaths are set in the inspector.
    private void Start()
    {
        // Fire-and-forget — any exceptions will be logged by LoadBeatAssetsAsync
        LoadBeatAssetsAsync().Forget();
    }
    #endregion

    /*
     Technical notes (why UniTask vs Coroutines)

     - GC allocation reduction:
       * UniTask uses value-type task containers and avoids allocating closures and IEnumerator objects that Unity Coroutines create per StartCoroutine call.
       * The implementation above avoids per-frame allocations by awaiting `UniTask.Yield()` and using lightweight UniTask tasks; this lowers GC pressure during gameplay, especially when loads/awaits occur frequently.

     - Error handling (try/catch):
       * UniTask integrates with C# async/await patterns so exceptions propagate naturally and can be caught with try/catch inside async methods.
       * This makes centralized error handling simpler and less error-prone than capturing exceptions from coroutine callbacks.

     - Cancellation & composability:
       * UniTask supports CancellationToken, `UniTask.WhenAll`, and combinators which make composing multiple async operations straightforward (as used above).

     - Notes on Resources vs Addressables:
       * This example uses `Resources.LoadAsync` for simplicity. For larger projects or dynamic content management prefer Addressables — UniTask can await Addressables async operations similarly.
    */

}
