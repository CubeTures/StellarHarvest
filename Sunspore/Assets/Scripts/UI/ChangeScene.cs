using UnityEngine;
using UnityEngine.SceneManagement;

class ChangeScene : MonoBehaviour
{
	public void OnClick(int scene)
	{
		SceneManager.LoadScene(scene);
	}

	public void Exit()
	{
		Application.Quit();
	}
}