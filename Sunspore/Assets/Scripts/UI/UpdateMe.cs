using UnityEngine;

class UpdateMe : MonoBehaviour
{
	public static UpdateMe Instance
	{
		get; private set;
	}

	public delegate void _Update();
	private _Update onUpdate;

	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		if (onUpdate == null) return;
		foreach (_Update _update in onUpdate.GetInvocationList())
		{
			_update?.Invoke();
		}
	}

	public void Subscribe(_Update _update)
	{
		onUpdate += _update;
	}

	public void Unsubscribe(_Update _update)
	{
		onUpdate -= _update;
	}
}