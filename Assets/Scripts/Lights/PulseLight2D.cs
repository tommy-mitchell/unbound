using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Sirenix.OdinInspector;

public class PulseLight2D : MonoBehaviour
{
    private struct LightIntensityPair {
        public Light2D        component { get; }
        public float   falloffIntensity { get; }

        public LightIntensityPair(Light2D _light)
        {
            component        = _light;
            falloffIntensity = _light.shapeLightFalloffSize;
        }
    }

    [SerializeField, Tooltip("Mininimum and maximum intensity values.")]
    private Vector2 range = new Vector2(.55f, .95f);

    [SerializeField, Tooltip("Change intensity with discrete or continuous steps.")]
    private bool useDiscreteSteps = false;

        private float discreteTimer;

    [SerializeField, HideIf(nameof(useAnimator))]
    private float pulseSpeed = 0f;

    [SerializeField, Tooltip("Pulse lights based on animation frequency.")]
    private bool useAnimator = true;

    private Animator _animator;

    [SerializeField, Tooltip("Pulse all lights in children, or explicitly choose in-editor.")]
    private bool useAllChildLights = true;

    [SerializeField, HideIf(nameof(useAllChildLights))]
    private List<Light2D> _lightObjects;
    
    private List<LightIntensityPair> lights;
    
    private float Speed { get => useAnimator ? _animator.GetCurrentAnimatorStateInfo(0).length / 2 : pulseSpeed; }

    private static FieldInfo m_FalloffField =  typeof( Light2D ).GetField( "m_ShapeLightFalloffSize", BindingFlags.NonPublic | BindingFlags.Instance );

    private void Start()
    {
        if(useAnimator)
            _animator = GetComponent<Animator>();

        if(useAllChildLights)
            _lightObjects = GetComponentsInChildren<Light2D>().ToList();

        if(useDiscreteSteps)
            discreteTimer = Speed;

        lights = new List<LightIntensityPair>();
        
        List<Light2D> lightList = useAllChildLights ? GetComponentsInChildren<Light2D>().ToList() : _lightObjects;
        
        foreach(Light2D light in lightList)
            lights.Add(new LightIntensityPair(light));
    }

    private void Update()
    {
        //if(useAnimator)
            //frequency = _animator.GetCurrentAnimatorStateInfo(0).speed;

        bool canStep = !useDiscreteSteps || ( useDiscreteSteps &&  discreteTimer <= 0 );

        if(canStep)
        {
            /*foreach(LightIntensityPair light in lights)
                light.component.intensity = GetIntensityChange(light.component.intensity, light.initialIntensity);*/

            foreach(LightIntensityPair light in lights)
            {
                if(useDiscreteSteps)
                {
                    bool isMovingForward = light.component.intensity == range.x;

                    light.component.intensity = isMovingForward ? range.y : range.x;
                    m_FalloffField.SetValue(light.component, light.component.shapeLightFalloffSize + ( isMovingForward ? 1 : -1 ) * ( range.y - range.x ));
                }
                else
                {
                    float lerpValue = Mathf.PingPong(Time.time, 1);

                    light.component.intensity = Mathf.Lerp(range.x, range.y, lerpValue);//Mathf.Lerp(range.x, range.y, Mathf.PingPong(Time.time, 1));// * Speed;

                    float falloffRange = range.y - range.x;
                    float falloffValue = Mathf.Lerp(light.falloffIntensity, light.falloffIntensity + falloffRange, 
                        lerpValue);

                    m_FalloffField.SetValue(light.component, falloffValue);
                }
            }

            if(useDiscreteSteps)
                discreteTimer = Speed;
        }

        discreteTimer -= Time.deltaTime;
    }
}
