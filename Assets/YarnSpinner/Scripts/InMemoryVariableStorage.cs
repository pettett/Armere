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
using System.Collections.Generic;

namespace Yarn.Unity
{

    public interface IVariableAddon
    {
        string prefix { get; }

        Value this[string name]
        {
            get;
            set;
        }

    }


    /// <summary>
    /// An simple implementation of DialogueUnityVariableStorage, which
    /// stores everything in memory.
    /// </summary>
    /// <remarks>
    /// This class does not perform any saving or loading on its own, but
    /// you can enumerate over the variables by using a `foreach` loop:
    /// 
    /// <![CDATA[
    /// ```csharp    
    /// // 'storage' is an InMemoryVariableStorage    
    /// foreach (var variable in storage) {
    ///         string name = variable.Key;
    ///         Yarn.Value value = variable.Value;
    /// }   
    /// ```
    /// ]]>
    /// 
    /// </remarks>    
    public class InMemoryVariableStorage : VariableStorageBehaviour, IEnumerable<KeyValuePair<string, Value>>
    {

        /// Where we actually keeping our variables
        private Dictionary<string, Value> variables = new Dictionary<string, Value>();


        public IVariableAddon addon;


        /// <summary>
        /// A default value to apply when the object wakes up, or when
        /// ResetToDefaults is called.
        /// </summary>
        [System.Serializable]
        public class DefaultVariable
        {
            /// <summary>
            /// The name of the variable.
            /// </summary>
            /// <remarks>
            /// Do not include the `$` prefix in front of the variable
            /// name. It will be added for you.
            /// </remarks>
            public string name;

            /// <summary>
            /// The value of the variable, as a string.
            /// </summary>
            /// <remarks>
            /// This string will be converted to the appropriate type,
            /// depending on the value of <see cref="type"/>.
            /// </remarks>
            public string value;

            /// <summary>
            /// The type of the variable.
            /// </summary>
            public Value.Type type;
        }

        /// <summary>
        /// The list of default variables that should be present in the
        /// InMemoryVariableStorage when the scene loads.
        /// </summary>
        public DefaultVariable[] defaultVariables;

        [Header("Optional debugging tools")]

        /// A UI.Text that can show the current list of all variables. Optional.
        [SerializeField]
        internal UnityEngine.UI.Text debugTextView = null;





        /// Reset to our default values when the game starts
        internal void Awake()
        {
            ResetToDefaults();
        }

        /// <summary>
        /// Removes all variables, and replaces them with the variables
        /// defined in <see cref="defaultVariables"/>.
        /// </summary>
        public override void ResetToDefaults()
        {
            Clear();

            // For each default variable that's been defined, parse the
            // string that the user typed in in Unity and store the
            // variable
            foreach (DefaultVariable variable in defaultVariables)
            {
                var v = AddDefault(variable);
                SetValue("$" + variable.name, v);
            }
        }
        public static Value AddDefault(DefaultVariable variable)
        {
            object value;

            switch (variable.type)
            {
                case Value.Type.Number:
                    float f = 0.0f;
                    float.TryParse(variable.value, out f);
                    value = f;
                    break;

                case Value.Type.String:
                    value = variable.value;
                    break;

                case Value.Type.Bool:
                    bool b = false;
                    bool.TryParse(variable.value, out b);
                    value = b;
                    break;

                case Value.Type.Variable:
                    // We don't support assigning default variables from
                    // other variables yet
                    Debug.LogErrorFormat("Can't set variable {0} to {1}: You can't " +
                        "set a default variable to be another variable, because it " +
                        "may not have been initialised yet.", variable.name, variable.value);
                    return Value.NULL;

                case Value.Type.Null:
                    value = null;
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException();

            }

            return new Value(value);


        }

        /// <summary>
        /// Stores a <see cref="Value"/>.
        /// </summary>
        /// <param name="variableName">The name to associate with this
        /// variable.</param>
        /// <param name="value">The value to store.</param>
        public override void SetValue(string variableName, Value value)
        {
            if (variableName.StartsWith(addon.prefix))
            {
                addon[variableName.Substring(addon.prefix.Length)] = new Value(value);
            }
            else
            {
                // Copy this value into our list
                variables[variableName] = new Value(value);
            }
        }
        const string questPrefix = "$Quest_";
        const string itemPrefix = "$Item_";
        const string questStagePrefix = "$QuestStage_";

        /// <summary>
        /// Retrieves a <see cref="Value"/> by name.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve
        /// the value of.</param>
        /// <returns>The <see cref="Value"/>. If a variable by the name of
        /// <paramref name="variableName"/> is not present, returns a value
        /// representing `null`.</returns>
        public override Value GetValue(string variableName)
        {
            // If we don't have a variable with this name, return the null
            // value

            if (variableName.StartsWith(questPrefix))
            {
                //it is a quest
                string quest = variableName.Substring(questPrefix.Length);

                if (QuestManager.TryGetQuest(quest, out var q))

                    //there is a quest with this name, return it's current state
                    return new Value("Active");
                else if (QuestManager.TryGetCompletedQuest(quest, out var qw))

                    //there is a quest with this name, return it's current state
                    return new Value("Completed");
                else
                    return new Value("Inactive");
            }


            else if (variableName.StartsWith(questStagePrefix))
            {
                //it is a quest
                string quest = variableName.Substring(questStagePrefix.Length);

                if (QuestManager.TryGetQuest(quest, out var q))

                    //there is a quest with this name, return it's current state
                    return new Value(q.stage);
                else if (QuestManager.TryGetCompletedQuest(quest, out var qw))

                    //there is a quest with this name, return it's current state
                    return new Value(qw.stage + 1);
                else
                    return new Value(-1);
            }

            else if (variableName.StartsWith(addon.prefix))
            {
                return addon[variableName.Substring(addon.prefix.Length)];
            }

            else if (variableName.StartsWith(itemPrefix))
            {
                ItemName item = (ItemName)System.Enum.Parse(typeof(ItemName), variableName.Substring(itemPrefix.Length));
                return new Value(InventoryController.ItemCount(item));
            }


            else if (variables.ContainsKey(variableName) == false)
            {
                print("null variable");
                return Value.NULL;
            }


            return variables[variableName];
        }

        /// <summary>
        /// Removes all variables from storage.
        /// </summary>
        public override void Clear()
        {
            variables.Clear();
        }

        /// If we have a debug view, show the list of all variables in it
        internal void Update()
        {
            if (debugTextView != null)
            {
                var stringBuilder = new System.Text.StringBuilder();
                foreach (KeyValuePair<string, Yarn.Value> item in variables)
                {
                    string debugDescription;
                    switch (item.Value.type)
                    {
                        case Value.Type.Bool:
                            debugDescription = item.Value.AsBool.ToString();
                            break;
                        case Value.Type.Null:
                            debugDescription = "null";
                            break;
                        case Value.Type.Number:
                            debugDescription = item.Value.AsNumber.ToString();
                            break;
                        case Value.Type.String:
                            debugDescription = $@"""{item.Value.AsString}""";
                            break;
                        default:
                            debugDescription = "<unknown>";
                            break;

                    }
                    stringBuilder.AppendLine(string.Format("{0} = {1}",
                                                            item.Key,
                                                            debugDescription));
                }
                debugTextView.text = stringBuilder.ToString();
                debugTextView.SetAllDirty();
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that iterates over all
        /// variables in this object.
        /// </summary>
        /// <returns>An iterator over the variables.</returns>
        IEnumerator<KeyValuePair<string, Value>> IEnumerable<KeyValuePair<string, Yarn.Value>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Value>>)variables).GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> that iterates over all
        /// variables in this object.
        /// </summary>
        /// <returns>An iterator over the variables.</returns>        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Value>>)variables).GetEnumerator();
        }
    }
}