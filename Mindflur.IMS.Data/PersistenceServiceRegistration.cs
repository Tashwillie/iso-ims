using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Repository;
using Mindflur.IMS.Data.Repository.AutoMapper;
using System.Diagnostics.CodeAnalysis;

namespace Mindflur.IMS.Data
{
	public static class PersistenceServiceRegistration
	{
		[ExcludeFromCodeCoverage]
		public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
		{
			var dbProvider = "MSSQL";

			switch (dbProvider)
			{
				case "MSSQL":
					services.AddDbContext<IMSDEVContext>(options =>
						 options.UseSqlServer(configuration.GetConnectionString("DataConnectionString"),
											sqlServerOptionsAction: sqlOptions =>
											{
												// sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
												//Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
												sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
											}));
					break;

				default:
					break;
			}
			services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));
			services.AddScoped<ITenantMasterRepository, TenanatMasterRepository>();
			services.AddScoped<IChecklistRepository, ChecklistRepository>();
			services.AddScoped<IAgendaRepository, AgendaMasterRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<ISurveyMasterDatumRepository, SurveyMasterDatumRepository>();
			services.AddScoped<IMinutesRepository, MinutesRepository>();
			services.AddScoped<IMeetingPlanRepository, MeetingPlanRepository>();
			services.AddScoped<IMeetingParticipantsRepository, MeetingParticipantsRepository>();
			services.AddScoped<ITaskMasterRepository, TaskMasterRepository>();
			services.AddScoped<IRoleMasterRepository, RoleMasterRepository>();
			services.AddScoped<IMeetingAttendanceRepository, MeetingAttendanceRepository>();
			services.AddScoped<IChartRepository, ChartRepository>();
			services.AddScoped<ITaskCompletionRepository, TaskComplitionRepository>();
			services.AddScoped<IAuditPlanRepository, AuditPlanRepository>();
			services.AddScoped<IAuditItemsRepository, AuditItemsRepository>();
			services.AddScoped<IMeetingAgendaMappingRepository, MeetingAgendaMappingRepository>();
			services.AddScoped<ITaskCompletionRepository, TaskComplitionRepository>();
			services.AddScoped<IUserPointRepository, UserPointRepository>();
			services.AddScoped<IIncidentQuesttionaireRepository, IncidentQuesttionaireRepository>();
			services.AddScoped<IMasterDataRepository, MasterDataRepository>();
			services.AddScoped<IAuditableItemClauseRepository, AuditableItemClauseRepository>();
			services.AddScoped<IAuditItemClauseRepository, AuditItemClauseRepository>();
			services.AddScoped<IRiskRepository, RiskManagementInherentRiskRepository>();
			services.AddScoped<IRiskTreatmentRepository, RiskTreatmentRepository>();
			services.AddScoped<IIncidentQuestionMasterRepository, IncidentQuestionMasterRepository>();
			services.AddScoped<IAuditFindingRepository, AuditFindingRepository>();
			services.AddScoped<IChecklistQuestionRepository, ChecklistQuestionRepository>();
			services.AddScoped<IProjectRepository, ProjectRepository>();
			services.AddScoped<IAuditChecklistRepository, AuditChecklistRepository>();
			services.AddScoped<IClauseMasterRepository, ClauseMasterRepository>();
			services.AddScoped<IAuditProgramRepository, AuditProgramRepository>();
			services.AddScoped<ITaskTagRepository, TaskTagRepository>();
			services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
			services.AddScoped<IProgramStandardsRepository, ProgramStandardsRepository>();
			services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
			services.AddScoped<IQuestionMasterRepository, QuestionMasterRepository>();
			services.AddScoped<IProjectTagRepository, ProjectTagRepository>();
			services.AddScoped<IOfferedAnswerMasterRepository, OfferedAnswerRepository>();
			services.AddScoped<ISurveyResponseRepsoitory, SurveyResponseRepository>();
			services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
			services.AddScoped<ISurveyQuestionAnswerRepository, SurveyQuestionAnswerRepository>();
			services.AddScoped<ISupplierMasterRepository, SupplierMasterRepository>();
			services.AddScoped<IIncidentCorrectibveActionMappingRepository, IncidentCaMappingRepository>();
			services.AddScoped<IDocumentRepository, DocumentRepository>();
			services.AddScoped<IDocumentTagsRepository, DocumentTagsRepository>();
			services.AddScoped<IParticipantsRepository, ParticipantsRepository>();
			services.AddScoped<IPermissionRepositoy, PermissionRepositoy>();
			services.AddScoped<IFilesRepository, FilesRepository>();
			services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
			services.AddScoped<IRoleMappingRepository, RoleMappingRepository>();
			services.AddScoped<IAuditSettingrepository, AuditSettingRepository>();
			services.AddScoped<ITenantThemeRepository, TenantThemeRepository>();
			services.AddScoped<IControllerMasterRepository, ControllerMasterRepository>();
			services.AddScoped<IActionControllerRepository, ActionControllerRepository>();
			services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
			services.AddScoped<IIncidentClassificationRepository, IncidentClassificationRepository>();
			services.AddScoped<IDepartmentMasterRepository, DepartmentMasterRepository>();
			services.AddScoped<IOpportunitiesMasterRepository, OpportunitiesMasterRepository>();
			services.AddScoped<IMeetingSupplierMappingRepository, MeetingSupplierMappingRepository>();
			services.AddScoped<ISurveySupplierMappingRepository, SurveySupplierMappingRepository>();
			services.AddScoped<IIncidentMasterRepository, IncidentMasterRepository>();
			services.AddScoped<IIncidentMasterClassificationTagRepository, IncidentMasterClassificationTagRepsository>();
			services.AddScoped<ICommentRepository, CommentRepository>();
			services.AddScoped<IAuditChecklistFindingRepository, AuditChecklistFindingRepository>();
			services.AddScoped<IWorkItemRepository, WorkItemRepository>();
			services.AddScoped<IWorkItemStandardRepository, WorkItemStandardRepository>();
			services.AddScoped<ICorrectiveActionMetaDataRepository, CorrectiveActionMetaDataRepository>();
			services.AddScoped<INonConformanceMetaDataRepository, NonConformanceMetaDataRepository>();
			services.AddScoped<IAuditFindingMappingRepository, AuditFindingMappingRepository>();
			services.AddScoped<IClauseRepository, ClausesRepository>();
			services.AddScoped<IWorkItemWorkItemTokenRepository, WorkItemWorkItemTokenRepository>();
			services.AddScoped<ICommonTokenRepository, CommonTokenRepository>();
			services.AddScoped<IDocumentChangeRequestRepositroy, DocumentChangeRequestRepository>();
			services.AddScoped<IProcessMasterRepository, ProcessMasterRepository>();
			services.AddScoped<IRiskAssesmentMasterRepository, RiskAssesmentMasterRepository>();
			services.AddScoped<IAgendaSummaryMasterRepository, AgendaSummaryMasterRepository>();
			services.AddScoped<IDocumentRoleRepository, DocumentRoleRepository>();
			services.AddScoped<IDocumentUserRepository, DocumentUserRepository>();
			return services;
		}

		[ExcludeFromCodeCoverage]
		public static IServiceCollection AddPersistanceAutoMapper(this IServiceCollection services)
		{
			var mappingConfig = new MapperConfiguration(mc =>
			{
				mc.AddProfile(new RepositoryProfile());
			});

			IMapper mapper = mappingConfig.CreateMapper();
			services.AddSingleton(mapper);

			return services;
		}
	}
}