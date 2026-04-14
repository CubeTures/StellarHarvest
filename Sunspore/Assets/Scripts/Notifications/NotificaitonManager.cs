using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class NotificationManager : MonoBehaviour
{
	[SerializeField] GameObject notificationPrefab;

	public static NotificationManager Instance
	{
		get; private set;
	}

	private Queue<Notification> timedNotifications = new(), fixedNotifications = new();

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void PushTimedNotification(string content)
	{
		if (TutorialManager.Instance.IsTutorialActive) return;
		timedNotifications.Enqueue(CreateInstance(false, content));
		Rebuild();
	}

	public void PushFixedNotification(string content)
	{
		fixedNotifications.Enqueue(CreateInstance(true, content));
		Rebuild();
	}

	public void PopFixedNotification()
	{
		if (fixedNotifications.Count <= 0) return;
		Notification notif = fixedNotifications.Peek();
		fixedNotifications.Dequeue();
		Destroy(notif.gameObject);
		Rebuild();
	}

	void Update()
	{
		if (timedNotifications.Count > 0 && timedNotifications.Peek().IsExpired())
		{
			Notification notif = timedNotifications.Peek();
			timedNotifications.Dequeue();
			Destroy(notif.gameObject);
			Rebuild();
		}
	}

	Notification CreateInstance(bool isFixed, string content)
	{
		GameObject obj = Instantiate(notificationPrefab, transform);
		Notification notif = obj.GetComponent<Notification>();
		notif.SetContent(isFixed, content);
		return notif;
	}

	void Rebuild()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
	}
}