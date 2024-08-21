using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;

namespace Mindflur.IMS.Business
{
	public class ChartBusiness : IChartBusiness
	{
		private readonly IChartRepository _chartRepository;

		private readonly ISurveySupplierMappingRepository _surveySupplierMappingRepository;

		public ChartBusiness(IChartRepository chartRepository, ISurveySupplierMappingRepository surveySupplierMappingRepository)
		{
			_chartRepository = chartRepository;
			_surveySupplierMappingRepository = surveySupplierMappingRepository;
		}

		public async Task<BarChartGraphView> GetChartBarGraph(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetChartBarGraph(categoryId, tenantId);

			var chartView = new BarChartGraphView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();
			chartView.Date = new List<string>();
			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
				chartView.Date.Add(chartItem.ChartItemDate);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartNonConformanceDonuts(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetNonConformanceDonutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartTaskMasterDonuts(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetTaskMasterDonutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartCorrectiveActionDonuts(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetCorrectiveActionDonutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartInherentRiskDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetInherentRiskDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartSiteOfIncidentDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetSiteOfIncidentDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartRiskTreatmentDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetRiskTreatmentDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetChartProjectManagementDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetProjectManagementDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetAuditFindingDounutChart(int categoryId, int tenantId, int auditId)
		{
			var chartResult = await _chartRepository.GetAuditFindingDounutChart(categoryId, tenantId, auditId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetAuditFindingClassificationDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetAuditFindingClassificationDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetAuditFindingByRiskTypeDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetAuditFindingByRiskTypeDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetComplianceDounutChart(int categoryId, int auditId, int tenantId)
		{
			var chartResult = await _chartRepository.GetComplianceDounutChart(auditId, categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetAuditFindingByDepartmentDounutChart(int categoryId, int tenantId, int auditId)
		{
			var chartResult = await _chartRepository.GetAuditFindingByDepartmentDounutChart(categoryId, tenantId, auditId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetProjectTaskStatusDounutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetProjectTaskStatusDounutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> getInternalAuditNcChart(int tenantId, int auditId)
		{
			var chartResult = await _chartRepository.getInternalAuditNcChart(tenantId, auditId);
			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> getInternalAuditOverAllCompliance(int categoryId, int tenantId, int auditId)
		{
			var chartResult = await _chartRepository.getInternalAuditOverAllCompliance(categoryId, tenantId, auditId);
			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<ComplianceClauseBarGraphView> GetComplianceClausesBargarph(int categoryId, int auditId, int tenantId)
		{
			var chartResult = await _chartRepository.GetComplianceClauses(auditId, categoryId, tenantId);

			var chartView = new ComplianceClauseBarGraphView();
			chartView.Clause = new List<string>();
			chartView.Compliance = new List<bool>();
			chartView.Total = new List<int>();
			foreach (var chartItem in chartResult)
			{
				chartView.Clause.Add(chartItem.Clause);
				chartView.Compliance.Add(chartItem.Compliance);
				chartView.Total.Add(chartItem.Total);
			}
			return chartView;
		}

		public async Task<ComplianceClauseBarGraphView> GetAuditComplianceBySectionBarGraph(int tenantId, int auditId)
		{
			var chartResult = await _chartRepository.GetInternalAuditComplianceBySectionChart(tenantId, auditId);

			var chartView = new ComplianceClauseBarGraphView();
			chartView.Clause = new List<string>();
			chartView.Compliance = new List<bool>();
			chartView.Total = new List<int>();
			foreach (var chartItem in chartResult)
			{
				chartView.Clause.Add(chartItem.Clause);
				chartView.Compliance.Add(chartItem.Compliance);
				chartView.Total.Add(chartItem.Total);
			}
			return chartView;
		}

		public async Task<SurveyResponseView> GetResponseBySurveyQuestionId(int categoryId, int surveyId, int tenantId)
		{
			var chartResult = await _chartRepository.GetResponseBySurveyQuestionId(categoryId, surveyId, tenantId);

			var chartView = new SurveyResponseView();
			chartView.QuestionName = new List<string>();
			chartView.ResponseName = new List<string>();
			chartView.ResponseCount = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.QuestionName.Add(chartItem.QuestionName);
				chartView.ResponseName.Add(chartItem.ResponseName);
				chartView.ResponseCount.Add(chartItem.ResponseCount);
			}

			return chartView;
		}

		public async Task<AuditFindingBarGraphView> GetAuditFindingsBarGraph(int categoryId, int auditId, int tenantId)
		{
			var chartResult = await _chartRepository.GetAuditFindings(auditId, categoryId, tenantId);

			var chartView = new AuditFindingBarGraphView();
			chartView.Classification = new List<string>();
			chartView.FindingStatus = new List<string>();
			chartView.ChartItemValue = new List<int>();
			foreach (var chartItem in chartResult)
			{
				chartView.Classification.Add(chartItem.Classification);
				chartView.FindingStatus.Add(chartItem.FindingStatus);
				chartView.ChartItemValue.Add(chartItem.ChartItemValue);
			}
			return chartView;
		}

		public async Task<DonutsChartView> getAuditFindingCategoryWithRiskRatingByDepartment(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetAuditFindingsCategory(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();
			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}
			return chartView;
		}

		public async Task<DonutsChartView> GetAuditLandingPageCharts(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetAuditLandingPageCharts(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetmanagementReviewLandingPageCharts(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetMRMLandingPageCharts(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetSupplierCharts(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetSupplierLandingPageCharts(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetMrmTaskCharts(int categoryId, int meetingId, int tenantId)
		{
			var chartResult = await _chartRepository.GetManagementReviewTaskListCharts(categoryId, meetingId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetTaskListsCharts(int meetingId, int tenantId)
		{
			var chartResult = await _chartRepository.GetTaskListDonutCharts(meetingId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetALLCorrectiveAction(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetCAListDonutCharts(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetDonutForSurveyMasterData(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetSurveyListDonutCharts(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetWorkItemStatus(int categoryId, int sourceId, int tenantId)
		{
			var chartResult = await _chartRepository.GetWorkItemStatus(categoryId, sourceId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetDocumentsDonutChart(int categoryId, int tenantId)
		{
			var chartResult = await _chartRepository.GetDocumentDonoutChart(categoryId, tenantId);

			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}

		public async Task<DonutsChartView> GetSurveyMasterResponceDonutChart( int categoryId, int surveyId, int tenantId)
		{
			var chartResult = await _chartRepository.GetSurveyMasterResponceDonutChart(tenantId, categoryId, surveyId);
			var chartView = new DonutsChartView();
			chartView.Keys = new List<string>();
			chartView.Values = new List<int>();

			foreach (var chartItem in chartResult)
			{
				chartView.Keys.Add(chartItem.ChartItemKey);
				chartView.Values.Add(chartItem.ChartItemValue);
			}

			return chartView;
		}
	}
}