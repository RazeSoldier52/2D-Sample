using TMPro;
using UnityEngine;
using System.Collections;

public class Flag : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] TMP_Text messageText;
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private bool isHandlingTrigger=false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isHandlingTrigger)
        {
            isHandlingTrigger=true;
            StartCoroutine(DisplayMessageCoroutine());
        }
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
