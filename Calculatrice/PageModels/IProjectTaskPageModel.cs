using Calculatrice.Models;
using CommunityToolkit.Mvvm.Input;

namespace Calculatrice.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}