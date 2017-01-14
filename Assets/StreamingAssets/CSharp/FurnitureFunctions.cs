public static class FurnitureFunctions
{
    public static float GetWorkshopPercentageProgress(Furniture furniture)
    {
        float curProcessingTime = furniture.Parameters["cur_processing_time"].ToFloat();
        float maxProcessingTime = furniture.Parameters["max_processing_time"].ToFloat();
        int isProcessing = furniture.Parameters["cur_processed_inv"].ToInt();

        float perc = 0f;
        if (isProcessing > 0)
        {
            if (maxProcessingTime != 0f)
            {
                perc = curProcessingTime * 100f / maxProcessingTime;
                if (perc > 100f)
                {
                    perc = 100f;
                }
            }
        }
        return perc;
    }

    public static string PowerCellPress_StatusInfo(Furniture furniture)
    {
        return string.Format("Status: {0:0}%", GetWorkshopPercentageProgress(furniture));
    }

    public static string WaterGenerator_StatusInfo(Furniture furniture)
    {
        return string.Format("Ice left: {0:0}%", 100f - GetWorkshopPercentageProgress(furniture));
    }
}