using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public class Confirmation : MonoBehaviour
    {
        public Animator Animator;
        public void Play(float3 position)
        {
            // Set it active in case it is not
            if (!Animator.gameObject.activeSelf)
                Animator.gameObject.SetActive(true);

            transform.position = position;
            Animator.Play("Confirmation");
        }
    }
}
