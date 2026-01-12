namespace Mayril.InventorySystem
{
    /// <summary>
    /// 인벤토리 시스템의 아이템을 나타냅니다.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// 이 아이템 정의의 고유 식별자입니다.
        /// </summary>
        int ItemCode { get; }

        /// <summary>
        /// 아이템의 수량입니다.
        /// </summary>
        int Quantity { get; }

        /// <summary>
        /// 함께 쌓을 수 있는 최대 아이템 수입니다.
        /// 쌓을 수 없는 아이템의 경우 1을 반환합니다.
        /// </summary>
        int MaxStack { get; }

        /// <summary>
        /// 다른 아이템의 지정된 수량을 이 아이템과 쌓을 수 있는지 확인합니다.
        /// 아이템은 동일한 Id를 가지고 있고 쌓을 수 있는 공간이 있을 때만 쌓을 수 있습니다.
        /// </summary>
        /// <param name="sourceItem">확인할 원본 아이템. 이 아이템과 동일한 Id를 가져야 합니다.</param>
        /// <param name="quantityToStack">쌓을 수량. 음수가 아니어야 하며 sourceItem.Quantity를 초과할 수 없습니다.</param>
        /// <returns>아이템이 동일한 Id를 가지고 쌓기가 가능하면 true, 그렇지 않으면 false.</returns>
        bool CanStack(IItem sourceItem, int quantityToStack);

        /// <summary>
        /// 다른 아이템에서 지정된 수량을 이 아이템으로 옮깁니다.
        /// 두 아이템은 동일한 <see cref="ItemCode"/>를 가져야 합니다. 이 작업은 이 아이템의 수량을 증가시키고
        /// 원본 아이템의 수량을 같은 양만큼 감소시킵니다.
        /// </summary>
        /// <param name="sourceItem">옮길 원본 아이템. 이 아이템과 동일한 <see cref="ItemCode"/>를 가져야 합니다.</param>
        /// <param name="quantity">옮길 수량. 양수여야 하며 sourceItem.Quantity를 초과할 수 없습니다.</param>
        /// <exception cref="System.ArgumentException">아이템의 Id가 다르거나 수량이 유효하지 않을 때 발생합니다.</exception>
        /// <exception cref="System.InvalidOperationException">쌓기가 MaxStack을 초과할 때 발생합니다.</exception>
        void Stack(IItem sourceItem, int quantity);

        /// <summary>
        /// 수량을 변경할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="delta">추가/차감할 양.</param>
        bool CanModifyQuantity(int delta);

        /// <summary>
        /// 이 아이템의 수량을 변경합니다.
        /// </summary>
        /// <param name="delta">추가/차감할 양.</param>
        void ModifyQuantity(int delta);
    }
}