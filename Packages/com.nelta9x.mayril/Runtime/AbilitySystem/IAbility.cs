using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 게임 내 어빌리티(스킬, 아이템으로 인한 효과 등)를 표현하는 클래스입니다.
    /// </summary>
    public interface IAbility
    {
        /// <summary>
        /// 어빌리티 비헤이비어.
        /// </summary>
        public AbilityBehavior Behavior { get; }
        
        /// <summary>
        /// 어빌리티 이름.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 어빌리티 소유자가 스폰되었을 때 호출됩니다.
        /// </summary>
        public void OnOwnerSpawned();

        /// <summary>
        /// 어빌리티가 소유자에게 추가된 후 호출됩니다.
        /// </summary>
        public void OnAdded();

        /// <summary>
        /// 어빌리티가 소유자에게서 제거된 후 호출됩니다.
        /// </summary>
        public void OnRemoved();

        /// <summary>
        /// 어빌리티 소유자가 죽었을 때 호출됩니다.
        /// </summary>
        public void OnOwnerDied();

        /// <summary>
        /// 소유자가 이동했을 때 호출됩니다.
        /// </summary>
        public void OnOwnerMoved();

        /// <summary>
        /// 시전 준비를 시작 시 호출됩니다. (마나 소모 전)
        /// </summary>
        public bool OnAbilityPhaseStart();

        /// <summary>
        /// 시전 준비 중단 시 호출됩니다. (기절, 침묵 등)
        /// </summary>
        public void OnAbilityPhaseInterrupted();

        /// <summary>
        /// 스펠을 시작할 때 호출됩니다. (마나 소모 후)
        /// </summary>
        public void OnSpellStart();

        /// <summary>
        /// 채널링이 종료되었을 때 호출됩니다.
        /// </summary>
        public void OnAbilityEndChannel();

        /// <summary>
        /// 발사체가 히트되었을 때 호출됩니다.
        /// </summary>
        public void OnProjectileHitEntity(Entity projectileEntity);

        /// <summary>
        /// 발사체가 종료되었을 때 호출됩니다.
        /// </summary>
        public void OnProjectileFinish(Entity projectileEntity);

        /// <summary>
        /// 내가 누군가를 죽였을 때 호출됩니다.
        /// </summary>
        public void OnKill();

        /// <summary>
        /// 내가 전리품을 획득했을 때 호출됩니다.
        /// </summary>
        public void OnLoot(Entity loot);

        /// <summary>
        /// 피해를 줬을 때 호출됩니다.
        /// </summary>
        public void OnDamage();

        /// <summary>
        /// 피해를 받았을 때 호출됩니다.
        /// </summary>
        public void OnDamaged();
    }
}