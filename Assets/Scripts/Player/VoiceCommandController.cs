using UnityEngine;
using UnityEngine.Windows.Speech; // For Windows Speech Recognition
using System.Collections.Generic;
using System.Linq;

public class VoiceCommandController : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    void Start()
    {
        // Define voice commands and their actions
        keywords.Add("jump", () => 
        {
            Debug.Log("Player jumped!");
            // Example: Make a player object jump
            GameObject player = GameObject.Find("Player");
            if (player != null)
                player.GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        });

        keywords.Add("move forward", () => 
        {
            Debug.Log("Moving forward!");
            // Example: Move player forward
            GameObject player = GameObject.Find("Player");
            if (player != null)
                player.transform.Translate(Vector3.forward * 2f);
        });

        keywords.Add("stop", () => 
        {
            Debug.Log("Stopped!");
            // Example: Stop player movement
            GameObject player = GameObject.Find("Player");
            if (player != null)
                player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        });

        // Initialize the KeywordRecognizer with the defined keywords
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        // When a phrase is recognized, invoke the corresponding action
        if (keywords.ContainsKey(args.text))
        {
            keywords[args.text].Invoke();
        }
    }

    void OnDestroy()
    {
        // Clean up the recognizer when the GameObject is destroyed
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}