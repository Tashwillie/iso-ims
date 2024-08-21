using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class IncidentClassificationBusiness : IIncidentClassificationBusiness
	{
		private readonly IIncidentClassificationRepository _incidentClassificationRepository;

		public IncidentClassificationBusiness(IIncidentClassificationRepository incidentClassificationRepository)
		{
			_incidentClassificationRepository = incidentClassificationRepository;
		}

		public async Task<IncidentManagementAccidentClassification> GetIncidentClassificationById(int tenantId, int id)
		{
			var incident = await _incidentClassificationRepository.GetByIncidentId(id);

			if (incident == null)
			{
				incident = new IncidentManagementAccidentClassification();
				return incident;

			}
			else
			{
				return incident;
			}
		}

		public async Task UpdateIncidentClassification(IncidentClassificationPostView incidentClassification, int tenantId, int id)
		{
			var incident = await _incidentClassificationRepository.GetByIncidentId(id);

			if (incident == null)
			{
				incident = new IncidentManagementAccidentClassification();
				incident.IncidentId = id;
				incident.IsAccident = incidentClassification.IsAccident;
				incident.IsAccidentText = incidentClassification.IsAccidentText;
				incident.IsInjury = incidentClassification.IsInjury;
				incident.IsInjuryText = incidentClassification.IsInjuryText;
				incident.IsEnvironmentalIncident = incidentClassification.IsEnvironmentalIncident;
				incident.IsEnvironmentalIncidentText = incidentClassification.IsEnvironmentalIncidentText;
				incident.IsEquipmentFailure = incidentClassification.IsEquipmentFailure;
				incident.IsEquipmentFailureText = incidentClassification.IsEquipmentFailureText;
				incident.IsFinancialIncident = incidentClassification.IsFinancialIncident;
				incident.IsFinancialIncidentText = incidentClassification.IsFinancialIncidentText;
				incident.IsNaturalEventDisaster = incidentClassification.IsNaturalEventDisaster;
				incident.IsNaturalEventDisasterText = incidentClassification.IsNaturalEventDisasterText;
				incident.IsNonConformanceQuality = incidentClassification.IsNonConformanceQuality;
				incident.IsNonConformanceQualityText = incidentClassification.IsNonConformanceQualityText;
				incident.IsProductionInterruption = incidentClassification.IsProductionInterruption;
				incident.IsProductionInterruptionText = incidentClassification.IsProductionInterruptionText;
				incident.IsWastage = incidentClassification.IsWastage;
				incident.IsWastageText = incidentClassification.IsWastageText;
				incident.FirstaidInjury = incidentClassification.FirstaidInjury;
				incident.LostTimeInjury = incidentClassification.LostTimeInjury;
				incident.FatalInjury = incidentClassification.FatalInjury;
				incident.ReportableInjury = incidentClassification.ReportableInjury;
				incident.MinorInjury = incidentClassification.MinorInjury;
				incident.BriefDescription = incidentClassification.BriefDescription;
				await _incidentClassificationRepository.AddAsync(incident);
			}
			else
			{
				incident.IncidentId = id;
				incident.IsAccident = incidentClassification.IsAccident;
				incident.IsAccidentText = incidentClassification.IsAccidentText;
				incident.IsInjury = incidentClassification.IsInjury;
				incident.IsInjuryText = incidentClassification.IsInjuryText;
				incident.IsEnvironmentalIncident = incidentClassification.IsEnvironmentalIncident;
				incident.IsEnvironmentalIncidentText = incidentClassification.IsEnvironmentalIncidentText;
				incident.IsEquipmentFailure = incidentClassification.IsEquipmentFailure;
				incident.IsEquipmentFailureText = incidentClassification.IsEquipmentFailureText;
				incident.IsFinancialIncident = incidentClassification.IsFinancialIncident;
				incident.IsFinancialIncidentText = incidentClassification.IsFinancialIncidentText;
				incident.IsNaturalEventDisaster = incidentClassification.IsNaturalEventDisaster;
				incident.IsNaturalEventDisasterText = incidentClassification.IsNaturalEventDisasterText;
				incident.IsNonConformanceQuality = incidentClassification.IsNonConformanceQuality;
				incident.IsNonConformanceQualityText = incidentClassification.IsNonConformanceQualityText;
				incident.IsProductionInterruption = incidentClassification.IsProductionInterruption;
				incident.IsProductionInterruptionText = incidentClassification.IsProductionInterruptionText;
				incident.IsWastage = incidentClassification.IsWastage;
				incident.IsWastageText = incidentClassification.IsWastageText;
				incident.FirstaidInjury = incidentClassification.FirstaidInjury;
				incident.LostTimeInjury = incidentClassification.LostTimeInjury;
				incident.FatalInjury = incidentClassification.FatalInjury;
				incident.ReportableInjury = incidentClassification.ReportableInjury;
				incident.MinorInjury = incidentClassification.MinorInjury;
				incident.BriefDescription = incidentClassification.BriefDescription;
				await _incidentClassificationRepository.UpdateAsync(incident);
			}
		}

		public async Task DeleteIncidentClassification(int tenantId, int id)
		{
			var incident = await _incidentClassificationRepository.GetByIncidentId(id);

			if (incident == null)
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.IncidentClassificationErrorMessage), id);
			}
			else
			{
				await _incidentClassificationRepository.DeleteAsync(incident);
			}
		}
	}
}