using System;
using Game.Gameplay.Grid.Runtime;
using UnityEngine;

namespace Game.Gameplay.Building.Presentation {
    [DisallowMultipleComponent]
    public sealed class BuildRaycastController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GridService gridService;

        [Header("Raycast")]
        [SerializeField] private LayerMask placementMask = ~0;
    }
}