using Pathfinding;
using UnityEngine;

namespace Utility
{
    [RequireComponent(typeof (Seeker)), RequireComponent(typeof (FunnelModifier))]
    public class PathPostProcessor : MonoBehaviour
    {
        static PathPostProcessor _instance;

        [SerializeField] Seeker _seeker;

        public static PathPostProcessor Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<PathPostProcessor>();
                if (_instance == null) throw new UnassignedReferenceException("No PathPostProcessor available");
                return _instance;
            }
        }

        // Use this for initialization
        void Awake()
        {
            if (_seeker == null) _seeker = GetComponent<Seeker>();
            if (_instance != null && _instance != this)
            {
                Debug.LogError("more than one PathPostProcessor active!");
                Destroy(this);
                if (_seeker != null) Destroy(_seeker);
                var fm = GetComponent<FunnelModifier>();
                if (fm != null) Destroy(fm);
                return;
            }
            _instance = this;
        }

        public void Process(Path p)
        {
            if (_seeker == null) return;
            _seeker.PostProcess(p);
        }
    }
}