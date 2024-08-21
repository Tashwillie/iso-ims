using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class IncidentQuesttionaireRepository : BaseRepository<IncidentQuesttionaire>, IIncidentQuesttionaireRepository
	{
		public IncidentQuesttionaireRepository(IMSDEVContext dbContext, ILogger<IncidentQuesttionaire> logger) : base(dbContext, logger)
		{
		}
	}
}