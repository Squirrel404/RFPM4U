using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine
{
    /// <summary>
    /// An add-on module for Cm Camera that adds a final offset to the camera
    /// </summary>
    [AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Rotation Offset")]
    [ExecuteAlways]
    [SaveDuringPlay]
    public class CinemachineRotationOffset : CinemachineExtension
    {
        /// <summary>
        /// A variable you can use to differentiate multiple rotation offsets
        /// </summary>
        [Tooltip("A variable you can use to differentiate multiple rotation offsets")]
        public new string tag = "Head bob";

        /// <summary>
        /// Offset the camera's rotation by this much (camera space)
        /// </summary>
        [Tooltip("Offset the camera's rotation by this much (camera space)")]
        [FormerlySerializedAs("m_Offset")]
        public Vector3 Offset = Vector3.zero;

        /// <summary>
        /// When to apply the offset
        /// </summary>
        [Tooltip("When to apply the offset")]
        [FormerlySerializedAs("m_ApplyAfter")]
        public CinemachineCore.Stage ApplyAfter = CinemachineCore.Stage.Aim;

        /// <summary>
        /// The camera's rotation before the offset was applied
        /// </summary>
        [Tooltip("The camera's rotation before the offset was applied")]
        public Quaternion rotationBeforeOffset;

        private void Reset()
        {
            Offset = Vector3.zero;
            ApplyAfter = CinemachineCore.Stage.Aim;
        }

        /// <summary>
        /// Applies the specified offset to the camera state
        /// </summary>
        /// <param name="vcam">The virtual camera being processed</param>
        /// <param name="stage">The current pipeline stage</param>
        /// <param name="state">The current virtual camera state</param>
        /// <param name="deltaTime">The current applicable deltaTime</param>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage == ApplyAfter)
            {
                rotationBeforeOffset = transform.rotation;
                Quaternion offset = Quaternion.Euler(Offset);
                state.OrientationCorrection *= offset;
            }
        }
    }
}
