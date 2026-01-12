#nullable enable

using System;
using System.Collections.Generic;

namespace Mayril.InventorySystem
{
    /// <summary>
    /// 아이템을 관리하는 데 사용되는 인벤토리 슬롯 컬렉션을 나타냅니다.
    /// </summary>
    public class ItemCollection
    {
        private readonly List<InventorySlot> _slots;
        private readonly List<IItem?> _items;
        private readonly List<int> _occupiedIndices;
        private int _occupiedSlotCount = 0;
        private int _firstAvailableSlotIndex = -1;

        public ItemCollection() : this(0)
        {
        }

        /// <summary>
        /// 지정된 슬롯 수로 <see cref="ItemCollection"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="initialSlots">슬롯 수.</param>
        public ItemCollection(int initialSlots)
        {
            _slots = new List<InventorySlot>(initialSlots);
            _items = new List<IItem?>(initialSlots);
            _occupiedIndices = new List<int>();
            Extend(initialSlots);
        }

        /// <summary>
        /// 인벤토리의 슬롯 데이터 목록입니다.
        /// </summary>
        public IReadOnlyList<InventorySlot> Slots => _slots;

        /// <summary>
        /// 현재 아이템이 들어있는 슬롯 인덱스 목록입니다.
        /// </summary>
        public IReadOnlyList<int> OccupiedIndices => _occupiedIndices;

        /// <summary>
        /// 인벤토리에서 현재 사용 중인 슬롯 수입니다.
        /// </summary>
        public int OccupiedSlotCount
        {
            get => _occupiedSlotCount;
        }

        /// <summary>
        /// 새 아이템을 추가할 수 있는 사용 가능한 슬롯 수입니다.
        /// </summary>
        public int AvailableSlotCount
        {
            get => TotalSlotCount - OccupiedSlotCount;
        }

        /// <summary>
        /// 인벤토리의 총 슬롯 수입니다.
        /// </summary>
        public int TotalSlotCount
        {
            get => _slots.Count;
        }

        /// <summary>
        /// 인벤토리에서 첫 번째 사용 가능한 슬롯의 인덱스입니다. 사용 가능한 슬롯이 없으면 -1입니다.
        /// </summary>
        public int FirstAvailableSlotIndex
        {
            get => _firstAvailableSlotIndex;
        }

        /// <summary>
        /// 제공된 슬롯 인덱스가 유효한지 반환합니다.
        /// </summary>
        /// <param name="slotIndex">확인할 슬롯 인덱스.</param>
        /// <returns>슬롯 인덱스가 유효하면 true, 그렇지 않으면 false.</returns>
        public bool IsValidSlotIndex(int slotIndex)
        {
            return 0 <= slotIndex && slotIndex < _slots.Count;
        }

        /// <summary>
        /// 특정 슬롯이 사용 가능한지(비어 있는지) 확인합니다.
        /// </summary>
        /// <param name="slotIndex">확인할 슬롯의 인덱스.</param>
        /// <returns>슬롯이 사용 가능하면 true, 그렇지 않으면 false.</returns>
        public bool IsAvailableSlot(int slotIndex)
        {
            return IsValidSlotIndex(slotIndex) && _slots[slotIndex].IsEmpty;
        }

        /// <summary>
        /// 특정 슬롯 인덱스의 아이템을 가져옵니다.
        /// </summary>
        /// <param name="slotIndex">아이템을 가져올 슬롯의 인덱스.</param>
        /// <returns>지정된 슬롯 인덱스의 아이템, 슬롯이 비어 있으면 null.</returns>
        public IItem? GetItemAt(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return null;
            }

            return _items[slotIndex];
        }

        /// <summary>
        /// 특정 슬롯에 아이템을 추가합니다.
        /// </summary>
        /// <param name="item">추가할 아이템.</param>
        /// <param name="slotIndex">아이템이 배치될 슬롯 인덱스.</param>
        public bool AddItemAt(IItem? item, int slotIndex)
        {
            if (item == null)
            {
                return false;
            }

            if (!IsAvailableSlot(slotIndex))
            {
                return false;
            }

            _items[slotIndex] = item;
            _slots[slotIndex] = new InventorySlot(item.ItemCode, item.Quantity);
            _occupiedSlotCount++;

            // 사용 중인 슬롯 인덱스 업데이트
            int pos = _occupiedIndices.BinarySearch(slotIndex);
            if (pos < 0)
            {
                _occupiedIndices.Insert(~pos, slotIndex);
            }

            // 사용 가능한 슬롯 인덱스 업데이트
            if (_firstAvailableSlotIndex == slotIndex)
            {
                _firstAvailableSlotIndex = -1;
                for (int i = slotIndex + 1; i < _slots.Count; i++)
                {
                    if (_slots[i].IsEmpty)
                    {
                        _firstAvailableSlotIndex = i;
                        break;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 지정된 슬롯에서 아이템을 제거합니다.
        /// </summary>
        /// <param name="slotIndex">아이템을 제거할 슬롯 인덱스.</param>
        /// <returns>제거된 아이템, 슬롯이 비어 있었으면 null.</returns>
        public IItem? RemoveItemAt(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return null;
            }

            var existingItem = _items[slotIndex];
            if (existingItem == null)
            {
                return null;
            }

            _items[slotIndex] = null;
            _slots[slotIndex] = default;
            _occupiedSlotCount--;

            // 사용 중인 슬롯 인덱스 업데이트
            _occupiedIndices.Remove(slotIndex);

            // 사용 가능한 슬롯 인덱스 업데이트
            if (_firstAvailableSlotIndex == -1 || slotIndex < _firstAvailableSlotIndex)
            {
                _firstAvailableSlotIndex = slotIndex;
            }

            return existingItem;
        }

        /// <summary>
        /// 특정 슬롯의 데이터(수량 등)가 변경되었을 때 호출하여 내부 슬롯 데이터를 업데이트합니다.
        /// </summary>
        public void SyncSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return;
            }
            
            var item = _items[slotIndex];
            if (item != null)
            {
                _slots[slotIndex] = new InventorySlot(item.ItemCode, item.Quantity);
            }
            else
            {
                _slots[slotIndex] = default;
            }
        }

        /// <summary>
        /// 특정 인덱스의 슬롯 자체를 제거합니다. (전체 슬롯 수 감소)
        /// </summary>
        public void RemoveSlotAt(int index)
        {
            if (!IsValidSlotIndex(index))
            {
                return;
            }

            if (_items[index] != null)
            {
                _occupiedSlotCount--;
            }

            _slots.RemoveAt(index);
            _items.RemoveAt(index);

            // 사용 중인 슬롯 인덱스 캐시 업데이트
            for (int i = _occupiedIndices.Count - 1; i >= 0; i--)
            {
                if (_occupiedIndices[i] == index)
                {
                    _occupiedIndices.RemoveAt(i);
                }
                else if (_occupiedIndices[i] > index)
                {
                    _occupiedIndices[i]--;
                }
            }

            // 첫 번째 가용 슬롯 인덱스 재계산
            UpdateFirstAvailableSlotIndex();
        }

        /// <summary>
        /// 첫번째 사용 가능한 슬롯을 업데이트합니다.
        /// </summary>
        private void UpdateFirstAvailableSlotIndex()
        {
            _firstAvailableSlotIndex = -1;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _firstAvailableSlotIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 지정된 수의 추가 슬롯을 추가하여 인벤토리를 확장합니다.
        /// </summary>
        /// <param name="additionalSlotCount">추가할 슬롯 수.</param>
        public void Extend(int additionalSlotCount)
        {
            if (additionalSlotCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(additionalSlotCount), "Additional slots must be a non-negative number.");
            }

            if (additionalSlotCount == 0)
            {
                return;
            }

            int oldSlotCount = _slots.Count;
            for (int i = 0; i < additionalSlotCount; i++)
            {
                _slots.Add(default);
                _items.Add(null);
            }

            // 사용 가능한 슬롯 인덱스 업데이트
            if (_firstAvailableSlotIndex == -1)
            {
                _firstAvailableSlotIndex = oldSlotCount;
            }
        }

        /// <summary>
        /// 끝에서 지정된 수의 슬롯을 제거하여 인벤토리를 축소합니다.
        /// 제거된 슬롯의 아이템은 손실됩니다.
        /// </summary>
        /// <param name="sizeToShrink">제거할 슬롯 수.</param>
        /// <param name="itemsToLose">축소 중 손실될 아이템으로 채워질 리스트.</param>
        public void Shrink(int sizeToShrink, List<IItem>? itemsToLose = null)
        {
            if (sizeToShrink == 0)
            {
                return;
            }

            if (sizeToShrink < 0 || sizeToShrink > TotalSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeToShrink),
                    "Size to shrink must be between 0 and the total number of slots.");
            }

            // 리스트 끝에서 지정된 수의 슬롯을 제거합니다.
            int oldTotalSlotCount = TotalSlotCount;
            int startIndex = oldTotalSlotCount - sizeToShrink;

            // 손실될 아이템을 수집하고 캐시를 업데이트합니다
            for (int i = startIndex; i < oldTotalSlotCount; i++)
            {
                var item = _items[i];
                if (item != null)
                {
                    itemsToLose?.Add(item);
                    _occupiedSlotCount--;
                    _occupiedIndices.Remove(i);
                }
            }

            _slots.RemoveRange(startIndex, sizeToShrink);
            _items.RemoveRange(startIndex, sizeToShrink);

            // 제거된 범위에 있었던 경우 사용 가능한 슬롯 인덱스를 업데이트합니다.
            if (_firstAvailableSlotIndex != -1 && _firstAvailableSlotIndex >= _slots.Count)
            {
                _firstAvailableSlotIndex = -1;
                for (int i = 0; i < _slots.Count; i++)
                {
                    if (_slots[i].IsEmpty)
                    {
                        _firstAvailableSlotIndex = i;
                        break;
                    }
                }
            }
        }
    }
}