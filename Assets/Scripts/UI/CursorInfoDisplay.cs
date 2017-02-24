#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Text;
using UnityEngine;

public class CursorInfoDisplay
{
    private MouseController mc;
    private BuildModeController bmc;
    private int validPostionCount;
    private int invalidPositionCount;

    public CursorInfoDisplay(MouseController mouseController, BuildModeController buildModeController)
    {
        mc = mouseController;
        bmc = buildModeController;
    }

    public string MousePosition(Tile t)
    {
        if (t == null)
        {
            return string.Empty;
        }

        return string.Format("X:{0} Y:{1} Z:{2}", t.X.ToString(), t.Y.ToString(), t.Z.ToString());
    }

    public void GetPlacementValidationCounts()
    {
        validPostionCount = invalidPositionCount = 0;

        for (int i = 0; i < mc.GetDragObjects().Count; i++)
        {
            Tile t1 = GetTileUnderDrag(mc.GetDragObjects()[i].transform.position);
            if (World.Current.FurnitureManager.IsPlacementValid(bmc.buildModeType, t1) &&
               (t1.PendingBuildJobs == null || (t1.PendingBuildJobs != null && t1.PendingBuildJobs.Count == 0)))
            {
                validPostionCount++;
            }
            else
            {
                invalidPositionCount++;
            }
        }
    }

    public string ValidBuildPositionCount()
    {
        return validPostionCount.ToString();
    }

    public string InvalidBuildPositionCount()
    {
        return invalidPositionCount.ToString();
    }

    public string GetCurrentBuildRequirements()
    {
        ProjectPorcupine.OrderActions.Build buildOrder = PrototypeManager.Furniture.Get(bmc.buildModeType).GetOrderAction<ProjectPorcupine.OrderActions.Build>();
        if (buildOrder != null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in buildOrder.Inventory)
            {
                string requiredMaterialCount = (item.Amount * validPostionCount).ToString();
                sb.Append(string.Format("{0}x {1}", requiredMaterialCount, item.Type)); // TODO: LocalizationTable.GetLocalization(item.Type))
                if (buildOrder.Inventory.Count > 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        return "furnitureJobPrototypes is null";
    }

    private Tile GetTileUnderDrag(Vector3 gameObject_Position)
    {
        return WorldController.Instance.GetTileAtWorldCoord(gameObject_Position);
    }
}
