using UnityEngine;

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Player character configuration settings
    /// 플레이어 캐릭터 설정값
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Song of the Stars/Configs/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement Settings")]
        [Tooltip("Distance player moves per grid step")]
        public float moveDistance = 2f;

        [Tooltip("Speed of movement animation")]
        public float moveSpeed = 8f;

        [Header("Free Movement")]
        [Tooltip("Focus cost for free movement mode")]
        public float freeMoveFocusCost = 50f;

        [Tooltip("Duration of free movement in seconds")]
        public float freeMoveDuration = 1f;

        [Header("Assassination")]
        [Tooltip("Range for melee assassination")]
        public float assassinationRange = 1.5f;

        [Tooltip("Maximum range for ranged assassination")]
        public float maxAssassinationRange = 15f;

        [Tooltip("Maximum number of targets to check for assassination")]
        public int maxAssassinationTargets = 10;

        [Header("Performance")]
        [Tooltip("Buffer size for assassination detection")]
        public int assassinationBufferSize = 10;
    }
}
