using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class ControllerMasterBusiness : IControllerMasterBusiness
    {
        public readonly IControllerMasterRepository _controllerMasterRepository;
        public ControllerMasterBusiness(IControllerMasterRepository controllerMasterRepository)
        {
            _controllerMasterRepository = controllerMasterRepository;
        }
        public async Task<PaginatedItems<ContollerListView>> GetControllerList(GetControllerListRequest getListRequest)
        {
            return await _controllerMasterRepository.GetControllerList(getListRequest);
        }
        public async Task AddController(PostControllerView controllerMaster, int tenantId)
        {
            ControllerMaster cm = new ControllerMaster();

            cm.ControllerName = controllerMaster.ControllerName;
            await _controllerMasterRepository.AddAsync(cm);
        }
        public async Task<ControllerMaster> GetControllerById(int tenantId, int id)
        {
            var controller = await _controllerMasterRepository.GetByIdAsync(id);
            return controller == null ? throw new NotFoundException(string.Format(ConstantsBusiness.ContollerNotFoundErrorMessage), id) : controller;
        }
        public async Task UpdateController(PostControllerView controller, int tenantId, int id)
        {
            var controllerList = await _controllerMasterRepository.GetByIdAsync(id);
            if (controllerList == null)
            {
                throw new NotFoundException( string.Format(ConstantsBusiness.ContollerNotFoundErrorMessage), id);
            }
            else
            {

                controllerList.ControllerName = controller.ControllerName;
                await _controllerMasterRepository.UpdateAsync(controllerList);
            }
        }
        public async Task DeleteController(int tenantId, int id)
        {
            var controllerList = await _controllerMasterRepository.GetByIdAsync(id);
            if (controllerList == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.ContollerNotFoundErrorMessage), id);
            }
            await _controllerMasterRepository.DeleteAsync(controllerList);
        }
    }
}
