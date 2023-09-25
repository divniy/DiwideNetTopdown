using System;
using UnityEngine;

namespace Diwide.Topdown
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float distance;
        [SerializeField] private float height;
        [SerializeField] private Vector3 centerOffset;
        [SerializeField] private float interpolationRatio;
        

        private Transform _target;
        private bool _isFollow;
        Vector3 _cameraOffset = Vector3.zero;

        private void Start()
        {
            _cameraOffset.z = -distance;
            _cameraOffset.y = height;
        }

        public void FollowTarget(Transform target)
        {
            _target = target;
            _isFollow = true;
            Turn();
        }

        private void LateUpdate()
        {
            if (_target != null && _isFollow)
            {
                Follow();
            }
        }

        private void Turn()
        {
            transform.position = _target.position + _target.TransformVector(_cameraOffset);
            transform.LookAt(_target.position + centerOffset);
        }

        private void Follow()
        {
            transform.position = Vector3.Lerp(transform.position,
                _target.position + _target.TransformVector(_cameraOffset), interpolationRatio * Time.deltaTime);
            transform.LookAt(_target.position + centerOffset);
        }
    }
}