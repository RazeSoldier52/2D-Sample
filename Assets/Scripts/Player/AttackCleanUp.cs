using Mono.Cecil;
using UnityEngine;

public class AttackCleanUp : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player != null)
        {
            player.DeactivateHitbox();
        }

    }
}
