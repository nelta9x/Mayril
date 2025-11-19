using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯 값 하나를 나타내는 클래스입니다.
    /// </summary>
    [Serializable]
    public class StatValue : ISerializationCallbackReceiver
    {
        public delegate void ValueChangedDelegate(StatValue statValue, float oldValue);
        
        [SerializeField] private string name;
        [SerializeField] private float baseValue;
        [SerializeField] private float value;
        private bool _isDirty;
        private ISerializationCallbackReceiver _serializationCallbackReceiverImplementation;
        private readonly StatModifierContainer _modifiers = new();

        /// <summary>
        /// 값이 변경되었을 때 호출됩니다.
        /// </summary>
        public event ValueChangedDelegate OnValueChanged;

        public StatValue(string statName)
        {
            name = statName;
            Reset(0f);
        }

        public StatValue(string statName, float baseValue)
        {
            name = statName;
            Reset(baseValue);
        }

        /// <summary>
        /// 스탯 이름.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 기반 값.
        /// </summary>
        public float BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                RecalculateValue();
            }
        }

        /// <summary>
        /// 최종 값.
        /// 기반 값에 모든 모다피아어들을 반영했을 때의 값.
        /// </summary>
        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    RecalculateValue();
                }

                return value;
            }
        }
        
        /// <summary>
        /// 모든 모디파이어들.
        /// </summary>
        public StatModifierContainer Modifiers => _modifiers;

        /// <summary>
        /// 현재 값을 초기화합니다.
        /// 초기화 시에는 값 변경 관련 이벤트가 호출되지 않습니다.
        /// </summary>
        /// <param name="newBaseValue">새 기반 값.</param>
        public void Reset(float newBaseValue)
        {
            Reset(newBaseValue, Enumerable.Empty<StatModifier>());
        }

        /// <summary>
        /// 현재 값을 초기화합니다.
        /// 초기화 시에는 값 변경 관련 이벤트가 호출되지 않습니다.
        /// </summary>
        /// <param name="newBaseValue">새 기반 값.</param>
        /// /// <param name="modifiers">모디파이어들.</param>
        public void Reset(float newBaseValue, IEnumerable<StatModifier> modifiers)
        {
            _isDirty = false;
            _modifiers.Clear();
            baseValue = newBaseValue;
            value = baseValue;
            foreach (var modifier in modifiers)
            {
                _modifiers.Add(modifier);
            }

            value = _modifiers.Apply(baseValue);
        }

        /// <summary>
        /// 모디파이어를 추가합니다.
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            _isDirty = true;
            _modifiers.Add(modifier);
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public bool RemoveModifier(StatModifier modifier)
        {
            if (!_modifiers.Remove(modifier))
            {
                return false;
            }
            
            _isDirty = true;

            return true;
        }

        /// <summary>
        /// 값을 재계산합니다.
        /// </summary>
        private void RecalculateValue()
        {
            float oldValue = value;
            value = _modifiers.Apply(baseValue);
            if (value != oldValue)
            {
                OnValueChanged?.Invoke(this, oldValue);
            }

            _isDirty = false;
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// 유니티 직렬화 직후에 호출되는 함수.
        /// </summary>
        public void OnAfterDeserialize()
        {
            RecalculateValue();
        }
    }
}