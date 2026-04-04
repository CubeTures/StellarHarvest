using System.Collections;
using UnityEngine;
using UnityEngine.UI;

class RebuildMe: MonoBehaviour
{
	[SerializeField] RectTransform inner, outer;

	void OnEnable()
	{
		StartCoroutine(RebuildNextFrame());
	}

	IEnumerator RebuildNextFrame()
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(inner);
		LayoutRebuilder.ForceRebuildLayoutImmediate(outer);
	}
}