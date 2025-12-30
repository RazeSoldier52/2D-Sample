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
    [Header("CheckPoint")]
    [SerializeField] int spawnPointPriority;
    [SerializeField] Transform spawnPointTransform;
    [SerializeField] private bool checkpointTriggered;
    [Header("Flag")]
    [SerializeField] GameObject flag;
    [SerializeField] private Color activeFlagColor = Color.green;
    [SerializeField] private Color inactiveFlagColor = Color.white;
    void Start()
    {
        flag.GetComponent<SpriteRenderer>().color = inactiveFlagColor;
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
        if (checkpointTriggered) return; 
        if(other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.ModifySpawnPoint(spawnPointTransform.position, spawnPointPriority);
            checkpointTriggered = true;
        }
    }
    public void UpdateColor(int currentSpawnPointPriority)
    {
        if(currentSpawnPointPriority==spawnPointPriority)
        {
            flag.GetComponent<SpriteRenderer>().color = activeFlagColor;
        }
        else
            flag.GetComponent<SpriteRenderer>().color = inactiveFlagColor;
    }
    private void OnEnable()
    {
        PlayerController.onPlayerChangedCheckpoint += UpdateColor;
    }
    private void OnDisable()
    {
        PlayerController.onPlayerChangedCheckpoint -= UpdateColor;
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
