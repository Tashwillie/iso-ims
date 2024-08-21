using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class IncidentQuestionMasterBusiness : IIncidentQuesttionMasterBusiness
    {
        private readonly IIncidentQuestionMasterRepository _incidentQuestionMasterRepository;

        public IncidentQuestionMasterBusiness(IIncidentQuestionMasterRepository incidentQuestionMasterRepository)
        {
            _incidentQuestionMasterRepository = incidentQuestionMasterRepository;
        }

        public async Task<IList<IncidentQuestionMaster>> GetAllQuestionMaster()
        {
            return await _incidentQuestionMasterRepository.GetAllQuestionMaster();
        }
    }
}