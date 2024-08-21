using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class OfferedAnswerMasterBusiness : IOfferedAnswerMasterBusiness
    {
        private readonly IOfferedAnswerMasterRepository _offeredAnswerMasterRepository;
        public OfferedAnswerMasterBusiness(IOfferedAnswerMasterRepository offeredAnswerMasterRepository)
        {
            _offeredAnswerMasterRepository = offeredAnswerMasterRepository;
        }
        public async Task<IList<OfferedAnswerMaster>> getAllOfferedAnswer()
        {
            return await _offeredAnswerMasterRepository.getAllOfferedAnswer();
        }
        public async Task<OfferedAnswerMaster> getOfferedANswerById(int Id)
        {
            var offeredAnswer = await _offeredAnswerMasterRepository.GetByIdAsync(Id);
            return offeredAnswer == null ? throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id) : offeredAnswer;
        }
        public async Task AddOfferedAnswer(OfferedAnswerMaster offeredAnswerMaster)
        {
            await _offeredAnswerMasterRepository.AddAsync(offeredAnswerMaster);
        }
        public async Task UpdateOfferedAnswer(PutQuestionMasterView offeredAnswerMaster, int Id)
        {
            var offeredAnswer = await _offeredAnswerMasterRepository.GetByIdAsync(Id);
            if (offeredAnswer == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            }
            else
            {
                offeredAnswer.Title = offeredAnswerMaster.Title;
                await _offeredAnswerMasterRepository.UpdateAsync(offeredAnswer);
            }


        }
        public async Task DeleteOfferedAnswer(int Id)
        {
            var offeredAnswer = await _offeredAnswerMasterRepository.GetByIdAsync(Id);
            if (offeredAnswer == null)
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            await _offeredAnswerMasterRepository.DeleteAsync(offeredAnswer);

        }

    }
}
