using System;
using UnityEngine;

namespace Anatidae {
    public class HighscoreUI : MonoBehaviour
    {
        [SerializeField] RectTransform highscoreEntryContainer;
        [SerializeField] GameObject highscoreEntryPrefab;
        [SerializeField] RectTransform viewport;
        [SerializeField][Tooltip("Nombre de champs à afficher")] int numHighscoreEntries = 10;
        [SerializeField][Tooltip("Rendre le premier score plus gros")] bool makeFirstBigger = true;
        [SerializeField][Tooltip("Défiler les scores de haut en bas automatiquement")] bool autoscroll = false;

        private event Action mainThreadQueuedCallbacks;

        public void OnEnable()
        {
            if (!HighscoreManager.HasFetchedHighscores)
            {
                HighscoreManager.GetHighscores().ContinueWith(task => {
                    if (task.IsFaulted)
                        Debug.LogError(task.Exception);
                    else
                        mainThreadQueuedCallbacks += UpdateHighscoreEntries;
                });
            } else mainThreadQueuedCallbacks += UpdateHighscoreEntries;
        }

        void Update()
        {
            if (mainThreadQueuedCallbacks != null)
            {
                mainThreadQueuedCallbacks.Invoke();
                mainThreadQueuedCallbacks = null;
            }

            if (autoscroll)
            {
                Vector3 scrollPosition = highscoreEntryContainer.localPosition;
                scrollPosition.y = Mathf.Lerp(
                    0,
                    highscoreEntryContainer.sizeDelta.y - viewport.sizeDelta.y,
                    Mathf.Clamp01(Mathf.PingPong(Time.time/5f, 1.5f)-0.25f)
                );
                highscoreEntryContainer.localPosition = scrollPosition;
            }
        }


        void UpdateHighscoreEntries()
        {
            float prefabHeight = highscoreEntryPrefab.GetComponent<RectTransform>().sizeDelta.y;
            foreach (Transform child in highscoreEntryContainer.transform)
            {
                Destroy(child.gameObject);
            }

            int i = 0;
            foreach (var pair in HighscoreManager.Highscores)
            {
                GameObject entry = Instantiate(highscoreEntryPrefab, highscoreEntryContainer);
                entry.transform.localPosition = new Vector3(
                    0f,
                    -i * prefabHeight + 10f,
                    0f
                );
                HighscoreEntry highscoreEntry = entry.GetComponent<HighscoreEntry>();
                highscoreEntry.SetData(pair.Key, pair.Value);
                if (makeFirstBigger && i == 0)
                    highscoreEntry.SetScale(1.3f);
                i++;
                if (i >= numHighscoreEntries)
                    break;
            }

            highscoreEntryContainer.sizeDelta = new Vector2(
                highscoreEntryContainer.sizeDelta.x,
                i * 50
            );
        }
    }
}
