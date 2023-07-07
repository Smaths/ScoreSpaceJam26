using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Blobs
{
    public enum BlobType { Good, Bad }

    [RequireComponent(typeof(NavMeshAgent))]
    public class Blob : MonoBehaviour
    {
        #region Fields
        [SerializeField] private int _points;
        private const float _animationTime = 0.3f;
        [BoxGroup("Dependencies")]
        [SceneObjectsOnly]
        [SerializeField] private GameObject _playerBlob;

        [Title("Blob AI", "Brains for blobs characters good and bad.", TitleAlignments.Split)]
        [SerializeField] private BlobType _blobType;
        [SerializeField] private float _speed = 4f;

        [Header("Transformation")]
        [SuffixLabel("second(s)")] [HideIf("_blobType", BlobType.Bad)]
        [Tooltip("The duration the character stays in the untransformed state.")]
        [SerializeField] private Vector2 _untransformedDuration = new(5f, 6f);
        [SuffixLabel("second(s)")] [HideIf("_blobType", BlobType.Bad)]
        [Tooltip("The duration the character stays in the transformed state.")]
        [SerializeField] private Vector2 _transformedDuration = new(3f, 4f);
        [SuffixLabel("second(s)")] [HideIf("_blobType", BlobType.Bad)]
        [Tooltip("Rate at which the character transforms from one state to the other.")]
        [SerializeField] private float _transformationTime = 0.25f;

        [Header("Masks")]
        [Tooltip("Select ground layer to check if random wander point is on the walkable ground.")]
        [SerializeField] private LayerMask _groundMask;
        [Tooltip("Select layers for the character to avoid when finding a random wander position.")]
        [SerializeField] private LayerMask _avoidMask;
        [Tooltip("Select layers for the character to use for chasing other things down.")]
        [SerializeField] private LayerMask _sightMask;

        [Title("Sight", "Will start to chase players in range", TitleAlignments.Split, false)]
        [Tooltip("How far the blob can see.")]
        [SuffixLabel("meter(s)")] [MinValue(0.01f)]
        [SerializeField] private float _sightRange = 4f;

        [Header("Patrol")]
        [SuffixLabel("meter(s)")] [MinValue(0)]
        [SerializeField] private float _patrolRadius = 4f;
        [LabelText("Distance Threshold")]
        [SuffixLabel("meter(s)")] [MinValue(0.01f)]
        [Tooltip("Range the character can be from randomly selected target position before searching for a new one. Lower numbers are strict, higher numbers are more accommodating.")]
        [SerializeField] private float _patrolDistanceThreshold = 0.25f;
        [SuffixLabel("second(s)")] [MinValue(0)]
        [Tooltip("How long the character will wait before finding a new position.")]
        [SerializeField] private Vector2 _patrolWaitTime = new(2f, 4f);

        [Header("Transforms")]
        [Required] [ChildGameObjectsOnly]
        [SerializeField] private Transform _blob;
        [Required] [ChildGameObjectsOnly]
        [SerializeField] private SkinnedMeshRenderer _blobRenderer;
        [Optional] [ChildGameObjectsOnly]
        [SerializeField] private Transform _hat;

        [Header("Colors")]
        [SerializeField] private Color _goodBlobColor = new(0.749f, 0.753f, 0.247f, 1.0f);
        [SerializeField] private Color _badBlobColor = new(0.682f, 0.298f, 0.294f, 1.0f);
        private Material _blobMaterial;

        [Header("Death FX")]
        [ChildGameObjectsOnly]
        [SerializeField] private GameObject _deathFX;
        private ParticleSystem _deathPS;

        private NavMeshAgent _navMeshAgent;
        #endregion

        #region Public Properties
        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        public int Points => _points;
        public BlobType BlobType => _blobType;
        public LayerMask GroundMask => _groundMask;
        public LayerMask SightMask => _sightMask;
        public LayerMask AvoidMask => _avoidMask;
        public GameObject PlayerBlob
        {
            get
            {
                if (_playerBlob == null)
                    _playerBlob = GameObject.FindWithTag("Player");
                return _playerBlob;
            }
        }
        public float Speed => _speed;
        public float PatrolRadius => _patrolRadius;
        public float PatrolDistanceThreshold => _patrolDistanceThreshold;
        public Vector2 PatrolWaitTime => _patrolWaitTime;
        public float SightRange => _sightRange;
        public float AnimationTime => _animationTime;
        public float TransformationTime => _transformationTime;
        public Transform BlobTransform => _blob;
        public SkinnedMeshRenderer BlobRenderer => _blobRenderer;
        public Transform Hat => _hat;
        public Material BlobMaterial => _blobMaterial;
        public Color GoodBlobColor => _goodBlobColor;
        public Color BadBlobColor => _badBlobColor;
        public GameObject DeathFX => _deathFX;
        #endregion

        #region Lifecycle
        private void OnDrawGizmosSelected()
        {
            // Draw the search radius gizmo in the Unity Editor
            Gizmos.color = new Color(0.343f, 0.681f, 0.569f);
            Gizmos.DrawWireSphere(transform.position, _patrolRadius);

            Gizmos.color = new Color(0.581f, 0.229f, 0.237f);
            Gizmos.DrawWireSphere(transform.position, _sightRange);
        }

        private void Awake()
        {
            _deathPS ??= _deathFX.GetComponent<ParticleSystem>();
            _navMeshAgent ??= GetComponent<NavMeshAgent>();
            _blobMaterial ??= _blobRenderer.materials[0];
        }
        #endregion

        // Public Methods
        public float GetTransformationInterval(bool isTransformed)
        {
            float randomMin = isTransformed ? _transformedDuration.x : _untransformedDuration.x;
            float randomMax = isTransformed ? _transformedDuration.y : _untransformedDuration.y;
            float randomDuration = Random.Range(randomMin, randomMax);
            return randomDuration;
        }

        public float GetDeathAnimationDuration()
        {
            return _deathPS.main.duration >= _animationTime ? _deathPS.main.duration : _animationTime;;
        }
    }
}