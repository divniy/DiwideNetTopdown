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
                StartCoroutine(AutoFocus());
                StartCoroutine(AutoFire());
            }
        }

        private IEnumerator AutoFire()
        {
            while (true)
            {
                if (_target == null) yield break;
                // Fire(transform.TransformPoint(firePoint), transform.rotation);
                Fire();
                yield return new WaitForSeconds(attackDelay);
            }
        }

        private IEnumerator AutoFocus()
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

        public void Fire()
        {
            var position = transform.TransformPoint(firePoint);
            photonView.RPC("FireRPC", RpcTarget.AllViaServer, position, transform.rotation);
        }

        [PunRPC]
        public void FireRPC(Vector3 position, Quaternion rotation, PhotonMessageInfo info)
        {
            // float lag = 0f;
            float lag = (float) (PhotonNetwork.Time - info.SentServerTime);
            
            var bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            bullet.InitializeProjectile(photonView.Owner, (rotation * Vector3.forward), Mathf.Abs(lag));
            // bullet.Parent = gameObject;
            // PhotonNetwork.Instantiate(bulletPrefab.name, transform.TransformPoint(firePoint), transform.rotation);
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine) return;
            Vector2 moveVector = _controls.gameplay.Move.ReadValue<Vector2>();
            
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
            if (bullet == null || bullet.Owner == PhotonNetwork.LocalPlayer) return;

            health -= bullet.Damage;
            // Destroy(other.gameObject);
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
