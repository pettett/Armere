// GENERATED AUTOMATICALLY FROM 'Assets/Mechanics/Input/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Ground Action Map"",
            ""id"": ""66bfb76e-bb2c-4bed-ba71-81c67d697519"",
            ""actions"": [
                {
                    ""name"": ""CameraMove"",
                    ""type"": ""Value"",
                    ""id"": ""8882bad0-c251-459d-925e-e07d73fcfa63"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Walk"",
                    ""type"": ""Value"",
                    ""id"": ""d1e16abe-efb5-4620-820b-d91946a48675"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""VerticalMovement"",
                    ""type"": ""Value"",
                    ""id"": ""7c3b1d65-fa87-4660-9da5-1646436bb786"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""3907b19c-586c-41f2-8a93-cafaafd60b6b"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""5eecaf65-bb90-48a9-946d-ab2f1c418dfd"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""228d5467-0084-411f-8f1c-5c8b47804c3e"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""AltAttack"",
                    ""type"": ""Button"",
                    ""id"": ""11566447-fe97-4fc2-aeed-809d06359ac4"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""a826cc0f-3d09-4cbb-ba20-19e148cad4f2"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Action"",
                    ""type"": ""Button"",
                    ""id"": ""a6851873-980f-4bc0-becf-ff327cc84807"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Shield"",
                    ""type"": ""Button"",
                    ""id"": ""82df823c-2d3c-4844-81fd-7dc3f11568fc"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Aim"",
                    ""type"": ""Button"",
                    ""id"": ""de0a89f2-a86c-4c70-8905-7f9064c6c5ab"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""TabMenu"",
                    ""type"": ""Button"",
                    ""id"": ""def74620-7e63-4933-90ec-bee1e21aaf93"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Console"",
                    ""type"": ""Button"",
                    ""id"": ""87c30319-f012-45cd-8a4e-26b53291af39"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""KO"",
                    ""type"": ""Button"",
                    ""id"": ""42f9125d-94a5-4316-911e-64a0c2e88121"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ChangeFocus"",
                    ""type"": ""Button"",
                    ""id"": ""628b1bbc-a1eb-4839-ae4b-61091018b62b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""OpenInventory"",
                    ""type"": ""Button"",
                    ""id"": ""a8bd5d9f-02ae-4f01-899c-ada1781d562a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""OpenMap"",
                    ""type"": ""Button"",
                    ""id"": ""0808449f-d747-42a5-bea6-d715e4e0d44d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""OpenQuests"",
                    ""type"": ""Button"",
                    ""id"": ""d6332e4e-94d7-48cc-9624-d9660d3d6d68"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""EquipBow"",
                    ""type"": ""Button"",
                    ""id"": ""62a2e969-ccf2-4680-b0eb-03d86cea8b62"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""1d72ffc6-7020-49c2-a287-41302671de8b"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""de359bf0-b513-40d2-9b31-f0445b8a558c"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Walk"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a67ea3c7-89cc-48b9-b2b8-31f46a91a68c"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1e3eb31d-1d14-4c8f-aff3-725391822994"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d560115a-f911-407c-989a-97f62941d3a1"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ed46fbd0-8729-4375-b5f5-8cdf420955d1"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""1cb9c99c-5934-496c-839b-171f26a1a1e7"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c7de42d-4934-44f1-a9a3-1c410cddc2ca"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""aa22319e-c67d-450d-976b-6dee545dea18"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5ab416a1-c7c4-423a-824d-722d62553a17"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bfc602a5-7682-4a28-9466-8bdf05fb9c76"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fdc7a348-962f-4b46-8f7f-bed6e74c67ac"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3ace50b7-b3d8-4096-8ad1-d735b2e679e9"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5bd28b52-271d-4584-9709-581c9316986e"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""72148e46-886a-41a5-9c80-561d6f1f94ed"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""25a710a7-2ec2-4a87-8258-12fde99be70c"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""992d4385-8d00-4e0f-9568-42299ba3f8df"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Shield"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bd417be3-49f7-46db-a63e-34609e1e907c"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Shield"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fc232e64-0318-4882-9108-24dc9953cf79"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Aim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""430e8acb-5682-48e2-9249-17cb59b49db1"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""AltAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""ArrowKeys"",
                    ""id"": ""7d3b6718-f72d-4df3-be78-52e3ac35adf8"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""fbfac4da-6b6c-497b-bed4-b607813859cb"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""186ef6b3-c312-4640-ac54-24ca8624f2fd"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""64cfe621-deaf-48c3-a30a-225d7b32740f"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""d6b94f08-0908-41e4-b2fe-8d2a37ff3031"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""d2461737-3151-49fa-a122-7a8776ce5c33"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": ""InvertVector2"",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2422bcc9-190f-435a-bf15-57d8621caa61"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5af1578b-9d4f-4bc2-94ea-c87424f35a3e"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""TabMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1936b794-8ace-4e74-bd8e-31743912cb77"",
                    ""path"": ""<Keyboard>/f1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Console"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""daa6c3cc-4adc-4c0a-9e49-91a4f921279f"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""db0fd616-696b-425d-9928-09e6e3a53bfd"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""933d51b4-f74e-4374-a841-a4bc6cd61060"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c2c0c83b-eb16-4191-bfb9-d2e18d99188e"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""KO"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ca0d7a58-08af-4290-8145-819ed3709fcb"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ChangeFocus"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6027ae5c-4f44-40e3-b7b6-20068260c683"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""OpenInventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""27f70e2d-386f-477b-973e-6ad51f7291ec"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""OpenMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""15b12117-473a-4828-97ac-ab470c69631c"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""OpenQuests"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""256ef89d-6c32-4f6c-8487-5ce8fbe49ff9"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""EquipBow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Debug"",
            ""id"": ""edf04f3e-56b9-4730-9c1d-c2292fb88b00"",
            ""actions"": [
                {
                    ""name"": ""ShowReadoutScreen"",
                    ""type"": ""Button"",
                    ""id"": ""0f94751c-4294-4af1-b46f-0d4ef7d0cffe"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""QuickSave"",
                    ""type"": ""Button"",
                    ""id"": ""fbf37076-2f49-4a7b-9e4b-1e76e9fff7ad"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""QuickLoad"",
                    ""type"": ""Button"",
                    ""id"": ""fb48d02d-bb08-4a91-a3fd-e4bda5aa96ef"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4c087a17-c5c8-4cc1-8eec-72328320f613"",
                    ""path"": ""<Keyboard>/f3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ShowReadoutScreen"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ebff80e7-3b8d-4da7-8189-d4e9f58729d2"",
                    ""path"": ""<Keyboard>/f6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""QuickSave"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c0a4eb6e-d538-44e4-b002-5e460c473e6a"",
                    ""path"": ""<Keyboard>/f7"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""QuickLoad"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""0cea4bfc-3cd5-4d20-8805-70984c79358a"",
            ""actions"": [
                {
                    ""name"": ""TrackedDeviceOrientation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e6c469f3-cd1d-4012-a96d-1b309d186a02"",
                    ""expectedControlType"": ""Quaternion"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TrackedDevicePosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""8c711dfc-45ea-4aa7-a89c-d6315db0790f"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""a81be6fb-c435-460b-9cbd-4a8308a95575"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MiddleClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""17a373e7-02a2-448e-a6e8-6dcac58302b1"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ScrollWheel"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6929761c-e999-4947-bd92-24c9931ea9ac"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""PassThrough"",
                    ""id"": ""f13ba10c-8f18-4016-8871-6634b0dcd82e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Point"",
                    ""type"": ""PassThrough"",
                    ""id"": ""44069e9f-fd9f-4832-bb53-aa05f1ed2c6e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""PassThrough"",
                    ""id"": ""8eeed524-55b8-45c5-99bd-1c7326def5c3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Submit"",
                    ""type"": ""PassThrough"",
                    ""id"": ""234617cf-1eff-48f1-9ba6-4aa3ef2cdf06"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Navigate"",
                    ""type"": ""PassThrough"",
                    ""id"": ""652ef122-7b4b-415a-8608-1eaf5de66621"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Scroll"",
                    ""type"": ""Value"",
                    ""id"": ""8097f580-de40-48ab-9996-a1c3c8e6b9c5"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""NavigateHorizontal"",
                    ""type"": ""Value"",
                    ""id"": ""9972e50c-f80f-4620-9aef-35ef6c13881e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""d53c8101-5ac5-41f7-8e41-715cb7474978"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Scroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""09740128-d85b-4747-8125-41a8369b7c5b"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NavigateHorizontal"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""8383832d-cb73-467c-ad22-5c1086726639"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""NavigateHorizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""101dcdd8-3837-4ec8-b7f3-15e61268e421"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""NavigateHorizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""a9a745a1-8fe5-4816-a7c4-6803979fa703"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e063ba05-1647-4cd5-b550-781b654f270a"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""1035b54b-7800-458d-a310-94f37b6958d7"",
                    ""path"": ""<Gamepad>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""8e90205c-c1d3-49e9-8e97-ae198933f46e"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""761c562d-aced-43fc-90b6-638ee149b234"",
                    ""path"": ""<Gamepad>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""f4fe8cc1-b0cf-4e60-8a88-c3d310c4887c"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""be655828-f0da-4484-85e8-6390c1873484"",
                    ""path"": ""<Gamepad>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""0b9310be-e1a1-4e53-a5b7-005251cadbf6"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""35d7a9dd-2304-4b29-abed-da4423f53320"",
                    ""path"": ""<Gamepad>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c249eaa3-613d-4f1a-bb48-0baba73082e5"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Joystick"",
                    ""id"": ""d530e43f-81e6-47e4-abc3-6af392e63883"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""38ffabf9-fd1d-48d7-bad2-e57c91e42012"",
                    ""path"": ""<Joystick>/stick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""226fe761-8f87-459b-a4be-1cc976a62d21"",
                    ""path"": ""<Joystick>/stick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""eb4a5b58-11eb-49f6-87d4-c91b6145d302"",
                    ""path"": ""<Joystick>/stick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""de9f58b3-494a-488a-85b5-928fb154d7a1"",
                    ""path"": ""<Joystick>/stick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""ec5fb051-dabf-44c0-8e06-e010a30d179e"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e1d825ee-6ce8-48b3-967e-b4edd227e624"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""5136c819-3d5f-4156-bed1-4496e5ce646e"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""23e2bc6c-c664-428c-b71c-543a87e28b56"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""3d322c0d-4734-41cf-93e7-0a425be5ef26"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""c0355808-2141-4a24-a98c-0da6203a1a05"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""6ef658fa-eff7-4922-a9d5-19f85857b0dd"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""783fcdf9-05fe-4198-bd08-6a99e2b4d46f"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b2a62efc-ec70-4407-857d-c9341714b4c2"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""4b609a28-42ce-45f6-8d07-edd57043af87"",
                    ""path"": ""*/{Submit}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b54d9cca-506a-40ef-8675-fc24c0ee9d1b"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bffe991a-a8ed-442c-bc51-399d5b08c8d4"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5b03bfd7-0cb9-42a1-86f1-2d10fd438d23"",
                    ""path"": ""*/{Cancel}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""57c5d472-b237-4d81-a41d-45f8a4e64434"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""99b6f8d7-1128-4889-a77c-a7ad75d2b140"",
                    ""path"": ""<Pen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03904a2b-ac64-46a8-9b07-5f4f402b8f4a"",
                    ""path"": ""<Touchscreen>/touch*/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Touch"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9dea25af-cf28-4ca5-ab88-13910851b112"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""64713910-1062-4fbc-bbb8-1ec47c9c988b"",
                    ""path"": ""<Pen>/tip"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""618ddb63-b2d6-4469-a65f-1f2623e9626f"",
                    ""path"": ""<Touchscreen>/touch*/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Touch"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""538ca1b5-45b6-4228-8e67-195dfb42cffe"",
                    ""path"": ""<XRController>/trigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""724a9ba9-7f5f-4f69-9d4c-3dfd4ab88a52"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ScrollWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80cd52a2-e803-44d6-950b-ddd6870529bf"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""MiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e5b1b3fe-18fd-476c-bb88-2d17432621b3"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ef7350f2-07be-431d-bca9-a3abd618c751"",
                    ""path"": ""<XRController>/devicePosition"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""TrackedDevicePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7f14e3ce-c7b3-403a-8037-87da2faae597"",
                    ""path"": ""<XRController>/deviceRotation"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""TrackedDeviceOrientation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""AlwaysActive"",
            ""id"": ""6d910ca2-0cbe-4180-916c-2cb10f87722f"",
            ""actions"": [
                {
                    ""name"": ""ChangeSelection"",
                    ""type"": ""Button"",
                    ""id"": ""5a1dfa8b-ca30-4c39-8fe0-c24ae3cda290"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectSpell"",
                    ""type"": ""Value"",
                    ""id"": ""490d233b-8860-428c-b8b1-b91b9f4cea1e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f439ecc6-009a-40a5-b8ce-e997f05ddb34"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dd2762f5-08bf-48a2-ace4-8ec46371fb93"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d110bf6e-dd75-4f5f-97a4-7a53cd77059c"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": ""Scale"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0fa32e23-b357-46e7-9456-f4823b32d2eb"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=2)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a8761580-cb23-4ee9-9205-51e38ec797bb"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=3)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d3c3c5ba-5fe8-4c3f-be88-268070e6e0a9"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=4)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6e6735c2-7b07-40ff-ba73-492e49b5c785"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=5)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""97e810af-2195-4235-af3d-34fa5bf50708"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=6)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b8411a7c-37ff-441f-9322-c54e33390f88"",
                    ""path"": ""<Keyboard>/8"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=7)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5fa979d2-5e8b-4ed9-8022-cde5940ed0a3"",
                    ""path"": ""<Keyboard>/9"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=8)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""834e6500-99d7-42ad-a3f1-94af56ce2787"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=9)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectSpell"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<SwitchProControllerHID>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""KeyboardMouse"",
            ""bindingGroup"": ""KeyboardMouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Ground Action Map
        m_GroundActionMap = asset.FindActionMap("Ground Action Map", throwIfNotFound: true);
        m_GroundActionMap_CameraMove = m_GroundActionMap.FindAction("CameraMove", throwIfNotFound: true);
        m_GroundActionMap_Walk = m_GroundActionMap.FindAction("Walk", throwIfNotFound: true);
        m_GroundActionMap_VerticalMovement = m_GroundActionMap.FindAction("VerticalMovement", throwIfNotFound: true);
        m_GroundActionMap_Sprint = m_GroundActionMap.FindAction("Sprint", throwIfNotFound: true);
        m_GroundActionMap_Crouch = m_GroundActionMap.FindAction("Crouch", throwIfNotFound: true);
        m_GroundActionMap_Attack = m_GroundActionMap.FindAction("Attack", throwIfNotFound: true);
        m_GroundActionMap_AltAttack = m_GroundActionMap.FindAction("AltAttack", throwIfNotFound: true);
        m_GroundActionMap_Jump = m_GroundActionMap.FindAction("Jump", throwIfNotFound: true);
        m_GroundActionMap_Action = m_GroundActionMap.FindAction("Action", throwIfNotFound: true);
        m_GroundActionMap_Shield = m_GroundActionMap.FindAction("Shield", throwIfNotFound: true);
        m_GroundActionMap_Aim = m_GroundActionMap.FindAction("Aim", throwIfNotFound: true);
        m_GroundActionMap_TabMenu = m_GroundActionMap.FindAction("TabMenu", throwIfNotFound: true);
        m_GroundActionMap_Console = m_GroundActionMap.FindAction("Console", throwIfNotFound: true);
        m_GroundActionMap_KO = m_GroundActionMap.FindAction("KO", throwIfNotFound: true);
        m_GroundActionMap_ChangeFocus = m_GroundActionMap.FindAction("ChangeFocus", throwIfNotFound: true);
        m_GroundActionMap_OpenInventory = m_GroundActionMap.FindAction("OpenInventory", throwIfNotFound: true);
        m_GroundActionMap_OpenMap = m_GroundActionMap.FindAction("OpenMap", throwIfNotFound: true);
        m_GroundActionMap_OpenQuests = m_GroundActionMap.FindAction("OpenQuests", throwIfNotFound: true);
        m_GroundActionMap_EquipBow = m_GroundActionMap.FindAction("EquipBow", throwIfNotFound: true);
        // Debug
        m_Debug = asset.FindActionMap("Debug", throwIfNotFound: true);
        m_Debug_ShowReadoutScreen = m_Debug.FindAction("ShowReadoutScreen", throwIfNotFound: true);
        m_Debug_QuickSave = m_Debug.FindAction("QuickSave", throwIfNotFound: true);
        m_Debug_QuickLoad = m_Debug.FindAction("QuickLoad", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
        m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", throwIfNotFound: true);
        m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
        m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
        m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
        m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
        m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
        m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
        m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
        m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
        m_UI_Scroll = m_UI.FindAction("Scroll", throwIfNotFound: true);
        m_UI_NavigateHorizontal = m_UI.FindAction("NavigateHorizontal", throwIfNotFound: true);
        // AlwaysActive
        m_AlwaysActive = asset.FindActionMap("AlwaysActive", throwIfNotFound: true);
        m_AlwaysActive_ChangeSelection = m_AlwaysActive.FindAction("ChangeSelection", throwIfNotFound: true);
        m_AlwaysActive_SelectSpell = m_AlwaysActive.FindAction("SelectSpell", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Ground Action Map
    private readonly InputActionMap m_GroundActionMap;
    private IGroundActionMapActions m_GroundActionMapActionsCallbackInterface;
    private readonly InputAction m_GroundActionMap_CameraMove;
    private readonly InputAction m_GroundActionMap_Walk;
    private readonly InputAction m_GroundActionMap_VerticalMovement;
    private readonly InputAction m_GroundActionMap_Sprint;
    private readonly InputAction m_GroundActionMap_Crouch;
    private readonly InputAction m_GroundActionMap_Attack;
    private readonly InputAction m_GroundActionMap_AltAttack;
    private readonly InputAction m_GroundActionMap_Jump;
    private readonly InputAction m_GroundActionMap_Action;
    private readonly InputAction m_GroundActionMap_Shield;
    private readonly InputAction m_GroundActionMap_Aim;
    private readonly InputAction m_GroundActionMap_TabMenu;
    private readonly InputAction m_GroundActionMap_Console;
    private readonly InputAction m_GroundActionMap_KO;
    private readonly InputAction m_GroundActionMap_ChangeFocus;
    private readonly InputAction m_GroundActionMap_OpenInventory;
    private readonly InputAction m_GroundActionMap_OpenMap;
    private readonly InputAction m_GroundActionMap_OpenQuests;
    private readonly InputAction m_GroundActionMap_EquipBow;
    public struct GroundActionMapActions
    {
        private @PlayerControls m_Wrapper;
        public GroundActionMapActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraMove => m_Wrapper.m_GroundActionMap_CameraMove;
        public InputAction @Walk => m_Wrapper.m_GroundActionMap_Walk;
        public InputAction @VerticalMovement => m_Wrapper.m_GroundActionMap_VerticalMovement;
        public InputAction @Sprint => m_Wrapper.m_GroundActionMap_Sprint;
        public InputAction @Crouch => m_Wrapper.m_GroundActionMap_Crouch;
        public InputAction @Attack => m_Wrapper.m_GroundActionMap_Attack;
        public InputAction @AltAttack => m_Wrapper.m_GroundActionMap_AltAttack;
        public InputAction @Jump => m_Wrapper.m_GroundActionMap_Jump;
        public InputAction @Action => m_Wrapper.m_GroundActionMap_Action;
        public InputAction @Shield => m_Wrapper.m_GroundActionMap_Shield;
        public InputAction @Aim => m_Wrapper.m_GroundActionMap_Aim;
        public InputAction @TabMenu => m_Wrapper.m_GroundActionMap_TabMenu;
        public InputAction @Console => m_Wrapper.m_GroundActionMap_Console;
        public InputAction @KO => m_Wrapper.m_GroundActionMap_KO;
        public InputAction @ChangeFocus => m_Wrapper.m_GroundActionMap_ChangeFocus;
        public InputAction @OpenInventory => m_Wrapper.m_GroundActionMap_OpenInventory;
        public InputAction @OpenMap => m_Wrapper.m_GroundActionMap_OpenMap;
        public InputAction @OpenQuests => m_Wrapper.m_GroundActionMap_OpenQuests;
        public InputAction @EquipBow => m_Wrapper.m_GroundActionMap_EquipBow;
        public InputActionMap Get() { return m_Wrapper.m_GroundActionMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GroundActionMapActions set) { return set.Get(); }
        public void SetCallbacks(IGroundActionMapActions instance)
        {
            if (m_Wrapper.m_GroundActionMapActionsCallbackInterface != null)
            {
                @CameraMove.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnCameraMove;
                @CameraMove.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnCameraMove;
                @CameraMove.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnCameraMove;
                @Walk.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnWalk;
                @Walk.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnWalk;
                @Walk.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnWalk;
                @VerticalMovement.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnVerticalMovement;
                @VerticalMovement.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnVerticalMovement;
                @VerticalMovement.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnVerticalMovement;
                @Sprint.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSprint;
                @Crouch.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnCrouch;
                @Crouch.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnCrouch;
                @Crouch.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnCrouch;
                @Attack.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAttack;
                @AltAttack.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAltAttack;
                @AltAttack.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAltAttack;
                @AltAttack.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAltAttack;
                @Jump.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnJump;
                @Action.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAction;
                @Action.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAction;
                @Action.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAction;
                @Shield.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnShield;
                @Shield.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnShield;
                @Shield.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnShield;
                @Aim.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAim;
                @Aim.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAim;
                @Aim.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnAim;
                @TabMenu.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnTabMenu;
                @TabMenu.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnTabMenu;
                @TabMenu.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnTabMenu;
                @Console.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnConsole;
                @Console.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnConsole;
                @Console.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnConsole;
                @KO.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnKO;
                @KO.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnKO;
                @KO.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnKO;
                @ChangeFocus.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnChangeFocus;
                @ChangeFocus.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnChangeFocus;
                @ChangeFocus.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnChangeFocus;
                @OpenInventory.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenInventory;
                @OpenInventory.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenInventory;
                @OpenInventory.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenInventory;
                @OpenMap.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenMap;
                @OpenMap.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenMap;
                @OpenMap.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenMap;
                @OpenQuests.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenQuests;
                @OpenQuests.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenQuests;
                @OpenQuests.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnOpenQuests;
                @EquipBow.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnEquipBow;
                @EquipBow.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnEquipBow;
                @EquipBow.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnEquipBow;
            }
            m_Wrapper.m_GroundActionMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @CameraMove.started += instance.OnCameraMove;
                @CameraMove.performed += instance.OnCameraMove;
                @CameraMove.canceled += instance.OnCameraMove;
                @Walk.started += instance.OnWalk;
                @Walk.performed += instance.OnWalk;
                @Walk.canceled += instance.OnWalk;
                @VerticalMovement.started += instance.OnVerticalMovement;
                @VerticalMovement.performed += instance.OnVerticalMovement;
                @VerticalMovement.canceled += instance.OnVerticalMovement;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
                @Crouch.started += instance.OnCrouch;
                @Crouch.performed += instance.OnCrouch;
                @Crouch.canceled += instance.OnCrouch;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @AltAttack.started += instance.OnAltAttack;
                @AltAttack.performed += instance.OnAltAttack;
                @AltAttack.canceled += instance.OnAltAttack;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Action.started += instance.OnAction;
                @Action.performed += instance.OnAction;
                @Action.canceled += instance.OnAction;
                @Shield.started += instance.OnShield;
                @Shield.performed += instance.OnShield;
                @Shield.canceled += instance.OnShield;
                @Aim.started += instance.OnAim;
                @Aim.performed += instance.OnAim;
                @Aim.canceled += instance.OnAim;
                @TabMenu.started += instance.OnTabMenu;
                @TabMenu.performed += instance.OnTabMenu;
                @TabMenu.canceled += instance.OnTabMenu;
                @Console.started += instance.OnConsole;
                @Console.performed += instance.OnConsole;
                @Console.canceled += instance.OnConsole;
                @KO.started += instance.OnKO;
                @KO.performed += instance.OnKO;
                @KO.canceled += instance.OnKO;
                @ChangeFocus.started += instance.OnChangeFocus;
                @ChangeFocus.performed += instance.OnChangeFocus;
                @ChangeFocus.canceled += instance.OnChangeFocus;
                @OpenInventory.started += instance.OnOpenInventory;
                @OpenInventory.performed += instance.OnOpenInventory;
                @OpenInventory.canceled += instance.OnOpenInventory;
                @OpenMap.started += instance.OnOpenMap;
                @OpenMap.performed += instance.OnOpenMap;
                @OpenMap.canceled += instance.OnOpenMap;
                @OpenQuests.started += instance.OnOpenQuests;
                @OpenQuests.performed += instance.OnOpenQuests;
                @OpenQuests.canceled += instance.OnOpenQuests;
                @EquipBow.started += instance.OnEquipBow;
                @EquipBow.performed += instance.OnEquipBow;
                @EquipBow.canceled += instance.OnEquipBow;
            }
        }
    }
    public GroundActionMapActions @GroundActionMap => new GroundActionMapActions(this);

    // Debug
    private readonly InputActionMap m_Debug;
    private IDebugActions m_DebugActionsCallbackInterface;
    private readonly InputAction m_Debug_ShowReadoutScreen;
    private readonly InputAction m_Debug_QuickSave;
    private readonly InputAction m_Debug_QuickLoad;
    public struct DebugActions
    {
        private @PlayerControls m_Wrapper;
        public DebugActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ShowReadoutScreen => m_Wrapper.m_Debug_ShowReadoutScreen;
        public InputAction @QuickSave => m_Wrapper.m_Debug_QuickSave;
        public InputAction @QuickLoad => m_Wrapper.m_Debug_QuickLoad;
        public InputActionMap Get() { return m_Wrapper.m_Debug; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DebugActions set) { return set.Get(); }
        public void SetCallbacks(IDebugActions instance)
        {
            if (m_Wrapper.m_DebugActionsCallbackInterface != null)
            {
                @ShowReadoutScreen.started -= m_Wrapper.m_DebugActionsCallbackInterface.OnShowReadoutScreen;
                @ShowReadoutScreen.performed -= m_Wrapper.m_DebugActionsCallbackInterface.OnShowReadoutScreen;
                @ShowReadoutScreen.canceled -= m_Wrapper.m_DebugActionsCallbackInterface.OnShowReadoutScreen;
                @QuickSave.started -= m_Wrapper.m_DebugActionsCallbackInterface.OnQuickSave;
                @QuickSave.performed -= m_Wrapper.m_DebugActionsCallbackInterface.OnQuickSave;
                @QuickSave.canceled -= m_Wrapper.m_DebugActionsCallbackInterface.OnQuickSave;
                @QuickLoad.started -= m_Wrapper.m_DebugActionsCallbackInterface.OnQuickLoad;
                @QuickLoad.performed -= m_Wrapper.m_DebugActionsCallbackInterface.OnQuickLoad;
                @QuickLoad.canceled -= m_Wrapper.m_DebugActionsCallbackInterface.OnQuickLoad;
            }
            m_Wrapper.m_DebugActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ShowReadoutScreen.started += instance.OnShowReadoutScreen;
                @ShowReadoutScreen.performed += instance.OnShowReadoutScreen;
                @ShowReadoutScreen.canceled += instance.OnShowReadoutScreen;
                @QuickSave.started += instance.OnQuickSave;
                @QuickSave.performed += instance.OnQuickSave;
                @QuickSave.canceled += instance.OnQuickSave;
                @QuickLoad.started += instance.OnQuickLoad;
                @QuickLoad.performed += instance.OnQuickLoad;
                @QuickLoad.canceled += instance.OnQuickLoad;
            }
        }
    }
    public DebugActions @Debug => new DebugActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_TrackedDeviceOrientation;
    private readonly InputAction m_UI_TrackedDevicePosition;
    private readonly InputAction m_UI_RightClick;
    private readonly InputAction m_UI_MiddleClick;
    private readonly InputAction m_UI_ScrollWheel;
    private readonly InputAction m_UI_Click;
    private readonly InputAction m_UI_Point;
    private readonly InputAction m_UI_Cancel;
    private readonly InputAction m_UI_Submit;
    private readonly InputAction m_UI_Navigate;
    private readonly InputAction m_UI_Scroll;
    private readonly InputAction m_UI_NavigateHorizontal;
    public struct UIActions
    {
        private @PlayerControls m_Wrapper;
        public UIActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;
        public InputAction @TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;
        public InputAction @RightClick => m_Wrapper.m_UI_RightClick;
        public InputAction @MiddleClick => m_Wrapper.m_UI_MiddleClick;
        public InputAction @ScrollWheel => m_Wrapper.m_UI_ScrollWheel;
        public InputAction @Click => m_Wrapper.m_UI_Click;
        public InputAction @Point => m_Wrapper.m_UI_Point;
        public InputAction @Cancel => m_Wrapper.m_UI_Cancel;
        public InputAction @Submit => m_Wrapper.m_UI_Submit;
        public InputAction @Navigate => m_Wrapper.m_UI_Navigate;
        public InputAction @Scroll => m_Wrapper.m_UI_Scroll;
        public InputAction @NavigateHorizontal => m_Wrapper.m_UI_NavigateHorizontal;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @TrackedDeviceOrientation.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDevicePosition.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDevicePosition.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDevicePosition.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
                @RightClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
                @MiddleClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
                @ScrollWheel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
                @ScrollWheel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
                @ScrollWheel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
                @Click.started -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Point.started -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Point.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Point.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Cancel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Submit.started -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
                @Submit.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
                @Submit.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
                @Navigate.started -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
                @Navigate.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
                @Navigate.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
                @Scroll.started -= m_Wrapper.m_UIActionsCallbackInterface.OnScroll;
                @Scroll.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnScroll;
                @Scroll.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnScroll;
                @NavigateHorizontal.started -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigateHorizontal;
                @NavigateHorizontal.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigateHorizontal;
                @NavigateHorizontal.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigateHorizontal;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
                @TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
                @TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
                @TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
                @MiddleClick.started += instance.OnMiddleClick;
                @MiddleClick.performed += instance.OnMiddleClick;
                @MiddleClick.canceled += instance.OnMiddleClick;
                @ScrollWheel.started += instance.OnScrollWheel;
                @ScrollWheel.performed += instance.OnScrollWheel;
                @ScrollWheel.canceled += instance.OnScrollWheel;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Point.started += instance.OnPoint;
                @Point.performed += instance.OnPoint;
                @Point.canceled += instance.OnPoint;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @Submit.started += instance.OnSubmit;
                @Submit.performed += instance.OnSubmit;
                @Submit.canceled += instance.OnSubmit;
                @Navigate.started += instance.OnNavigate;
                @Navigate.performed += instance.OnNavigate;
                @Navigate.canceled += instance.OnNavigate;
                @Scroll.started += instance.OnScroll;
                @Scroll.performed += instance.OnScroll;
                @Scroll.canceled += instance.OnScroll;
                @NavigateHorizontal.started += instance.OnNavigateHorizontal;
                @NavigateHorizontal.performed += instance.OnNavigateHorizontal;
                @NavigateHorizontal.canceled += instance.OnNavigateHorizontal;
            }
        }
    }
    public UIActions @UI => new UIActions(this);

    // AlwaysActive
    private readonly InputActionMap m_AlwaysActive;
    private IAlwaysActiveActions m_AlwaysActiveActionsCallbackInterface;
    private readonly InputAction m_AlwaysActive_ChangeSelection;
    private readonly InputAction m_AlwaysActive_SelectSpell;
    public struct AlwaysActiveActions
    {
        private @PlayerControls m_Wrapper;
        public AlwaysActiveActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ChangeSelection => m_Wrapper.m_AlwaysActive_ChangeSelection;
        public InputAction @SelectSpell => m_Wrapper.m_AlwaysActive_SelectSpell;
        public InputActionMap Get() { return m_Wrapper.m_AlwaysActive; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(AlwaysActiveActions set) { return set.Get(); }
        public void SetCallbacks(IAlwaysActiveActions instance)
        {
            if (m_Wrapper.m_AlwaysActiveActionsCallbackInterface != null)
            {
                @ChangeSelection.started -= m_Wrapper.m_AlwaysActiveActionsCallbackInterface.OnChangeSelection;
                @ChangeSelection.performed -= m_Wrapper.m_AlwaysActiveActionsCallbackInterface.OnChangeSelection;
                @ChangeSelection.canceled -= m_Wrapper.m_AlwaysActiveActionsCallbackInterface.OnChangeSelection;
                @SelectSpell.started -= m_Wrapper.m_AlwaysActiveActionsCallbackInterface.OnSelectSpell;
                @SelectSpell.performed -= m_Wrapper.m_AlwaysActiveActionsCallbackInterface.OnSelectSpell;
                @SelectSpell.canceled -= m_Wrapper.m_AlwaysActiveActionsCallbackInterface.OnSelectSpell;
            }
            m_Wrapper.m_AlwaysActiveActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ChangeSelection.started += instance.OnChangeSelection;
                @ChangeSelection.performed += instance.OnChangeSelection;
                @ChangeSelection.canceled += instance.OnChangeSelection;
                @SelectSpell.started += instance.OnSelectSpell;
                @SelectSpell.performed += instance.OnSelectSpell;
                @SelectSpell.canceled += instance.OnSelectSpell;
            }
        }
    }
    public AlwaysActiveActions @AlwaysActive => new AlwaysActiveActions(this);
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("KeyboardMouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IGroundActionMapActions
    {
        void OnCameraMove(InputAction.CallbackContext context);
        void OnWalk(InputAction.CallbackContext context);
        void OnVerticalMovement(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnAltAttack(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnAction(InputAction.CallbackContext context);
        void OnShield(InputAction.CallbackContext context);
        void OnAim(InputAction.CallbackContext context);
        void OnTabMenu(InputAction.CallbackContext context);
        void OnConsole(InputAction.CallbackContext context);
        void OnKO(InputAction.CallbackContext context);
        void OnChangeFocus(InputAction.CallbackContext context);
        void OnOpenInventory(InputAction.CallbackContext context);
        void OnOpenMap(InputAction.CallbackContext context);
        void OnOpenQuests(InputAction.CallbackContext context);
        void OnEquipBow(InputAction.CallbackContext context);
    }
    public interface IDebugActions
    {
        void OnShowReadoutScreen(InputAction.CallbackContext context);
        void OnQuickSave(InputAction.CallbackContext context);
        void OnQuickLoad(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnTrackedDeviceOrientation(InputAction.CallbackContext context);
        void OnTrackedDevicePosition(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
        void OnMiddleClick(InputAction.CallbackContext context);
        void OnScrollWheel(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnPoint(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnSubmit(InputAction.CallbackContext context);
        void OnNavigate(InputAction.CallbackContext context);
        void OnScroll(InputAction.CallbackContext context);
        void OnNavigateHorizontal(InputAction.CallbackContext context);
    }
    public interface IAlwaysActiveActions
    {
        void OnChangeSelection(InputAction.CallbackContext context);
        void OnSelectSpell(InputAction.CallbackContext context);
    }
}
