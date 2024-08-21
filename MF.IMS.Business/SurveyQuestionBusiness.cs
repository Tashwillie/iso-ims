using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;

namespace Mindflur.IMS.Business
{
	public class SurveyQuestionBusiness : ISurveyQuestionBusiness
	{
		private readonly ISurveyQuestionRepository _surveyQuestionRepository;
		private readonly ISurveyQuestionAnswerRepository _surveyQuestionAnswerRepository;
		private readonly IActivityLogRepository _activityLogRepository;

		public SurveyQuestionBusiness(ISurveyQuestionRepository surveyQuestionRepository, ISurveyQuestionAnswerRepository surveyQuestionAnswerRepository, IActivityLogRepository activityLogRepository)
		{
			_surveyQuestionRepository = surveyQuestionRepository;
			_surveyQuestionAnswerRepository = surveyQuestionAnswerRepository;
			_activityLogRepository = activityLogRepository;
		}

		public async Task AddSurveyQuestion(SurveyQuestionPostView surveyQuestionPostView, int surveyId)
		{
			await _surveyQuestionRepository.AddSurveyQuestions(surveyQuestionPostView, surveyId);
		}

		public async Task<PaginatedItems<GetSuvreyQuestionListView>> GetAllSurveyQuestionsPaginated(GetSuvreyQuestionList getListrequest, int surveyId)
		{
			return await _surveyQuestionRepository.GetAllSurveyQuestionsPaginated(getListrequest, surveyId);
		}

		public async Task<GetSurveyQuestionPreview> GetSurveyQuestionPreview( int tenantId, int surveyQuestionId)
		{
			return await _surveyQuestionRepository.GetSurveyQuestionPreview(tenantId, surveyQuestionId);
		}

		public async Task UpdateSurveyQuestionAnswer(PutQuestionbyId putQuestionbyId, int Id)
		{
			var questionAnswer = await _surveyQuestionRepository.GetByIdAsync(Id);
			if (questionAnswer == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
			}
			else
			{
				questionAnswer.OfferedAnswerId = putQuestionbyId.OfferedAnswerId;
				questionAnswer.Comments = putQuestionbyId.Comments;
				
				await _surveyQuestionRepository.UpdateAsync(questionAnswer);
			}
		}
		public async Task DeleteSurveyQuestionData(int id, int tenantId)
		{
			var surveyQuestion = await _surveyQuestionRepository.GetByIdAsync(id);
			if (surveyQuestion == null)
			{
				throw new NotFoundException(String.Format(ConstantsBusiness.SurveyQuestionIdNotFoundErrorMessage), id);
			}
			else 
			{
				await _surveyQuestionRepository.DeleteAsync(surveyQuestion);
			
			}
		}
	}
}