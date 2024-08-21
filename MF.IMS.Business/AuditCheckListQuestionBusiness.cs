using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
	public class AuditCheckListQuestionBusiness: IAuditCheckListQuestionBusiness
	{
		private readonly IChecklistQuestionRepository _checklistQuestionRepository;
	public AuditCheckListQuestionBusiness(IChecklistQuestionRepository checklistQuestionRepository) 
		{
			_checklistQuestionRepository = checklistQuestionRepository;

		}

		public async Task<PaginatedItems<CheckListMasterView>> GetCheckListMasterList(CheckListView checkListView)
		{
			return await _checklistQuestionRepository.GetCheckListMasterList( checkListView);

        }
		public async Task<IList<ChecklistMaster>> GetChecklistQuestions()
		{
			return await _checklistQuestionRepository.GetChecklistQuestions();
		}

		public async Task<IList<GetCheckListAuditProgramIdview>> GetChecklistAuditProgramId(int tenantId,int auditProgramId)
		{
			return await _checklistQuestionRepository.GetChecklistAuditProgramId( tenantId, auditProgramId);

        }
        public async Task AddChecklistQuestionsMaster(PostCheckListView postCheckListView, int tenantId)
		{
			ChecklistMaster checklist = new ChecklistMaster();

			checklist.ClauseMasterId = postCheckListView.ClauseMasterId;	
			checklist.Questions = postCheckListView.Questions;
			checklist.TenantId = tenantId; 
			//checklist.OrderNo = postCheckListView.OrderNo;

			await _checklistQuestionRepository.AddAsync( checklist );
			var checklists = await _checklistQuestionRepository.GetByIdAsync(checklist.Id);
			checklists.OrderNo = checklist.Id - 1;
			await _checklistQuestionRepository.UpdateAsync(checklists);
        }

		public async Task<GetCheckListById> GetChecklistMasterById(int tenantId, int Id)
		{
            return await _checklistQuestionRepository.GetChecklistMasterById(tenantId, Id);
        }

		public async Task<IList<GetCheckList>> GetCheckListDropdown(int tenantId)
		{
			return await _checklistQuestionRepository.GetCheckListDropdown(tenantId);

        }
        public async Task UpdateCheckList(PutCheckListView putCheckListView, int Id, int tenantId)
		{
			var questions = await _checklistQuestionRepository.GetByIdAsync(Id);
			if (questions == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.QuestionNotFoundErrorMessage), Id);
			}
			else
			{
				questions.ClauseMasterId = putCheckListView.ClauseMasterId;
				questions.Questions = putCheckListView.Questions;
				questions.TenantId= tenantId;
				//questions.OrderNo = putCheckListView.OrderNo;
				await _checklistQuestionRepository.UpdateAsync(questions);
			}
		}

		public async Task DeleteChecklistQuestion(int Id, int tenantId)
		{
			var checklist = await _checklistQuestionRepository.GetByIdAsync(Id);
			if (checklist == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.QuestionNotFoundErrorMessage), Id);
			}
			await _checklistQuestionRepository.DeleteAsync(checklist);
		}





	}
}
