using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diwide.Topdown
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static PlayerController LocalPlayerInstance;
        public static PlayerController TargetPlayerInstance;
        
        [SerializeField] private ProjectileController bulletPrefab;
        
        [SerializeField, Range(1f, 10f)] 
        private float moveSpeed = 2f;
        [SerializeField, Range(.5f, 5f)] 
        private float maxSpeed = 2f;
        [SerializeField, Range(.1f, 1f)] 
        private float attackDelay = .1f;
        [SerializeField, Range(.1f, 1f)] 
        private float rotateDelay = .25f;

        [SerializeField] private Vector3 firePoint;
        
        [Range(1f, 100f)]
        public float health = 100f;
        
        private PlayerControls _controls;

        private Rigidbody _rigidbody;
        
        private Transform _target;
        
        void Awake()
        {
            _controls = new PlayerControls();

            if (photonView.IsMine)
            {
                LocalPlayerInstance = this;
                if(TargetPlayerInstance != null) SetTarget(TargetPlayerInstance.transform);
            }
            else
            {
                TargetPlayerInstance = this;
                if(LocalPlayerInstance != null) LocalPlayerInstance.SetTarget(transform);
            }

            DontDestroyOnLoad(gameObject);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _controls.gameplay.Enable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _controls.gameplay.Disable();
        }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            
            if (photonView.IsMine)
            {
                // StartCoroutine(Fire());
                StartCoroutine(Focus());
            }
        }

        private IEnumerator Fire()
        {
            while (true)
            {
                if (_target == null) yield break;
                var bullet = Instantiate(bulletPrefab, transform.TransformPoint(firePoint), transform.rotation);
                bullet.Parent = gameObject;
                yield return new WaitForSeconds(attackDelay);
            }
        }

        private IEnumerator Focus()
        {
            while (true)
            {
                if (_target == null) yield break;
                transform.LookAt(_target);
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
                yield return new WaitForSeconds(rotateDelay);
            }
            
            // yield return 
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine) return;
            Vector2 moveVector = _controls.gameplay.Move.ReadValue<Vector2>();
            // Vector2 moveVector = _actionMap.FindAction("Move").ReadValue<Vector2>();
            
            if (moveVector == Vector2.zero) return;

            var velocity = _rigidbody.velocity;
            velocity += new Vector3(moveVector.x, 0, moveVector.y) * moveSpeed * Time.fixedDeltaTime;
            velocity.y = 0;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            _rigidbody.velocity = velocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<ProjectileController>();
            if (bullet == null || bullet.Parent == gameObject) return;

            health -= bullet.Damage;
            Destroy(other.gameObject);
            if (health <= 0f) Debug.Log($"Player with name {name} is dead");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(firePoint, .2f);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(health);
            }
            else
            {
                health = (float)stream.ReceiveNext();
            }
        }
    }
}
