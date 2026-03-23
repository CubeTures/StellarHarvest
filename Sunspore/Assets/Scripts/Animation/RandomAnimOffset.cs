using UnityEngine;

[RequireComponent(typeof(Animation))]
class RandomAnimOffset: MonoBehaviour
{
	void Awake()
	{
		var anim = GetComponent<Animation>();
		foreach (AnimationState state in anim)
		{
			state.time = Random.Range(0f, state.length);
			state.speed = Random.Range(0.8f, 1.2f);
			anim.Play(state.name);
		}
	}
}