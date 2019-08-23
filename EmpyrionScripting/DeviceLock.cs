﻿using System;
using System.Threading;
using Eleon.Modding;
using EmpyrionNetAPIDefinitions;

namespace EmpyrionScripting
{
    public class DeviceLock : IDeviceLock
    {
        readonly Action unlockAction;

        public DeviceLock(IPlayfield playfield, IStructure structure, VectorInt3 position)
        {
            var witherror = false;
            try
            {
                if (ScriptExecQueue.Iteration % EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles != 0) return;

                if (playfield.IsStructureDeviceLocked(structure.Id, position)) return;

                var lockkey = $"{structure.Id}#{position.x}#{position.y}#{position.z}";

                var e = new AutoResetEvent(false);
                playfield.LockStructureDevice(structure.Id, position, true, (s) =>
                {
                    if (witherror)
                    {
                        Log($"Lock:Callback:Error {playfield.Name} {structure.Id} {position}", LogLevel.Debug);
                        playfield.LockStructureDevice(structure.Id, position, false, null);
                    }
                    else
                    {
                        Success = s;
                        e.Set();
                    }
                });
                witherror = !e.WaitOne(10000);
                if (witherror) Log($"Lock:WaitOne:Error {playfield.Name} {structure.Id} {position}", LogLevel.Debug);

                if (Success) unlockAction = () =>
                 {
                     e.Reset();
                     playfield.LockStructureDevice(structure.Id, position, false, (s) => e.Set());
                     if (!e.WaitOne(10000)) Log($"Unlock:Timeout {playfield.Name} {structure.Id} {position}", LogLevel.Debug);
                 };
            }
            catch (Exception error)
            {
                witherror = true;
                throw new Exception($"DeviceLock:failed on Playfield:{playfield?.Name} at Id:{structure.Id} on {position} with: {error}");
            }
        }

        public static Action<string, LogLevel> Log { get; set; }
        public bool Success { get; private set; }

        public void Dispose()
        {
            unlockAction?.Invoke();
        }
    }
}