using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Anatidae {
    public class HighscoreEntry : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text scoreText;

        public void SetData(string name, int score)
        {
            nameText.text = name;
            scoreText.text = score.ToString();
        }

        public void SetScale(float scale)
        {
            nameText.fontSize = (int)(nameText.fontSize * scale);
            scoreText.fontSize = (int)(scoreText.fontSize * scale);
        }
    }
}