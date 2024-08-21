using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class IncidentQuesttionaireBusiness : IIncidentQuesttionnaireBusiness
    {

        private readonly IIncidentQuesttionaireRepository _incidentQuesttionaireRepository;
        public IncidentQuesttionaireBusiness(IIncidentQuesttionaireRepository incidentQuesttionaireRepository)
        {

            _incidentQuesttionaireRepository = incidentQuesttionaireRepository;

        }

        
        public async Task AddIncidentQuestions(IncidentQuestionPut iq)//Add Incidents
        {



            foreach (var question in iq.IncidentQuesttionaire)
            {
                var newquestion = new IncidentQuesttionaire
                {
                    IncidentId = question.IncidentId,
                    QuestionId = question.QuestionId,
                    Response = question.Response,
                    Description = question.Description,
                };

                await _incidentQuesttionaireRepository.AddAsync(newquestion);

            }




        }

        public async Task<IncidentQuesttionaire> GetIncidentQuestionbyId(int incidentQuestionId)
        {
            var incident = await _incidentQuesttionaireRepository.GetByIdAsync(incidentQuestionId);
            return incident == null ? throw new NotFoundException(string.Format(ConstantsBusiness.IncidentQuesttionaireNotFound), incidentQuestionId) : incident;
        }


        public async Task<IncidentQuesttionaire> UpdateIncidentQuestion(IncidentQuesttionaire iq, int incidentId)
        {
            var incidents = await _incidentQuesttionaireRepository.GetByIdAsync(incidentId);
            if (incidents == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IncidentQuesttionaireNotFound), incidentId);
            }
            else
            {
                incidents.QuestionId = iq.QuestionId;
                incidents.IncidentId = iq.IncidentId;
                incidents.Description = iq.Description;
                incidents.Response = iq.Response;

                await _incidentQuesttionaireRepository.UpdateAsync(incidents);
                return iq;
            }
        }
        public async Task DeleteIncidentQuesttion(int incidentId)
        {
            var incidents = await _incidentQuesttionaireRepository.GetByIdAsync(incidentId);
            if (incidents == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IncidentQuesttionaireNotFound), incidentId);
            }
            await _incidentQuesttionaireRepository.DeleteAsync(incidents);

        }
    }


}
