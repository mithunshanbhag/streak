using Streak.Ui.Models.Storage;
using Streak.Ui.Models.ViewModels.ResultModels;

namespace Streak.Ui.Models;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Habit, HabitViewModel>();

        CreateMap<AppSetting, ReminderSettingsViewModel>()
            .ForMember(x => x.IsReminderEnabled, opt => opt.MapFrom(src => src.IsReminderEnabled == 1));
    }
}