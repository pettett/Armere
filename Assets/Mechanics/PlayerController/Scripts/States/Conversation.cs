using UnityEngine;

namespace PlayerController
{
    [System.Serializable]
    public class Conversation : MovementState
    {
        public override string StateName => "In Conversation";
        public override void Start()
        {

            c.cameraController.lockingMouse = false;

            c.rb.velocity = Vector3.zero;
            c.cameraController.DisableControl();
            c.rb.isKinematic = true;
            c.cutsceneCamera.Priority = 50;
        }
        public override void End()
        {            
            c.cameraController.lockingMouse = true;
            c.rb.isKinematic = false;
            c.cameraController.EnableControl();
            c.cutsceneCamera.Priority = 0;
        }

        public override void Update() {

            Cursor.lockState = CursorLockMode.None;
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.vertical.id, 0);
        }


        public void RunSellMenu(System.Action<ItemType, int> onSelectItem){
            UIController.singleton.buyMenu.SetActive(true);
	      //Wait for a buy
            UIController.singleton.buyMenu.GetComponentInChildren<InventoryUI>().onItemSelected = onSelectItem;
        }
        public void CloseSellMenu(){
            //Close the buy menu
            UIController.singleton.buyMenu.SetActive(false);
            UIController.singleton.buyMenu.GetComponentInChildren<InventoryUI>().onItemSelected = null;

        }

    }
}