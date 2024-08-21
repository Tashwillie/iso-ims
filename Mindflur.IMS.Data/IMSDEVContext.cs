using Microsoft.EntityFrameworkCore;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data
{
	public partial class IMSDEVContext : DbContext
	{
		public IMSDEVContext()
		{
		}

		public IMSDEVContext(DbContextOptions<IMSDEVContext> options)
			: base(options)
		{
		}

		public virtual DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
		public virtual DbSet<AgendaMaster> AgendaMasters { get; set; } = null!;
		public virtual DbSet<AgendaSummaryMaster> AgendaSummaryMasters { get; set; } = null!;
		public virtual DbSet<AuditChecklist> AuditChecklists { get; set; } = null!;
		public virtual DbSet<AuditChecklistFinding> AuditChecklistFindings { get; set; } = null!;
		public virtual DbSet<AuditFinding> AuditFindings { get; set; } = null!;
		public virtual DbSet<AuditFindingsMapping> AuditFindingsMappings { get; set; } = null!;
		public virtual DbSet<AuditItemClause> AuditItemClauses { get; set; } = null!;
		public virtual DbSet<AuditPlan> AuditPlans { get; set; } = null!;
		public virtual DbSet<AuditProgram> AuditPrograms { get; set; } = null!;
		public virtual DbSet<AuditSettings> AuditSettings { get; set; } = null!;
		public virtual DbSet<AuditableItem> AuditableItems { get; set; } = null!;
		public virtual DbSet<AuditableItemClause> AuditableItemClauses { get; set; } = null!;
		public virtual DbSet<ChecklistMaster> ChecklistMasters { get; set; } = null!;
		public virtual DbSet<ClauseMaster> ClauseMasters { get; set; } = null!;
		public virtual DbSet<Comment> Comments { get; set; } = null!;
		public virtual DbSet<CommonToken> Tokens { get; set; } = null!;
		public virtual DbSet<CorrectiveAction> CorrectiveActions { get; set; } = null!;
		public virtual DbSet<DepartmentMaster> DepartmentMasters { get; set; }
		public virtual DbSet<Documents> Documents { get; set; } = null!;
		public virtual DbSet<DocumentTags> DocumentTags { get; set; } = null!;
        public virtual DbSet<DocumentUsers> DocumentUsers { get; set; } = null!;
        public virtual DbSet<DocumentRoles> DocumentRoles { get; set; } = null!;
        public virtual DbSet<CorrectiveActionTaskMasterMapping> CorrectiveActionTaskMasterMappings { get; set; } = null!;
		public virtual DbSet<EmailNotification> EmailNotifications { get; set; } = null!;
		public virtual DbSet<FileRepository> FileRepositories { get; set; } = null!;
		public virtual DbSet<IncidentCorrectiveActionMapping> IncidentCorrectiveActionMappings { get; set; } = null!;
		public virtual DbSet<IncidentManagementAccidentClassification> IncidentManagementAccidentClassifications { get; set; } = null!;
		public virtual DbSet<IncidentQuestionMaster> IncidentQuestionMasters { get; set; } = null!;

		public virtual DbSet<IncidentMasterClassificationTag> IncidentMasterClassificationTags { get; set; } = null!;
		public virtual DbSet<IncidentQuesttionaire> IncidentQuesttionaires { get; set; } = null!;
		public virtual DbSet<IssueCorrectiveAction> IssueCorrectiveActions { get; set; } = null!;

		public virtual DbSet<MeetingSupplierMapping> MeetingSupplierMappings { get; set; } = null!;
		public virtual DbSet<MasterDataGroup> MasterDataGroups { get; set; } = null!;
		public virtual DbSet<MasterDatum> MasterData { get; set; } = null!;
		public virtual DbSet<MeetingAgendaMapping> MeetingAgendaMappings { get; set; } = null!;
		public virtual DbSet<MeetingAttendence> MeetingAttendences { get; set; } = null!;
		public virtual DbSet<MeetingParticipant> MeetingParticipants { get; set; } = null!;
		public virtual DbSet<MeetingPlan> MeetingPlans { get; set; } = null!;
		public virtual DbSet<MeetingRegister> MeetingRegisters { get; set; } = null!;
		public virtual DbSet<MinutesOfMeeting> MinutesOfMeetings { get; set; } = null!;
		public virtual DbSet<ModuleMaster> ModuleMasters { get; set; } = null!;
		public virtual DbSet<NonConformity> NonConformities { get; set; } = null!;
		public virtual DbSet<NonConformanceMetadata> NonConformanceMetadatas { get; set; } = null!;
		public virtual DbSet<NonConformityCorrectiveActionMapping> NonConformityCorrectiveActionMappings { get; set; } = null!;
		public virtual DbSet<OfferedAnswerMaster> OfferedAnswerMasters { get; set; } = null!;
		public virtual DbSet<OpportunitiesMaster> OpportunitiesMasters { get; set; } = null!;
		public virtual DbSet<UserPoints> UserPoints { get; set; } = null!;
		public virtual DbSet<ProgramStandard> ProgramStandards { get; set; } = null!;
		public virtual DbSet<ProjectMetaData> Projects { get; set; } = null!;
		public virtual DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
		public virtual DbSet<ProjectTag> ProjectTags { get; set; } = null!;
		public virtual DbSet<ProjectTask> ProjectTasks { get; set; } = null!;
		public virtual DbSet<QuestionMaster> QuestionMasters { get; set; } = null!;
		public virtual DbSet<Risk> Risks { get; set; } = null!;
		public virtual DbSet<RiskTreatment> RiskTreatments { get; set; } = null!;
		public virtual DbSet<RoleMaster> RoleMasters { get; set; } = null!;
		public virtual DbSet<SiteNavigation> SiteNavigations { get; set; } = null!;

		public virtual DbSet<SupplierMaster> SupplierMasters { get; set; } = null!;
		public virtual DbSet<SurveyMaster> SurveyMasters { get; set; } = null!;
		public virtual DbSet<SurveyMasterDatum> SurveyMasterData { get; set; } = null!;
		public virtual DbSet<OfferedAnswerMaster> OfferedAnswerMaster { get; set; } = null!;
		public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; } = null!;
		public virtual DbSet<SurveyQuestionAnswer> SurveyQuestionAnswers { get; set; } = null!;
		public virtual DbSet<SurveyResponseMaster> SurveyResponseMasters { get; set; } = null!;
		public virtual DbSet<SurveySupplierMapping> SurveySupplierMappings { get; set; } = null!;
		public virtual DbSet<TaskComplition> TaskComplitions { get; set; } = null!;
		public virtual DbSet<TaskMetaData> TaskMasters { get; set; } = null!;
		public virtual DbSet<TaskTag> TaskTags { get; set; } = null!;
		public virtual DbSet<TenanttMaster> TenanttMasters { get; set; } = null!;
		public virtual DbSet<TenantTheme> TenantThemes { get; set; } = null!;
		public virtual DbSet<UserMaster> UserMasters { get; set; } = null!;
		public virtual DbSet<UserSettingMaster> UserSettingMasters { get; set; } = null!;
		public virtual DbSet<Participant> Participants { get; set; } = null!;
		public virtual DbSet<PermissionMaster> PermissionMasters { get; set; } = null!;
		public virtual DbSet<RolePermission> RolePermissions { get; set; } = null!;
		public virtual DbSet<KCRoleToRoleMapping> KCRoleToRoleMappings { get; set; } = null!;
		public virtual DbSet<ContollerActionMaster> ControllerActionMasters { get; set; } = null!;
		public virtual DbSet<ControllerMaster> ControllerMasters { get; set; } = null!;
		public virtual DbSet<ObservationMaster> ObservationMasters { get; set; } = null!;
		public virtual DbSet<WorkItemMaster> WorkItemMasters { get; set; } = null!;

		public virtual DbSet<WorkItemWorkItemToken> WorkItemWorkItemTokens { get; set; } = null!;
		public virtual DbSet<WorkItemStandard> WorkItemStandards { get; set; } = null!;
		public virtual DbSet<CorrectiveActionMetadata> CorrectiveActionMetaDatas { get; set; } = null!;

		public virtual DbSet<Clause> Clauses { get; set; } = null!;
		public virtual DbSet<IncidentMetaData> IncidentMetaDatas { get; set; } = null!;

		public virtual DbSet<DocumentChangeRequestMaster> DocumentChangeRequestMasters { get; set; } = null!;

		public virtual DbSet<ProcessMaster> ProcessMasters { get; set; } = null!;

		public virtual DbSet<RiskAssesmentMaster> RiskAssesmentMasters { get; set; } = null!;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
				optionsBuilder.UseSqlServer("Server=Beast;Database=IMS.DEV;User ID=sa;Password=Local.DB@IMS;");
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<IncidentMetaData>(entity =>
			{
				entity.ToTable("IncidentMetaData");
				entity.HasKey(e => e.IncidentMetadataId);
			});

			modelBuilder.Entity<Clause>(entity =>
			{
				entity.ToTable("Clauses");
				entity.HasKey(e => e.ClauseId);
			});
			modelBuilder.Entity<AgendaSummaryMaster>(entity =>
			{
				entity.ToTable("AgendaSummaryMaster");
				entity.HasKey(e => e.Id);
			});
			modelBuilder.Entity<ActivityLog>(entity =>
			{
				entity.ToTable("ActivityLog");
				entity.HasKey(e => e.ActivityLogId);
				entity.Property(e => e.Description).HasMaxLength(200)
					.IsUnicode(false);
				entity.Property(e => e.Details).IsUnicode(false);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
			});
			modelBuilder.Entity<CorrectiveActionMetadata>(entity =>
			{
				entity.ToTable("CorrectiveActionMetaData");
				entity.HasKey(e => e.Id);

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<NonConformanceMetadata>(entity =>
			{
				entity.ToTable("NonConformanceMetaData");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<WorkItemWorkItemToken>(entity =>
			{
				entity.ToTable("WorkItem_WorkItemToken");
				entity.HasKey(e => e.Id);
			});

			modelBuilder.Entity<OpportunitiesMaster>(entity =>
			{
				entity.ToTable("OpportunitiesMaster");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
				entity.Property(e => e.OpportunitesDescription).HasMaxLength(200)
				   .IsUnicode(false);
			});
			modelBuilder.Entity<Comment>(entity =>
			{
				entity.ToTable("Comment");
				entity.HasKey(e => e.CommentId);
				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.DeletedOn).HasColumnType("date");
				entity.Property(e => e.CommentContent).HasMaxLength(500)
				   .IsUnicode(false);
			});
			modelBuilder.Entity<WorkItemMaster>(entity =>
			{
				entity.ToTable("WorkItemMaster");
				entity.HasKey(e => e.WorkItemId);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});
			modelBuilder.Entity<WorkItemStandard>(entity =>
			{
				entity.ToTable("WorkItemStandard");
				entity.HasKey(e => e.WorkItemStandardId);
			});

			modelBuilder.Entity<IncidentMasterClassificationTag>(entity =>
			{
				entity.ToTable("IncidentMasterClassificationTag");
				entity.HasKey(e => e.Id);
			});
			modelBuilder.Entity<ObservationMaster>(entity =>
			{
				entity.ToTable("ObservationMaster");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Title)
				   .HasMaxLength(500)
				   .IsUnicode(false);
				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
				entity.Property(e => e.Description).HasMaxLength(500)
				   .IsUnicode(false);
			});
			modelBuilder.Entity<DepartmentMaster>(entity =>
			{
				entity.ToTable("DepartmentMaster");
				entity.HasKey(e => e.DepartmentId);
			});
			modelBuilder.Entity<MeetingSupplierMapping>(entity =>
			{
				entity.ToTable("MeetingSupplierMapping");
				entity.HasKey(e => e.Id);
			});

			modelBuilder.Entity<AgendaMaster>(entity =>
			{
				entity.HasKey(e => e.AgendaId);

				entity.ToTable("AgendaMaster");

				entity.Property(e => e.Title)
					.HasMaxLength(500)
					.IsUnicode(false);
			});

			modelBuilder.Entity<AuditChecklist>(entity =>
			{
				entity.ToTable("AuditChecklist");

				entity.Property(e => e.Comments).IsUnicode(false);
			});

			modelBuilder.Entity<AuditChecklistFinding>(entity =>
			{
				entity.ToTable("AuditChecklistFinding");

				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<AuditFinding>(entity =>
			{
				entity.ToTable("AuditFinding");

				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.Title).IsUnicode(false);
				entity.Property(e => e.Description).IsUnicode(false);
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<AuditFindingsMapping>(entity =>
			{
				entity.ToTable("AuditFindingsMapping");
				entity.HasKey(e => e.Id);
			});
			modelBuilder.Entity<AuditItemClause>(entity =>
			{
			});

			modelBuilder.Entity<AuditPlan>(entity =>
			{
				entity.ToTable("AuditPlan");

				entity.Property(e => e.Objectives).IsUnicode(false);

				entity.Property(e => e.Scope).IsUnicode(false);
			});

			modelBuilder.Entity<AuditProgram>(entity =>
			{
				entity.ToTable("AuditProgram");

				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.DueDate).HasColumnType("date");

				entity.Property(e => e.FromDate).HasColumnType("date");

				entity.Property(e => e.Title)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<AuditSettings>(entity =>
			{
				entity.HasKey(e => e.AuditSettingId);
				entity.ToTable("AuditSettings");

				entity.Property(e => e.CreatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<AuditableItem>(entity =>
			{
				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.Description).IsUnicode(false);

				entity.Property(e => e.EndDate).HasColumnType("date");

				entity.Property(e => e.StartDate).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<AuditableItemClause>(entity =>
			{
			});

			modelBuilder.Entity<ChecklistMaster>(entity =>
			{
				entity.ToTable("ChecklistMaster");

				entity.Property(e => e.Questions).IsUnicode(false);
			});

			modelBuilder.Entity<ClauseMaster>(entity =>
			{
				entity.ToTable("ClauseMaster");

				entity.Property(e => e.ClauseName).IsUnicode(false);

				entity.Property(e => e.ClauseNo)
					.HasMaxLength(50)
					.IsUnicode(false);
			});
			modelBuilder.Entity<Documents>(entity =>
			{
				entity.ToTable("Documents");
				entity.HasKey(e => e.DocumentId);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
				entity.Property(e => e.ApprovedOn).HasColumnType("date");
				entity.Property(e => e.DeletedOn).HasColumnType("date");
				entity.Property(e => e.Title)
					.HasMaxLength(150)
					.IsUnicode(false);
				entity.Property(e => e.HtmlContent)
					.HasMaxLength(500)
					.IsUnicode(false);
			});
			modelBuilder.Entity<FileRepository>(entity =>
			{
				entity.ToTable("FileRepository");
				entity.HasKey(e => e.FileRepositoryId);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.DeletedOn).HasColumnType("date");

				entity.Property(e => e.FullName)
					.HasMaxLength(300)
					.IsUnicode(false);
				entity.Property(e => e.BlobStorageFilePath)
					.HasMaxLength(300)
					.IsUnicode(false);
			});
			modelBuilder.Entity<DocumentTags>(entity =>
			{
				entity.ToTable("DocumentTags");
				entity.HasKey(e => e.DocumentTagId);
			});
            modelBuilder.Entity<DocumentRoles>(entity =>
            {
                entity.ToTable("DocumentRoles");
                entity.HasKey(e => e.DocumentRolesId);
            });
            modelBuilder.Entity<DocumentUsers>(entity =>
            {
                entity.ToTable("DocumentUsers");
                entity.HasKey(e => e.DocumentUserId);
            });

            modelBuilder.Entity<ControllerMaster>(entity =>
			{
				entity.ToTable("ControllerMaster");
				entity.HasKey(e => e.ControllerId);
				entity.Property(e => e.ControllerName).HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<ContollerActionMaster>(entity =>
			{
				entity.ToTable("ControllerActionMaster");
				entity.HasKey(e => e.ActionId);
				entity.Property(e => e.ControllerAction).HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<CorrectiveAction>(entity =>
			{
				entity.ToTable("CorrectiveAction");

				entity.Property(e => e.ActionRequired).IsUnicode(false);

				entity.Property(e => e.ActionToPrevent).IsUnicode(false);

				entity.Property(e => e.ChangeInQMS).HasColumnName("ChangeInQMS");

				entity.Property(e => e.ChangesInQMSDescription)
					.IsUnicode(false)
					.HasColumnName("ChangesInQMSDescription");

				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.Description).IsUnicode(false);

				entity.Property(e => e.DueDate).HasColumnType("date");

				entity.Property(e => e.RiskAssessmentDescription).IsUnicode(false);

				entity.Property(e => e.RootCauseAnalysis).IsUnicode(false);

				entity.Property(e => e.UpdatedOn).HasColumnType("date");

				entity.Property(e => e.WhyAnalysis1).IsUnicode(false);

				entity.Property(e => e.WhyAnalysis2).IsUnicode(false);

				entity.Property(e => e.WhyAnalysis3).IsUnicode(false);

				entity.Property(e => e.WhyAnalysis4).IsUnicode(false);

				entity.Property(e => e.WhyAnalysis5).IsUnicode(false);
			});

			modelBuilder.Entity<CorrectiveActionTaskMasterMapping>(entity =>
			{
				entity.ToTable("CorrectiveActionTaskMasterMapping");
			});

			modelBuilder.Entity<EmailNotification>(entity =>
			{
				entity.ToTable("EmailNotification");

				entity.Property(e => e.ModuleName)
					.HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<IncidentCorrectiveActionMapping>(entity =>
			{
				entity.ToTable("IncidentCorrectiveActionMapping");
			});

			modelBuilder.Entity<IncidentManagementAccidentClassification>(entity =>
			{
				entity.ToTable("IncidentManagementAccidentClassification");

				entity.HasKey(e => e.Id);
			});

			modelBuilder.Entity<IncidentQuestionMaster>(entity =>
			{
				entity.ToTable("IncidentQuestionMaster");

				entity.Property(e => e.Questions)
					.HasMaxLength(150)
					.IsUnicode(false);
			});

			modelBuilder.Entity<IncidentQuesttionaire>(entity =>
			{
				entity.ToTable("IncidentQuesttionaire");

				entity.Property(e => e.Description).IsUnicode(false);
			});

			modelBuilder.Entity<IssueCorrectiveAction>(entity =>
			{
				entity.HasKey(e => e.IssueId);

				entity.ToTable("IssueCorrectiveAction");
			});

			modelBuilder.Entity<MasterDataGroup>(entity =>
			{
				entity.ToTable("MasterDataGroup");

				entity.Property(e => e.Name)
					.HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<MasterDatum>(entity =>
			{
				entity.Property(e => e.Items)
					.HasMaxLength(100)
					.IsUnicode(false);
			});

			modelBuilder.Entity<CommonToken>(entity =>
			{
				entity.ToTable("WorkItem_Tokens");
				entity.HasKey(e => e.TokenId);
				entity.Property(e => e.TokenName)
					.HasMaxLength(100)
					.IsUnicode(false);
			});

			modelBuilder.Entity<MeetingAgendaMapping>(entity =>
			{
				entity.HasKey(e => e.MappingId);

				entity.ToTable("MeetingAgendaMapping");
			});

			modelBuilder.Entity<MeetingAttendence>(entity =>
			{
				entity.ToTable("MeetingAttendence");

				entity.Property(e => e.Id).ValueGeneratedOnAdd();
			});

			modelBuilder.Entity<MeetingParticipant>(entity =>
			{
				entity.Property(e => e.Id).ValueGeneratedOnAdd();
				entity.Property(e => e.Participants)
					.HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<MeetingPlan>(entity =>
			{
				entity.ToTable("MeetingPlan");

				entity.Property(e => e.EndDate).HasColumnType("date");

				entity.Property(e => e.Location)
					.HasMaxLength(100)
					.IsUnicode(false);

				entity.Property(e => e.StartDate).HasColumnType("date");

				entity.Property(e => e.Title)
					.HasMaxLength(100)
					.IsUnicode(false);
			});

			modelBuilder.Entity<MeetingRegister>(entity =>
			{
				entity.ToTable("MeetingRegister");

				entity.Property(e => e.EndTime).HasColumnType("date");

				entity.Property(e => e.StartTime).HasColumnType("date");
			});

			modelBuilder.Entity<MinutesOfMeeting>(entity =>
			{
				entity.ToTable("MinutesOfMeeting");

				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<ModuleMaster>(entity =>
			{
				entity.ToTable("ModuleMaster");

				entity.Property(e => e.ModuleName)
					.HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<NonConformity>(entity =>
			{
				entity.ToTable("NonConformity");

				entity.Property(e => e.DueDate).HasColumnType("date");

				entity.Property(e => e.Description).IsUnicode(false);

				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(e => e.Title)
					.HasMaxLength(150)
					.IsUnicode(false);

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<NonConformityCorrectiveActionMapping>(entity =>
			{
				entity.ToTable("NonConformityCorrectiveActionMapping");
			});

			modelBuilder.Entity<OfferedAnswerMaster>(entity =>
			{
				entity.ToTable("OfferedAnswerMaster");
				entity.HasNoKey();
			});

			modelBuilder.Entity<Participant>(entity =>
			{
				entity.ToTable("Participants");
				entity.Property(e => e.ParticipantId).ValueGeneratedOnAdd();
				entity.Property(e => e.MarkPresent).HasColumnType("date");
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
				entity.Property(e => e.DeletedOn).HasColumnType("date");
			});

			modelBuilder.Entity<PermissionMaster>(entity =>
			{
				entity.ToTable("PermissionsMaster");
				entity.Property(e => e.PermissionId).ValueGeneratedOnAdd();
			});
			modelBuilder.Entity<RolePermission>(entity =>
			{
				entity.ToTable("RolePermission");
				entity.Property(e => e.RolePermissionId).ValueGeneratedOnAdd();
			});
			modelBuilder.Entity<UserPoints>(entity =>
			{
				entity.ToTable("UserPoints");

				entity.Property(e => e.Comments)
					.HasMaxLength(200)
					.IsUnicode(false);

				entity.Property(e => e.CreatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<ProjectMetaData>(entity =>
			{
				entity.ToTable("Project");
				entity.HasKey(e => e.ProjectId);

				entity.Property(e => e.Budget).HasColumnType("money");

				entity.Property(e => e.EndDate).HasColumnType("date");

				entity.Property(e => e.StartDate).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<ProjectTask>(entity =>
			{
				entity.ToTable("ProjectTask");
			});

			modelBuilder.Entity<QuestionMaster>(entity =>
			{
				entity.HasKey(e => e.QuestionId);

				entity.ToTable("QuestionMaster");

				entity.Property(e => e.Title)
					.HasMaxLength(250)
					.IsUnicode(false);
			});

			modelBuilder.Entity<Risk>(entity =>
			{
				entity.ToTable("Risk");

				entity.Property(e => e.CurrentControls)
					.HasMaxLength(150)
					.IsUnicode(false);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(e => e.InitialDate).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<RiskTreatment>(entity =>
			{
				entity.ToTable("RiskTreatment");

				entity.Property(e => e.DueDate).HasColumnType("date");

				entity.Property(e => e.ReviewedOn).HasColumnType("date");

				entity.Property(e => e.OpportunityId).IsUnicode(false);

				entity.Property(e => e.MitigationPlan)
					.HasMaxLength(150)
					.IsUnicode(false);
			});

			modelBuilder.Entity<RoleMaster>(entity =>
			{
				entity.ToTable("RoleMaster");

				entity.Property(e => e.RoleName)
					.HasMaxLength(250)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SiteNavigation>(entity =>
			{
				entity.ToTable("SiteNavigation");

				entity.Property(e => e.Actions)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.Header)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.NavigationId)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.NavigationLink).IsUnicode(false);

				entity.Property(e => e.Resource)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.Title)
					.HasMaxLength(200)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SupplierMaster>(entity =>
			{
				entity.HasKey(e => e.SupplierId);

				entity.ToTable("SupplierMaster");

				entity.Property(e => e.SupplierLocation)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.ContactPerson)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.ContactNumber)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.EmailAddress)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.SupplierName)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<SurveyMaster>(entity =>
			{
				entity.ToTable("SurveyMaster");

				entity.Property(e => e.Description).IsUnicode(false);

				entity.Property(e => e.GroupTitle)
					.HasMaxLength(150)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SurveyMasterDatum>(entity =>
			{
				entity.HasKey(e => e.SurveyId)
					.HasName("PK_SurveyMaster1");

				entity.Property(e => e.CreatedOn).HasColumnType("datetime");

				entity.Property(e => e.Description)
					.HasMaxLength(500)
					.IsUnicode(false);

				entity.Property(e => e.EndTime).HasColumnType("datetime");

				entity.Property(e => e.StartDate).HasColumnType("datetime");

				entity.Property(e => e.Title)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.UpdatedIon)
					.HasColumnType("datetime")
					.HasColumnName("UpdatedIOn");
			});
			modelBuilder.Entity<OfferedAnswerMaster>(entity =>
			{
				entity.ToTable("OfferedAnswerMaster");

				entity.Property(e => e.Title)
					.HasMaxLength(50)
					.IsUnicode(false);
			});
			modelBuilder.Entity<SurveyQuestionAnswer>(entity =>
			{
				entity.ToTable("SurveyQuestionAnswer");
			});

			modelBuilder.Entity<SurveyResponseMaster>(entity =>
			{
				entity.HasKey(e => e.SurveyResponseId);

				entity.ToTable("SurveyResponseMaster");
			});
			modelBuilder.Entity<SurveySupplierMapping>(entity =>
			{
				entity.ToTable("SurveySupplierMapping");
				entity.HasKey(e => e.Id);
			});

			modelBuilder.Entity<TaskComplition>(entity =>
			{
				entity.ToTable("TaskComplition");

				entity.Property(e => e.Comments).IsUnicode(false);

				entity.Property(e => e.CreatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<TaskMetaData>(entity =>
			{
				entity.HasKey(e => e.TaskId);

				entity.ToTable("TaskMetaData");
			});

			modelBuilder.Entity<TaskTag>(entity =>
			{
				entity.ToTable("TaskTag");
			});

			modelBuilder.Entity<TenanttMaster>(entity =>
			{
				entity.HasKey(e => e.TenantId);

				entity.ToTable("TenanttMaster");

				entity.Property(e => e.ClientName)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.Name)
					.HasMaxLength(250)
					.IsUnicode(false);

				entity.Property(e => e.ShortCode)
					.HasMaxLength(250)
					.IsUnicode(false);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
				entity.Property(e => e.DeletedOn).HasColumnType("date");
			});

			modelBuilder.Entity<TenantTheme>(entity =>
			{
				entity.HasKey(e => e.ThemeId);

				entity.ToTable("TenantTheme");

				entity.Property(e => e.CreatedOn).HasColumnType("date");

				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});
			modelBuilder.Entity<UserMaster>(entity =>
			{
				entity.HasKey(e => e.UserId);

				entity.ToTable("UserMaster");

				entity.Property(e => e.EmailId)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.FirstName)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.LastName)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.KCUserId)
					.HasMaxLength(250)
					.IsUnicode(false);
				entity.Property(e => e.KCUsername)
					.HasMaxLength(250)
					.IsUnicode(false);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
				entity.Property(e => e.DeletedOn).HasColumnType("date");
			});
			modelBuilder.Entity<KCRoleToRoleMapping>(entity =>
			{
				entity.ToTable("KCRoleToRoleMapping");
				entity.HasKey(e => e.KCRoleToRoleMappingId);
			});
			modelBuilder.Entity<UserSettingMaster>(entity =>
			{
				entity.ToTable("UserSettingMaster");

				entity.Property(e => e.Name)
					.HasMaxLength(50)
					.IsUnicode(false);
			});

			modelBuilder.Entity<DocumentChangeRequestMaster>(entity =>
			{
				entity.ToTable("DocumentChangeRequestMaster");

				entity.HasKey(e => e.ChangeRequestId);
				entity.Property(e => e.RequestedOn).HasColumnType("date");
			}
			);

			modelBuilder.Entity<ProcessMaster>(entity =>
			{
				entity.ToTable("ProcessMaster");

				entity.HasKey(e => e.ProcessId);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
				entity.Property(e => e.UpdatedOn).HasColumnType("date");
			});

			modelBuilder.Entity<RiskAssesmentMaster>(entity =>
			{
				entity.ToTable("RiskAssesmentMaster");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.CreatedOn).HasColumnType("date");
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}