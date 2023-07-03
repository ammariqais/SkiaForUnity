using Unity.Profiling;

namespace UnityEngine.U2D.Animation
{
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-1)]
    [ExecuteInEditMode]
    internal class SpriteSkinUpdateHelper : MonoBehaviour
    {
        public System.Action<GameObject> onDestroyingComponent
        {
            get; 
            set;
        }
        
        ProfilerMarker m_ProfilerMarker = new ProfilerMarker("SpriteSkinUpdateHelper.LateUpdate");

        void OnDestroy() => onDestroyingComponent?.Invoke(gameObject);

        void LateUpdate()
        {
#if ENABLE_ANIMATION_BURST && ENABLE_ANIMATION_COLLECTION            
            if (SpriteSkinComposite.instance.helperGameObject != gameObject)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            
            m_ProfilerMarker.Begin();
            SpriteSkinComposite.instance.LateUpdate();
            m_ProfilerMarker.End();
#endif
        }
    }
}
