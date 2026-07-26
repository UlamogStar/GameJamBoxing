using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Plays an imported Maya animation and uses the camera imported with that scene.
/// Assign the root GameObject of the imported FBX, its camera, and the animation
/// clip in the Inspector after placing the FBX in the scene.
/// </summary>
public class MayaSequenceController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera mayaCamera;
    [SerializeField] private GameObject mayaSequenceRoot;
    [SerializeField] private AnimationClip mayaAnimation;

    private PlayableGraph animationGraph;
    private Animator animationTarget;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        ReturnToMainCamera();
    }

    /// <summary>Called by the Start button.</summary>
    public void PlayMayaSequence()
    {
        if (mayaCamera == null || mayaAnimation == null || mayaSequenceRoot == null)
        {
            Debug.LogWarning("Maya sequence is not configured. Assign the Maya Camera, the BoxerAnim scene root, and the animation clip imported from BoxerAnim.ma on Main Camera.", this);
            return;
        }

        if (mainCamera != null)
            mainCamera.enabled = false;

        mayaCamera.enabled = true;
        PlayAnimation();
    }

    /// <summary>Called by GameOverKO before the game-over sequence begins.</summary>
    public void ReturnToMainCamera()
    {
        StopAnimation();

        if (mayaCamera != null)
            mayaCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;
    }

    private void PlayAnimation()
    {
        StopAnimation();

        animationGraph = PlayableGraph.Create("Maya Sequence");
        animationTarget = GetAnimationTarget();
        animationTarget.enabled = true;
        animationTarget.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animationTarget.Rebind();

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(animationGraph, animationTarget.name, animationTarget);
        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(animationGraph, mayaAnimation);
        output.SetSourcePlayable(clipPlayable);
        animationGraph.Play();
    }

    private Animator GetAnimationTarget()
    {
        if (mayaSequenceRoot == null)
            return null;

        // The imported clip contains paths for the complete Maya hierarchy. It must
        // therefore be evaluated from the root, rather than once per boxer rig.
        Animator rootAnimator = mayaSequenceRoot.GetComponent<Animator>();
        if (rootAnimator != null)
            return rootAnimator;

        // Generic Maya scene clips can animate the complete imported hierarchy
        // without an Animator being included on the root by the importer.
        return mayaSequenceRoot.AddComponent<Animator>();
    }

    private void StopAnimation()
    {
        if (animationGraph.IsValid())
            animationGraph.Destroy();

        if (animationTarget != null)
        {
            animationTarget.Rebind();
            animationTarget.Update(0f);
            animationTarget = null;
        }
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}
