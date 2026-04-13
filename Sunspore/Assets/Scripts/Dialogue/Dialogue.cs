using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sunspore
{
    public class Dialogue : MonoBehaviour
    {
        string characterName;
        private string[] messageList; // list of dialogue collected from the csv
        private string[] characterNameList; // per-line speaker names collected from the csv
        private int currentMessageIndex = 0;
        public Sprite SamSprite;
        public Sprite TheManSprite;
        public Image characterImage;
        public TMP_Text characterNameText;
        public TMP_Text messageText;
        public float speed = 0.01f;

        // CSV configuration
        public TextAsset csvFile; // Assign a CSV in the Inspector
        public bool csvHasHeader = true;
        int nameColumnIndex = 0;
        int poseColumnIndex = 2; // Column index for poses (usually the second column)
        public string dialogueTypeToUse = "TUTORIAL"; // Which dialogue type to use from the CSV
        public bool isEvent = false;

        private bool dialogueActive = false;
        private bool isTyping = false; // Track if typewriter effect is currently running
        private Coroutine typewriterCoroutine = null; // Reference to the current typewriter coroutine
        public AudioClip dialogueSFX;
        Animator _animator;
        AudioSource _audioSource;
        private bool _exitTriggered = false;

        void Awake()
        {
            _animator = GetComponent<Animator>();

            _audioSource = GetComponent<AudioSource>();

            if (_animator == null)
            {
                Debug.LogError("Animator not found on character!");
            }

            // Play tutorial dialogue if no steps have been completed (first time playing the game)
            if ( /*PlayerPrefManager.GetEventCount() == 0*/
                true
            )
            {
                _animator.SetTrigger("Enter");
                dialogueTypeToUse = "TUTORIAL";
            }
            else
            {
                dialogueTypeToUse = "";
            }

            // If a CSV is provided, load dialogue from it
            if (csvFile != null)
            {
                LoadDialogueFromCsv(csvFile, csvHasHeader, nameColumnIndex, dialogueTypeToUse);
            }
            else
            {
                // Fallback: try to load Resources/Dialogue.csv if present
                var csvFromResources = Resources.Load<TextAsset>("Dialogue");
                if (csvFromResources != null)
                {
                    LoadDialogueFromCsv(
                        csvFromResources,
                        csvHasHeader,
                        nameColumnIndex,
                        dialogueTypeToUse
                    );
                }
            }

            StartDialogue();
        }

        public void ResetDialogue()
        {
            currentMessageIndex = 0;
            dialogueActive = false;
            isTyping = false;
            typewriterCoroutine = null;
            StopAllCoroutines(); // Stop any running typewriter effects
            messageText.text = ""; // Clear the text
            characterNameText.text = ""; // Clear the character name
            characterImage.sprite = null;
        }

        void SetSpeakerUI(int messageIndex)
        {
            if (characterNameText == null || characterImage == null)
            {
                return;
            }

            if (
                characterNameList == null
                || messageIndex < 0
                || messageIndex >= characterNameList.Length
            )
            {
                return;
            }

            string speaker = characterNameList[messageIndex]?.Trim();
            if (string.IsNullOrWhiteSpace(speaker))
            {
                characterNameText.text = "";
                characterImage.sprite = null;
                return;
            }

            // Update visible speaker name
            characterNameText.text = speaker;

            // Map speaker name to the configured portrait sprite
            if (speaker.Equals("SAM", System.StringComparison.OrdinalIgnoreCase))
            {
                characterImage.sprite = SamSprite;
            }
            else if (speaker.Equals("THE MAN", System.StringComparison.OrdinalIgnoreCase))
            {
                characterImage.sprite = TheManSprite;
            }
            else
            {
                characterImage.sprite = null;
            }
        }

        public void SetDialogueType(string newDialogueType)
        {
            dialogueTypeToUse = newDialogueType;
            if (csvFile != null)
            {
                LoadDialogueFromCsv(csvFile, csvHasHeader, nameColumnIndex, dialogueTypeToUse);
                ResetDialogue();
            }
        }

        public void StartDialogue()
        {
            if (messageList == null || messageList.Length == 0)
            {
                return;
            }

            // Check if the first message is "EVENT"
            string firstMessage = messageList[currentMessageIndex].Trim().ToUpper();
            // Debug.Log($"First message: '{firstMessage}'");
            if (firstMessage == "EVENT")
            {
                isEvent = true;
                Debug.Log("EVENT detected at start - pausing dialogue");
                // messageText.text = ""; // Clear the text so "EVENT" doesn't show
                // Debug.Log("EVENT detected at start - pausing dialogue");
                // Don't start TypeWriterEffect, wait for CompleteEvent() to be called
            }
            else
            {
                SetSpeakerUI(currentMessageIndex);
                typewriterCoroutine = StartCoroutine(TypeWriterEffect()); // Start with typewriter effect
            }

            dialogueActive = true;
        }

        // Update is called once per frame
        void Update()
        {
            // Press any key to advance the dialogue
            if (Input.GetMouseButtonDown(0) && dialogueActive && !isEvent)
            {
                // If currently typing, skip to end of current text
                if (isTyping)
                {
                    SkipTypewriter();
                    return;
                }

                // Otherwise, advance to next message
                currentMessageIndex++;
                if (currentMessageIndex < messageList.Length)
                {
                    // Check if the next message is "EVENT" before proceeding
                    string nextMessage = messageList[currentMessageIndex].Trim().ToUpper();
                    // Debug.Log($"Next message: '{nextMessage}' (index {currentMessageIndex})");
                    if (nextMessage == "END")
                    {
                        // End of tutorial
                        if (DialogueManager._dm != null && DialogueManager._dm.GetEventCount() == 1)
                        {
                            if (!_exitTriggered)
                            {
                                _exitTriggered = true;
                                _animator.SetTrigger("Exit");
                                Debug.Log("End of tutorial");
                            }
                        }
                    }
                    else if (nextMessage == "EVENT")
                    {
                        isEvent = true;
                        Debug.Log("EVENT detected - pausing dialogue");
                    }
                    else
                    {
                        SetSpeakerUI(currentMessageIndex);
                        typewriterCoroutine = StartCoroutine(TypeWriterEffect());
                    }
                }
                else
                {
                    if (!_exitTriggered)
                    {
                        _exitTriggered = true;
                        _animator.SetTrigger("Exit");
                        Debug.Log("End of tutorial");
                    }
                }
            }
        }

        IEnumerator TypeWriterEffect()
        {
            isTyping = true;
            string full = messageList[currentMessageIndex];

            int[] clusterIndexes = StringInfo.ParseCombiningCharacters(full);
            for (int i = 0; i < clusterIndexes.Length; i++)
            {
                int nextIndex =
                    (i + 1 < clusterIndexes.Length) ? clusterIndexes[i + 1] : full.Length;
                messageText.text = full.Substring(0, nextIndex);

                if (i % 4 == 0)
                {
                    // Change pitch randomly
                    if (characterNameText.text == "SAM")
                    {
                        _audioSource.pitch = UnityEngine.Random.Range(0.7f, 0.8f);
                    }
                    else
                    {
                        _audioSource.pitch = UnityEngine.Random.Range(0.4f, 0.5f);
                    }
                    PlaySound(dialogueSFX);
                }

                yield return new WaitForSeconds(speed);
            }

            // Ensure full text is displayed and mark as done typing
            messageText.text = full;
            isTyping = false;
        }

        void SkipTypewriter()
        {
            // Stop the current typewriter coroutine
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            // Immediately display the full text
            if (currentMessageIndex < messageList.Length)
            {
                messageText.text = messageList[currentMessageIndex];
            }

            isTyping = false;
        }

        // Call this method from external scripts when the event is complete
        public bool CompleteEvent()
        {
            if (!isEvent)
            {
                Debug.LogWarning("CompleteEvent called but no event is active!");
                return false;
            }

            // Debug.Log("CompleteEvent called - resuming dialogue");
            isEvent = false;
            Debug.Log("EVENT completed - resuming dialogue");
            currentMessageIndex++;

            if (currentMessageIndex < messageList.Length)
            {
                // Check if the next message is also "EVENT"
                string nextMessage = messageList[currentMessageIndex].Trim().ToUpper();
                // Debug.Log($"After event, next message: '{nextMessage}' (index {currentMessageIndex})");

                if (nextMessage == "EVENT")
                {
                    isEvent = true;
                    Debug.Log("Another EVENT detected - pausing dialogue again");
                    // messageText.text = ""; // Clear the text so "EVENT" doesn't show
                    // Debug.Log("Another EVENT detected - pausing dialogue again");
                    // Don't start TypeWriterEffect, wait for CompleteEvent() to be called again
                }
                else
                {
                    SetSpeakerUI(currentMessageIndex);
                    typewriterCoroutine = StartCoroutine(TypeWriterEffect());
                }
            }
            else
            {
                _animator.SetTrigger("Exit");
            }

            Debug.Log("CompleteEvent called - dialogue complete");
            return true;
        }

        public void LoadDialogueFromCsv(
            TextAsset csv,
            bool hasHeader,
            int nameCol,
            string dialogueType
        )
        {
            if (csv == null)
            {
                return;
            }

            List<string> messages = new List<string>();
            List<string> poses = new List<string>();
            List<string> characterNames = new List<string>();

            string[] lines = csv.text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                return;
            }

            // Parse header to find column indices
            List<string> headerFields = new List<string>();
            int startIndex = 0;

            if (hasHeader && lines.Length > 0)
            {
                headerFields = ParseCsvLine(lines[0]);
                startIndex = 1;
            }

            // Find the column index for the specified dialogue type
            int dialogueColumnIndex = -1;
            if (hasHeader && headerFields.Count > 0)
            {
                for (int h = 0; h < headerFields.Count; h++)
                {
                    string header = headerFields[h].Trim();
                    if (
                        string.Equals(header, dialogueType, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(
                            header,
                            dialogueType.Replace("_", " "),
                            StringComparison.OrdinalIgnoreCase
                        )
                        || string.Equals(
                            header,
                            dialogueType.Replace("_", "."),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        dialogueColumnIndex = h;
                        break;
                    }
                }
            }

            if (dialogueColumnIndex == -1)
            {
                Debug.LogWarning(
                    $"Could not find dialogue type '{dialogueType}' in CSV headers. Available headers: {string.Join(", ", headerFields)}"
                );
                return;
            }

            // Speaker name column is expected to be `NAME` in the CSV header.
            // Fall back to the previous behavior (dialogue column - 1) if not found.
            if (hasHeader && headerFields.Count > 0)
            {
                nameColumnIndex = headerFields.FindIndex(h =>
                    string.Equals(h.Trim(), "NAME", StringComparison.OrdinalIgnoreCase)
                );
                if (nameColumnIndex == -1)
                {
                    nameColumnIndex = dialogueColumnIndex - 1;
                }

                // Pose is optional; current CSV doesn't include it.
                // Keep old behavior as fallback if a pose column exists.
                poseColumnIndex = headerFields.FindIndex(h =>
                    string.Equals(h.Trim(), "POSE", StringComparison.OrdinalIgnoreCase)
                );
                if (poseColumnIndex == -1)
                {
                    poseColumnIndex = dialogueColumnIndex + 1;
                }
            }

            // Process data rows
            // The speaker is taken from the `NAME` column; if NAME is blank, we keep the previous speaker.
            string currentCharacterName = "";
            for (int i = startIndex; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                List<string> fields = ParseCsvLine(line);
                if (fields == null || fields.Count == 0)
                {
                    continue;
                }

                // Get dialogue message from the specified dialogue type column
                if (dialogueColumnIndex < fields.Count)
                {
                    string msg = fields[dialogueColumnIndex].Trim();
                    // msg = DecodeUnicodeEscapes(msg);
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        // Get character name from the name column for this row
                        string rowName = "";
                        if (nameColumnIndex >= 0 && nameColumnIndex < fields.Count)
                        {
                            rowName = fields[nameColumnIndex].Trim();
                            if (!string.IsNullOrEmpty(rowName))
                            {
                                currentCharacterName = rowName;
                            }
                        }

                        messages.Add(msg);
                        characterNames.Add(currentCharacterName);
                        // Debug.Log($"Loaded message {messages.Count - 1}: [{currentCharacterName}] '{msg}'");

                        // Get pose from the pose column
                        string pose = "";
                        if (poseColumnIndex < fields.Count)
                        {
                            pose = fields[poseColumnIndex].Trim();
                        }
                        poses.Add(pose);
                    }
                }
            }

            messageList = messages.ToArray();
            characterNameList = characterNames.ToArray();
        }

        private List<string> ParseCsvLine(string line)
        {
            // Minimal CSV parser that supports quoted fields and escaped quotes
            List<string> result = new List<string>();
            if (line == null)
            {
                return result;
            }

            bool inQuotes = false;
            System.Text.StringBuilder fieldBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        bool doubleQuote = i + 1 < line.Length && line[i + 1] == '"';
                        if (doubleQuote)
                        {
                            fieldBuilder.Append('"');
                            i++; // Skip escaped quote
                        }
                        else
                        {
                            inQuotes = false; // End of quoted field
                        }
                    }
                    else
                    {
                        fieldBuilder.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(fieldBuilder.ToString());
                        fieldBuilder.Length = 0;
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        fieldBuilder.Append(c);
                    }
                }
            }
            result.Add(fieldBuilder.ToString());
            return result;
        }

        private string DecodeUnicodeEscapes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            // Convert sequences like \uXXXX and surrogate pairs into actual Unicode characters
            // Also handles other common escapes (\n, \t, etc.)
            return Regex.Unescape(input);
        }

        void PlaySound(AudioClip clip)
        {
            _audioSource.PlayOneShot(clip);
        }

        // Fetch a random dialogue message from a specific column
        public string[] FetchDialogueFromColumn(string columnName)
        {
            if (csvFile == null)
            {
                Debug.LogWarning("No CSV file assigned to Dialogue component!");
                return new string[0];
            }

            List<string> messages = new List<string>();
            string[] lines = csvFile.text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            );

            if (lines.Length == 0)
            {
                return new string[0];
            }

            // Parse header to find column index
            List<string> headerFields = ParseCsvLine(lines[0]);
            int columnIndex = -1;

            for (int h = 0; h < headerFields.Count; h++)
            {
                string header = headerFields[h].Trim();
                if (string.Equals(header, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    columnIndex = h;
                    break;
                }
            }

            if (columnIndex == -1)
            {
                Debug.LogWarning($"Could not find column '{columnName}' in CSV headers.");
                return new string[0];
            }

            // Process data rows
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                List<string> fields = ParseCsvLine(line);
                if (fields == null || fields.Count == 0)
                {
                    continue;
                }

                if (columnIndex < fields.Count)
                {
                    string msg = fields[columnIndex].Trim();
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        messages.Add(msg);
                    }
                }
            }

            return messages.ToArray();
        }

        // Fetch dialogue quip
        public string FetchDialogueQuip(string trigger, string type)
        {
            if (csvFile == null)
            {
                Debug.LogWarning("CSV file is null!");
                return "";
            }

            // Parse all lines from CSV
            string[] lines = csvFile.text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            );
            if (lines.Length == 0)
            {
                Debug.LogWarning("CSV file is empty!");
                return "";
            }

            // Parse header row to find the type column
            List<string> headerFields = ParseCsvLine(lines[0]);

            // Find the "type" column index
            int typeColumnIndex = -1;
            for (int h = 0; h < headerFields.Count; h++)
            {
                string header = headerFields[h].Trim();
                if (string.Equals(header, type, StringComparison.OrdinalIgnoreCase))
                {
                    typeColumnIndex = h;
                    break;
                }
            }

            if (typeColumnIndex == -1)
            {
                Debug.LogWarning(
                    $"Could not find type column '{type}' in CSV headers. Available: {string.Join(", ", headerFields)}"
                );
                return "";
            }

            if (typeColumnIndex == 0)
            {
                Debug.LogWarning(
                    $"Type column '{type}' is the first column, no trigger column to the left."
                );
                return "";
            }

            // Get the trigger column (column to the left of the type column)
            int triggerColumnIndex = typeColumnIndex - 1;

            // Loop through data rows to find the row where the trigger column matches our trigger value
            int rowIndex = -1;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                List<string> rowFields = ParseCsvLine(lines[i]);

                // Check if the trigger column value matches our trigger
                if (triggerColumnIndex < rowFields.Count)
                {
                    string cellValue = rowFields[triggerColumnIndex].Trim();
                    if (string.Equals(cellValue, trigger, StringComparison.OrdinalIgnoreCase))
                    {
                        rowIndex = i;
                        break;
                    }
                }
            }

            if (rowIndex == -1)
            {
                Debug.LogWarning(
                    $"Could not find row with trigger value '{trigger}' in column {triggerColumnIndex} ('{headerFields[triggerColumnIndex]}')"
                );
                return "";
            }

            // Get the dialogue text from the type column at the found row
            List<string> rowData = ParseCsvLine(lines[rowIndex]);

            if (typeColumnIndex < rowData.Count)
            {
                return rowData[typeColumnIndex].Trim();
            }
            else
            {
                Debug.LogWarning(
                    $"Type column index {typeColumnIndex} out of range for row {rowIndex}."
                );
                return "";
            }
        }

        public void PlayDialogue(string dialogueType)
        {
            dialogueTypeToUse = dialogueType;

            _animator.SetTrigger("Enter");

            // If a CSV is provided, load dialogue from it
            if (csvFile != null)
            {
                LoadDialogueFromCsv(csvFile, csvHasHeader, nameColumnIndex, dialogueTypeToUse);
            }
            else
            {
                // Fallback: try to load Resources/Dialogue.csv if present
                var csvFromResources = Resources.Load<TextAsset>("Dialogue");
                if (csvFromResources != null)
                {
                    LoadDialogueFromCsv(
                        csvFromResources,
                        csvHasHeader,
                        nameColumnIndex,
                        dialogueTypeToUse
                    );
                }
            }

            StartDialogue();
        }
    }
}
