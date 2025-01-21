using System.Threading.Tasks;

namespace TelegramAutomate.Abstract
{
    public interface IUpdateCommand<TResult>
    {

        Task<bool> IsEnabled();

        Task<bool> OnlyAdmins();

        Task<bool> CanHandle(TResult msg);

        string CommandName();

        string CommandDescription();

        Task<TResult> ExecuteCommandAsync(TResult msg);

    }
}
