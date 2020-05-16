﻿using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions : ICsScriptFunctions
    {
        public IList<IItemMoveInfo> Move(IItemsData item, IStructureData structure, string names, int? maxLimit = null) => ConveyorHelpers.Move(Root, item, structure, names, maxLimit);
        public IList<IItemMoveInfo> Fill(IItemsData item, IStructureData structure, StructureTankType type, int? maxLimit = null) => ConveyorHelpers.Fill(Root, item, structure, type, maxLimit ?? 100);
        public bool IsLocked(IStructureData structure, IBlockData block) => Root.GetCurrentPlayfield().IsStructureDeviceLocked(structure.GetCurrent().Id, block.Position);
        public void WithLockedDevice(IStructureData structure, IBlockData block, Action action, Action lockFailed = null)
        {
            using (var locked = ConveyorHelpers.CreateDeviceLock(Root, Root.GetCurrentPlayfield(), structure.GetCurrent(), block.Position))
            {
                if (locked.Success) action();
                else                lockFailed?.Invoke();
            }
        }
    }
}
