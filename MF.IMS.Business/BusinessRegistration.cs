using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mindflur.IMS.Application.AuditPlanReportt.AuditPlanDataSource;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.CorrectiveActionLayout.DataSource;
using Mindflur.IMS.Application.DocumentChangeRequestReport.DataSource;
using Mindflur.IMS.Application.IncidentManagementLayout;
using Mindflur.IMS.Application.InternalAuditLayouts.DataSource;
using Mindflur.IMS.Application.ManagementReviewReports.AgendaLayouts;
using Mindflur.IMS.Application.ManagementReviewReports.MinutesOfMEeetingLayout;
using Mindflur.IMS.Application.NonConformanceLayouts.NcDataSource;
using Mindflur.IMS.Application.ObservationReport.ObservationDataSource;
using Mindflur.IMS.Application.SupplierReports.DataSource;
using Mindflur.IMS.Business.Service;
using System.Diagnostics.CodeAnalysis;

namespace Mindflur.IMS.Business
{
    public static class BusinessRegistration
	{
		[ExcludeFromCodeCoverage]
		public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IEmailService, EmailService>();
			services.AddSingleton<IMessageService, MessageService>();
			services.AddSingleton<IKeyClockBusiness, KeyClockBusiness>();
			services.AddTransient<IManagementReviewBusiness, ManagementReviewBusiness>();
			services.AddTransient<IInternalAuditBusiness, InternalAuditBusiness>();
			services.AddTransient<INonConformanceBusiness, NonConformanceBusiness>();
			services.AddTransient<IUserBusiness, UserBusiness>();
			services.AddTransient<ITaskMasterBusiness, TaskMasterBusiness>();
			services.AddTransient<IChartBusiness, ChartBusiness>();
			services.AddTransient<ITaskCompletionBusiness, TaskCompletionBusiness>();
			services.AddTransient<IUserPointsBusiness, UserPointsBusiness>();
			services.AddTransient<IMessageService, MessageService>();
			services.AddTransient<IIncidentQuesttionnaireBusiness, IncidentQuesttionaireBusiness>();
			services.AddTransient<IIncidentQuesttionnaireBusiness, IncidentQuesttionaireBusiness>();
			services.AddTransient<IMasterDataBusiness, MasterDataBusiness>();
			services.AddTransient<IRiskBusiness, RiskBusiness>();
			services.AddTransient<IRiskTreatmentBusiness, RiskTreatmentBusiness>();
			services.AddTransient<IIncidentQuesttionMasterBusiness, IncidentQuestionMasterBusiness>();			
			services.AddTransient<IProjectsBusiness, ProjectBusiness>();
			services.AddTransient<IProjectMemberBusiness, ProjectMemberBusiness>();
			services.AddTransient<IQuestionMasterBusiness, QuestionMasterBusiness>();
			services.AddTransient<IProjectTaskBusiness, ProjectTaskBusiness>();
			services.AddTransient<ISurveyMasterDatumBusiness, SurveyMasterDatumBusiness>();
			services.AddTransient<IOfferedAnswerMasterBusiness, OfferedAnswerMasterBusiness>();
			services.AddTransient<ISurveyQuestionBusiness, SurveyQuestionBusiness>();
			services.AddTransient<ISurveyQuetionAnswerBusiness, SurveyQuestionAnswerBusiness>();
			services.AddTransient<ISurveyResponseBusiness, SurveyResponseBusiness>();
			services.AddTransient<ISupplierMasterBusiness, SupplierMasterBusiness>();
			services.AddTransient<IAuditCheckListQuestionBusiness, AuditCheckListQuestionBusiness>();
			services.AddTransient<IRoleBusiness, RoleBusiness>();
			services.AddTransient<ITenantMasterBusiness, TenanatMasterBusiness>();
			services.AddTransient<ICheckListMasterBusiness, CheckListMasterBusiness>();
			services.AddTransient<IDocumentBusiness, DocumentBusiness>();
			services.AddTransient<IParticipantsBusiness, ParticipantsBusiness>();
			services.AddTransient<IPermissionBusiness, PermissionBusiness>();
			services.AddTransient<IRolePermissionBusiness, RolePermissionBusiness>();
			services.AddTransient<IFileRepositoryBusiness, FileRepsoitoryBusiness>();
			services.AddTransient<IAuditSettingBusiness, AuditSettingBusiness>();
			services.AddTransient<ITenantThemeBusiness, TenantThemeBusiness>();
			services.AddTransient<IActivityLogBusiness, ActivityLogBusiness>();
			services.AddTransient<IControllerMasterBusiness, ControllerMasterBusiness>();
			services.AddTransient<IIncidentClassificationBusiness, IncidentClassificationBusiness>();
			services.AddTransient<IDepartmentMasterBusiness, DepartmentMasterBusiness>();
			services.AddTransient<IAuditPlanBusiness, AuditPlanBusiness>();
			services.AddTransient<ISurveySupplierMappingBusiness, SurveySupplierMappingBusiness>();
			services.AddTransient<IBackTraceBusiness, BackTraceBusiness>();
			services.AddTransient<IAuditFindingBusiness, AuditFindingBusiness>();
			services.AddTransient<ICorrectiveActionBusiness, CorrectiveActionBusiness>();
			services.AddTransient<IIncidentMasterBusiness, IncidentMasterBusiness>();
			services.AddTransient<IAuditFindingBusiness, AuditFindingBusiness>();
			services.AddTransient<ICommentBusiness, CommentBusiness>();			
			services.AddTransient<IWorkItemBusiness, WorkItemBusiness>();
			services.AddTransient<IAuditableItemBusiness, AuditableItemBusiness>();
			services.AddTransient<IOpportunitiesMasterBusiness, OpportunitiesMasterBusiness>();
			services.AddTransient<ICommonTokenBusiness, CommonTokenBusiness>();
			services.AddTransient<IClauseBusiness, ClauseBusiness>();
			services.AddScoped<IDocumentChangeRequestBusiness, DocumentChangeRequestBusiness>();
			services.AddScoped<IAuditClauseBusiness, AuditClauseBusiness>();
			services.AddScoped<IProcessMasterBusiness, ProcessMasterBusiness>();
			services.AddScoped<IInternalAuditDataSourceBusiness, InternalAuditDataSourceBusiness>();
			services.AddScoped<IRiskAssesmentMasterBusiness, RiskAssesmentMasterBusiness>();
			services.AddScoped<IMinutesBusiness, MinutesBusiness>();
			services.AddScoped<IMRMAgendaReportBusiness, MRMAgendaReportBusiness>();
			services.AddScoped<IMrmMinutesBusiness, MrmMinuteBusiness>();
			services.AddScoped<ICorrectiveActionReportBusiness, CorrectiveActionReportBusiness>();
			services.AddScoped<INonConfermanceDataSourceBusiness, NonConfermanceDataSourceBusiness>();
			services.AddScoped<IDocumentChangeRequestSourceBusiness, DocumentChangeRequestSourceBusiness>();
			services.AddScoped<IObservationReportBuisiness, ObservationReportBuisiness>();
			services.AddScoped<IIncidentDataSourceBusiness, IncidentDataSourceBusiness>();
			services.AddScoped<IAuditPlanReportBuisiness, AuditPlanReportBuisiness>();
            services.AddScoped<ISupplierSourceBusiness, SupplierSourceBusiness>();
            return services;
		}
	}
}