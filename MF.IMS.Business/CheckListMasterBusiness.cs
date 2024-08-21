using DocumentFormat.OpenXml.Bibliography;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
	public class CheckListMasterBusiness: ICheckListMasterBusiness
	{
		private readonly IChecklistRepository _checklistRepository;
		private readonly IAuditChecklistRepository _auditChecklistRepository;
		public CheckListMasterBusiness(IChecklistRepository checklistRepository, IAuditChecklistRepository auditChecklistRepository)
		{
			_checklistRepository = checklistRepository;
			_auditChecklistRepository = auditChecklistRepository;
		}
		public async Task<IList<ChecklistView>> GetChecklistAsync(int internalAuditId)
		{
			IList<ChecklistView> checklistCollectionView = new List<ChecklistView>();

			var checcklistItems = await _checklistRepository.GetChecklistAsync(internalAuditId);

			var clauses = checcklistItems.DistinctBy(clause => clause.ClauseId);

			foreach (var clause in clauses)
			{
				var checklistForAClause = checcklistItems.Where(checklistItem => checklistItem.ClauseId == clause.ClauseId).OrderBy(checklistItem => checklistItem.SequenceNumber);

				var checklistItems = new List<ChecklistItem>();

				foreach (var checklistItemForAClause in checklistForAClause)
				{
					var checklistItem = new ChecklistItem
					{
						ChecklistItemId = checklistItemForAClause.QuestionId,
						Question = checklistItemForAClause.QuestionText,
						Complaince = checklistItemForAClause.IsCompliance,
						Classification = checklistItemForAClause.ClassificationTitle,
						Comments = checklistItemForAClause.Comments,
						SequenceNumber = checklistItemForAClause.SequenceNumber
					};

					checklistItems.Add(checklistItem);
				}
				var checkList = new ChecklistView
				{
					ClauseId = clause.ClauseId,
					ClauseText = clause.ClauseTitle,
					Checklist = checklistItems
				};

				checklistCollectionView.Add(checkList);
			}
			return checklistCollectionView;
		}

        public async Task<IList<AuditChecklistView>> GetAuditChecklistByAuditId(int auditId, int tenantId)
        {
            IList<AuditChecklistView> checklistData = new List<AuditChecklistView>();
            var clauses = await _auditChecklistRepository.GetClauseIdFromAuditProgram(auditId, tenantId);
			var clause = clauses.DistinctBy(clause => clause.ClauseId).OrderBy(clause => clause.ClauseId);

            foreach (var clauseid in clause)
            {

                var checklistResponse = await _auditChecklistRepository.GetAuditChecklistByAuditId(auditId, tenantId, clauseid.ClauseId);
                var checklistItems = new List<AuditChecklistView>();
                foreach (var checklist in checklistResponse)
                {

					var checklistItem = new AuditChecklistView
					{
						AuditChecklistId = checklist.AuditChecklistId,
						Questions = checklist.Questions,
						ComplianceComments = checklist.ComplianceComments,
						hasCompliance = checklist.hasCompliance,
						HasReviewed = checklist.HasReviewed,
						Reviewed = checklist.Reviewed,
						Tags = checklist.Tags,
					};
                    checklistData.Add(checklistItem);
                }
            }
			return checklistData;

        }

        public async Task<IList<AuditReportBarChartDataForCompliance>> GetChecklistForBarChartByAuditId(int auditId, int tenantId)
        {
            IList<AuditReportBarChartDataForCompliance> checklistData = new List<AuditReportBarChartDataForCompliance>();
            var clauses = await _auditChecklistRepository.GetClauseIdFromAuditProgram(auditId, tenantId);
            var clause = clauses.DistinctBy(clause => clause.ClauseId).OrderBy(clause => clause.ClauseId);

            foreach (var clauseid in clause)
            {

                var checklistResponse = await _auditChecklistRepository.GetChecklistForBarChartByAuditId(auditId, tenantId, clauseid.ClauseId);
                var checklistItems = new List<AuditReportBarChartDataForCompliance>();
                foreach (var checklist in checklistResponse)
                {

                    var checklistItem = new AuditReportBarChartDataForCompliance
                    {
                        ClauseId = checklist.ClauseId,
                        ClauseNumber = checklist.ClauseNumber,
                        ClauseText = checklist.ClauseText,
                        hasCompliance = checklist.hasCompliance,
                      
                    };
                    checklistData.Add(checklistItem);
                }
            }
            return checklistData;

        }


        public async Task<ChecklistSeverityView> GetChecklistSeverityForReport(int auditId, int severityId)
		{
			// var checklistSeverityView = new ChecklistSeverityView();
			var checklistSiverity = await _checklistRepository.GetChecklistSeverityForReport(auditId, severityId);

			var checklists = checklistSiverity;

			var severityitems = new ChecklistSeverityView
			{
				Severity = checklists.Severity,
				Total = checklists.Total,
			};
			

			return checklistSiverity;
		}

		public async Task<AuditChecklist> AddChecklistResponce(AuditChecklist checklistResponce)
		{
			return await _auditChecklistRepository.AddAsync(checklistResponce);
		}

		public async Task<AuditChecklist> GetChecklistResponceById(int checkListId)
		{
			var responce = await _auditChecklistRepository.GetByIdAsync(checkListId);
			return responce == null ? throw new NotFoundException(string.Format(ConstantsBusiness.ResponceNotFoundErrorMessage), checkListId) : responce;
		}

		public async Task UpdateChecklistResponce(int Id, PutAuditChecklistViewModel responce, int userId, int tenantId, string path)
		{
			await _auditChecklistRepository.UpdateChecklist(Id, responce, userId, tenantId, path);
		}

		public async Task DeleteChecklistResponse(int checklistResponceId)
		{
			var response = await _auditChecklistRepository.GetByIdAsync(checklistResponceId);
			if (response == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.ChecklistNotFoundErrorMessage), checklistResponceId);
			}
			await _auditChecklistRepository.DeleteAsync(response);
		}
		

		public async Task<IList<AuditChecklistView>> GetAuditChecklistForClause(int auditId, int clauseId, int tenantId)
		{
			return await _auditChecklistRepository.GetAuditChecklistForClause(auditId, clauseId, tenantId);
		}

		public async Task<IList<AuditChecklistView>> GetAuditChecklistWithComplianceFilter(int auditId, bool compliance, int tenantId)
		{
			return await _auditChecklistRepository.GetAuditChecklistWithComplianceFilter(auditId, compliance, tenantId);
		}
		public async Task<IList<ChecklistQuestionsForReport>> GetChecklistQuestionsForClauseReport(int auditid, int tenantId)
		{
			IList<ChecklistQuestionsForReport> checklistData = new List<ChecklistQuestionsForReport>();
			var clauses = await _auditChecklistRepository.GetClauseIdFromAuditProgram(auditid, tenantId);
			var clause = clauses.DistinctBy(clause => clause.ClauseId).OrderBy(clause => clause.ClauseId);

			foreach (var clauseid in clause)
			{

				var checklistResponse = await _auditChecklistRepository.GetChecklistQuestionsForClauseReport(auditid, tenantId, clauseid.ClauseId);
				var checklistItems = new List<ChecklistQuestionsForReport>();
				foreach (var checklist in checklistResponse)
				{

					var checklistItem = new ChecklistQuestionsForReport
					{
						ClauseNo=checklist.ClauseNo,
						Questions = checklist.Questions,
						ComplianceComments = checklist.ComplianceComments,
						hasCompliance = checklist.hasCompliance,
						Severity= checklist.Severity,	
					};
					checklistData.Add(checklistItem);
				}
			}
			return checklistData;
		}

		public async Task<ClauseDetails> GetClauseIdFromChecklistId(int auditId, int checklistId, int tenantId)
		{
			return await _auditChecklistRepository.GetClauseIdFromChecklistId(auditId, checklistId, tenantId);
		}

		public async Task<IList<AuditItemDetailsFromClauses>> GetAuditItemsFromClauseId(int auditId, int clauseId, int tenantId)
		{
			return await _auditChecklistRepository.GetAuditItemsFromClauseId(auditId, clauseId, tenantId);
		}

		public async Task<AuditChecklistDetails> GetAuditChecklistDetailsFromChecklistId(int checklistId, int tenantId)
		{
			return await _auditChecklistRepository.GetAuditChecklistDetailsFromChecklistId(checklistId, tenantId);
		}
        public async Task<IList<AuditChecklistView>> GetchecklistDetailsByDepartmentId(int auditId, int tenantId, int departmentId)
        {
            IList<AuditChecklistView> checklistData = new List<AuditChecklistView>();
            var clauses = await _auditChecklistRepository.GetClauseIdFromDepartmentId(auditId, tenantId, departmentId);
            var clause = clauses.DistinctBy(clause => clause.ClauseId).OrderBy(clause => clause.ClauseId);

            foreach (var clauseid in clause)
            {

                var checklistResponse = await _auditChecklistRepository.GetAuditChecklistByAuditId(auditId, tenantId, clauseid.ClauseId);
                var checklistItems = new List<AuditChecklistView>();
                foreach (var checklist in checklistResponse)
                {

                    var checklistItem = new AuditChecklistView
                    {
                        AuditChecklistId = checklist.AuditChecklistId,
                        Questions = checklist.Questions,
                        ComplianceComments = checklist.ComplianceComments,
                        hasCompliance = checklist.hasCompliance,
                        HasReviewed = checklist.HasReviewed,
                        Reviewed = checklist.Reviewed,
                        Tags = checklist.Tags,
                    };
                    checklistData.Add(checklistItem);
                }
            }
            return checklistData;
        }

		public  async Task<IList<CategoryListView>> GetcategoryList(int masterDataGroupId)
		{
			return await _auditChecklistRepository.GetcategoryList(masterDataGroupId);
		}
		public  async Task<IList<ClauseDetails>> GetChecklistByStandard(int auditId, int tenantId, int standardId)
		{
			return await _auditChecklistRepository.GetChecklistByStandard(auditId, tenantId, standardId);
		}
		public async Task<IList<AuditChecklistView>> GetchecklistDetailsByStandardId(int auditId, int tenantId, int standardId)
		{
			IList<AuditChecklistView> checklistData = new List<AuditChecklistView>();
			var clauses = await _auditChecklistRepository.GetChecklistByStandard(auditId, tenantId, standardId);
			var clause = clauses.DistinctBy(clause => clause.ClauseId).OrderBy(clause => clause.ClauseId);
			foreach (var clauseid in clause)
			{

				var checklistResponse = await _auditChecklistRepository.GetAuditChecklistByAuditId(auditId, tenantId, clauseid.ClauseId);
				var checklistItems = new List<AuditChecklistView>();
				foreach (var checklist in checklistResponse)
				{

					var checklistItem = new AuditChecklistView
					{
						AuditChecklistId = checklist.AuditChecklistId,
						Questions = checklist.Questions,
						ComplianceComments = checklist.ComplianceComments,
						hasCompliance = checklist.hasCompliance,
						HasReviewed = checklist.HasReviewed,
						Reviewed = checklist.Reviewed,
						Tags = checklist.Tags,
					};
					checklistData.Add(checklistItem);
				}
			}
			return checklistData;
		}
	}
}
