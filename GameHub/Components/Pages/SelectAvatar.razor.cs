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
            const string folderPath = "Icons";
            for (int i = 1; i <= 20; i++)
            {
                Avatars.Add($"/{folderPath}/avatar_{i}.png");
            }
        }

        protected async Task SelectAvatarFunc(string avatar)
        {
            await OnAvatarSelected.InvokeAsync(avatar);
        }

        // Frontend styling for each avatar option
        protected string GetAvatarStyle(string avatar)
        {
            var isSelected = avatar == AfterSelectAvatar;
            var isUsed = UsedAvatars?.Contains(avatar) == true;

            var style = @"
                width:5rem;
                height:5rem;
                border-radius:50%;
                overflow:hidden;
                display:flex;
                align-items:center;
                justify-content:center;
                cursor:pointer;
                padding:0;
                transition: transform 0.15s ease-in-out, box-shadow 0.15s ease-in-out, border-color 0.15s ease-in-out;
            ";

            if (isSelected)
            {
                style += "border: 3px solid #4a90e2; transform: scale(1.08); box-shadow: 0 0 10px rgba(74, 144, 226, 0.55);";
            }
            else if (isUsed)
            {
                style += "border: 2px solid #cbd5e1; opacity: 0.45; box-shadow: 0 2px 4px rgba(0,0,0,0.1);";
            }
            else
            {
                style += "border: 2px solid transparent; box-shadow: 0 3px 8px rgba(0,0,0,0.2);";
            }

            return style;
        }
    }
}
