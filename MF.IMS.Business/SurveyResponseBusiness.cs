using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class SurveyResponseBusiness : ISurveyResponseBusiness
    {
        private readonly ISurveyResponseRepsoitory _surveyResponseRepsoitory;
        public SurveyResponseBusiness(ISurveyResponseRepsoitory surveyResponseRepsoitory)
        {
            _surveyResponseRepsoitory = surveyResponseRepsoitory;
        }



        public async Task<IList<GetSurveyResponseView>> getAllSurveyResponse()
        {
            return await _surveyResponseRepsoitory.getAllSurveyResponse();


        }
        public async Task UpsertSurveyResponse(SurveyResponsePostView surveyResponsePost, int userId, int surveyId)
        {
            await _surveyResponseRepsoitory.UpsertSurveyResponse(surveyResponsePost, userId, surveyId);





        }
        public async Task UpdateServiceResponse(SurveyResponseMaster surveyResponseMaster, int Id)
        {
            var surveyResponse = await _surveyResponseRepsoitory.GetByIdAsync(Id);
            if (surveyResponse == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            }
            else
            {
                surveyResponse.SurveyQuestionId = surveyResponseMaster.SurveyQuestionId;
                surveyResponse.SurveyId = surveyResponseMaster.SurveyId;
                surveyResponse.SurveyOfferedAnswerId = surveyResponseMaster.SurveyOfferedAnswerId;
                surveyResponse.UserId = surveyResponseMaster.UserId;
                await _surveyResponseRepsoitory.UpdateAsync(surveyResponseMaster);

            }


        }
    }
}
