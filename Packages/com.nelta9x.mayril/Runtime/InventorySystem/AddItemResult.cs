namespace Mayril.InventorySystem
{
    /// <summary>
    /// 아이템을 인벤토리에 추가한 결과를 나타냅니다.
    /// </summary>
    public readonly struct AddItemResult
    {
        /// <summary>
        /// 작업의 성공 여부입니다.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 아이템이 배치되거나 쌓인 슬롯의 인덱스입니다.
        /// 작업이 실패하면 -1을 반환합니다.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// 기존 아이템과 겹쳐졌는지 여부입니다.
        /// 아이템이 새 슬롯에 추가되었거나 작업이 실패한 경우 false입니다.
        /// </summary>
        public bool WasStacked { get; }

        private AddItemResult(bool success, int slotIndex, bool wasStacked)
        {
            Success = success;
            SlotIndex = slotIndex;
            WasStacked = wasStacked;
        }

        /// <summary>
        /// 작업 실패를 나타내는 결과를 생성합니다.
        /// </summary>
        public static AddItemResult Failed => new AddItemResult(false, -1, false);

        /// <summary>
        /// 아이템이 새 슬롯에 추가되었음을 나타내는 결과를 생성합니다.
        /// </summary>
        /// <param name="slotIndex">아이템이 추가된 슬롯의 인덱스입니다.</param>
        public static AddItemResult Added(int slotIndex) => new AddItemResult(true, slotIndex, false);

        /// <summary>
        /// 아이템이 기존 아이템과 겹쳐졌음을 나타내는 결과를 생성합니다.
        /// </summary>
        /// <param name="slotIndex">아이템이 겹쳐진 슬롯의 인덱스입니다.</param>
        public static AddItemResult Stacked(int slotIndex) => new AddItemResult(true, slotIndex, true);
        
        public static implicit operator bool(AddItemResult result) => result.Success;
    }
}