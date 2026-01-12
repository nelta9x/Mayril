#nullable enable

using System;
using Unity.Netcode;

namespace Mayril.InventorySystem
{
    /// <summary>
    /// 인벤토리의 단일 슬롯 데이터를 나타내는 구조체입니다.
    /// 네트워크 전송을 위해 직렬화 가능하며, 아이템 코드와 수량 정보를 담습니다.
    /// </summary>
    public struct InventorySlot : INetworkSerializable, IEquatable<InventorySlot>
    {
        /// <summary>
        /// 아이템의 고유 코드입니다. 0 이하는 빈 슬롯을 의미합니다.
        /// </summary>
        public int ItemCode;

        /// <summary>
        /// 아이템의 수량입니다.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// 슬롯이 비어있는지 여부를 반환합니다.
        /// 아이템 코드가 0 이하이거나 수량이 0 이하인 경우 true를 반환합니다.
        /// </summary>
        public bool IsEmpty => ItemCode <= 0 || Quantity <= 0;

        /// <summary>
        /// 지정된 아이템 코드와 수량으로 <see cref="InventorySlot"/> 구조체의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="itemCode">아이템 고유 코드.</param>
        /// <param name="quantity">아이템 수량.</param>
        public InventorySlot(int itemCode, int quantity)
        {
            ItemCode = itemCode;
            Quantity = quantity;
        }

        /// <summary>
        /// 네트워크를 통해 슬롯 데이터를 읽거나 씁니다.
        /// </summary>
        /// <typeparam name="T">읽기 또는 쓰기 인터페이스 타입.</typeparam>
        /// <param name="serializer">데이터를 처리할 직렬화 도구.</param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ItemCode);
            serializer.SerializeValue(ref Quantity);
        }

        /// <summary>
        /// 현재 슬롯이 다른 슬롯과 동일한 데이터를 가지고 있는지 확인합니다.
        /// </summary>
        /// <param name="other">비교할 다른 슬롯.</param>
        /// <returns>데이터가 동일하면 true, 그렇지 않으면 false.</returns>
        public bool Equals(InventorySlot other)
        {
            return ItemCode == other.ItemCode && Quantity == other.Quantity;
        }

        /// <summary>
        /// 지정된 객체가 현재 슬롯과 동일한 <see cref="InventorySlot"/>인지 확인합니다.
        /// </summary>
        /// <param name="obj">비교할 객체.</param>
        /// <returns>객체가 InventorySlot이고 데이터가 동일하면 true, 그렇지 않으면 false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is InventorySlot other && Equals(other);
        }

        /// <summary>
        /// 이 인스턴스의 해시 코드를 반환합니다.
        /// </summary>
        /// <returns>해시 코드 값.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemCode, Quantity);
        }

        /// <summary>
        /// 두 슬롯의 데이터가 동일한지 비교합니다.
        /// </summary>
        public static bool operator ==(InventorySlot left, InventorySlot right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 두 슬롯의 데이터가 다른지 비교합니다.
        /// </summary>
        public static bool operator !=(InventorySlot left, InventorySlot right)
        {
            return !left.Equals(right);
        }
    }
}