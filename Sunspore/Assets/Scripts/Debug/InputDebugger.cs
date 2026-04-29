using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InputDebugger : MonoBehaviour
{
	private readonly List<RaycastResult> _results = new();
	private const int NonClickableLayer = 1; // "Non Clickable"

	private void Update()
	{
		if (!Input.GetMouseButtonDown(0)) return;

		PointerEventData pointer = new(EventSystem.current)
		{
			position = Input.mousePosition
		};

		EventSystem.current.RaycastAll(pointer, _results);

		if (_results.Count == 0)
		{
			// Debug.Log("[InputDebugger] Click hit nothing.");
			return;
		}

		// Debug.Log($"[InputDebugger] Click raycast hit {_results.Count} object(s):");
		foreach (RaycastResult result in _results)
		{
			GameObject go = result.gameObject;
			if (go != null && go.layer == NonClickableLayer)
			{
				continue;
			}
			// Debug.Log($"  → {go.name} | depth: {result.depth} | sortingOrder: {result.sortingOrder} | module: {result.module.GetType().Name}", go);
		}

		// Debug.Log($"[InputDebugger] Top consumer: {_results[0].gameObject.name}", _results[0].gameObject);
	}
}