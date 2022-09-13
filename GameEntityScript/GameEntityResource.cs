using System;
using System.Numerics;
using AltV.Net;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using AltV.Net.EntitySync.SpatialPartitions;

namespace GameEntityScript
{
	public class GameEntityResource : Resource 
	{
        private static void InitEntitySync()
        {
            AltEntitySync.Init(
                1,
                (threadId) => 100,
                (threadId) => false,
                (threadCount, repository) => new ServerEventNetworkLayer(threadCount, repository),
                (entity, threadCount) => (entity.Id % threadCount),
                (entityId, entityType, threadCount) => (entityId % threadCount),
                (threadId) => new LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 300),
                new IdProvider()
            );
        }

        private void RegisterExports()
        {
            Alt.Export("createGameEntity", new Func<long, Vector3, int, int, ulong>(this.CreateGameEntity));
            Alt.Export("removeGameEntity", new Action<long, long>(this.RemoveGameEntity));
            Alt.Export("doesGameEntityExist", new Func<long, long, bool>(this.DoesGameEntityExist));
            Alt.Export("setGameEntityPosition", new Action<long, long, Vector3>(this.SetGameEntityPosition));
            Alt.Export("getGameEntityPosition", new Func<long, long, Vector3>(this.GetGameEntityPosition));
            Alt.Export("getGameEntityRange", new Func<long, long, uint>(this.GetGameEntityRange));
            Alt.Export("setGameEntityDimension", new Action<long, long, int>(this.SetGameEntityDimension));
            Alt.Export("getGameEntityDimension", new Func<long, long, int>(this.GetGameEntityDimension));
            Alt.Export("setGameEntityData", new Action<long, long, String, object>(this.SetGameEntityData));
            Alt.Export("getGameEntityData", new Func<long, long, String, object?>(this.GetGameEntityData));
            Alt.Export("resetGameEntityData", new Action<long, long, String>(this.ResetGameEntityData));
        }

        private static IEntity? GetGameEntity(long id, long type)
        {

            if (!AltEntitySync.TryGetEntity((ulong)id, (ulong)type, out IEntity entity))
                return null;

            return entity;
        }

        private ulong CreateGameEntity(long type, Vector3 position, int dimension, int range)
        {
            IEntity entity = AltEntitySync.CreateEntity((ulong) type, position, dimension, (uint) range);

            return entity.Id;
        }

        private void RemoveGameEntity(long id, long type)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity != null) AltEntitySync.RemoveEntity(entity);
        }

        private bool DoesGameEntityExist(long id, long type)
        {
            IEntity? entity = GetGameEntity(id, type);

            return entity != null;
        }

        private void SetGameEntityPosition(long id, long type, Vector3 position)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::SetGameEntityPosition was called with invalid entity!");
                return;
            }

            entity.Position = position;
        }

        private Vector3 GetGameEntityPosition(long id, long type)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::GetGameEntityPosition was called with invalid entity!");
                return new Vector3();
            }

            return entity.Position;
        }

        private uint GetGameEntityRange(long id, long type)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::GetGameEntityRange was called with invalid entity!");
                return 0;
            }

            return entity.Range;
        }

        private void SetGameEntityDimension(long id, long type, int dimension)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::SetGameEntityDimension was called with invalid entity!");
                return;
            }

            entity.Dimension = dimension;
        }

        private int GetGameEntityDimension(long id, long type)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::GetGameEntityDimension was called with invalid entity!");
                return 0;
            }

            return entity.Dimension;
        }

        private void SetGameEntityData(long id, long type, String key, object? value)
        {
            IEntity? entity = GetGameEntity(id, type);

            if(entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::SetGameEntityData was called with invalid entity!");
                return;
            }

            if (value == null)
                entity.ResetData(key);

            else 
                entity.SetData(key, value);
        }

        private object? GetGameEntityData(long id, long type, String key)
        {
            IEntity? entity = GetGameEntity(id, type);

            if (entity == null)
            {
                Alt.Log("[WARN] GameEntityResource::GetGameEntityData was called with invalid entity!");
                return null;
            }


            if (!entity.TryGetData(key, out object result))
            {
                Alt.Log("[WARN] GameEntityResource::GetGameEntityData was called with invalid data key!");
                return null;
            }

            return result;
        }

        private void ResetGameEntityData(long id, long type, String key)
        {
            this.SetGameEntityData(id, type, key, null);
        }

        public override void OnStart()
        {
            InitEntitySync();
            this.RegisterExports();
        }

        public override void OnStop()
        {
            AltEntitySync.Stop();
        }
    }
}
