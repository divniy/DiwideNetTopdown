using System.Collections;
using Photon.Pun;
using UniRx;
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
        [SerializeField, Range(.3f, 3f)] 
        private float attackDelay = .1f;
        [SerializeField, Range(.1f, 1f)] 
        private float rotateDelay = .25f;

        [SerializeField] private Vector3 firePoint;
        
        [Range(1f, 100f)]
        public float maxHealth = 100f;
        
        private PlayerControls _controls;

        private Rigidbody _rigidbody;

        private Collider _collider;
        
        private Transform _target;

        private CameraController _cameraController;

        private bool _isControllable = true;
        
        public ReactiveProperty<float> Health { get; private set; }
        
        public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }
        
        void Awake()
        {
            _controls = new PlayerControls();
            Health = new ReactiveProperty<float>(maxHealth);
            IsDead = Health.Select(v => v <= 0).ToReactiveProperty();

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
            _collider = GetComponent<Collider>();

            if (photonView.IsMine)
            {
                _cameraController = Camera.main.GetComponent<CameraController>();
                _cameraController.FollowTarget(transform);
                
                //todo: Remove this
                Health.Subscribe(_ => Debug.Log($"Health is now {Health.Value}"));
            }

            IsDead.Where(v => v == true).Subscribe(_ =>
            {
                photonView.RPC("EndOfRound", RpcTarget.All);
            });
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
        
        [PunRPC]
        public void EndOfRound()
        {
            _isControllable = false;
            _rigidbody.velocity = Vector3.zero;
            _collider.enabled = false;
            
            if (photonView.IsMine)
            {
                // StopAllCoroutines();
                StartCoroutine(ShowResultsThenExit());
            }
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine || !_isControllable) return;
            
            Vector2 moveVector = _controls.gameplay.Move.ReadValue<Vector2>();
            
            if (moveVector == Vector2.zero) return;

            var velocity = _rigidbody.velocity;
            var deltaVelocity = transform.TransformVector(new Vector3(moveVector.x, 0, moveVector.y));
            velocity += deltaVelocity * moveSpeed * Time.fixedDeltaTime;
            // velocity += new Vector3(moveVector.x, 0, moveVector.y) * moveSpeed * Time.fixedDeltaTime;
            velocity.y = 0;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            _rigidbody.velocity = velocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<ProjectileController>();
            if (bullet == null || bullet.Owner == PhotonNetwork.LocalPlayer) return;

            if (bullet != null && photonView.IsMine && bullet.Owner != PhotonNetwork.LocalPlayer)
            {
                Health.Value = Mathf.Max(Health.Value - bullet.Damage, 0);
            }
            
            // health -= bullet.Damage;
            // Destroy(other.gameObject);
            // if (health <= 0f) Debug.Log($"Player with name {name} is dead");
        }

        private IEnumerator ShowResultsThenExit()
        {
            if (IsDead.Value)
            {
                Debug.Log("You lose");
            }
            else
            {
                Debug.Log("You win");
            }

            yield return new WaitForSeconds(5);

            PhotonNetwork.LeaveRoom();
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
                stream.SendNext(Health.Value);
            }
            else
            {
                Health.Value = (float)stream.ReceiveNext();
            }
        }
    }
}
