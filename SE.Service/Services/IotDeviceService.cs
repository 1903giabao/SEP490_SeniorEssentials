using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface IIotDeviceService
    {
        Task<IBusinessResult> CreateIotDevice(CreateIotDeviceRequest req);
        //Task<IBusinessResult> UpdateIotDevice(int deviceId, CreateIotDeviceRequest req);
        Task<IBusinessResult> GetAllIotDevices();
        Task<IBusinessResult> GetIotDeviceById(int deviceId);
        Task<IBusinessResult> UpdateIotDeviceStatus(int deviceId, string status);
    }

    public class IotDeviceService : IIotDeviceService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public IotDeviceService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateIotDevice(CreateIotDeviceRequest req)
        {
            try
            {
                // Validate DeviceName
                if (string.IsNullOrWhiteSpace(req.DeviceName))
                {
                    return new BusinessResult(Const.FAIL_READ, "DEVICE NAME MUST NOT BE EMPTY");
                }

                // Validate SerialNumber
                if (string.IsNullOrWhiteSpace(req.SerialNumber))
                {
                    return new BusinessResult(Const.FAIL_READ, "SERIAL NUMBER MUST NOT BE EMPTY");
                }

                // Validate BatteryLevel
                if (req.BatteryLevel < 0 || req.BatteryLevel > 100)
                {
                    return new BusinessResult(Const.FAIL_READ, "BATTERY LEVEL MUST BE BETWEEN 0 AND 100");
                }

                var device = _mapper.Map<Iotdevice>(req);
                device.LastConnected = DateTime.UtcNow; // Set the last connected time to now

                var result = await _unitOfWork.IotdeviceRepository.CreateAsync(device);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

/*        public async Task<IBusinessResult> UpdateIotDevice(int deviceId, CreateIotDeviceRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.DeviceName))
                {
                    return new BusinessResult(Const.FAIL_READ, "DEVICE NAME MUST NOT BE EMPTY");
                }

                if (string.IsNullOrWhiteSpace(req.SerialNumber))
                {
                    return new BusinessResult(Const.FAIL_READ, "SERIAL NUMBER MUST NOT BE EMPTY");
                }

                if (req.BatteryLevel < 0 || req.BatteryLevel > 100)
                {
                    return new BusinessResult(Const.FAIL_READ, "BATTERY LEVEL MUST BE BETWEEN 0 AND 100");
                }

                var device = await _unitOfWork.IotdeviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "CANNOT FIND DEVICE");
                }

                device.DeviceName = req.DeviceName;
                device.SerialNumber = req.SerialNumber;
                device.BatteryLevel = req.BatteryLevel;
                device.Note = req.Note;
                device.Status = req.Status;
                device.LastConnected = DateTime.UtcNow; 

                var result = await _unitOfWork.IotdeviceRepository.UpdateAsync(device);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Device updated successfully.", req);
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update device.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }*/

        public async Task<IBusinessResult> GetAllIotDevices()
        {
            try
            {
                var devices = await _unitOfWork.IotdeviceRepository.GetAllAsync();
                var deviceDtos = _mapper.Map<List<IotDeviceDto>>(devices);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, deviceDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetIotDeviceById(int deviceId)
        {
            try
            {
                if (deviceId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid device ID.");
                }

                var device = await _unitOfWork.IotdeviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Device not found.");
                }

                var deviceDto = _mapper.Map<IotDeviceDto>(device);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, deviceDto);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateIotDeviceStatus(int deviceId, string status)
        {
            try
            {
                var device = await _unitOfWork.IotdeviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Device not found.");
                }

                device.Status = status; // Update the status

                var result = await _unitOfWork.IotdeviceRepository.UpdateAsync(device);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Device status updated successfully.");
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update device status.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }
    }
}
