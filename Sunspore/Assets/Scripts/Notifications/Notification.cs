using TMPro;
using UnityEngine;
using UnityEngine.UI;

class Notification: MonoBehaviour
{
	// handles display, sizing, and time
	[SerializeField] TMP_Text text;
	[SerializeField] Image image;
	[SerializeField] Sprite timedSprite, fixedSprite;

	float time;

	public void SetContent(bool isFixed, string content)
	{
		image.sprite = isFixed ? fixedSprite : timedSprite;
		image.pixelsPerUnitMultiplier = isFixed ? 6 : 1;
		text.text = content;
		time = Time.time;
	}

	public bool IsExpired()
	{
		return time - Time.time >= 5;
	}
}