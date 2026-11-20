using ImposterGame.Services;
using Microsoft.AspNetCore.Components;

namespace ImposterGame.Pages
{
    public partial class SelectAvatar : ComponentBase
    {
        [Parameter] public EventCallback<string> OnAvatarSelected { get; set; }
        [Parameter] public string? AfterSelectAvatar { get; set; }
        [Parameter] public IEnumerable<string>? UsedAvatars { get; set; }

        public List<string> Avatars { get; } = new();

        protected override void OnInitialized()
        {
            for (int i = 1; i <= GameService.AvatarCount; i++)
            {
                Avatars.Add(GameService.GetAvatarUri(i));
            }
        }

        protected async Task SelectAvatarFunc(string avatar)
        {
            await OnAvatarSelected.InvokeAsync(avatar);
        }

        protected string GetAvatarCssClass(string avatar)
        {
            if (avatar == AfterSelectAvatar)
            {
                return "avatar-option selected";
            }

            if (UsedAvatars?.Contains(avatar) == true)
            {
                return "avatar-option used";
            }

            return "avatar-option";
        }
    }
}
