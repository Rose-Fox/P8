using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Devkit.Modularis.Channels;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;

public class DemonstrationPoseRecorder : MonoBehaviour
{
    [SerializeField] private GestureRecorderEvents _gestureRecorderEvents;
    [SerializeField] private TransformChannel leftHandTransform,rightHandTransform;
    private TransformChannel _correctTransform;
    private Transform[] _transforms;
    private bool _isRecording;
    private float _recordingStartTime;


    private void OnHandRootTransformChanged()
    {
        if (!_correctTransform.isSet)
        {
            _transforms = null;
            return;
        }
        _transforms = _correctTransform.Value.GetComponentsInChildren<Transform>();
    }


    private void BeginRecording()
    {
        if (!_correctTransform.isSet)
        {
            _gestureRecorderEvents.DoneRecordingDemonstrationGesture();
            return;
        }
        _gestureRecorderEvents.CurrentGesture.ResetPoseData(Gesture.PoseDataType.Demonstration);
        if (_gestureRecorderEvents.CurrentGesture.Type == Gesture.GestureType.Static)
        {
            _gestureRecorderEvents.CurrentGesture.AddPoseFrame(Gesture.PoseDataType.Demonstration,new Gesture.PoseFrameData
            {
                positions = _transforms.Select(e => e.localPosition).ToList(),
                rotations = _transforms.Select(e => e.localRotation).ToList(),
                time = 0
            });
            
            _gestureRecorderEvents.DoneRecordingDemonstrationGesture();
            return;
        }
        _isRecording = true;
        _recordingStartTime = Time.time;
    }

    private void StopRecording()
    {
        _isRecording = false;
    }
    private void FixedUpdate()
    {
        if (_isRecording && _correctTransform.isSet)
        {
            _gestureRecorderEvents.CurrentGesture.AddPoseFrame(Gesture.PoseDataType.Demonstration, new Gesture.PoseFrameData
            {
                positions = _transforms.Select(e => e.localPosition).ToList(),
                rotations = _transforms.Select(e => e.localRotation).ToList(),
                time = Time.time - _recordingStartTime
            });
        }
    }

    private void OnEnable()
    {
        if (_gestureRecorderEvents != null)
        {
            _gestureRecorderEvents.OnStartRecordingDemonstrationGesture += BeginRecording;
            _gestureRecorderEvents.OnDoneRecordingDemonstrationGesture += StopRecording;
            _gestureRecorderEvents.OnGestureSelected += UpdateTransforms;
            _correctTransform.RegisterCallback(OnHandRootTransformChanged);
        }
    }

    private void UpdateTransforms(object obj)
    {
        if (_correctTransform != null)
        {
            _correctTransform.UnregisterCallback(OnHandRootTransformChanged);
        }
        if (_gestureRecorderEvents.CurrentGesture.Handedness == Handedness.Right)
        {
            _correctTransform = rightHandTransform;
        }
        else
        {
            _correctTransform = leftHandTransform;
        }
        _correctTransform.RegisterCallback(OnHandRootTransformChanged);

        OnHandRootTransformChanged();
    }

    private void OnDisable()
    {
        if (_gestureRecorderEvents != null)
        {
            _gestureRecorderEvents.OnStartRecordingDemonstrationGesture -= BeginRecording;
            _gestureRecorderEvents.OnDoneRecordingDemonstrationGesture -= StopRecording;
            _gestureRecorderEvents.OnGestureSelected -= UpdateTransforms;
            _correctTransform.UnregisterCallback(OnHandRootTransformChanged);
        }

    }

    private Vector3 GetCenterPosition(Transform t)
    {
        var children = t.GetComponentsInChildren<Transform>();
        var center = children.Aggregate(Vector3.zero, (current, t) => current + t.position);
        return center / (children.Length);
    }
}
