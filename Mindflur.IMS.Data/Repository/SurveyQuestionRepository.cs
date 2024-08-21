using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class SurveyQuestionRepository : BaseRepository<SurveyQuestion>, ISurveyQuestionRepository
	{
		public SurveyQuestionRepository(IMSDEVContext dbContext, ILogger<SurveyQuestion> logger) : base(dbContext, logger)
		{
		}

	
		public async Task<PaginatedItems<QuestionDetailViewBySurveyId>> getQuestionBySurveyId(GetSurveyListRequest getListrequest, int id)
		{
			var rawData = (from que in _context.SurveyQuestions
						   join survey in _context.SurveyMasterData on que.SurveyId equals survey.SurveyId
						   join tm in _context.TenanttMasters on survey.TenantId equals tm.TenantId
						   join que1 in _context.QuestionMasters on que.QuestionId equals que1.QuestionId
						   where que.SurveyId == id && survey.TenantId == getListrequest.TenantId
						   select new QuestionDetailViewBySurveyId
						   {
							   QuestionId = que.QuestionId,
							   questionName = que1.Title,
							   SurveyQuestionId = que.SurveyQuestionId,
							   SequenceNumber = que.SequenceNumber
						   }).AsQueryable();
			var filteredData = DataExtensions.OrderBy(rawData, getListrequest.ListRequests.SortColumn, getListrequest.ListRequests.Sort == "asc")
							  .Skip(getListrequest.ListRequests.PerPage * (getListrequest.ListRequests.Page - 1))
							  .Take(getListrequest.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)getListrequest.ListRequests.PerPage);
			var model = new PaginatedItems<QuestionDetailViewBySurveyId>(getListrequest.ListRequests.Page, getListrequest.ListRequests.PerPage, totalPages, filteredData);
			return await Task.FromResult(model);
		}

		public async Task AddSurveyQuestions(SurveyQuestionPostView surveyQuestionPostView, int surveyId)
		{
			List<SurveyQuestion> surveyQuestions = surveyQuestionPostView.QuestionId.Select(questionId => new SurveyQuestion
			{
				QuestionId = questionId,
				OfferedAnswerId = surveyQuestionPostView.OfferedAnswerId,
				SurveyId = surveyId,
				Comments = surveyQuestionPostView.Comments,
				SequenceNumber = surveyQuestionPostView.SequenceNumber
			}).ToList();

			await _context.SurveyQuestions.AddRangeAsync(surveyQuestions);
			await _context.SaveChangesAsync();
			
		}
			public async Task<PaginatedItems<GetSuvreyQuestionListView>> GetAllSurveyQuestionsPaginated(GetSuvreyQuestionList getListrequest, int surveyId)
		{
			var rawData = (from survey in _context.SurveyQuestions
						   join questionMaster in _context.QuestionMasters on survey.QuestionId equals questionMaster.QuestionId
						   join offeredAnswers in _context.OfferedAnswerMasters on survey.OfferedAnswerId equals offeredAnswers.SurveyOfferedAnswerId
						   where survey.SurveyId == surveyId
						   select new GetSuvreyQuestionListView()
						   {
							   SurveyQuestionId = survey.SurveyQuestionId,
							   QuestionId = survey.QuestionId,
							   QuestionName = questionMaster.Title,
							   OfferedAnswerId = survey.OfferedAnswerId,
							   OfferedAnswer = offeredAnswers.Title,
							   SequenceNumber = survey.SequenceNumber,
							   Comments = survey.Comments,
						   }).OrderByDescending(survey =>survey.SurveyQuestionId).AsQueryable();
			var filteredData = DataExtensions.OrderBy(rawData, getListrequest.ListRequests.SortColumn, getListrequest.ListRequests.Sort == "asc")
							  .Skip(getListrequest.ListRequests.PerPage * (getListrequest.ListRequests.Page - 1))
							  .Take(getListrequest.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)getListrequest.ListRequests.PerPage);
			var model = new PaginatedItems<GetSuvreyQuestionListView>(getListrequest.ListRequests.Page, getListrequest.ListRequests.PerPage, totalPages, filteredData);
			return await Task.FromResult(model);
		}
		public async Task<GetSurveyQuestionPreview> GetSurveyQuestionPreview(int tenantId, int surveyQuestionId)
		{
			var data = (from survey in _context.SurveyQuestions
						join question in _context.QuestionMasters on survey.QuestionId equals question.QuestionId
						join answer in _context.OfferedAnswerMaster on survey.OfferedAnswerId equals answer.SurveyOfferedAnswerId
						where survey.SurveyQuestionId == surveyQuestionId

						select new GetSurveyQuestionPreview()
						{
							SurveyQuestionId = survey.SurveyQuestionId,
							QuestionId = survey.QuestionId,
							Question = question.Title,
							OfferedAnswerId = survey.OfferedAnswerId,
							OfferedAnswer = answer.Title,
							Comments = survey.Comments,
						}).AsQueryable();
			return data.FirstOrDefault();
						
		}

	}
}