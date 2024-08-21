namespace Mindflur.IMS.Data.Repository.Constants
{
    public static class CategoryToQueryList
    {
        public static IDictionary<int, string> GetQueries()
        {
            IDictionary<int, string> keyValuePairs = new Dictionary<int, string>();

            keyValuePairs.Add(1, QueryConstants.GET_Donuts_ChartData_NonConformance_ByDepartment);
            keyValuePairs.Add(2, QueryConstants.GET_Donuts_ChartData_NonConformance_ByNCTypes);
            keyValuePairs.Add(3, QueryConstants.GET_Donuts_ChartData_NonConformance_ByStatus);
            keyValuePairs.Add(4, QueryConstants.GET_BarGraph_ChartData_NonConformance_Open_Close);
            keyValuePairs.Add(5, QueryConstants.GET_BarGraph_ChartData_CorrectiveAction_Status);
            keyValuePairs.Add(6, QueryConstants.GET_BarGraph_ChartData_Task_Status);

            return keyValuePairs;
        }
    }
}