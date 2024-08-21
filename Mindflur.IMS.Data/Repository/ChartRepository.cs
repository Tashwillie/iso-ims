using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Data.Models.Custom;
using Mindflur.IMS.Data.Repository.Constants;
using AuditFindingChart = Mindflur.IMS.Data.Models.Custom.AuditFindingChart;

namespace Mindflur.IMS.Data.Repository
{
	public class ChartRepository : IChartRepository
	{
		private readonly IConfiguration _configuration;

		public ChartRepository(ILogger<ChartRepository> logger, IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<IList<DounutsChartItem>> GetDounutsChartResultBySQLQuery(string query)
		{
			IList<DounutsChartItem> dounutsChartItems = new List<DounutsChartItem>();

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<DounutsChartItem>(query);

			dounutsChartItems = result.ToList();

			return dounutsChartItems;
		}

		public async Task<IList<BarGraphChartItem>> GetBarChartResultBySQLQuery(string query)
		{
			IList<BarGraphChartItem> barGraphChartItem = null;

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<BarGraphChartItem>(query);

			barGraphChartItem = result.ToList();

			return barGraphChartItem;
		}

		public async Task<IList<ComplianceClause>> GetBarChartResultBySQLQuery1(string query)
		{
			IList<ComplianceClause> barGraphChartItem = null;

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<ComplianceClause>(query);

			barGraphChartItem = result.ToList();

			return barGraphChartItem;
		}

		public async Task<IList<surveyResponse>> GetBarChartResultBySQLQuery21(string query)
		{
			IList<surveyResponse> barGraphChartItem = null;

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<surveyResponse>(query);

			barGraphChartItem = result.ToList();

			return barGraphChartItem;
		}

		public async Task<IList<AuditFindingChart>> GetBarChartResultBySQLQuery2(string query)
		{
			IList<AuditFindingChart> barGraphChartItem = null;

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<AuditFindingChart>(query);

			barGraphChartItem = result.ToList();

			return barGraphChartItem;
		}

		public async Task<IList<AuditFindingRiskRatingChart>> GetBarChartResultBySQLQuery3(string query)
		{
			IList<AuditFindingRiskRatingChart> barGraphChartItem = null;

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<AuditFindingRiskRatingChart>(query);

			barGraphChartItem = result.ToList();

			return barGraphChartItem;
		}

		public async Task<IList<ComplianceClause>> GetAuditComplianceBarChartResultBySQLQuery(string query)
		{
			IList<ComplianceClause> barGraphChartItem = null;

			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();

			var result = await conn.QueryAsync<ComplianceClause>(query);

			barGraphChartItem = result.ToList();

			return barGraphChartItem;
		}

		public async Task<IList<BarGraphChartItem>> GetChartBarGraph(int category, int tenantId)
		{
			IList<BarGraphChartItem> barGraphChartItems = null;
			switch (category)
			{
				case 1:
					var baseQuesry = string.Format(QueryConstants.GET_BarGraph_ChartData_Task_Status, tenantId);

					barGraphChartItems = await GetBarChartResultBySQLQuery(baseQuesry);
					break;

				case 2:
					var baseQuery = string.Format(QueryConstants.GET_BarGraph_ChartData_NonConformance_Open_Close, tenantId);
					barGraphChartItems = await GetBarChartResultBySQLQuery(baseQuery);
					break;

				case 3:
					var baseQuery1 = string.Format(QueryConstants.GET_BarGraph_ChartData_CorrectiveAction_Status, tenantId);
					barGraphChartItems = await GetBarChartResultBySQLQuery(baseQuery1);
					break;
			}

			return barGraphChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetNonConformanceDonutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_NonConformance_ByDepartment, tenantId, (int)IMSModules.NonConformity);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_NonConformance_ByStatus, tenantId, (int)IMSModules.NonConformity);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_NonConformance_ByNCTypes, tenantId, (int)IMSModules.NonConformity);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetTaskMasterDonutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_TaskMAster_ByStatus, tenantId, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_TaskMaster_By_AssignTo, tenantId, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_TaskMaster_By_Priority, tenantId, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_Donuts_ChartData_TaskMAster_BySource, tenantId, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;

				case 5:
					var baseQuery4 = string.Format(QueryConstants.GET_Donuts_ChartData_TaskMaster_Classification, tenantId, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery4);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetCorrectiveActionDonutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_CorrectiveACtionTask_ByStatus, tenantId, (int)IMSModules.CorrectiveAction);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_CorrectiveACtionTask_ByAssignTo, tenantId, (int)IMSModules.CorrectiveAction);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_CorrectiveACtionTask_ByPriority, tenantId, (int)IMSModules.CorrectiveAction);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetInherentRiskDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_InherentRisk_ByDepartment, tenantId, (int)IMSModules.RiskManagement);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_InherentRisk_ByRiskType, tenantId, (int)IMSModules.RiskManagement);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_InherentRisk_ByRiskStatus, tenantId, (int)IMSModules.RiskManagement);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_Donuts_ChartData_InherentRisk_AllRiskTreatmentByStatus, tenantId, (int)IMSModules.RiskManagement, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetSiteOfIncidentDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_SiteOfIncident_ByDepartment, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_SiteOfIncident_By_Employee, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_SiteOfIncident_By_Status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_Donuts_ChartData_SiteOfIncidentCorrectiveAction_By_Status, tenantId, (int)IMSModules.IncidentManagement, (int)IMSModules.CorrectiveAction);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetRiskTreatmentDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_RiskTreatment_ByStatus, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_RiskTreatment_ByRiskRating, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_RiskTreatment_ByResponsiblePerson, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetProjectManagementDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_Projects_ByStatus, tenantId, (int)IMSModules.ProjectManagement);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Donuts_ChartData_Projects_ByPriority, tenantId, (int)IMSModules.WorkItemPriority);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_Projects_ByUser, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_Donuts_ChartData_Projects_ByPhase, tenantId, (int)IMSModules.ProjectManagement, (int)IMSModules.ProjectPhase);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;

				case 5:
					var baseQuery4 = string.Format(QueryConstants.GET_Donuts_ChartData_Projects_ByTask, tenantId, (int)IMSModules.ProjectPhase, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery4);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetAuditFindingDounutChart(int categoryId, int tenantId, int auditId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_AuditFinding_ByClassificationByAuditId, auditId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Audit_Finding_Chart_By_Status, auditId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetAuditFindingClassificationDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_AuditFinding_ByClassifications, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Audit_Finding_Category_Chart_By_Status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetAuditFindingByRiskTypeDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Audit_Finding_With_Risk_Rating, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetComplianceDounutChart(int auditId, int tenantId, int categoryId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_ByCompliance, auditId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetAuditFindingByDepartmentDounutChart(int categoryId, int tenantId, int auditId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_ByDepartment, tenantId, auditId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetProjectTaskStatusDounutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_Projects_Tasks_ByStatus, tenantId, (int)IMSModules.ProjectManagement, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> getInternalAuditNcChart(int tenantId, int auditId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;

			var baseQuery = string.Format(QueryConstants.GET_DonutsChart_InternalAudit_Nc_Types, tenantId, auditId);
			dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);

			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> getInternalAuditOverAllCompliance(int categoryId, int tenantId, int auditId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_DonutsChart_InternalAudit_OverAll_Compliance, tenantId, auditId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_InternalAudit_OverAll_Reviewd, tenantId, auditId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<ComplianceClause>> GetComplianceClauses(int categoryId, int auditId, int tenantId)
		{
			IList<ComplianceClause> barGraphChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_BarGraph_ChartData_ComplianceClause, auditId, tenantId);
					barGraphChartItems = await GetBarChartResultBySQLQuery1(baseQuery);
					break;
			}
			return barGraphChartItems;
		}

		public async Task<IList<surveyResponse>> GetResponseBySurveyQuestionId(int categoryId, int surveyId, int tenantId)
		{
			IList<surveyResponse> barGraphChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Donuts_ChartData_SurveyMasterData_ByResponse, surveyId, tenantId);
					barGraphChartItems = await GetBarChartResultBySQLQuery21(baseQuery);
					break;
			}
			return barGraphChartItems;
		}

		public async Task<IList<AuditFindingChart>> GetAuditFindings(int categoryId, int auditId, int tenantId)
		{
			IList<AuditFindingChart> barGraphChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_BarGraph_ChartData_AuditFinding_Open_Closes, auditId, tenantId);
					barGraphChartItems = await GetBarChartResultBySQLQuery2(baseQuery);
					break;
			}
			return barGraphChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetAuditFindingsCategory(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Audit_Finding_Category_DepartmentWise_respect_to_riskRating, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery); ;
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<ComplianceClause>> GetInternalAuditComplianceBySectionChart(int tenantId, int auditId)
		{
			IList<ComplianceClause> barGraphChartItems = null;
			var baseQuery = string.Format(QueryConstants.GET_BarGraph_InternalAudit_Compliance_Status_BySection, tenantId, auditId);
			barGraphChartItems = await GetAuditComplianceBarChartResultBySQLQuery(baseQuery);
			return barGraphChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetAuditLandingPageCharts(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_InternalAudits_NonConformities_By_Status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_InternalAudits_Corrective_Actions_By_Status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Internal_Audit_AuditableItems, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_Internal_Audit_By_Categories, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;

				case 5:
					var baseQuery4 = string.Format(QueryConstants.GET_Internal_Audit_By_Finding_Department, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery4);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetMRMLandingPageCharts(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Management_Review_Meeting_List_By_Status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Management_Review_Meeting_Agendas_List_By_Staus, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Management_Review_Meeting_Minutes_list_By_status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_Management_Review_Meeting_Minutes_list_By_AssignTo, tenantId, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;

				case 5:
					var baseQuery4 = string.Format(QueryConstants.GET_Management_Review_Meeting_Minutes_list_By_Priority, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery4);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetSupplierLandingPageCharts(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Supplier_List_By_status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Supplier_List_By_status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Donuts_ChartData_TaskMAster_ByStatus, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetManagementReviewTaskListCharts(int categoryId, int meetingId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_Management_Review_Meeting_Task_By_MeetingId, meetingId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_Management_Review_Meeting_Task_AssignTo_By_MeetingId, meetingId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_Management_Review_Meeting_Task_Priority_By_MeetingId, meetingId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetTaskListDonutCharts(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_TaskList_for_Corrective_Action, tenantId, (int)IMSModules.CorrectiveAction, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_TaskList_for_Non_Conformance, tenantId, (int)IMSModules.NonConformity, (int)IMSModules.CorrectiveAction, (int)IMSModules.TaskMaster);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetCAListDonutCharts(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_CA_List_for_Non_Conformance, tenantId, (int)IMSModules.CorrectiveAction);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_CA_List_for_Incident, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetSurveyListDonutCharts(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_SurveyList, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_SurveyList, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetWorkItemStatus(int categoryId, int sourceId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_WorkItem_Status, tenantId, sourceId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_WorkItems_byAssignTo, tenantId, sourceId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.Get_WorkItems_By_Department, tenantId, sourceId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_TotalModules_BySourceId, tenantId, sourceId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetDocumentDonoutChart(int categoryId, int tenantId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_DocumentChart_By_Status, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_DocumentChart_By_Category, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;

				case 3:
					var baseQuery2 = string.Format(QueryConstants.GET_DocumentChart_By_Priority, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery2);
					break;

				case 4:
					var baseQuery3 = string.Format(QueryConstants.GET_DocumentChart_By_Classification, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery3);
					break;
			}
			return dounutsChartItems;
		}

		public async Task<IList<DounutsChartItem>> GetSurveyMasterResponceDonutChart(  int tenantId, int categoryId, int surveyId)
		{
			IList<DounutsChartItem> dounutsChartItems = null;
			switch (categoryId)
			{
				case 1:
					var baseQuery = string.Format(QueryConstants.GET_DocumentChart_By_SurveyIdMaxResponces, surveyId,tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery);
					break;

				case 2:
					var baseQuery1 = string.Format(QueryConstants.GET_DocumentChart_By_SurveyIdMinResponces,  surveyId, tenantId);
					dounutsChartItems = await GetDounutsChartResultBySQLQuery(baseQuery1);
					break;
			}
			return dounutsChartItems;
		}
	}
}