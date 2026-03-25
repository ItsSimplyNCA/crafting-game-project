using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Game.Bootstrap {
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour {
        public static GameBootstrap Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SceneReferences sceneReferences;

        [Header("Boot Policy")]
        [SerializeField] private bool resolveReferencesOnAwake = true;
        [SerializeField] private bool failFastOnMissingCoreReferences = true;
        [SerializeField] private bool resetLegacyUIStateOnBoot = true;
        [SerializeField] private bool lockCursorOnBoot = true;
        [SerializeField] private bool keepAliveAcrossScenes = false;
        [SerializeField] private bool verboseLogging = true;

        private readonly Dictionary<Type, UnityEngine.Object> registry = new();

        public bool IsInitialized { get; private set; }
        public SceneReferences Scene => sceneReferences;

        public event Action<GameBootstrap> Bootstrapped;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Debug.LogError("GameBootstrap: több bootstrap példány van a scene-ben. Az új példány törlődik.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (keepAliveAcrossScenes) {
                DontDestroyOnLoad(gameObject);
            }

            Initialize();
        }

        private void OnDestroy() {
            if (Instance == this) Instance = null;

            registry.Clear();
            IsInitialized = false;
        }

        [ContextMenu("Initialize")]
        public void Initialize() {
            if (IsInitialized) return;

            CacheSceneReferences();

            if (sceneReferences == null) {
                Fail("GameBootstrap: nincs SceneReferences komponens a scene-ben.");
                return;
            }

            if (resolveReferencesOnAwake) sceneReferences.ResolveMissingReferences();

            if (!sceneReferences.ValidateRequiredCore(out string validationError)) {
                Fail(validationError);
                return;
            }

            BuildRegistry();
            ApplyInitialPolicies();

            IsInitialized = true;

            if (verboseLogging) LogBootstrapState();

            Bootstrapped?.Invoke(this);
        }

        [ContextMenu("Rebuild Registry")]
        public void RebuildRegistry() {
            CacheSceneReferences();

            if (sceneReferences == null) {
                Debug.LogError("GameBootstrap: nem lehet registry-t építeni, mert a SceneReferences hiányzik.", this);
                return;
            }

            BuildRegistry();
            Debug.Log("GameBootstrap: registry újraépítve.", this);
        }

        [ContextMenu("Log Bootstrap State")]
        public void LogBootstrapState() {
            Debug.Log(
                "=== GameBootstrap ===\n" +
                $"Initialized: {IsInitialized}\n" +
                $"SceneReferences: {GetName(sceneReferences)}\n" +
                $"Registered Services: {registry.Count}\n" +
                $"WorldGrid: {GetName(sceneReferences != null ? sceneReferences.WorldGrid : null)}\n" +
                $"BuildingSystem: {GetName(sceneReferences != null ? sceneReferences.BuildingSystem : null)}\n" +
                $"InventorySystem: {GetName(sceneReferences != null ? sceneReferences.InventorySystem : null)}\n" +
                $"MainCamera: {GetName(sceneReferences != null ? sceneReferences.MainCamera : null)}\n" +
                $"PlayerController: {GetName(sceneReferences != null ? sceneReferences.PlayerController : null)}\n" +
                $"CrafterPlayerInteractor: {GetName(sceneReferences != null ? sceneReferences.CrafterPlayerInteractor : null)}\n" +
                $"CrafterUI: {GetName(sceneReferences != null ? sceneReferences.CrafterUI : null)}",
                this
            );
        }

        public static T Get<T>() where T : UnityEngine.Object {
            if (Instance == null) {
                throw new InvalidOperationException(
                    $"GameBootstrap.Get<{typeof(T).Name}> hívás sikertelen: nincs aktív GameBootstrap."
                );
            }

            return Instance.Resolve<T>();
        }

        public T Resolve<T>() where T : UnityEngine.Object {
            if (TryResolve(out T service)) return service;

            throw new InvalidOperationException(
                $"GameBootstrap: nincs regisztrálva ilyen service: {typeof(T).Name}"
            );
        }

        public bool TryResolve<T>(out T service) where T : UnityEngine.Object {
            if (registry.TryGetValue(typeof(T), out UnityEngine.Object obj) && obj is T typed) {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        private void CacheSceneReferences() {
            if (sceneReferences != null) return;

            sceneReferences = GetComponent<SceneReferences>();

            if (sceneReferences == null) {
                sceneReferences = FindObjectOfType<SceneReferences>();
            }
        }

        private void BuildRegistry() {
            registry.Clear();

            Register(sceneReferences);
            Register(sceneReferences.WorldGrid);
            Register(sceneReferences.BuildingSystem);
            Register(sceneReferences.InventorySystem);
            Register(sceneReferences.MainCamera);
            Register(sceneReferences.PlayerController);
            Register(sceneReferences.CrafterPlayerInteractor);
            Register(sceneReferences.CrafterUI);
        }

        private void ApplyInitialPolicies() {
            if (resetLegacyUIStateOnBoot) {
                if (sceneReferences.InventorySystem != null) {
                    sceneReferences.InventorySystem.Close();
                }

                if (sceneReferences.CrafterUI != null) {
                    sceneReferences.CrafterUI.Hide();
                }
            }

            if (lockCursorOnBoot) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Register<T>(T service) where T : UnityEngine.Object {
            if (service == null) return;
            registry[typeof(T)] = service;
        }

        private void Fail(string message) {
            Debug.Log(message, this);

            if (!failFastOnMissingCoreReferences) return;

            enabled = false;
            throw new InvalidOperationException(message);
        }

        private static string GetName(UnityEngine.Object obj) {
            return obj == null ? "NULL" : obj.name;
        }
    }
}
