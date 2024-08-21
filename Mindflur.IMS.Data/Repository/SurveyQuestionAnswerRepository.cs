using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Models.Custom;

namespace Mindflur.IMS.Data.Repository
{
    public class SurveyQuestionAnswerRepository : BaseRepository<SurveyQuestionAnswer>, ISurveyQuestionAnswerRepository
    {

        private readonly IConfiguration _configuration;

        public SurveyQuestionAnswerRepository(IMSDEVContext dbContext, ILogger<SurveyQuestionAnswer> logger, IConfiguration configuration) : base(dbContext, logger)
        {
            _configuration = configuration;
        }
        public async Task<IList<GetSurveyQuestionAnswer>> GetAllSurveyQuestionAnswe()
        {

            var Question = await (from questionAnswe in _context.SurveyQuestionAnswers

                                  join offeredAnswer in _context.OfferedAnswerMasters on questionAnswe.OfferedAnswerId equals offeredAnswer.SurveyOfferedAnswerId
                                  select new GetSurveyQuestionAnswer()
                                  {
                                      SurveyQuestionAnswerId = questionAnswe.SurveyQuestionAnswerId,
                                      SurveyQuestionId = questionAnswe.SurveyQuestionId,
                                      OfferedAnswer = offeredAnswer.Title,
                                      SequenceId = questionAnswe.SequenceId,



                                  }).ToListAsync();
            return await Task.FromResult(Question);

        }

        public async Task<IEnumerable<ResponseListItemData>> getSurveyResponseAsync(int surveyId, int tenantId)
        {

            using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

            conn.Open();
            return await conn.QueryAsync<ResponseListItemData>(
                @"Select surveyQueAns.SurveyQuestionId as QuestionId,   qm.Title as QuestionText ,ofs.Title as Options,COUNT(surveyQueAns.OfferedAnswerId) 
                as Response From SurveyQuestionAnswer as surveyQueAns
                join SurveyQuestions as sq on surveyQueAns.SurveyQuestionId=sq.QuestionId
                join QuestionMaster as qm on surveyQueAns.SurveyQuestionId=qm.QuestionId
                join SurveyMasterData as sd on sq.SurveyId=sd.SurveyId
                join TenanttMaster as tm on sd.TenantId = tm.TenantId
                join OfferedAnswerMaster as ofs on surveyQueAns.OfferedAnswerId = ofs.SurveyOfferedAnswerId
                where sq.SurveyId=@surveyId and sd.TenantId = @tenantId
                Group By ofs.Title,qm.Title,surveyQueAns.SurveyQuestionId", new { surveyId, tenantId }

                );
        }
        public async Task<IList<QuestionsDataView>> GetQuestionAnswers(int surveyId)
        {
            var questions = await (from smd in _context.SurveyMasterData
                                   join srm in _context.SurveyResponseMasters on smd.SurveyId equals srm.SurveyId

                                   join qm in _context.QuestionMasters on srm.SurveyQuestionId equals qm.QuestionId
                                   join ofm in _context.OfferedAnswerMaster on srm.SurveyOfferedAnswerId equals ofm.SurveyOfferedAnswerId
                                   where smd.SurveyId == surveyId
                                   select new QuestionsDataView
                                   {
                                       Questions = qm.Title,
                                       Answers = ofm.Title,
                                   }).ToListAsync();
            return await Task.FromResult(questions);
        }
        public async Task<SupplierRatings> GetSupplierRatingFromSurvay(int supplierId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));
            conn.Open();
            return await conn.QueryFirstOrDefaultAsync<SupplierRatings>(@"Select  AVG( oam.Weightage )as SupplierRating from SurveyMasterData as smd 
            join SurveyResponseMaster as srm on smd.SurveyId = srm.SurveyId
            join SurveySupplierMapping as ssm on smd.SurveyId = ssm.SurveyMasterId
            join QuestionMaster as qm on srm.SurveyQuestionId = qm.QuestionId
            join OfferedAnswerMaster as oam on srm.SurveyOfferedAnswerId = oam.SurveyOfferedAnswerId
            where ssm.SupplierMasterId = @supplierId ", new { supplierId });
        }
        public async Task<IEnumerable<SurveyQuestionAnswerData>> GetAllQuestionsForSurvey(int surveyId, int tenantId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));
            conn.Open();
            return await conn.QueryAsync<SurveyQuestionAnswerData>(@"select smd.SurveyId as SurveyId,smd.Title as SurveyTitle, sq.QuestionId as QuestionId, qm.Title as QuestionText, Sqa.OfferedAnswerId as AnswerId,oam.Title as AnswerText from SurveyMasterData as smd
              join SurveyQuestions as sq on smd.SurveyId =sq.SurveyId
              join SurveyQuestionAnswer as sqa on sq.SurveyQuestionId = sqa.SurveyQuestionId
              join QuestionMaster as qm on sq.QuestionId = qm.QuestionId
              join OfferedAnswerMaster as oam on sqa.OfferedAnswerId = oam.SurveyOfferedAnswerId
              join TenanttMaster as tm on smd.TenantId = tm.TenantId
              where smd.SurveyId = @surveyId and tm.TenantId = @tenantId
              group by sq.QuestionId, smd.SurveyId,smd.Title,qm.Title, sqa.OfferedAnswerId, oam.Title", new { surveyId, tenantId });
        }

    }
}
