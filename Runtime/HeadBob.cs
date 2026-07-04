using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController controller;
    [SerializeField] CinemachineRotationOffset cameraRotationOffset;

    [Header("Position")]
    [SerializeField] Vector3 offset;
    [ShowNonSerializedField] Vector3 startLocalPositionOffset;
    Vector3 Offset { get { return offset + startLocalPositionOffset; } }
    [SerializeField] float amplitude = 0.015f;
    [SerializeField] Vector2 amplitudeHV = new Vector2(1, 1);

    [SerializeField] AnimationCurve horizontal;
    [SerializeField] AnimationCurve vertical;

    [Header("Rotation")]
    [SerializeField] float rotationAmount = 8;
    [SerializeField] float zRotationMult = 0.5f;
    [SerializeField] float rotationSmooth = 99;

    [Header("Return")]
    [SerializeField] float returnSpeed = 3;

    float returnT;
    Vector3 returnStartPos;

    Vector3 targetRot;

    void Awake()
    {
        startLocalPositionOffset = transform.localPosition;
    }

    void Update()
    {
        if (!controller.Moving)
        {
            returnT += Time.deltaTime * returnSpeed;

            transform.localPosition = Vector3.Lerp(returnStartPos, Offset, returnT);
            cameraRotationOffset.Offset = Vector3.Slerp(targetRot, Vector3.zero, returnT);

            return;
        }

        returnT = 0;

        Bob();

        returnStartPos = transform.localPosition;
    }

    void Bob()
    {
        float t = controller.walkPhase;
        float mult = amplitude * (controller.currentMoveSpeed / controller.walkSpeed);

        float x = horizontal.Evaluate(t) * mult * (controller.leftStep ? -1 : 1) * amplitudeHV.x;
        float y = vertical.Evaluate(t) * mult * amplitudeHV.y;

        transform.localPosition = new Vector3(0, y) + Camera.main.transform.right * x + Offset;

        float yRot = -x * rotationAmount;
        float xRot = y * rotationAmount;
        float zRot = -x * rotationAmount * 0.5f * zRotationMult;

        Vector3 orientation = Camera.main.transform.forward;
        targetRot = new Vector3(xRot * orientation.x, yRot * orientation.y, zRot * orientation.z);

        cameraRotationOffset.Offset = Vector3.Slerp(cameraRotationOffset.Offset, targetRot, Time.deltaTime * rotationSmooth);
    }

#if UNITY_EDITOR
    void Reset()
    {
        controller = transform.parent.GetComponent<PlayerController>();
        cameraRotationOffset = GameObject.Find("First Person Camera").GetComponent<CinemachineRotationOffset>();
        startLocalPositionOffset = transform.localPosition;
        SetCurvesToDefault();
    }

    void SetCurvesToDefault()
    {
        Keyframe[] keys = new Keyframe[3]
        {
            new Keyframe(0, 0),
            new Keyframe(0.5f, 1),
            new Keyframe(1, 0)
        };
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].weightedMode = WeightedMode.None;
        }
        horizontal = new AnimationCurve(keys);
        vertical = new AnimationCurve(keys);
    }
#endif
}
