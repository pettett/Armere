
namespace Armere.Inventory
{


    [System.Serializable]
    public class PotionItemUnique : ItemStackBase
    {

        public float potency;
        public float duration;


        //This should never be called
        public PotionItemUnique() : base(ItemName.EmptyPotion)
        {
        }

        public PotionItemUnique(ItemName name) : base(name)
        {
        }
    }
}