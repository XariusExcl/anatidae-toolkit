/*
 Le HighscoreManager est un singleton qui permet de récupérer et d'envoyer des highscores à la borne en json, qui est un serveur Node.js.
 La méthode GetHighscores() retourne un Dictionary<string, int> des scores de la borne.
 Il s'occupe également d'écouter l'évènement OnApplicationQuit pour retourner au menu principal de la borne.
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Anatidae {
    public class HighscoreManager : MonoBehaviour
    {
        struct HighscoreData
        {
            public Dictionary<string, int> highscores;
        }

        public static HighscoreManager Instance { get; private set; }
        public static Dictionary<string, int> Highscores { get; private set;}
        public static bool HasFetchedHighscores { get; private set; }
        public static bool IsHighscoreInputScreenShown { get; private set; }
        public static string PlayerName;

        [SerializeField] HighscoreNameInput highscoreNameInput;
        [SerializeField] HighscoreUI highscoreUi;

        void Awake()
        {
            if (Instance == null){
                Instance = this;
            }
            else{
                Destroy(gameObject);
            }

            if (Instance.highscoreNameInput is null)
                Debug.LogError("HighscoreNameInput de HighscoreManager n'est pas défini.");
            else highscoreNameInput.gameObject.SetActive(false);

            if (Instance.highscoreUi is null)
                Debug.LogError("HighscoreUI de HighscoreManager n'est pas défini.");
            else highscoreUi.gameObject.SetActive(false);
        }

        [DllImport("__Internal")]
        public static extern void BackToMenu();

        public static void ShowHighscores()
        {
            if (Instance.highscoreUi is null){
                Debug.LogError("HighscoreUI de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreUi.gameObject.SetActive(true);
        }

        public static void HideHighscores()
        {
            if (Instance.highscoreUi is null){
                Debug.LogError("HighscoreUI de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreUi.gameObject.SetActive(false);
        }
        
        public static void ShowHighscoreInput(int highscore)
        {
            if (Instance.highscoreNameInput is null){
                Debug.LogError("HighscoreNameInput de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreNameInput.ShowHighscoreInput(highscore);
            IsHighscoreInputScreenShown = true;
        }

        public static void DisableHighscoreInput()
        {
            if (Instance.highscoreNameInput is null){
                Debug.LogError("HighscoreNameInput de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreNameInput.gameObject.SetActive(false); 
            IsHighscoreInputScreenShown = false;
        }

        public static async Task<Dictionary<string, int>> GetHighscores()
        {
            HttpClient client = new HttpClient {
                BaseAddress = new Uri("http://localhost:3000/api/?game=" + PlayerSettings.productName)
            };
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync("");

            if (response.IsSuccessStatusCode) {
                var data = await response.Content.ReadAsStringAsync();
                try {
                    HighscoreData highscoreData = JsonConvert.DeserializeObject<HighscoreData>(data);
                    Highscores = highscoreData.highscores;
                    HasFetchedHighscores = true;
                } catch (Exception e) {
                    Debug.LogError(e);
                    return null;
                }
                client.Dispose();
                return Highscores;
            }
            else {
                Debug.LogError($"{(int)response.StatusCode} ({response.ReasonPhrase})");
                client.Dispose();
                return null;
            }
        }

        public static async Task<bool> SetHighscore(string name, int score)
        {
            HttpClient client = new HttpClient {
                BaseAddress = new Uri("http://localhost:3000/api/?game=" + PlayerSettings.productName)
            };
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            var content = new StringContent(JsonConvert.SerializeObject(new { name, score }));
            Debug.Log(content.ReadAsStringAsync().Result); 
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync("", content);

            if (response.IsSuccessStatusCode) {
                return true;
            }
            else {
                Debug.LogError($"{(int)response.StatusCode} ({response.Content.ReadAsStringAsync().Result})");
                client.Dispose();
                return false;
            }
        }

        public static bool IsHighscore(int score)
        {
            return IsHighscore(null, score);
        }
        
        public static bool IsHighscore(string name, int score)
        {
            Debug.Log($"Checking if {name} with score {score} is a highscore.");

            Debug.Log((Highscores.Count < 10)? "Less than 10 scores": $"Lowest score is {Highscores.Values.Skip(9).Take(1).First()}");

            if (name == null)
            {
                if (Highscores == null)
                    return false;
                if (Highscores.Count < 10)
                    return true;
                if (Highscores.Values.Skip(9).Take(1).First() < score)
                    return true;
            } else {
                if (Highscores.ContainsKey(name))
                    if (Highscores[name] < score)
                        return true;
            }
            return false;
        }

        public void OnApplicationQuit()
        {
            BackToMenu();
        }
    }
}

