using System;
using System.Threading.Tasks;

namespace SpaceKatMotionMapper.Services.Contract;

public interface ITimeAndDeadZoneVMService
{
    void UpdateByDefault();
    void UpdateByGuid(Guid id);
    Task ShowDialogAsync();
}
