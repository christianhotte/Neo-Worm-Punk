using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    public UnityEvent OnTriggerEnterEvent;

    [SerializeField] private bool destroyOnEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnTriggerEnterEvent.Invoke();
            if (destroyOnEnter)
                Destroy(gameObject);
        }
    }
}
