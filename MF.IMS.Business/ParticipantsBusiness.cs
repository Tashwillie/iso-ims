using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class ParticipantsBusiness : IParticipantsBusiness
    {
        private readonly IParticipantsRepository _participantsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditProgramRepository _auditProgramRepository;
        private readonly IMeetingPlanRepository _meetingPlanRepository;
        private readonly IMessageService _messageService;

        public ParticipantsBusiness(IMessageService messageService, IParticipantsRepository participantsRepository, IUserRepository userRepository, IAuditProgramRepository auditProgramRepository, IMeetingPlanRepository meetingPlanRepository)
        {
            _participantsRepository = participantsRepository;
            _userRepository = userRepository;
            _auditProgramRepository = auditProgramRepository;
            _meetingPlanRepository = meetingPlanRepository;
            _messageService = messageService;
        }

        public async Task AddPaticipants(AddParticipantViewModel addParticipant, int moduleId, int userId)
        {
           await  _participantsRepository.ValidateParticipants(addParticipant,moduleId);
            Participant participant = new Participant();
            participant.ModuleId = moduleId;
            participant.ModuleEntityId = addParticipant.ModuleEntityId;
            participant.RoleId = addParticipant.RoleId;
            participant.UserId = addParticipant.UserId;
            participant.CreatedBy = userId;
            participant.CreatedOn = DateTime.UtcNow;

            await _participantsRepository.AddAsync(participant);
        }

        public async Task<PaginatedItems<ParticipantsListView>> GetAuditParticipantsList(GetParticipantsList getParticipantsList, int moduleId)
        {
            return await _participantsRepository.GetAuditParticipantsList(getParticipantsList, moduleId);
        }

        public async Task<PaginatedItems<ParticipantsListView>> GetMinutesParticipantsList(GetParticipantsList getParticipantsList, int moduleId)
        {
            return await _participantsRepository.GetMinutesParticipantsList(getParticipantsList, moduleId);
        }

        public async Task UpdateParticipants(UpdateParticipantViewModel updateParticipant, int participantId, int userId)
        {
            var participant = await _participantsRepository.GetByIdAsync(participantId);
            if (participant == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.ParticipantNotFoundErrorMessage), participantId);
            }
            participant.UserId = updateParticipant.UserId;
            participant.RoleId = updateParticipant.RoleId;
            participant.UpdatedOn = DateTime.UtcNow;
            participant.UpdatedBy = userId;
            await _participantsRepository.UpdateAsync(participant);

         
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = participantId,              
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.InernalAuditSchedule,
                ItemId = participant.ParticipantId,
                Description = "Participant Has Been Updated",
                Title = "participant",
                Date = participant.UpdatedOn
            });
        }

        public async Task UpdateParticipantsMarkPresent(int participantId)
        {
            var participant = await _participantsRepository.GetByIdAsync(participantId);
            var meeting = await _meetingPlanRepository.GetByIdAsync(participant.ModuleEntityId);
            var audit = await _auditProgramRepository.GetByIdAsync(participant.ModuleEntityId);

            if (participant == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.ParticipantNotFoundErrorMessage), participantId);
            }
            else if (meeting != null)
            {
                if (meeting.ActualStart != null && meeting.ActualEnd == null)
                {
                    participant.MarkPresent = DateTime.UtcNow;
                    participant.IsPresent=true;
                    await _participantsRepository.UpdateAsync(participant);
                }
                else
                {
                    throw new BadRequestException(string.Format(ConstantsBusiness.MeetingParticipantNotMarkPresentErrorMessage));
                }
            }
            else if (audit != null)
            {
                if (audit.ActualStart != null && audit.ActualEnd == null)
                {
                    participant.MarkPresent = DateTime.UtcNow;
                    participant.IsPresent = true;
                    await _participantsRepository.UpdateAsync(participant);
                }
                else
                {
                    throw new BadRequestException(string.Format(ConstantsBusiness.AuditParticipantNotMarkPresentErrorMessage));
                }
            }
        }

        public async Task DeleteParticipants(int participantId, int userId)
        {
            var participant = await _participantsRepository.GetByIdAsync(participantId);
            if (participant == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.ParticipantNotFoundErrorMessage), participantId);
            }

            participant.DeletedOn = DateTime.UtcNow;
            participant.DeletedBy = userId;
            await _participantsRepository.UpdateAsync(participant);

            
            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
                Action = IMSControllerActionCategory.Delete,
                Module = IMSControllerCategory.InernalAuditSchedule,
                ItemId = participantId,
                Description = participantId + "Participant Has Been Deleted",
                Title = "Participant",
                Date = participant.DeletedOn
            });
        }
    }
}