#nullable enable

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Mayril.InventorySystem
{
    /// <summary>
    /// 아이템 추가 이벤트용 델리게이트입니다.
    /// </summary>
    /// <param name="item">추가된 아이템입니다.</param>
    /// <param name="slotIndex">아이템이 추가된 슬롯 인덱스입니다.</param>
    public delegate void ItemAddedDelegate(IItem item, int slotIndex);

    /// <summary>
    /// 아이템 제거 이벤트용 델리게이트입니다.
    /// </summary>
    /// <param name="item">제거된 아이템입니다.</param>
    /// <param name="slotIndex">아이템이 제거된 슬롯 인덱스입니다.</param>
    public delegate void ItemRemovedDelegate(IItem item, int slotIndex);

    /// <summary>
    /// 아이템 수량 변경 이벤트용 델리게이트입니다.
    /// </summary>
    /// <param name="item">수량이 변경된 아이템입니다.</param>
    /// <param name="slotIndex">아이템의 슬롯 인덱스입니다.</param>
    /// <param name="oldQuantity">변경 전 수량입니다.</param>
    /// <param name="newQuantity">변경 후 수량입니다.</param>
    public delegate void ItemQuantityChangedDelegate(IItem item, int slotIndex, int oldQuantity, int newQuantity);

    /// <summary>
    /// 아이템 이동 이벤트용 델리게이트입니다.
    /// </summary>
    /// <param name="item">이동된 아이템입니다.</param>
    /// <param name="fromIndex">이전 슬롯 인덱스입니다.</param>
    /// <param name="toIndex">새 슬롯 인덱스입니다.</param>
    public delegate void ItemMovedDelegate(IItem item, int fromIndex, int toIndex);

    /// <summary>
    /// 아이템을 보유하고 관리할 수 있는 인벤토리 컴포넌트의 기본 추상 클래스입니다.
    /// 아이템 추가, 제거, 수정 기능과 인벤토리 상태 추적 기능을 제공합니다.
    /// </summary>
    public abstract class Inventory : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Min(0)] private int initialSlotCount;
        
        /// <summary>
        /// 서버 권한을 가지고 있는지 여부입니다. 테스트를 위해 virtual로 선언합니다.
        /// </summary>
        protected virtual bool IsServerInstance => IsServer;

        /// <summary>
        /// 아이템이 인벤토리에 추가될 때 발생하는 이벤트입니다.
        /// </summary>
        public event ItemAddedDelegate? OnItemAdded;

        /// <summary>
        /// 아이템이 인벤토리에서 제거될 때 발생하는 이벤트입니다.
        /// </summary>
        public event ItemRemovedDelegate? OnItemRemoved;

        /// <summary>
        /// 인벤토리 내 아이템의 수량이 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        public event ItemQuantityChangedDelegate? OnItemQuantityChanged;

        /// <summary>
        /// 아이템의 위치가 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        public event ItemMovedDelegate? OnItemMoved;
        
        private ItemCollection _items = null!;
        private NetworkList<InventorySlot> _networkSlots = null!;

        /// <summary>
        /// 컴포넌트가 깨어날 때 호출되며, 아이템 컬렉션과 네트워크 슬롯 리스트를 초기화합니다.
        /// </summary>
        protected virtual void Awake()
        {
            _items = new ItemCollection(initialSlotCount);
            _networkSlots = new NetworkList<InventorySlot>(null);
        }

        /// <summary>
        /// 네트워크 객체가 스폰될 때 호출됩니다.
        /// 서버인 경우 초기 아이템 데이터를 네트워크 슬롯에 동기화하고, 클라이언트는 리스트 변경 이벤트를 구독합니다.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _networkSlots.Clear();
                for (int i = 0; i < _items.TotalSlotCount; i++)
                {
                    _networkSlots.Add(_items.Slots[i]);
                }
            }
            
            _networkSlots.OnListChanged += OnNetworkSlotsChanged;
        }

        /// <summary>
        /// 네트워크 객체가 디스폰될 때 호출됩니다.
        /// 네트워크 슬롯 리스트 변경 이벤트 구독을 해제합니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            _networkSlots.OnListChanged -= OnNetworkSlotsChanged;
        }

        /// <summary>
        /// 아이템 코드와 수량을 바탕으로 실제 아이템 객체를 생성합니다.
        /// 클라이언트 사이드 동기화 시 호출됩니다.
        /// </summary>
        /// <param name="itemCode">아이템 고유 코드입니다.</param>
        /// <param name="quantity">아이템 수량입니다.</param>
        /// <returns>생성된 아이템 객체입니다.</returns>
        protected abstract IItem? CreateItem(int itemCode, int quantity);

        /// <summary>
        /// 이 인벤토리의 아이템 컬렉션을 가져옵니다.
        /// </summary>
        public ItemCollection Items => _items;

        /// <summary>
        /// 현재 아이템이 들어있는 슬롯 목록입니다. (인덱스 반환)
        /// </summary>
        public IEnumerable<int> OccupiedIndices => _items.OccupiedIndices;
        
        /// <summary>
        /// 현재 아이템이 들어있는 슬롯의 개수입니다.
        /// </summary>
        public int OccupiedSlotCount => _items.OccupiedSlotCount;

        /// <summary>
        /// 새로운 아이템을 추가할 수 있는 빈 슬롯의 개수입니다.
        /// </summary>
        public int AvailableSlotCount => _items.AvailableSlotCount;

        /// <summary>
        /// 인벤토리의 전체 슬롯 개수입니다.
        /// </summary>
        public int TotalSlotCount => _items.TotalSlotCount;
        
        /// <summary>
        /// 인벤토리에서 첫 번째로 비어있는 슬롯의 인덱스입니다. 비어있는 슬롯이 없으면 -1을 반환합니다.
        /// </summary>
        public int FirstAvailableSlotIndex => _items.FirstAvailableSlotIndex;
        
        /// <summary>
        /// 특정 슬롯 인덱스에 있는 아이템을 가져옵니다.
        /// </summary>
        /// <param name="slotIndex">아이템을 가져올 슬롯의 인덱스입니다.</param>
        /// <returns>지정된 슬롯 인덱스의 아이템을 반환하며, 슬롯이 비어있으면 null을 반환합니다.</returns>
        public IItem? GetItemAt(int slotIndex) => _items.GetItemAt(slotIndex);
        
        /// <summary>
        /// 특정 슬롯이 비어있는지(사용 가능한지) 확인합니다.
        /// </summary>
        /// <param name="slotIndex">확인할 슬롯의 인덱스입니다.</param>
        /// <returns>슬롯이 비어있으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool IsAvailableSlot(int slotIndex) => _items.IsAvailableSlot(slotIndex);

        /// <summary>
        /// 아이템을 중첩하지 않고 특정 슬롯에 직접 설정합니다. (서버 전용)
        /// </summary>
        /// <param name="item">설정할 아이템입니다.</param>
        /// <param name="slotIndex">아이템이 배치될 슬롯의 인덱스입니다.</param>
        /// <returns>아이템이 성공적으로 설정되면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool SetItemAt(IItem item, int slotIndex)
        {
            if (!IsServerInstance)
            {
                return false;
            }

            if (GetItemAt(slotIndex) != null)
            {
                return false;
            }

            if (!_items.AddItemAt(item, slotIndex))
            {
                return false;
            }

            _networkSlots[slotIndex] = _items.Slots[slotIndex];
            OnItemAdded?.Invoke(item, slotIndex);

            return true;
        }

        /// <summary>
        /// 아이템을 특정 슬롯에 추가합니다. (서버 전용)
        /// </summary>
        /// <param name="item">추가할 아이템입니다.</param>
        /// <param name="slotIndex">아이템이 배치될 슬롯의 인덱스입니다.</param>
        public AddItemResult AddItemAt(IItem item, int slotIndex)
        {
            if (!IsServerInstance)
            {
                return AddItemResult.Failed;
            }

            var slotItem = GetItemAt(slotIndex);
            if (slotItem != null)
            {
                if (ModifyItemQuantityAt(slotIndex, item.Quantity))
                {
                    item.ModifyQuantity(-item.Quantity);
                    return AddItemResult.Stacked(slotIndex);
                }

                return AddItemResult.Failed;
            }

            if (!SetItemAt(item, slotIndex))
            {
                return AddItemResult.Failed;
            }

            return AddItemResult.Added(slotIndex);
        }

        /// <summary>
        /// 지정된 슬롯에서 아이템을 제거합니다. (서버 전용)
        /// </summary>
        /// <param name="slotIndex">아이템을 제거할 슬롯의 인덱스입니다.</param>
        /// <returns>제거된 아이템을 반환하며, 슬롯이 비어있으면 null을 반환합니다.</returns>
        public IItem? RemoveItemAt(int slotIndex)
        {
            if (!IsServerInstance)
            {
                return null;
            }

            var item = _items.RemoveItemAt(slotIndex);
            if (item == null)
            {
                return null;
            }

            _networkSlots[slotIndex] = default;
            OnItemRemoved?.Invoke(item, slotIndex);

            return item;
        }

        /// <summary>
        /// 두 슬롯의 아이템 위치를 서로 바꿉니다. (서버 전용)
        /// </summary>
        /// <param name="indexA">첫 번째 슬롯 인덱스입니다.</param>
        /// <param name="indexB">두 번째 슬롯 인덱스입니다.</param>
        /// <returns>성공적으로 위치가 변경되면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool SwapSlots(int indexA, int indexB)
        {
            if (!IsServerInstance)
            {
                return false;
            }

            if (!IsValidSlotIndex(indexA) || !IsValidSlotIndex(indexB))
            {
                return false;
            }

            if (indexA == indexB)
            {
                return true;
            }

            var itemA = _items.GetItemAt(indexA);
            var itemB = _items.GetItemAt(indexB);

            // 같은 아이템이고 쌓기가 가능하면 합치기 시도
            if (itemA != null && itemB != null && itemA.ItemCode == itemB.ItemCode && itemB.CanStack(itemA, itemA.Quantity))
            {
                if (ModifyItemQuantityAt(indexB, itemA.Quantity))
                {
                    RemoveItemAt(indexA);
                    return true;
                }
            }

            // 단순 위치 교환
            _items.RemoveItemAt(indexA);
            _items.RemoveItemAt(indexB);

            if (itemA != null)
            {
                _items.AddItemAt(itemA, indexB);
            }

            if (itemB != null)
            {
                _items.AddItemAt(itemB, indexA);
            }

            _networkSlots[indexA] = _items.Slots[indexA];
            _networkSlots[indexB] = _items.Slots[indexB];

            if (itemA != null)
            {
                OnItemMoved?.Invoke(itemA, indexA, indexB);
            }

            if (itemB != null)
            {
                OnItemMoved?.Invoke(itemB, indexB, indexA);
            }

            return true;
        }

        /// <summary>
        /// 특정 슬롯의 아이템 일부를 다른 슬롯으로 나눕니다. (서버 전용)
        /// </summary>
        /// <param name="fromIndex">원본 슬롯 인덱스입니다.</param>
        /// <param name="toIndex">대상 슬롯 인덱스입니다.</param>
        /// <param name="quantityToSplit">나눌 수량입니다.</param>
        /// <returns>성공적으로 나누어지면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool SplitStack(int fromIndex, int toIndex, int quantityToSplit)
        {
            if (!IsServerInstance)
            {
                return false;
            }

            if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex))
            {
                return false;
            }

            if (fromIndex == toIndex || quantityToSplit <= 0)
            {
                return false;
            }

            var sourceItem = GetItemAt(fromIndex);
            if (sourceItem == null || sourceItem.Quantity <= quantityToSplit)
            {
                return false;
            }

            var targetItem = GetItemAt(toIndex);
            if (targetItem != null)
            {
                // 대상 슬롯에 이미 아이템이 있고 동일한 종류라면 합치기 시도
                if (targetItem.ItemCode == sourceItem.ItemCode && targetItem.CanStack(sourceItem, quantityToSplit))
                {
                    if (ModifyItemQuantityAt(toIndex, quantityToSplit))
                    {
                        ModifyItemQuantityAt(fromIndex, -quantityToSplit);
                        return true;
                    }
                }
                return false;
            }

            // 대상 슬롯이 비어있는 경우 새 아이템 생성
            var newItem = CreateItem(sourceItem.ItemCode, quantityToSplit);
            if (newItem == null)
            {
                return false;
            }

            if (SetItemAt(newItem, toIndex))
            {
                ModifyItemQuantityAt(fromIndex, -quantityToSplit);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 특정 슬롯에 있는 아이템의 수량을 수정합니다. (서버 전용)
        /// </summary>
        /// <param name="slotIndex">아이템이 포함된 슬롯의 인덱스입니다.</param>
        /// <param name="quantityToModify">추가하거나 차감할 수량입니다.</param>
        public bool ModifyItemQuantityAt(int slotIndex, int quantityToModify)
        {
            if (!IsServerInstance)
            {
                return false;
            }

            var item = _items.GetItemAt(slotIndex);
            if (item == null)
            {
                return false;
            }

            if (!item.CanModifyQuantity(quantityToModify))
            {
                return false;
            }

            int oldQuantity = item.Quantity;
            item.ModifyQuantity(quantityToModify);
            
            if (item.Quantity <= 0)
            {
                RemoveItemAt(slotIndex);
            }
            else
            {
                _items.SyncSlot(slotIndex);
                _networkSlots[slotIndex] = _items.Slots[slotIndex];
                OnItemQuantityChanged?.Invoke(item, slotIndex, oldQuantity, item.Quantity);
            }

            return true;
        }
        
        /// <summary>
        /// 아이템을 인벤토리에 추가하려고 시도합니다. (서버 전용)
        /// </summary>
        public AddItemResult AddItem(IItem item)
        {
            if (!IsServerInstance)
            {
                return AddItemResult.Failed;
            }

            if (item.MaxStack > 1)
            {
                for (int i = 0; i < TotalSlotCount; i++)
                {
                    var existingItem = GetItemAt(i);
                    if (existingItem != null &&
                        existingItem.ItemCode == item.ItemCode &&
                        existingItem.CanStack(item, item.Quantity))
                    {
                        int quantityToAdd = item.Quantity;
                        if (ModifyItemQuantityAt(i, quantityToAdd))
                        {
                            item.ModifyQuantity(-quantityToAdd);
                            return AddItemResult.Stacked(i);
                        }
                    }
                }
            }

            int slotIndex = FirstAvailableSlotIndex;
            if (!SetItemAt(item, slotIndex))
            {
                return AddItemResult.Failed;
            }

            return AddItemResult.Added(slotIndex);
        }

        /// <summary>
        /// 지정된 수만큼 슬롯을 추가하여 인벤토리를 확장합니다. (서버 전용)
        /// </summary>
        public void Extend(int additionalSlotCount)
        {
            if (!IsServerInstance)
            {
                return;
            }

            _items.Extend(additionalSlotCount);
            for (int i = 0; i < additionalSlotCount; i++)
            {
                _networkSlots.Add(default);
            }
        }

        /// <summary>
        /// 끝에서부터 지정된 수만큼 슬롯을 제거하여 인벤토리를 축소합니다. (서버 전용)
        /// </summary>
        public void Shrink(int sizeToShrink, List<IItem>? itemsToLose = null)
        {
            if (!IsServerInstance)
            {
                return;
            }

            _items.Shrink(sizeToShrink, itemsToLose);
            for (int i = 0; i < sizeToShrink; i++)
            {
                _networkSlots.RemoveAt(_networkSlots.Count - 1);
            }
        }
        
        /// <summary>
        /// 유효한 슬롯 인덱스 여부를 반환합니다.
        /// </summary>
        /// <param name="slotIndex">확인할 슬롯 인덱스</param>
        /// <returns>유효한 슬롯 인덱스 여부</returns>
        public bool IsValidSlotIndex(int slotIndex) => _items.IsValidSlotIndex(slotIndex);

        /// <summary>
        /// 네트워크 슬롯 리스트의 내용이 변경되었을 때 호출되는 콜백입니다.
        /// 서버로부터 동기화된 데이터를 바탕으로 로컬 아이템 컬렉션을 업데이트합니다.
        /// </summary>
        /// <param name="changeEvent">발생한 리스트 변경 이벤트 상세 정보입니다.</param>
        private void OnNetworkSlotsChanged(NetworkListEvent<InventorySlot> changeEvent)
        {
            if (IsServerInstance)
            {
                return; // 서버는 직접 제어하므로 무시
            }

            int index = changeEvent.Index;
            switch (changeEvent.Type)
            {
                case NetworkListEvent<InventorySlot>.EventType.Add:
                    while (_items.TotalSlotCount <= index)
                    {
                        _items.Extend(1);
                    }
                    UpdateLocalSlot(index, changeEvent.Value);
                    break;
                case NetworkListEvent<InventorySlot>.EventType.Insert:
                    // 현재 ItemCollection은 Insert를 지원하지 않으므로 필요한 경우 추가 구현이 필요합니다.
                    break;
                case NetworkListEvent<InventorySlot>.EventType.Remove:
                case NetworkListEvent<InventorySlot>.EventType.RemoveAt:
                    _items.RemoveSlotAt(index);
                    break;
                case NetworkListEvent<InventorySlot>.EventType.Value:
                    UpdateLocalSlot(index, changeEvent.Value);
                    break;
                case NetworkListEvent<InventorySlot>.EventType.Clear:
                    _items.Shrink(_items.TotalSlotCount);
                    break;
            }
        }

        /// <summary>
        /// 클라이언트 사이드에서 특정 인덱스의 로컬 아이템 데이터를 네트워크 데이터와 동기화합니다.
        /// </summary>
        /// <param name="index">동기화할 슬롯 인덱스입니다.</param>
        /// <param name="networkSlot">네트워크로부터 수신한 슬롯 데이터입니다.</param>
        private void UpdateLocalSlot(int index, InventorySlot networkSlot)
        {
            var localItem = _items.GetItemAt(index);
            
            if (networkSlot.IsEmpty)
            {
                if (localItem != null)
                {
                    var removed = _items.RemoveItemAt(index);
                    if (removed != null)
                    {
                        OnItemRemoved?.Invoke(removed, index);
                    }
                }
            }
            else
            {
                if (localItem == null)
                {
                    var newItem = CreateItem(networkSlot.ItemCode, networkSlot.Quantity);
                    if (newItem != null)
                    {
                        _items.AddItemAt(newItem, index);
                        OnItemAdded?.Invoke(newItem, index);
                    }
                    else
                    {
                        Debug.LogWarning($"[Inventory] Failed to create item for ItemCode: {networkSlot.ItemCode} at index {index}");
                    }
                }
                else if (localItem.ItemCode != networkSlot.ItemCode)
                {
                    var removed = _items.RemoveItemAt(index);
                    if (removed != null)
                    {
                        OnItemRemoved?.Invoke(removed, index);
                    }
                    
                    var newItem = CreateItem(networkSlot.ItemCode, networkSlot.Quantity);
                    if (newItem != null)
                    {
                        _items.AddItemAt(newItem, index);
                        OnItemAdded?.Invoke(newItem, index);
                    }
                    else
                    {
                        Debug.LogWarning($"[Inventory] Failed to create item for ItemCode: {networkSlot.ItemCode} at index {index}");
                    }
                }
                else if (localItem.Quantity != networkSlot.Quantity)
                {
                    int oldQuantity = localItem.Quantity;
                    localItem.ModifyQuantity(networkSlot.Quantity - oldQuantity);
                    _items.SyncSlot(index);
                    OnItemQuantityChanged?.Invoke(localItem, index, oldQuantity, localItem.Quantity);
                }
            }
        }

        protected virtual void OnValidate()
        {
            if (initialSlotCount < 0)
            {
                initialSlotCount = 0;
            }
        }
    }
}