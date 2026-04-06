using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

class LoadingScreen : MonoBehaviour
{
	[SerializeField] RectTransform progressOuter, progressInner;

	IEnumerator Start()
	{
		yield return null;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

		while (asyncLoad.progress < 1f)
		{
			progressInner.sizeDelta = new(progressOuter.sizeDelta.x * asyncLoad.progress, progressInner.sizeDelta.y);
			yield return null;
		}
	}
}