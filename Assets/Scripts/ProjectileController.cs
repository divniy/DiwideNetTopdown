using System.Collections;
using Photon.Realtime;
using UnityEngine;

namespace Diwide.Topdown
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField, Range(1f, 10f)] 
        private float _moveSpeed = 3f;
        [SerializeField, Range(1f, 10f)] 
        private float _damage = 1f;
        [SerializeField, Range(1f, 15f)] 
        private float _lifetime = 7f;

        public float Damage => _damage;
        public Player Owner { get; private set; }
        
        void Start()
        {
            // StartCoroutine(OnDieCoroutine());
            Destroy(gameObject, _lifetime);
        }
        
        public void OnTriggerEnter(Collider _)
        {
            Destroy(gameObject);
        }

        // void Update()
        // {
        //     transform.position += transform.forward * (_moveSpeed * Time.deltaTime);
        // }

        // private IEnumerator OnDieCoroutine()
        // {
        //     yield return new WaitForSeconds(_lifetime);
        //     Destroy(gameObject);
        // }

        public void InitializeProjectile(Player owner, Vector3 originalDirection, float lagCompensation)
        {
            Owner = owner;

            transform.forward = originalDirection;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = originalDirection * _moveSpeed;
            rigidbody.position += rigidbody.velocity * lagCompensation;
        }
    }
}
