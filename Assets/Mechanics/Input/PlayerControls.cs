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
                    ""name"": ""SwitchWeaponSet"",
                    ""type"": ""Button"",
                    ""id"": ""4f8ff449-85f4-48f1-850c-056b0cf5bbaa"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SelectWeapon"",
                    ""type"": ""Value"",
                    ""id"": ""08949489-2e63-45e8-b72a-7891e6e7f714"",
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
                    ""name"": """",
                    ""id"": ""dec51b40-5696-4e6e-a74d-71aa3956a9e6"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SwitchWeaponSet"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""74178720-e353-43e7-bce0-babb9bfc2210"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7d1b4800-0654-48fd-b6f4-9e61a010aafa"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": ""Scale"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""39baec3a-3bd1-4755-a8e0-c7fbe7502769"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=2)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2a75b0f8-c76f-4d69-89c1-bf9c2799fbac"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=3)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""57da548d-df76-4003-befe-3d107a20ba43"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=4)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""42613d7e-3a8f-4154-94fa-86e828ad61e4"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=5)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7b5e75c0-4af3-46db-bc4b-dad5adb5bcaa"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=6)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c897bdd-ead6-4188-a57e-653bb3491e1e"",
                    ""path"": ""<Keyboard>/8"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=7)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c1d09be-bd5d-4517-8bed-e078bb0196f9"",
                    ""path"": ""<Keyboard>/9"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=8)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""929c8315-01dc-445f-beb3-1fd8fb408b77"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=9)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SelectWeapon"",
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
                }
            ]
        },
        {
            ""name"": ""Dialogue"",
            ""id"": ""3c6807f6-78c3-4920-80c6-2e410e6764b8"",
            ""actions"": [],
            ""bindings"": []
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
        m_GroundActionMap_SwitchWeaponSet = m_GroundActionMap.FindAction("SwitchWeaponSet", throwIfNotFound: true);
        m_GroundActionMap_SelectWeapon = m_GroundActionMap.FindAction("SelectWeapon", throwIfNotFound: true);
        m_GroundActionMap_KO = m_GroundActionMap.FindAction("KO", throwIfNotFound: true);
        m_GroundActionMap_ChangeFocus = m_GroundActionMap.FindAction("ChangeFocus", throwIfNotFound: true);
        // Dialogue
        m_Dialogue = asset.FindActionMap("Dialogue", throwIfNotFound: true);
        // Debug
        m_Debug = asset.FindActionMap("Debug", throwIfNotFound: true);
        m_Debug_ShowReadoutScreen = m_Debug.FindAction("ShowReadoutScreen", throwIfNotFound: true);
        m_Debug_QuickSave = m_Debug.FindAction("QuickSave", throwIfNotFound: true);
        m_Debug_QuickLoad = m_Debug.FindAction("QuickLoad", throwIfNotFound: true);
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
    private readonly InputAction m_GroundActionMap_SwitchWeaponSet;
    private readonly InputAction m_GroundActionMap_SelectWeapon;
    private readonly InputAction m_GroundActionMap_KO;
    private readonly InputAction m_GroundActionMap_ChangeFocus;
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
        public InputAction @SwitchWeaponSet => m_Wrapper.m_GroundActionMap_SwitchWeaponSet;
        public InputAction @SelectWeapon => m_Wrapper.m_GroundActionMap_SelectWeapon;
        public InputAction @KO => m_Wrapper.m_GroundActionMap_KO;
        public InputAction @ChangeFocus => m_Wrapper.m_GroundActionMap_ChangeFocus;
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
                @SwitchWeaponSet.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSwitchWeaponSet;
                @SwitchWeaponSet.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSwitchWeaponSet;
                @SwitchWeaponSet.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSwitchWeaponSet;
                @SelectWeapon.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSelectWeapon;
                @SelectWeapon.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSelectWeapon;
                @SelectWeapon.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnSelectWeapon;
                @KO.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnKO;
                @KO.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnKO;
                @KO.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnKO;
                @ChangeFocus.started -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnChangeFocus;
                @ChangeFocus.performed -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnChangeFocus;
                @ChangeFocus.canceled -= m_Wrapper.m_GroundActionMapActionsCallbackInterface.OnChangeFocus;
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
                @SwitchWeaponSet.started += instance.OnSwitchWeaponSet;
                @SwitchWeaponSet.performed += instance.OnSwitchWeaponSet;
                @SwitchWeaponSet.canceled += instance.OnSwitchWeaponSet;
                @SelectWeapon.started += instance.OnSelectWeapon;
                @SelectWeapon.performed += instance.OnSelectWeapon;
                @SelectWeapon.canceled += instance.OnSelectWeapon;
                @KO.started += instance.OnKO;
                @KO.performed += instance.OnKO;
                @KO.canceled += instance.OnKO;
                @ChangeFocus.started += instance.OnChangeFocus;
                @ChangeFocus.performed += instance.OnChangeFocus;
                @ChangeFocus.canceled += instance.OnChangeFocus;
            }
        }
    }
    public GroundActionMapActions @GroundActionMap => new GroundActionMapActions(this);

    // Dialogue
    private readonly InputActionMap m_Dialogue;
    private IDialogueActions m_DialogueActionsCallbackInterface;
    public struct DialogueActions
    {
        private @PlayerControls m_Wrapper;
        public DialogueActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputActionMap Get() { return m_Wrapper.m_Dialogue; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DialogueActions set) { return set.Get(); }
        public void SetCallbacks(IDialogueActions instance)
        {
            if (m_Wrapper.m_DialogueActionsCallbackInterface != null)
            {
            }
            m_Wrapper.m_DialogueActionsCallbackInterface = instance;
            if (instance != null)
            {
            }
        }
    }
    public DialogueActions @Dialogue => new DialogueActions(this);

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
        void OnSwitchWeaponSet(InputAction.CallbackContext context);
        void OnSelectWeapon(InputAction.CallbackContext context);
        void OnKO(InputAction.CallbackContext context);
        void OnChangeFocus(InputAction.CallbackContext context);
    }
    public interface IDialogueActions
    {
    }
    public interface IDebugActions
    {
        void OnShowReadoutScreen(InputAction.CallbackContext context);
        void OnQuickSave(InputAction.CallbackContext context);
        void OnQuickLoad(InputAction.CallbackContext context);
    }
}
