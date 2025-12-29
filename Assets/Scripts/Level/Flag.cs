using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Flag : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] TMP_Text messageText;
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private bool isHandlingTrigger=false;
    [SerializeField] int spawnPointPriority;
    [SerializeField] Transform spawnPointTransform;
    [SerializeField] public UnityEvent<Vector3, int> onPlayerPassedCheckpoint;
    void Start()
    {
        if (messageText!=null)
        {
            messageText.gameObject.SetActive(true);
            messageText.gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHandlingTrigger)
        {
            isHandlingTrigger=true;
            StartCoroutine(DisplayMessageCoroutine());
        }
        onPlayerPassedCheckpoint?.Invoke(spawnPointTransform.position,spawnPointPriority);    
    }
    private IEnumerator DisplayMessageCoroutine()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(displayDuration);
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
        isHandlingTrigger=false;
    }
}
