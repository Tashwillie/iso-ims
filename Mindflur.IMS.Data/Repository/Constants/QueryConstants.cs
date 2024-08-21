namespace Mindflur.IMS.Data.Repository.Constants
{
	public static class QueryConstants
	{
		/// <summary>
		/// Non Conformance Donut Chart
		/// </summary>
		public static readonly string GET_Donuts_ChartData_NonConformance_ByDepartment = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                From WorkItemMaster as Nc
                                                                                join TenanttMaster as tm on Nc.TenantId=tm.TenantId
                                                                                join MasterData as md on Nc.DepartmentId = md.Id where Nc.TenantId= {0} and Nc.WorkItemTypeId = {1}
                                                                                Group By Items";

		public static readonly string GET_Donuts_ChartData_NonConformance_ByNCTypes = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                From WorkItemMaster as Nc
                                                                                join TenanttMaster as tm on Nc.TenantId=tm.TenantId
                                                                                join MasterData as md on Nc.SourceId = md.Id where Nc.TenantId= {0} and Nc.WorkItemTypeId = {1} Group By Items";

		public static readonly string GET_Donuts_ChartData_NonConformance_ByStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                From WorkItemMaster as Nc
                                                                                join MasterData as md on Nc.StatusMasterDataId = md.Id
                                                                                join TenanttMaster as tm on Nc.TenantId=tm.TenantId
                                                                                where Nc.TenantId= {0}  and Nc.WorkItemTypeId ={1}
                                                                                Group By Items";

		/// <summary>
		///IncidentManagementSiteOfIncident Donut Chart
		/// </summary>

		public static readonly string GET_Donuts_ChartData_SiteOfIncident_ByDepartment = @"Select DepartmentName as ChartItemKey ,COUNT(*) as ChartItemValue From IncidentMetaData as ims

                                                                        join WorkItemMaster as Wm on ims.WorkItemId = wm.WorkItemId
                                                                        join DepartmentMaster as dm on wm.DepartmentId =dm.DepartmentId
                                                                        join TenanttMaster as tm on Wm.TenantId=tm.TenantId where wm.TenantId={0} Group By DepartmentName";

		public static readonly string GET_Donuts_ChartData_SiteOfIncident_By_Employee = @"Select CONCAT( FirstName ,'  ',LastName) as ChartItemKey ,COUNT(*) as ChartItemValue From IncidentMaster as ims
join UserMaster as US on ims.EmployeeName = US.UserId
join TenanttMaster as tm on ims.TenantId=tm.TenantId where ims.TenantId={0}
Group By FirstName,LastName";

		public static string GET_Donuts_ChartData_SiteOfIncident_By_Status = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From IncidentMetaData as ims
                                    join WorkItemMaster as Wm on ims.WorkItemId = Wm.WorkItemId
                                    join MasterData as md on Wm.StatusMasterDataId = md.Id
                                    join TenanttMaster as tm on Wm.TenantId =tm.TenantId where Wm.TenantId= {0} Group By Items ";

		public static readonly string GET_Donuts_ChartData_SiteOfIncidentCorrectiveAction_By_Status = @" Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as wm
  join MasterData as md on Wm.StatusMasterDataId = md.Id
  join TenanttMaster as tm on Wm.TenantId =tm.TenantId
  where Wm.TenantId= {0} and wm.SourceId ={1} and WorkItemTypeId = {2} Group By Items";

		/// <summary>
		///Risk Management Inherent Risk Donut Chart
		/// </summary>
		public static readonly string GET_Donuts_ChartData_InherentRisk_ByDepartment = @"Select DepartmentName as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                From WorkItemMaster as ims
                                                                                join DepartmentMaster as dm on ims.DepartmentId = dm.DepartmentId
                                                                                join TenanttMaster as tm on ims.TenantId=tm.TenantId
                                                                                where ims.TenantId= {0} And ims.WorkItemTypeId = {1}
                                                                                Group By DepartmentName";

		public static readonly string GET_Donuts_ChartData_InherentRisk_ByRiskStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                    From WorkItemMaster as risk 
                                                                                    join MasterData as md on risk.StatusMasterDataId = md.Id
                                                                                    join TenanttMaster as tm on risk.TenantId=tm.TenantId
                                                                                    where risk.TenantId={0} and risk.WorkItemTypeId = {1}
                                                                                    Group By Items";

		public static readonly string GET_Donuts_ChartData_InherentRisk_AllRiskTreatmentByStatus = @"  Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                     From WorkItemMaster as work
                                                                                     join MasterData as md on work.StatusMasterDataId = md.Id
                                                                                     join TenanttMaster as tm on work.TenantId = tm.TenantId
                                                                                     where work.TenantId = {0}  and work.SourceId = {1} and work.WorkItemTypeId = {2} Group By Items";

		public static readonly string GET_Donuts_ChartData_InherentRisk_ByRiskType = @"select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                            From WorkItemMaster as risk
                                                                            join MasterData as md on risk.WorkItemTypeId = md.Id
                                                                            join TenanttMaster as tm on risk.TenantId=tm.TenantId
                                                                            where risk.TenantId= {0} and risk.WorkItemTypeId = {1}
                                                                            Group By Items";

		public static readonly string GET_Donuts_ChartData_InherentRisk_ByRiskOwner = @"Select CONCAT( FirstName ,'  ',LastName) as ChartItemKey ,COUNT(*) as ChartItemValue From RiskManagementInherentRisk as risk
join UserMaster as US on risk.RiskOwner = US.UserId join TenanttMaster as tm on risk.TenantId=tm.TenantId where risk.TenantId={0} Group By FirstName,LastName";

		/// <summary>
		///Risk Management RiskTreatment Donut Chart
		/// </summary>
		///
		public static readonly string GET_Donuts_ChartData_RiskTreatment_ByStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From RiskTreatment as risk
                                                        join Risk as Rk on risk.RiskId=Rk.Id
                                                        join WorkItemMaster as Wm on Rk.WorkItemId= Wm.WorkItemId
                                                        join MasterData as md on Wm.StatusMasterDataId = md.Id
                                                        join TenanttMaster as tm on Wm.TenantId=tm.TenantId where Wm.TenantId= {0}Group By Items";

		public static readonly string GET_Donuts_ChartData_RiskTreatment_ByRiskRating = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From RiskTreatment as risk
join MasterData as md on risk.RiskRatingMasterDataId = md.Id join Risk as inherent on risk.RiskId=inherent.Id
join TenanttMaster as tm on inherent.TenantId=tm.TenantId where inherent.TenantId={0} Group By Items";

		public static readonly string GET_Donuts_ChartData_RiskTreatment_ByResponsiblePerson = @"Select CONCAT( FirstName ,'  ',LastName) as ChartItemKey ,COUNT(*) as ChartItemValue From RiskTreatment as risk
join UserMaster as US on risk.ResponsibleUserId = US.UserId
join Risk as inherent on risk.RiskId=inherent.Id
join TenanttMaster as tm on inherent.TenantId=tm.TenantId where inherent.TenantId={0}
Group By FirstName,LastName";

		/// <summary>
		///Task Master Donut Chart
		/// </summary>

		public static readonly string GET_Donuts_ChartData_TaskMAster_ByStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as task
	join MasterData as md on task.StatusMasterDataId = md.Id
join TenanttMaster as tm on task.TenantId=tm.TenantId where task.TenantId={0} and task.WorkItemTypeId={1} Group By Items ";

		public static readonly string GET_Donuts_ChartData_TaskMaster_By_AssignTo = @"Select CONCAT( FirstName ,'  ',LastName) as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as task
join UserMaster as US on task.ResponsibleUserId = US.UserId join TenanttMaster as tm on task.TenantId=tm.TenantId where task.TenantId={0} and task.WorkItemTypeId={1} Group By FirstName,LastName";

		public static readonly string GET_Donuts_ChartData_TaskMaster_By_Priority = @"Select w1.TokenName as ChartItemKey,count(w.WorkItemId)as ChartItemValue from WorkItemMaster as w
 Left Outer join WorkItem_WorkItemToken as wt on w.WorkItemId=wt.WorkItemId
 Left outer join WorkItem_Tokens as w1 on wt.TokenId =w1.TokenId
 Where w.WorkItemTypeId={1} and w.TenantId={0} group by w1.TokenName";

		public static readonly string GET_Donuts_ChartData_TaskMAster_BySource = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as task
	join MasterData as md on task.SourceId = md.Id
join TenanttMaster as tm on task.TenantId=tm.TenantId where task.TenantId={0} and task.WorkItemTypeId={1} Group By Items ";

		public static readonly string GET_Donuts_ChartData_TaskMaster_Classification = @"Select Items as ChartItemKey ,COUNT(task.WorkItemId) as ChartItemValue From WorkItemMaster as task
left Outer join TaskTag as tag on task.WorkItemId=tag.TaskId
left Outer join MasterData as master on tag.MasterDataTaskTagId=master.Id
Join TenanttMaster as tm on task.TenantId=tm.TenantId
Where task.TenantId={0} and task.WorkItemTypeId={1} group by Items";

		/// <summary>
		/// Corrective Action Donut Chart
		/// </summary>

		public static readonly string GET_Donuts_ChartData_CorrectiveACtionTask_ByStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as wi
                                                join MasterData as md on wi.StatusMasterDataId = md.Id
                                                join TenanttMaster as tm on wi.TenantId=tm.TenantId
                                                where wi.TenantId = {0} and WI.WorkItemTypeId = {1} Group By Items";

		public static readonly string GET_Donuts_ChartData_CorrectiveACtionTask_ByAssignTo = @"select CONCAT(FirstName , ' ', LastName) as ChartItemKey , Count(*) as ChartItemValue from WorkItemMaster as wi 
                                                                    join UserMaster as um on wi.ResponsibleUserId = um.UserId
                                                                    join TenanttMaster as tm on wi.TenantId = tm.TenantId
                                                                    where wi.TenantId = {0} and wi.WorkItemTypeId = {1}
                                                                    group by FirstName, LastName";

		public static readonly string GET_Donuts_ChartData_CorrectiveACtionTask_ByPriority = @"select TokenName as ChartItemKey ,COUNT(*) as ChartItemValue From TaskMetaData
as task
                                                            join WorkItemMaster as Wi on Task.WorkItemId=Wi.WorkItemId

                                                           join WorkItem_WorkItemToken as w2 on task.WorkItemId=w2.WorkItemId
															join WorkItem_Tokens as w3 on w2.TokenId=w3.TokenId
                                                            join TenanttMaster as tm on Wi.TenantId=tm.TenantId where Wi.TenantId={0} and Wi.SourceId=231
	                                                        Group By TokenName";//SourceId is Hack for now

		/// <summary>
		///
		/// Project Donut Chart
		/// </summary>
		public static readonly string GET_Donuts_ChartData_Projects_ByPriority = @"select TokenName as ChartItemKey,count(*) as ChartItemValue from Project as pj
                                                    join WorkItemMaster as wm  on pj.WorkItemId =wm.WorkItemId
                                                    join WorkItem_WorkItemToken as wt on pj.WorkItemId =wt.WorkItemId
                                                    join WorkItem_Tokens as w1 on wt.TokenId=w1.TokenId
                                                    join TenanttMaster as tm on wm.TenantId=tm.TenantId
                                                    where  tm.TenantId={0} and w1.ParentTokenId={1} group by TokenName";

		public static readonly string GET_Donuts_ChartData_Projects_ByStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as wm
  join MasterData as md on wm.StatusMasterDataId= md.Id
  join TenanttMaster as tm on wm.TenantId=tm.TenantId where wm.TenantId= {0} and Wm.WorkItemTypeId={1}
Group By Items";

		public static readonly string GET_Donuts_ChartData_Projects_ByUser = @"Select CONCAT( FirstName ,'  ',LastName) as ChartItemKey ,COUNT(*) as ChartItemValue From Project as project
	   join UserMaster as US on project.AssignedToUserId = US.UserId join TenanttMaster as tm on project.TenantId=tm.TenantId where project.TenantId={0}
Group By FirstName,LastName";

		public static readonly string GET_Donuts_ChartData_Projects_ByPhase = @" Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                                   From WorkItemMaster as work
                                                                                   join MasterData as md on work.StatusMasterDataId = md.Id
                                                                                   join TenanttMaster as tm on work.TenantId = tm.TenantId
                                                                                   where work.TenantId = {0}  and work.SourceId = {1} and work.WorkItemTypeId = {2} Group By Items";

		public static readonly string GET_Donuts_ChartData_Projects_ByTask = @"  Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                             From WorkItemMaster as work
                                                                             join MasterData as md on work.StatusMasterDataId = md.Id
                                                                             join TenanttMaster as tm on work.TenantId = tm.TenantId
                                                                             where work.TenantId = {0}  and work.SourceId = {1} and work.WorkItemTypeId = {2} Group By Items";

		/// <summary>
		/// Bar Graphs
		/// </summary>

		public static readonly string GET_BarGraph_ChartData_NonConformance_Open_Close = @"Select Items as ChartItemKey ,DATENAME(month,Nc.Date) as ChartItemDate,Count(DATENAME(month,Nc.Date)) as ChartItemValue from NonConformity as Nc
						join MasterData as Md on Nc.Status=md.Id
						join TenanttMaster as tm on Nc.TenantId=tm.TenantId
						where Nc.TenantId={0}
						Group by md.Items,(DATENAME(month,Nc.Date))";

		public static readonly string GET_BarGraph_ChartData_CorrectiveAction_Status = @"Select  Items as ChartItemKey , DATENAME(month,Ta.CreatedOn) as ChartItemDate, Count(DATENAME(month,Ta.CreatedOn))as ChartItemValue From NewCorrectiveAction as Ta
						                                                       join MasterData as md on Ta.Status=md.Id join TenanttMaster as tm on Ta.TenantId=tm.TenantId
						                                                       where Ta.TenantId={0} Group by md.Items,DATENAME(month,Ta.CreatedOn)";

		public static readonly string GET_BarGraph_ChartData_Task_Status = @"Select  Items as ChartItemKey , DATENAME(month,Ta.CreatedOn) as ChartItemDate, Count(DATENAME(month,Ta.CreatedOn))as ChartItemValue From TaskMetaData as Ta
						join MasterData as md on Ta.Status=md.Id join TenanttMaster as tm on Ta.TenantId=tm.TenantId
						where Ta.TenantId={0} Group by md.Items,DATENAME(month,Ta.CreatedOn)";

		/// <summary>
		///SurveyMasterData Donut Chart
		/// </summary>

		public static readonly string GET_Donuts_ChartData_CountOfResponses_BySurveyQuestionId = @"Select Title as ChartItemKey ,COUNT(*) as ChartItemValue From SurveyQuestionAnswer as surveyQueAns
join QuestionMaster as question on surveyQueAns.SurveyQuestionId =question.QuestionId
where surveyQueAns.SurveyQuestionId={0}
	Group By Title";

		/// <summary>
		///AuditFinding Donut Chart
		/// </summary>

		public static readonly string GET_Donuts_ChartData_AuditFinding_ByClassificationByAuditId = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue from WorkItemMaster as workItem
join AuditFindingsMapping as af on workItem.WorkItemId =af.WorkItemId
join AuditableItems as ai on af.AuditableItemId =ai.Id
join AuditProgram as ap on ai.AuditProgramId =ap.Id
join MasterData as md on workItem.CategoryId=md.Id
join TenanttMaster as tm on ap.TenantId=tm.TenantId
Where   ap.Id={0} and ap.TenantId={1} group by Items";

		public static readonly string GET_Donuts_ChartData_ByCompliance = @"Select Compliance as ChartItemKey ,COUNT(*) as ChartItemValue From AuditChecklist  as checklist
                                                                 join AuditProgram as ap on checklist.AuditProgramId = ap.Id
                                                                 join TenanttMaster as tm on ap.TenantId = tm.TenantId
																where checklist.AuditProgramId = {0} and ap.TenantId = {1}
																Group By Compliance";

		public static readonly string GET_Donuts_ChartData_ByDepartment = @"Select DepartmentName as ChartItemKey ,COUNT(*) as ChartItemValue
                                                                    From WorkItemMaster as ai
                                                                    join DepartmentMaster as dp on ai.DepartmentId= dp.DepartmentId
                                                                    join TenanttMaster as tm on ai.TenantId = tm.TenantId
                                                                    where  ai.SourceItemId = {1} and ai.TenantId = {0}
                                                                    Group By DepartmentName";

		/// <summary>
		///ProjectTask Status Donut Chart
		/// </summary>

		public static readonly string GET_Donuts_ChartData_Projects_Tasks_ByStatus = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue from WorkItemMaster as task
join WorkItemMaster as project on task.SourceItemId =project.WorkItemId
join MasterData as md on task.StatusMasterDataId=md.Id
where task.TenantId={0} and task.WorkItemTypeId={2} and project.WorkItemTypeId={1} group by Items";

		/// <summary>
		///Audit Checklist and Audit Finding Bar Graphs
		/// </summary>
		public static readonly string GET_BarGraph_ChartData_ComplianceClause = @"Select clm.ClauseNo  as Clause, Compliance as Compliance, COUNT(*) as Total From AuditChecklist  as ac
																						join AuditProgram as ap on ac.AuditProgramId = ap.Id
                                                                                        join TenanttMaster as tm on ap.TenantId = tm.TenantId
																						join ChecklistMaster as cm on ac.ChecklistMasterId = cm.Id
																						join ClauseMaster as clm on cm.ClauseMasterId = clm.Id
																						where ac.AuditProgramId = {0} and ap.TenantId ={1}
																						Group By Compliance, clm.ClauseNo";

		public static readonly string GET_Donuts_ChartData_SurveyMasterData_ByResponse = @"Select qm.Title as QuestionName ,ofs.Title as ResponseName,COUNT(*) as ResponseCount From SurveyQuestionAnswer as surveyQueAns
	join SurveyQuestions as sq on surveyQueAns.SurveyQuestionId=sq.QuestionId
	join QuestionMaster as qm on surveyQueAns.SurveyQuestionId=qm.QuestionId
	join SurveyMasterData as sd on sq.SurveyId=sd.SurveyId
	join TenanttMaster as tm on qm.TenantId=tm.TenantId
	join OfferedAnswerMaster as ofs on surveyQueAns.OfferedAnswerId = ofs.SurveyOfferedAnswerId
		where sq.SurveyId={0} and qm.TenantId={1}
	Group By ofs.Title,qm.Title

	";

		public static readonly string GET_BarGraph_ChartData_AuditFinding_Open_Closes = @"Select md.Items as Classification ,md1.Items as FindingStatus, Count(*) as ChartItemValue From AuditFinding as finding
																				join MasterData as md on finding.MasterDataFindingCategoryId = md.Id
																				join MasterData as md1 on finding.MasterDataFindingStatusId = md1.Id
																				join AuditProgram as ap on finding.AuditProgramId = ap.Id
																				join TenanttMaster as tm on ap.TenantId = tm.TenantId
																				where finding.AuditProgramId = {0} and ap.TenantId ={1}
																				Group By md.Items, md1.Items";

		public static readonly string GET_DonutsChart_InternalAudit_Nc_Types = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From AuditChecklist as ac
join AuditProgram as ap on ac.AuditProgramId=ap.Id
left join MasterData as md on ac.MasterDataClassificationId = md.Id
join TenanttMaster as tm on ap.TenantId=tm.TenantId where ap.TenantId={0} and ap.Id={1} Group By Items";

		public static readonly string GET_DonutsChart_InternalAudit_OverAll_Compliance = @"Select Compliance as ChartItemKey ,COUNT(*) as ChartItemValue From AuditChecklist  as checklist
join AuditProgram as ap on checklist.AuditProgramId=ap.Id
    where ap.TenantId={0} and ap.Id={1}
    Group By Compliance";

		public static readonly string GET_Donuts_InternalAudit_OverAll_Reviewd = @"Select Reviewed as ChartItemKey ,COUNT(*) as ChartItemValue From AuditChecklist  as checklist
join AuditProgram as ap on checklist.AuditProgramId=ap.Id
    where ap.TenantId={0} and ap.Id={1}
    Group By Reviewed";

		public static readonly string GET_BarGraph_InternalAudit_Compliance_Status_BySection = @"Select clm.ClauseName  as Clause, Compliance as Compliance, COUNT(*) as Total From AuditChecklist  as ac
																						join AuditProgram as ap on ac.AuditProgramId = ap.Id
                                                                                        join TenanttMaster as tm on ap.TenantId = tm.TenantId
																						join ChecklistMaster as cm on ac.ChecklistMasterId = cm.Id
																						join ClauseMaster as clm on cm.ClauseMasterId = clm.Id
																						where   ap.TenantId ={0} and ac.AuditProgramId = {1}
																						Group By Compliance, clm.ClauseName";

		//Audit Finding Charts
		public static readonly string GET_Audit_Finding_Chart_By_Status = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From AuditFindingsMapping as finding
join AuditableItems as Ai on finding.AuditableItemId =Ai.Id
join AuditProgram as ap on Ai.AuditProgramId = ap.Id
join WorkItemMaster as wm on finding.WorkItemId = wm.WorkItemId
join MasterData as md on wm.StatusMasterDataId = md.Id

join TenanttMaster as tm on ap.TenantId = tm.TenantId
where ap.Id ={0} and ap.TenantId = {1}
Group By Items";

		public static readonly string GET_Audit_Finding_With_Risk_Rating = @"Select  Items as ChartItemKey,COUNT(risk.RiskRating) as ChartItemValue from AuditProgram as ap

		join AuditFinding as af on ap.Id=af.AuditProgramId
		join RiskManagementInherentRisk as risk on af.Id=risk.SourceId

	   -- join DepartmentMaster as md on risk.ProcessDepartment = md.DepartmentId
		join MasterData as master on risk.RiskRating =master.Id
		join TenanttMaster as tm on ap.TenantId = tm.TenantId
		where ap.TenantId = {0} Group By Items";

		public static readonly string GET_Audit_Finding_Category_DepartmentWise_respect_to_riskRating = @"Select DepartmentName as ChartItemKey ,COUNT(*) as ChartItemValue from WorkItemMaster as ap
                                                                                            join AuditFindingsMapping as af on ap.WorkItemId=af.WorkItemId
                                                                                            join DepartmentMaster as md on ap.DepartmentId = md.DepartmentId
                                                                                            join TenanttMaster as tm on ap.TenantId = tm.TenantId
                                                                                            where ap.TenantId = {0}
                                                                                            Group By DepartmentName";

		public static readonly string GET_Donuts_ChartData_AuditFinding_ByClassifications = @"select Items as ChartItemKey ,COUNT(*) as ChartItemValue From AuditFindingsMapping as finding
join WorkItemMaster as master on finding.WorkItemId=master.WorkItemId
join MasterData as md on master.CategoryId = md.Id
join TenanttMaster as tm on master.TenantId = tm.TenantId where  master.TenantId = {0} Group By Items";

		public static readonly string GET_Audit_Finding_Category_Chart_By_Status = @"select Items as ChartItemKey ,COUNT(*) as ChartItemValue From AuditFindingsMapping as finding
join WorkItemMaster as master on finding.WorkItemId=master.WorkItemId
join MasterData as md on master.StatusMasterDataId = md.Id
join TenanttMaster as tm on master.TenantId = tm.TenantId where  master.TenantId = {0} Group By Items";

		// Charts Item for Internal Audit Landing Page
		public static readonly string GET_InternalAudits_NonConformities_By_Status = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue
 From NonConformity as Nc join MasterData as md on Nc.Status = md.Id join TenanttMaster as tm on Nc.TenantId=tm.TenantId  where Nc.TenantId={0} and Nc.Source=90 Group By Items";

		public static readonly string GET_InternalAudits_Corrective_Actions_By_Status = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as Nc 
join MasterData as md on Nc.StatusMasterDataId = md.Id 
where Nc.TenantId = {0} and Nc.WorkItemTypeId =215 Group By Items";

		public static readonly string GET_Internal_Audit_AuditableItems = @"select Items as ChartItemKey ,count(*)as ChartItemValue from AuditProgram as ap
            join AuditableItems as Ai on ap.Id=Ai.AuditProgramId
            join MasterData as md On Ai.Status=md.Id
            join TenanttMaster as tm on ap.TenantId=tm.TenantId
            where ap.TenantId={0} group by Items";

		public static readonly string GET_Internal_Audit_By_Categories = @"select Items as ChartItemKey ,count(*)as ChartItemValue from AuditProgram as ap

            join MasterData as md On ap.MasterDataCategoryId=md.Id
            join TenanttMaster as tm on ap.TenantId=tm.TenantId
            where ap.TenantId={0} group by Items";

		public static readonly string GET_Internal_Audit_By_Finding_Department = @"select DepartmentName as ChartItemKey ,count(*)as ChartItemValue from AuditFindingsMapping as af
                join WorkItemMaster as WM on af.WorkItemId=WM.WorkItemId           
               join DepartmentMaster as md On wm.DepartmentId=md.DepartmentId  
                join TenanttMaster as tm on WM.TenantId=tm.TenantId  
                 where WM.TenantId= {0} group by DepartmentName";

		// Charts Item for Management Review Landing Page
		public static readonly string GET_Management_Review_Meeting_List_By_Status = @"select Items as ChartItemKey,count(*)as ChartItemValue from MeetingPlan as mp
            join MasterData as md on mp.Status =md.Id
            join TenanttMaster as tm on mp.TenantId=tm.TenantId
            where mp.TenantId={0} group by Items";

		public static readonly string GET_Management_Review_Meeting_Agendas_List_By_Staus = @"select Items as ChartItemKey,count(*)as ChartItemValue from MeetingPlan as mp
            join MeetingAgendaMapping as map on mp.Id=map.MeetingId
            join MasterData as md on mp.Status =md.Id
            join TenanttMaster as tm on mp.TenantId=tm.TenantId
            where mp.TenantId={0} group by Items";

		public static readonly string GET_Management_Review_Meeting_Minutes_list_By_status = @"select Items as ChartItemKey,count(*)as ChartItemValue from MeetingPlan as mp
            join MinutesOfMeeting as map on mp.Id=map.MeetingId
            join MasterData as md on mp.Status =md.Id
            join TenanttMaster as tm on mp.TenantId=tm.TenantId
            where mp.TenantId={0} group by Items";

		public static readonly string GET_Management_Review_Meeting_Minutes_list_By_AssignTo = @"select FirstName as ChartItemKey,COUNT(*)as ChartItemValue from WorkItemMaster as task
join MinutesOfMeeting as mm on task.WorkItemId=mm.TaskId
join MeetingPlan as mp on mm.MeetingId=mp.Id
join UserMaster as md on task.ResponsibleUserId =md.UserId
join TenanttMaster as tm on mp.TenantId=tm.TenantId
where mp.TenantId={0} and task.WorkItemTypeId={1} group by FirstName";

		public static readonly string GET_Management_Review_Meeting_Minutes_list_By_Priority = @"select TokenName as ChartItemKey,COUNT(*)as ChartItemvalue from TaskMetaData as task
join WorkItemMaster as wm on task.WorkItemId=wm.WorkItemId
join MinutesOfMeeting as mm on task.WorkItemId=mm.TaskId
join MeetingPlan as mp on mm.MeetingId=mp.Id
join WorkItem_WorkItemToken as w2 on task.WorkItemId=w2.WorkItemId
join WorkItem_Tokens as w3 on w2.TokenId=w3.TokenId
join TenanttMaster as tm on mp.TenantId=tm.TenantId  where mp.TenantId={0}  group by TokenName";

		public static readonly string GET_Management_Review_Meeting_Task_By_MeetingId = @"select Items as ChartItemKey,COUNT(*)as ChartItemvalue from TaskMetaData as task
join WorkItemMaster as wm on task.WorkItemId=wm.WorkItemId
join MinutesOfMeeting as mm on task.WorkItemId=mm.TaskId
join MeetingPlan as mp on mm.MeetingId=mp.Id
join masterData as md on wm.StatusMasterDataId =md.Id
where  mp.Id={0} and  mp.TenantId={1} group by Items";

		public static readonly string GET_Management_Review_Meeting_Task_AssignTo_By_MeetingId = @"select FirstName as ChartItemKey,COUNT(*)as ChartItemvalue from TaskMetaData as task
join WorkItemMaster as wm on task.WorkItemId=wm.WorkItemId
join MinutesOfMeeting as mm on task.WorkItemId=mm.TaskId
join MeetingPlan as mp on mm.MeetingId=mp.Id
join UserMaster as md on wm.ResponsibleUserId =md.UserId
where  mp.Id={0} and  mp.TenantId={1} group by FirstName";

		public static readonly string GET_Management_Review_Meeting_Task_Priority_By_MeetingId = @"select TokenName as ChartItemKey,COUNT(*)as ChartItemvalue from TaskMetaData as task
join WorkItemMaster as wm on task.WorkItemId=wm.WorkItemId
join MinutesOfMeeting as mm on task.WorkItemId=mm.TaskId
join MeetingPlan as mp on mm.MeetingId=mp.Id
join WorkItem_WorkItemToken as w2 on task.WorkItemId=w2.WorkItemId
join WorkItem_Tokens as w3 on w2.TokenId=w3.TokenId
join TenanttMaster as tm on mp.TenantId=tm.TenantId  where mp.Id={0} and  mp.TenantId={1} and w3.ParentTokenId=61 group by TokenName";

		//Supplier Master landing Page Charts
		public static readonly string GET_Supplier_List_By_status = @"select Items as ChartItemKey,COUNT(*) as ChartItemValue from SupplierMaster as sp
join MasterData as md on sp.MasterDataSupplierStatusId=md.Id
join TenanttMaster as tm on sp.TenantId=tm.TenantId
where sp.TenantId={0} group by Items";

		public static readonly string GET_TaskList_for_Corrective_Action = @"select Items as ChartItemKey ,COUNT(*) as ChartItemValue from WorkItemMaster as task
join WorkItemMaster as ca on task.SourceItemId=ca.WorkItemId

join MasterData as md on task.StatusMasterDataId=md.Id
join TenanttMaster as tm on ca.TenantId=tm.TenantId
where ca.TenantId={0} and  ca.WorkItemTypeId={1} and task.WorkItemTypeId={2} group by Items";

		public static readonly string GET_TaskList_for_Non_Conformance = @"select Items as ChartItemKey ,COUNT(*) as ChartItemValue from WorkItemMaster as task
join WorkItemMaster as ca on task.SourceItemId=ca.WorkItemId
join WorkItemMaster as Nc on ca.SourceItemId=Nc.WorkItemId
join MasterData as md on task.StatusMasterDataId=md.Id
join TenanttMaster as tm on Nc.TenantId=tm.TenantId
where Nc.TenantId={0} and Nc.WorkItemTypeId={1} and ca.WorkItemTypeId={2} and task.WorkItemTypeId={3} group by Items";

		public static readonly string GET_CA_List_for_Non_Conformance = @"select Items as ChartItemKey,count(*)as ChartItemvalue
                                                                from WorkItemMaster as ca
                                                                    join MasterData as md on ca.StatusMasterDataId =md.Id
                                                                    join TenanttMaster as tm on ca.TenantId=tm.TenantId
                                                                    where ca.WorkItemTypeId= {1} and ca.TenantId= {0}
                                                                    group by Items";

		public static readonly string GET_CA_List_for_Incident = @"select Items as ChartItemKey,count(*)as ChartItemvalue from WorkItemMaster as ca
join MasterData as md on ca.SourceId =md.Id
join TenanttMaster as tm on ca.TenantId=tm.TenantId
where ca.TenantId={0} and  ca.SourceId = 219 and ca.WorkItemTypeId = 215 group by Items";

		public static readonly string GET_SurveyList = @"select Items as ChartItemKey,count(*)as ChartItemvalue from SurveyMasterData as survey
join MasterData as md on survey.MasterDataSurveyStatusId=md.Id
join TenanttMaster as tm on survey.TenantId=tm.TenantId
where survey.TenantId={0} group by Items";

		public static readonly string GET_WorkItem_Status = @"Select Items as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as wi
                                                join MasterData as md on wi.StatusMasterDataId = md.Id
                                                join TenanttMaster as tm on wi.TenantId=tm.TenantId
                                                where wi.TenantId={0} and wi.SourceId = {1} Group By Items";

		public static readonly string GET_WorkItems_byAssignTo = @"Select CONCAT(FirstName , ' ', LastName) as ChartItemKey , Count(*) as ChartItemValue From WorkItemMaster as wi 
                                                          join UserMaster as um on wi.AssignedToUserId = um.UserId
					                                       join TenanttMaster as tm on wi.TenantId = tm.TenantId
					                                      Where wi.TenantId = {0} and wi.SourceId = {1}
				                                             Group by FirstName, LastName";

		public static readonly string Get_WorkItems_By_Department = @"Select DepartmentName as ChartItemKey ,COUNT(*) as ChartItemValue From WorkItemMaster as wi
                                                join DepartmentMaster as md on wi.DepartmentId = md.DepartmentId
                                                join TenanttMaster as tm on wi.TenantId=tm.TenantId
                                                where wi.TenantId={0} and wi.SourceId = {1} Group By DepartmentName";

		public static readonly string GET_TotalModules_BySourceId = @"Select Items as ChartItemKey , COUNT (*) as ChartItemValue From WorkItemMaster as wi
                                                                       join MasterData as md on wi.SourceId = md.Id
			                                                           join TenanttMaster as tm on wi.TenantId=tm.TenantId
			                                                           where wi.TenantId= {0} and wi.SourceId = {1} Group By Items";

		//Document Chart Donout
		public static readonly string GET_DocumentChart_By_Status = @"select Items as ChartItemKey,Count(*) as ChartItemValue from Documents as doc
join MasterData as md on doc.DocumentStatusMasterDataId=md.Id
join TenanttMaster as tenant on doc.TenantId=tenant.TenantId
Where  doc.TenantId={0} group by Items";

		public static readonly string GET_DocumentChart_By_Category = @"select Items as ChartItemKey,Count(*) as ChartItemValue from Documents as doc
join MasterData as md on doc.DocumentCategoryMasterDataId=md.Id
join TenanttMaster as tenant on doc.TenantId=tenant.TenantId
Where  doc.TenantId={0} group by Items";

		public static readonly string GET_DocumentChart_By_Priority = @"select Items as ChartItemKey,Count(*) as ChartItemValue from Documents as doc
join MasterData as md on doc.DocumentPriorityMasterDataId=md.Id
join TenanttMaster as tenant on doc.TenantId=tenant.TenantId
Where  doc.TenantId={0} group by Items";

		public static readonly string GET_DocumentChart_By_Classification = @"select Items as ChartItemKey,Count(*) as ChartItemValue from Documents as doc
join DocumentTags as tags on doc.DocumentId=tags.DocumentId
join MasterData as md on tags.MasterDataDocumentTagId=md.Id
join TenanttMaster as tenant on doc.TenantId=tenant.TenantId
Where  doc.TenantId={0} group by Items";

		//Survey Chart Donut
		public static readonly string GET_DocumentChart_By_SurveyIdMaxResponces = @"SELECT OAM.Title AS ChartItemKey, COUNT(*) AS ChartItemValue
								FROM SurveyQuestions AS sq
								JOIN SurveyMasterData AS SMD ON sq.SurveyId = SMD.SurveyId
								JOIN OfferedAnswerMaster AS OAM ON sq.OfferedAnswerId = OAM.SurveyOfferedAnswerId
								JOIN TenanttMaster as TM on SMD.TenantId = Tm.TenantId
								WHERE sq.SurveyId = {0} and SMD.TenantId = {1} 
								GROUP BY OAM.Title
								HAVING COUNT(*) = (
									SELECT Max(CountItems)
									FROM (
										SELECT COUNT(*) AS CountItems
										FROM SurveyQuestions AS sq
									   JOIN SurveyMasterData AS SMD ON sq.SurveyId = SMD.SurveyId       
										JOIN OfferedAnswerMaster AS OAM ON sq.OfferedAnswerId = OAM.SurveyOfferedAnswerId
										WHERE sq.SurveyId = {0}
										GROUP BY OAM.Title
									) AS Counts
								);";

		public static readonly string GET_DocumentChart_By_SurveyIdMinResponces = @"SELECT OAM.Title AS ChartItemKey, COUNT(*) AS ChartItemValue
								FROM SurveyQuestions AS sq
								JOIN SurveyMasterData AS SMD ON sq.SurveyId = SMD.SurveyId
								JOIN OfferedAnswerMaster AS OAM ON sq.OfferedAnswerId = OAM.SurveyOfferedAnswerId
								JOIN TenanttMaster as TM on SMD.TenantId = Tm.TenantId
								WHERE sq.SurveyId = {0} and SMD.TenantId = {1} 
								GROUP BY OAM.Title
								HAVING COUNT(*) = (
									SELECT Min(CountItems)
									FROM (
										SELECT COUNT(*) AS CountItems
										FROM SurveyQuestions AS sq
									   JOIN SurveyMasterData AS SMD ON sq.SurveyId = SMD.SurveyId       
										JOIN OfferedAnswerMaster AS OAM ON sq.OfferedAnswerId = OAM.SurveyOfferedAnswerId
										WHERE sq.SurveyId = {0}
										GROUP BY OAM.Title
									) AS Counts
								);";
	}
}