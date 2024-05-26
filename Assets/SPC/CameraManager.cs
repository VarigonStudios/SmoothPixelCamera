using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace VARIGONSTUDIOS.SYSTEMS.SPC
{
    //[ExecuteInEditMode]
    public class SPCManager : MonoBehaviour //Smooth Pixel Camera enables smooth camera movement while maintaining a consistent Pixel Unit
    {

        //Cameras
        private Camera _gameCamera;
        private Camera _playerCamera;
        private CinemachineVirtualCamera _spCamera;
        private CinemachineFramingTransposer _spcFrame;


        private float _step;
        private static int zOffest = 3;


        //Input Actions
        [SerializeField] private InputAction i_pivot;
        [SerializeField] private InputAction i_rotate;
        [SerializeField] private InputAction i_reset;
        [SerializeField] private InputAction i_zoom;


        //Input Variables
        private Vector2 m_pivot;
        private float m_rotate;
        private float m_zoom;
        private bool m_reset;



        private void OnEnable()
        {
            i_pivot.Enable();
            i_rotate.Enable();
            i_reset.Enable();
            i_zoom.Enable();
        }

        private void OnDisable()
        {
            i_pivot.Disable();
            i_rotate.Disable();
            i_reset.Disable();
            i_zoom.Disable();
        }

        private void Awake()
        {
            _gameCamera = this.gameObject.GetComponent<Camera>();

            if (Camera.main == null)
            {
                Debug.Log("Main Camera doesn't Exist");
                return;
            }

            _playerCamera = Camera.main;
            _spCamera = _playerCamera.transform.parent.GetComponent<CinemachineVirtualCamera>();
            _spcFrame = _spCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        private void Update()
        {
            m_pivot = i_pivot.ReadValue<Vector2>();
            m_rotate = i_rotate.ReadValue<float>();
            m_reset = i_reset.triggered;
            m_zoom = i_zoom.ReadValue<float>();

            _spcFrame.m_CameraDistance = zOffest; //Keeps Player Camera in the proper z-layer


            #region GAME CAMERA SNAPPING

            if (_playerCamera.orthographicSize < 18) // Game Camera Snaps based on Othro Size Factor
            {
                _step = _playerCamera.orthographicSize > 12
                    ? MathF.Round(18 - _playerCamera.orthographicSize)
                    : MathF.Floor(128 / _playerCamera.orthographicSize);
            }

            var p = _playerCamera.transform.position;
            transform.position = new Vector3(Mathf.RoundToInt(p.x / _step) * _step,
            Mathf.RoundToInt(p.y / (_step / 2f)) * (_step / 2f), -1f);

            #endregion


            #region PLAYER CAMERA ROTATION

            //CAMERA RESET
            if (m_reset)
            {
                _spCamera.gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
            }

            //CAMERA ZOOM CONTROL
            float zoom = 0.005f * _spCamera.m_Lens.OrthographicSize; //Variable Zoom Speed

            if (_spCamera.m_Lens.OrthographicSize > 0.2)
                if (m_zoom != 0)
                {
                    if (m_zoom < 0)
                    {
                        _spCamera.m_Lens.OrthographicSize -= zoom;
                    }
                }

            if (_spCamera.m_Lens.OrthographicSize < 17)
            {
                if (m_zoom != 0)
                {
                    if (m_zoom > 0)
                    {
                        _spCamera.m_Lens.OrthographicSize += zoom;
                    }
                }
            }

            //CAMERA ROTATE CONTROL
            if (m_rotate != 0)
            {
                switch (m_rotate)
                {
                    case < 0:
                        _spCamera.gameObject.transform.Rotate(new Vector3(0, 0, -0.1f));
                        break;
                    case > 0:
                        _spCamera.gameObject.transform.Rotate(new Vector3(0, 0, +0.1f));
                        break;
                }
            }

            //CAMERA TILT CONTROL
            if (m_pivot.x != 0)
            {
                switch (m_pivot.x)
                {
                    case > 0:
                    {
                        if (_spCamera.gameObject.transform.eulerAngles.y is < 50f or >= 309f)
                        {
                            _spCamera.gameObject.transform.Rotate(new Vector3(0, +0.1f, 0));
                        }

                        break;
                    }
                    case < 0:
                    {
                        if (_spCamera.gameObject.transform.eulerAngles.y is <= 51f or > 310f)
                        {
                            _spCamera.gameObject.transform.Rotate(new Vector3(0, -0.1f, 0));
                        }

                        break;
                    }
                }
            }

            switch (m_pivot.y)
            {
                case 0:
                    return;
                case > 0:
                {
                    if (_spCamera.gameObject.transform.eulerAngles.x is < 30f or >= 329f)
                    {
                        _spCamera.gameObject.transform.Rotate(new Vector3(+0.1f, 0, 0));
                    }

                    break;
                }
                case < 0:
                {
                    if (_spCamera.gameObject.transform.eulerAngles.x is <= 31f or > 330f)
                    {
                        _spCamera.gameObject.transform.Rotate(new Vector3(-0.1f, 0, 0));
                    }

                    break;
                }
            }

            #endregion

        }

        private void OnDrawGizmos()
        {
            if (_gameCamera != null)
            {
                //Main Camera Rect

                var gcHeight = 2f * _gameCamera.orthographicSize;
                var gcWidth = gcHeight * _gameCamera.aspect;


                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector2(gcWidth, gcHeight));

                Gizmos.color = new Color(255, 0, 0, .1f);
                Gizmos.DrawCube(transform.position, new Vector2(gcWidth, gcHeight));

            }

            if (_playerCamera != null)
            {
                //Player Camera Rect
                Gizmos.color = Color.green;
                var pcHeight = 2f * _playerCamera.orthographicSize;
                var pcWidth = pcHeight * _playerCamera.aspect;
                var m = new Matrix4x4(new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0),
                    new Vector4(1, 1, 1, 0));

                Gizmos.matrix = _playerCamera.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector2.zero, new Vector2(pcWidth, pcHeight));

                Gizmos.color = new Color(0, 255, 255, .1f);
                Gizmos.DrawCube(Vector2.zero, new Vector2(pcWidth, pcHeight));
            }

        }
    }
}