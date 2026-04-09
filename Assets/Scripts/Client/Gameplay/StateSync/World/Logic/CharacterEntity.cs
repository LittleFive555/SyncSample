using System.Collections.Generic;
using System.Linq;
using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay.StateSync.World.Logic
{
    public class CharacterEntity: ICharacterEntity
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsLocal { get; private set; }

        public float DeltaX { get; private set; }
        public float DeltaY { get;private set; }

        public float X { get; private set; }
        public float Y { get; private set; }

        public const float MoveSpeed = 3f;

        private SortedList<long, Vector2> _predictedInputs = new SortedList<long, Vector2>();
        
        public CharacterEntity(string id, string name, bool isLocal)
        {
            Id = id;
            Name = name;
            IsLocal = isLocal;
        }

        public void ReceiveInput(long frame, float dx, float dy)
        {
            if (!IsLocal)
                return;

            DeltaX = dx;
            DeltaY = dy;
            
            if (GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                X += DeltaX * MoveSpeed * SyncStateWorldManager.Instance.FrameDeltaTime;
                Y += DeltaY * MoveSpeed * SyncStateWorldManager.Instance.FrameDeltaTime;
                _predictedInputs.Add(frame, new Vector2(X, Y));
            }
        }

        public void ReceiveWorldState(long frame, float x, float y, float dx, float dy)
        {
            if (IsLocal && GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                if (_predictedInputs.TryGetValue(frame, out var predictedInput))
                {
                    if (Mathf.Approximately(predictedInput.x, x) && Mathf.Approximately(predictedInput.y, y)) // 预测成功
                    {
                        _predictedInputs.Remove(frame);
                        return;
                    }
                    else // 预测失败
                    {
                        _predictedInputs.Clear();
                    }
                }
            }
            X = x;
            Y = y;
            DeltaX = dx;
            DeltaY = dy;
        }
    }
}