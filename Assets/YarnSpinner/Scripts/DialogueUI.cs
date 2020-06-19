/*

The MIT License (MIT)

Copyright (c) 2015-2017 Secret Lab Pty. Ltd. and Yarn Spinner contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Yarn.Unity
{

    /// <summary>
    /// Displays dialogue lines to the player, and sends user choices back
    /// to the dialogue system.
    /// </summary>
    /// <remarks>
    /// The DialogueUI component works closely with the <see
    /// cref="DialogueRunner"/> class. It receives <see cref="Line"/>s,
    /// <see cref="OptionSet"/>s and <see cref="Command"/>s from the
    /// DialogueRunner, and conveys them to the rest of the game. It is
    /// also responsible for relaying input from the user to the
    /// DialogueRunner, such as option selection or the signal to proceed
    /// to the next line.
    /// </remarks>
    /// <seealso cref="DialogueRunner"/>
    public class DialogueUI : Yarn.Unity.DialogueUIBehaviour
    {
        public InputAction skipKey;

        public InputAction option1Key;
        public InputAction option2Key;
        public InputAction option3Key;
        public InputAction option4Key;
        public InputAction exitKey;
        public UIStyle styling;

        public static DialogueUI singleton;
        public Text nameText;
        /// <summary>
        /// The object that contains the dialogue and the options.
        /// </summary>
        /// <remarks>
        /// This object will be enabled when conversation starts, and
        /// disabled when it ends.
        /// </remarks>
        public GameObject dialogueContainer;

        /// <summary>
        /// How quickly to show the text, in seconds per character
        /// </summary>
        [Tooltip("How quickly to show the text, in seconds per character")]
        public float textSpeed = 0.025f;

        /// <summary>
        /// The buttons that let the user choose an option.
        /// </summary>
        /// <remarks>
        /// The <see cref="Button"/> objects in this list will be enabled
        /// and disabled by the <see cref="DialogueUI"/>. Each button
        /// should have as a child object a <see cref="Text"/> or a <see
        /// cref="TMPro.TextMeshProUGUI"/> as a label; the text of this
        /// child object will be updated by the DialogueUI as necessary.
        ///
        /// You do not need to configure the On Click action of any of
        /// these buttons. The <see cref="DialogueUI"/> will configure them
        /// for you.
        /// </remarks>
        public List<Button> optionButtons;

        // When true, the user has indicated that they want to proceed to
        // the next line.
        private bool userRequestedNextLine = false;

        // The method that we should call when the user has chosen an
        // option. Externally provided by the DialogueRunner.
        private System.Action<int> currentOptionSelectionHandler;

        // When true, the DialogueRunner is waiting for the user to press
        // one of the option buttons.
        private bool waitingForOptionSelection = false;
        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when the dialogue starts.
        /// </summary>
        /// <remarks>
        /// Use this event to enable any dialogue-related UI and gameplay
        /// elements, and disable any non-dialogue UI and gameplay
        /// elements.
        /// </remarks>
        public UnityEngine.Events.UnityEvent onDialogueStart;

        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when the dialogue ends.
        /// </summary>
        /// <remarks>
        /// Use this event to disable any dialogue-related UI and gameplay
        /// elements, and enable any non-dialogue UI and gameplay elements.
        /// </remarks>
        public UnityEngine.Events.UnityEvent onDialogueEnd;

        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when a <see cref="Line"/> has been delivered.
        /// </summary>
        /// <remarks>
        /// This method is called before <see cref="onLineUpdate"/> is
        /// called. Use this event to prepare the scene to deliver a line.
        /// </remarks>
        public DialogueRunner.StringUnityEvent onLineStart;

        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when a line has finished being delivered.
        /// </summary>
        /// <remarks>
        /// This method is called after <see cref="onLineUpdate"/>. Use
        /// this method to display UI elements like a "continue" button.
        ///
        /// When this method has been called, the Dialogue UI will wait for
        /// the <see cref="MarkLineComplete"/> method to be called, which
        /// signals that the line should be dismissed.
        /// </remarks>
        /// <seealso cref="onLineUpdate"/>
        /// <seealso cref="MarkLineComplete"/>
        public UnityEngine.Events.UnityEvent onLineFinishDisplaying;

        /// <summary>
        /// A <see cref="DialogueRunner.StringUnityEvent"/> that is called
        /// when the visible part of the line's localised text changes.
        /// </summary>
        /// <remarks>
        /// The <see cref="string"/> parameter that this event receives is
        /// the text that should be displayed to the user. Use this method
        /// to display line text to the user.
        ///
        /// The <see cref="DialogueUI"/> class gradually reveals the
        /// localised text of the <see cref="Line"/>, at a rate of <see
        /// cref="textSpeed"/> seconds per character. <see
        /// cref="onLineUpdate"/> will be called multiple times, each time
        /// with more text; the final call to <see cref="onLineUpdate"/>
        /// will have the entire text of the line.
        ///
        /// If <see cref="MarkLineComplete"/> is called before the line has
        /// finished displaying, which indicates that the user has
        /// requested that the Dialogue UI skip to the end of the line,
        /// <see cref="onLineUpdate"/> will be called once more, to display
        /// the entire text.
        ///
        /// If <see cref="textSpeed"/> is `0`, <see cref="onLineUpdate"/>
        /// will be called just once, to display the entire text all at
        /// once.
        ///
        /// After the final call to <see cref="onLineUpdate"/>, <see
        /// cref="onLineFinishDisplaying"/> will be called to indicate that
        /// the line has finished appearing.
        /// </remarks>
        /// <seealso cref="textSpeed"/>
        /// <seealso cref="onLineFinishDisplaying"/>
        public DialogueRunner.StringUnityEvent onLineUpdate;

        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when a line has finished displaying, and should be removed from
        /// the screen.
        /// </summary>
        /// <remarks>
        /// This method is called after the <see cref="MarkLineComplete"/>
        /// has been called. Use this method to dismiss the line's UI
        /// elements.
        ///
        /// After this method is called, the next piece of dialogue content
        /// will be presented, or the dialogue will end.
        /// </remarks>
        public UnityEngine.Events.UnityEvent onLineEnd;

        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when an <see cref="OptionSet"/> has been displayed to the user.
        /// 
        /// </summary>
        /// <remarks>
        /// Before this method is called, the <see cref="Button"/>s in <see
        /// cref="optionButtons"/> are enabled or disabled (depending on
        /// how many options there are), and the <see cref="Text"/> or <see
        /// cref="TMPro.TextMeshProUGUI"/> is updated with the correct
        /// text.
        ///
        /// Use this method to ensure that the active <see
        /// cref="optionButtons"/>s are visible, such as by enabling the
        /// object that they're contained in.
        /// </remarks>
        public UnityEngine.Events.UnityEvent onOptionsStart;

        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that is called
        /// when an option has been selected, and the <see
        /// cref="optionButtons"/> should be hidden.
        /// </summary>
        /// <remarks>
        /// This method is called after one of the <see
        /// cref="optionButtons"/> has been clicked, or the <see
        /// cref="SelectOption(int)"/> method has been called.
        ///
        /// Use this method to hide all of the <see cref="optionButtons"/>,
        /// such as by disabling the object they're contained in. (The
        /// DialogueUI won't hide them for you individually.)
        /// </remarks>
        public UnityEngine.Events.UnityEvent onOptionsEnd;

        /// <summary>
        /// A <see cref="DialogueRunner.StringUnityEvent"/> that is called
        /// when a <see cref="Command"/> is received.
        /// </summary>
        /// <remarks>
        /// Use this method to dispatch a command to other parts of your game.
        /// 
        /// This method is only called if the <see cref="Command"/> has not
        /// been handled by a command handler that has been added to the
        /// <see cref="DialogueRunner"/>, or by a method on a <see
        /// cref="MonoBehaviour"/> in the scene with the attribute <see
        /// cref="YarnCommandAttribute"/>.
        /// 
        /// {{|note|}}
        /// When a command is delivered in this way, the <see cref="DialogueRunner"/> will not pause execution. If you want a command to make the DialogueRunner pause execution, see <see cref="DialogueRunner.AddCommandHandler(string,
        /// DialogueRunner.BlockingCommandHandler)"/>.
        /// {{|/note|}}
        ///
        /// This method receives the full text of the command, as it appears between
        /// the `<![CDATA[<<]]>` and `<![CDATA[>>]]>` markers.
        /// </remarks>
        /// <seealso cref="DialogueRunner.AddCommandHandler(string,
        /// DialogueRunner.CommandHandler)"/> 
        /// <seealso cref="DialogueRunner.AddCommandHandler(string,
        /// DialogueRunner.BlockingCommandHandler)"/> 
        /// <seealso cref="YarnCommandAttribute"/>
        public DialogueRunner.StringUnityEvent onCommand;

        public int[] buttonIDs;



        internal void Awake()
        {
            singleton = this;
            // Start by hiding the container
            if (dialogueContainer != null)
                dialogueContainer.SetActive(false);

            foreach (var button in optionButtons)
            {
                button.gameObject.SetActive(false);
            }
            skipKey.Enable();
            skipKey.started += (InputAction.CallbackContext c) => MarkLineComplete();

            option1Key.started += _ => OnInputButton(0);
            option2Key.started += _ => OnInputButton(1);
            option3Key.started += _ => OnInputButton(2);
            option4Key.started += _ => OnInputButton(3);

            option1Key.Enable();
            option2Key.Enable();
            option3Key.Enable();
            option4Key.Enable();

            exitKey.Enable();
            exitKey.started += OnExitKey;
        }
        void OnInputButton(int index)
        {
            if (buttonIDs.Length > index && buttonIDs[index] != -1)
            {
                SelectOption(buttonIDs[index]);
            }
        }
        void OnExitKey(InputAction.CallbackContext c)
        {
            if (escapeOption != -1)
            {
                SelectOption(escapeOption);
                escapeOption = -1;
            }
        }
        string cachedLine = null;

        /// Runs a line.
        /// <inheritdoc/>
        public override Dialogue.HandlerExecutionType RunLine(Yarn.Line line, ILineLocalisationProvider localisationProvider, System.Action onLineComplete)
        {
            // Start displaying the line; it will call onComplete later
            // which will tell the dialogue to continue

            // The final text we'll be showing for this line.
            string text = localisationProvider.GetLocalisedTextForLine(line);

            if (text == null)
            {
                Debug.LogWarning($"Line {line.ID} doesn't have any localised text.");
                text = line.ID;
            }



            if (cachedLine == null)
            {
                //need to cache this line and move to the next
                cachedLine = text;
                return Dialogue.HandlerExecutionType.ContinueExecution;
            }
            else
            {
                if (cachedLine == "") return Dialogue.HandlerExecutionType.ContinueExecution;
                RunCachedLine(onLineComplete);

                cachedLine = text;
                return Dialogue.HandlerExecutionType.PauseExecution;
            }
        }

        public void RunCachedLine(System.Action onLineComplete)
        {
            if (cachedLine == null)
            {
                onLineComplete?.Invoke();
            }
            else
            {
                StartCoroutine(DoRunLine(cachedLine,
                        () => StartCoroutine(WaitForNextLineRequest(onLineComplete))));
            }

        }

        /// Show a line of dialogue, gradually        
        private IEnumerator DoRunLine(string text, System.Action onComplete)
        {
            onLineStart?.Invoke(text);

            userRequestedNextLine = false;


            string[] s = text.Split(':');
            text = s[1];
            nameText.text = s[0];



            //search for escape codes
            text = text.Replace("\\n", "\n");

            if (textSpeed > 0.0f)
            {
                // Display the line one character at a time
                var stringBuilder = new StringBuilder();

                for (int i = 0; i < text.Length; i++)
                {
                    if (i == 0 && text[i] == '<' || i != 0 && text[i] == '<' && text[i - 1] != '\\')
                    {
                        //skip to the end of the tag and do not include it as taking time
                        //find the next occurrence of '>' after this one
                        int end = text.Substring(i).IndexOf('>') + i;
                        string markup = text.Substring(i, end - i + 1);

                        if (markup.Contains("wait"))
                        {
                            //command for the text writer
                            string time = markup.Split(' ')[1];
                            time = time.Replace('>', ' ');
                            time = time.Trim(' ');
                            float t = float.Parse(time);
                            //only make the user wait if they have not requested the next line
                            if (!userRequestedNextLine)
                                yield return new WaitForSeconds(t);
                        }
                        else
                        {
                            markup = markup.Replace("itemCol", '#' + styling.itemNameColorHex);
                            markup = markup.Replace("numberCol", '#' + styling.numberColorHex);
                            markup = markup.Replace("npcCol", '#' + styling.NPCNameColorHex);
                            stringBuilder.Append(markup);
                        }

                        //always skip the markup
                        i = end + 1;
                    }
                    //do not include the use of backslash to mark special characters - do not check if at end of text
                    if (text[i] == '\\' && i != text.Length - 1 && (text[i + 1] == '<' || text[i + 1] == '>'))
                        i++;

                    stringBuilder.Append(text[i]);



                    if (!userRequestedNextLine)
                    {
                        // If the user has requested the next line, the text should be built instantly
                        onLineUpdate?.Invoke(stringBuilder.ToString());

                        yield return new WaitForSeconds(textSpeed);
                    }

                }

                //display the finished line
                if (userRequestedNextLine)
                {
                    onLineUpdate?.Invoke(stringBuilder.ToString());
                }
            }
            else
            {
                // Display the entire line immediately if textSpeed <= 0
                onLineUpdate?.Invoke(text);
            }


            onComplete?.Invoke();
        }


        private IEnumerator WaitForNextLineRequest(System.Action onComplete)
        {
            // We're now waiting for the player to move on to the next line
            userRequestedNextLine = false;

            // Indicate to the rest of the game that the line has finished being delivered
            onLineFinishDisplaying?.Invoke();

            while (userRequestedNextLine == false)
            {
                yield return null;
            }

            // Avoid skipping lines if textSpeed == 0
            yield return new WaitForEndOfFrame();
            // Hide the text and prompt
            onLineEnd?.Invoke();

            onComplete?.Invoke();
        }

        /// Runs a set of options.
        /// <inheritdoc/>
        public override void RunOptions(Yarn.OptionSet optionSet, ILineLocalisationProvider localisationProvider, System.Action<int> onOptionSelected)
        {
            StartCoroutine(DoRunOptions(optionSet, localisationProvider, onOptionSelected));
        }

        int escapeOption = -1;

        /// Show a list of options, and wait for the player to make a
        /// selection.
        private IEnumerator DoRunOptions(Yarn.OptionSet optionsCollection, ILineLocalisationProvider localisationProvider, System.Action<int> selectOption)
        {
            escapeOption = -1;
            //draw out the line for this question
            if (cachedLine != null)
                yield return DoRunLine(cachedLine, null);
            cachedLine = null;

            // Do a little bit of safety checking
            if (optionsCollection.Options.Length > optionButtons.Count)
            {
                Debug.LogWarning("There are more options to present than there are" +
                                 "buttons to present them in. This will cause problems.");
            }

            // Display each option in a button, and make it visible
            int i = 0;

            waitingForOptionSelection = true;

            currentOptionSelectionHandler = selectOption;

            buttonIDs = new int[optionButtons.Count];
            for (int ind = 0; ind < buttonIDs.Length; ind++)
                buttonIDs[ind] = -1;

            foreach (var optionString in optionsCollection.Options)
            {
                optionButtons[i].gameObject.SetActive(true);

                // When the button is selected, tell the dialogue about it
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectOption(optionString.ID));



                var optionText = localisationProvider.GetLocalisedTextForLine(optionString.Line);

                if (optionText == null)
                {
                    Debug.LogWarning($"Option {optionString.Line.ID} doesn't have any localised text");
                    optionText = optionString.Line.ID;
                }

                //find markup within the text
                int m = optionText.IndexOf('<');
                if (m != -1)
                {
                    int e = optionText.IndexOf('>');
                    string markup = optionText.Substring(m + 1, e - m - 1);
                    optionText = optionText.Remove(m, e - m + 1);
                    switch (markup)
                    {
                        case "Exit":
                            //bind the escape key to this option
                            escapeOption = optionString.ID;
                            optionText = string.Format("[{0}] - {1}", exitKey.controls[0].displayName, optionText);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //use default option keys
                    buttonIDs[i] = optionString.ID;
                }

                var unityText = optionButtons[i].GetComponentInChildren<Text>();
                if (unityText != null)
                {
                    unityText.text = optionText;
                }

                var textMeshProText = optionButtons[i].GetComponentInChildren<TMPro.TMP_Text>();
                if (textMeshProText != null)
                {
                    textMeshProText.text = optionText;
                }

                i++;
            }
            optionButtons[0].Select();
            onOptionsStart?.Invoke();

            // Wait until the chooser has been used and then removed 
            while (waitingForOptionSelection)
            {
                yield return null;
            }

            // Hide all the buttons
            foreach (var button in optionButtons)
            {
                button.gameObject.SetActive(false);
            }
            for (int inde = 0; inde < buttonIDs.Length; inde++)
                buttonIDs[inde] = -1;

            escapeOption = -1;
            onOptionsEnd?.Invoke();
        }



        /// Runs a command.
        /// <inheritdoc/>
        public override Dialogue.HandlerExecutionType RunCommand(Yarn.Command command, System.Action onCommandComplete)
        {
            // Dispatch this command via the 'On Command' handler.
            onCommand?.Invoke(command.Text);

            // Signal to the DialogueRunner that it should continue
            // executing. (This implementation of RunCommand always signals
            // that execution should continue, and never calls
            // onCommandComplete.)
            return Dialogue.HandlerExecutionType.ContinueExecution;
        }

        /// Called when the dialogue system has started running.
        /// <inheritdoc/>
        public override void DialogueStarted()
        {
            cachedLine = null;
            // Enable the dialogue controls.
            if (dialogueContainer != null)
                dialogueContainer.SetActive(true);

            onDialogueStart?.Invoke();
        }

        /// Called when the dialogue system has finished running.
        /// <inheritdoc/>
        public override void DialogueComplete()
        {
            if (cachedLine != null && !cachedLine.Contains("<skip>"))
                StartCoroutine(DoRunLine(cachedLine,
                        () => StartCoroutine(WaitForNextLineRequest(FinishDialogue))));
            else
                FinishDialogue();
            cachedLine = null;
        }
        //because lines have to be cached to display options and text at the same time, the last line must be displayed separately
        void FinishDialogue()
        {
            print("finished the dialogue");
            onDialogueEnd?.Invoke();

            // Hide the dialogue interface.
            if (dialogueContainer != null)
                dialogueContainer.SetActive(false);
        }

        /// <summary>
        /// Signals that the user has finished with a line, or wishes to
        /// skip to the end of the current line.
        /// </summary>
        /// <remarks>
        /// This method is generally called by a "continue" button, and
        /// causes the DialogueUI to signal the <see
        /// cref="DialogueRunner"/> to proceed to the next piece of
        /// content.
        ///
        /// If this method is called before the line has finished appearing
        /// (that is, before <see cref="onLineFinishDisplaying"/> is
        /// called), the DialogueUI immediately displays the entire line
        /// (via the <see cref="onLineUpdate"/> method), and then calls
        /// <see cref="onLineFinishDisplaying"/>.
        /// </remarks>
        public void MarkLineComplete()
        {
            userRequestedNextLine = true;
        }

        /// <summary>
        /// Signals that the user has selected an option.
        /// </summary>
        /// <remarks>
        /// This method is called by the <see cref="Button"/>s in the <see
        /// cref="optionButtons"/> list when clicked.
        ///
        /// If you prefer, you can also call this method directly.
        /// </remarks>
        /// <param name="optionID">The <see cref="OptionSet.Option.ID"/> of
        /// the <see cref="OptionSet.Option"/> that was selected.</param>
        public void SelectOption(int optionID)
        {
            if (waitingForOptionSelection == false)
            {
                Debug.LogWarning("An option was selected, but the dialogue UI was not expecting it.");
                return;
            }
            waitingForOptionSelection = false;
            currentOptionSelectionHandler?.Invoke(optionID);
        }

    }

}
