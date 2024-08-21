using DocumentFormat.OpenXml.Wordprocessing;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class RiskAssesmentMasterBusiness : IRiskAssesmentMasterBusiness
	{
		public readonly IRiskAssesmentMasterRepository _riskAssesmentMasterRepository;
		public readonly IWorkItemRepository _workItemRepository;
		public RiskAssesmentMasterBusiness(IRiskAssesmentMasterRepository riskAssesmentMasterRepository, IWorkItemRepository workItemRepository)
		{
			_riskAssesmentMasterRepository = riskAssesmentMasterRepository;
			_workItemRepository = workItemRepository;
		}

		public async Task<PaginatedItems<RiskAssesmentMasterGridView>>GetRiskList(GetRiskAssesmentRequestList getRiskList)
		{
			return await _riskAssesmentMasterRepository.GetRiskAssessmentList(getRiskList);
		}

		public async Task<RiskAssesmentMasterview> GetRiskListByRiskId( int riskId )
		{
			return await _riskAssesmentMasterRepository.GetRiskByRiskId( riskId);
		}

       

		public async Task UpsertRiskAssessment(PostRiskAssesmentView editRiskAssessmentView, int riskId, int userId)
		{
            var sourceImpact = await _workItemRepository.GetTokenDetailsForRisk(riskId, (int)IMSRiskDetails.ImapctLevel);

            var sourceProbability = await _workItemRepository.GetTokenDetailsForRisk(riskId, (int)IMSRiskDetails.ProbabilityLevel);
			await _riskAssesmentMasterRepository.UpsertRiskAssessment(editRiskAssessmentView, sourceImpact.TokenId, sourceProbability.TokenId,  userId, riskId);

        }

		public async Task DeleteRiskAssessment(int id)
		{
			var assessment = await _riskAssesmentMasterRepository.GetByIdAsync(id);
			if (assessment == null)
			{
                throw new NotFoundException(string.Format(ConstantsBusiness.RiskAssessmentNotFound), id);
            }
			await _riskAssesmentMasterRepository.DeleteAsync(assessment);
		}
    }
}
