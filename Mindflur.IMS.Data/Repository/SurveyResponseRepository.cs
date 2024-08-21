using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class SurveyResponseRepository : BaseRepository<SurveyResponseMaster>, ISurveyResponseRepsoitory
    {
        public SurveyResponseRepository(IMSDEVContext dbContext, ILogger<SurveyResponseMaster> logger) : base(dbContext, logger)
        {
        }
        public async Task<IList<GetSurveyResponseView>> getAllSurveyResponse()
        {
            var response = await (from surveyResponse in _context.SurveyResponseMasters
                                  join surveyMaster in _context.SurveyMasterData on surveyResponse.SurveyId equals surveyMaster.SurveyId
                                  join OfferedMaster in _context.OfferedAnswerMaster on surveyResponse.SurveyOfferedAnswerId equals OfferedMaster.SurveyOfferedAnswerId
                                  join user in _context.UserMasters on surveyResponse.UserId equals user.UserId
                                  select new GetSurveyResponseView()
                                  {
                                      SurveyResponseId = surveyResponse.SurveyResponseId,
                                      SurveyId = surveyMaster.Title,
                                      SurveyOfferedAnswerId = OfferedMaster.Title,
                                      SurveyQuestionId = surveyResponse.SurveyQuestionId,
                                      FullName = $"{user.FirstName}  {user.LastName}",



                                  }).ToListAsync();
            return await Task.FromResult(response);



        }
        public async Task<PaginatedItems<GetSurveyResponseViewBySurveyID>> getSurveyResponseBySurveyId(GetSurveyListRequest getListRequest, int id)
        {
            var rawData = (from surveyResponse in _context.SurveyResponseMasters
                           join survey in _context.SurveyMasterData on surveyResponse.SurveyId equals survey.SurveyId
                           join tm in _context.TenanttMasters on survey.TenantId equals tm.TenantId
                           join md in _context.SurveyQuestions on surveyResponse.SurveyQuestionId equals md.SurveyQuestionId
                           join md1 in _context.QuestionMasters on md.SurveyQuestionId equals md1.QuestionId
                           join md2 in _context.OfferedAnswerMasters on surveyResponse.SurveyOfferedAnswerId equals md2.SurveyOfferedAnswerId
                           join us in _context.UserMasters on surveyResponse.UserId equals us.UserId
                           where id == surveyResponse.SurveyId && getListRequest.TenantId == survey.TenantId
                           select new GetSurveyResponseViewBySurveyID()
                           {


                               SurveyResponseId = surveyResponse.SurveyResponseId,
                               SurveyQuestionName = md1.Title,
                               SurveyOfferedAnswerName = md2.Title,
                               User = $"{us.FirstName} {us.LastName}"
                           }).AsQueryable();
            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                              .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<GetSurveyResponseViewBySurveyID>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        public async Task UpsertSurveyResponse(SurveyResponsePostView surveyResponsePost, int userId, int surveyId)
        {
            var existingResponse = await _context.SurveyResponseMasters.Where(rp => rp.SurveyId == surveyId).ToListAsync();
            if (existingResponse.Any())
            {
                _context.SurveyResponseMasters.RemoveRange(existingResponse);
                await _context.SaveChangesAsync();
            }
            var newResponse = new List<SurveyResponseMaster>();
            foreach (var questions in surveyResponsePost.Responses)
            {

                var responses = new SurveyResponseMaster()
                {
                    SurveyId = surveyId,
                    UserId = userId,
                    SurveyQuestionId = questions.Questions,
                    SurveyOfferedAnswerId = questions.Answers,
                };
                newResponse.Add(responses);


            }
            if (newResponse.Any())
            {
                await _context.SurveyResponseMasters.AddRangeAsync(newResponse);
                await _context.SaveChangesAsync();
            }



        }




    }
}
