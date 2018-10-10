using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UI
{
    /// <summary>
    /// BurnoffEffect
    /// </summary>
    [AddComponentMenu( "UI/BurnoffEffect" )]
    public class BurnoffEffect : MonoBehaviour
    {
        [SerializeField]
        private Material BurnoffShader;

        [SerializeField]
        private Color TintColor = Color.white;

        [SerializeField]
        private Color BoarderColor = Color.red;

        [SerializeField]
        private float EffectLength = 0.75f;

        [Serializable]
        public class BurnoffCompleteEvent : UnityEvent { };
        public BurnoffCompleteEvent onBurnComplete = new BurnoffCompleteEvent();

        private Material ShaderInstance;
        private Coroutine EffectRoutine;
        private UnityAction BurnCompleteCallback;
        private List<GameObject> DisabledChildren = new List<GameObject>();

        private void Start()
        {
            BeginEffect();
        }

        public void BeginEffect( UnityAction callbackFunction = null )
        {
            if( EffectRoutine == null )
            {
                ShaderInstance = this.gameObject.AddComponent<Image>().material = Instantiate( BurnoffShader );

                ShaderInstance.SetColor( "_Tint", TintColor );
                ShaderInstance.SetColor( "_Boarder", BoarderColor );

                BurnCompleteCallback = callbackFunction;

                EffectRoutine = StartCoroutine( TickEffect() );
            }
        }

        private IEnumerator TickEffect()
        {
            yield return null; // ensure that any children have been cleaned up from hierarchy before caching
            var children = this.gameObject.transform.Cast<Transform>().Select( obj => obj.gameObject );
            foreach( var child in children )
            {
                if( child.activeInHierarchy )
                {
                    child.SetActive( false );
                    DisabledChildren.Add( child );
                }
            }

            var framerate = ( ( Application.targetFrameRate != -1 ) ? Application.targetFrameRate : 60f );
            var burnPerSecond = EffectLength / framerate;
            var burnRate = burnPerSecond / EffectLength;
            var tickRate = new WaitForSeconds( burnPerSecond );

            float cutoff;
            while( ( cutoff = ShaderInstance.GetFloat( "_Cutoff" ) ) < 1f )
            {
                ShaderInstance.SetFloat( "_Cutoff", cutoff + burnRate );

                yield return tickRate;
            }

            ShaderInstance.SetFloat( "_Cutoff", 0f );

            Destroy( this.gameObject.GetComponent<Image>() );

            foreach( var child in DisabledChildren )
                child.SetActive( true );

            DisabledChildren.Clear();

            if( BurnCompleteCallback != null )
                BurnCompleteCallback();

            EffectRoutine = null;

            onBurnComplete.Invoke();
        }
    }
}
