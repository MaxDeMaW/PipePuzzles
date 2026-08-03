using UnityEngine;
using UnityEngine.Serialization;

namespace Pipes
{
    [CreateAssetMenu(fileName = "GameAnimationSetup", menuName = "Pipes/Game Animation Setup")]
    public sealed class GameAnimationSetup : ScriptableObject
    {
        [Header("Path Fill")]
        [Tooltip("Delay between painting consecutive matched pipes (seconds).")]
        [FormerlySerializedAs("pathFillStepSeconds")]
        [SerializeField] private float _pathFillStepSeconds = 0.125f;

        [Header("Pipe Colors")]
        [Tooltip("Not connected to the left wall.")]
        [FormerlySerializedAs("normalColor")]
        [SerializeField] private Color _normalColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [Tooltip("Connected from the left wall, waiting / idle highlight.")]
        [FormerlySerializedAs("connectedColor")]
        [SerializeField] private Color _connectedColor = new Color(0.82f, 0.74f, 0.42f, 1f);
        [Tooltip("Fill animation before destroy.")]
        [FormerlySerializedAs("filledColor")]
        [SerializeField] private Color _filledColor = new Color(1f, 0.85f, 0.15f, 1f);

        [Header("Drop / Fall")]
        [FormerlySerializedAs("dropDurationSeconds")]
        [SerializeField] private float _dropDurationSeconds = 0.35f;

        [Header("Rotate Punch")]
        [Tooltip("Peak local scale while rotating a pipe.")]
        [FormerlySerializedAs("rotatePunchScale")]
        [SerializeField] private float _rotatePunchScale = 1.25f;
        [Tooltip("Full up+down duration in seconds.")]
        [FormerlySerializedAs("rotatePunchDuration")]
        [SerializeField] private float _rotatePunchDuration = 0.18f;

        [Header("Score UI")]
        [FormerlySerializedAs("scoreCountDuration")]
        [SerializeField] private float _scoreCountDuration = 0.45f;
        [FormerlySerializedAs("scorePunchScale")]
        [SerializeField] private float _scorePunchScale = 1.35f;
        [FormerlySerializedAs("scorePunchDuration")]
        [SerializeField] private float _scorePunchDuration = 0.22f;

        public float PathFillStepSeconds => Mathf.Max(0.01f, _pathFillStepSeconds);
        public int PathFillStepMs => Mathf.Max(1, Mathf.RoundToInt(PathFillStepSeconds * 1000f));

        public float DropDurationSeconds => Mathf.Max(0.01f, _dropDurationSeconds);

        public float RotatePunchScale => Mathf.Max(1.01f, _rotatePunchScale);
        public float RotatePunchDuration => Mathf.Max(0.05f, _rotatePunchDuration);

        public float ScoreCountDuration => Mathf.Max(0.05f, _scoreCountDuration);
        public float ScorePunchScale => Mathf.Max(1.01f, _scorePunchScale);
        public float ScorePunchDuration => Mathf.Max(0.05f, _scorePunchDuration);

        public Color NormalColor => _normalColor;
        public Color ConnectedColor => _connectedColor;
        public Color FilledColor => _filledColor;
    }
}
