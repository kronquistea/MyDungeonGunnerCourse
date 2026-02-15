using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineTargetGroup))]

public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup cinemachineTargetGroup;

    private void Awake()
    {
        // Load components
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        SetCinemachineTargetGroup();
    }

    /// <summary>
    /// Set the cinemachine camera target group
    /// </summary>
    private void SetCinemachineTargetGroup()
    {
        // Create target group for cinemachine for the cinemachine camera to follow
        CinemachineTargetGroup.Target cinemachineGroupTarget_player = new CinemachineTargetGroup.Target
        {
            weight = 1f,
            radius = 1f,
            target = GameManager.Instance.GetPlayer().transform
        };

        // Array to hold all cinemachine target group targets
        // Cinemachine will follow both the player and the aiming cursor, that is the point of the array
        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[]
        {
            cinemachineGroupTarget_player
        };

        // m_Targets is the same as the list of targets for the CinemachineTargetGroup in Unity
        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }
}
