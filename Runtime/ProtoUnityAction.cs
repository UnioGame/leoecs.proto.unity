// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.Unity {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    [DefaultExecutionOrder (100)]
    public abstract class ProtoUnityAction<T> : MonoBehaviour where T : struct {
        [SerializeField] string _senderName;
        [SerializeField] string _worldName;

        ProtoPool<T> _pool;
        GameObject _sender;
        int _senderHash;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public string WorldName () => _senderName;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        protected virtual bool IsValidForEvent () => ProtoUnityWorlds.Connected ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        protected GameObject Sender () => _sender;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public string SenderName () => _senderName;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        protected int SenderHash () => _senderHash;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        protected ref T NewEvent () {
            if (_pool == null) {
                var world = ProtoUnityWorlds.Get (string.IsNullOrEmpty (_worldName) ? default : _worldName);
                _pool = (ProtoPool<T>) world.Pool (typeof (T));
                _sender = gameObject;
                _senderHash = _sender.GetInstanceID ();
            }
            _pool.NewEntity (out var e);
            return ref _pool.Get (e);
        }
    }
}
